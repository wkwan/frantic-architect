//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Unibill.Impl;
using Uniject;

namespace Unibill.Impl {
    public class AmazonAppStoreBillingService : IBillingService {
        #region IBillingService implementation

        private IBillingServiceCallback callback;
        private ProductIdRemapper remapper;
        private ILogger logger;
        private IRawAmazonAppStoreBillingInterface amazon;
        private TransactionDatabase tDb;

        public AmazonAppStoreBillingService(IRawAmazonAppStoreBillingInterface amazon, ProductIdRemapper remapper, TransactionDatabase tDb, ILogger logger) {
            this.remapper = remapper;
            this.logger = logger;
            logger.prefix = "UnibillAmazonBillingService";
            this.amazon = amazon;
            this.tDb = tDb;
        }

        public void initialise (IBillingServiceCallback biller) {
            this.callback = biller;
            amazon.initialise(this);
            amazon.initiateItemDataRequest(remapper.getAllPlatformSpecificProductIds());
        }

        public void purchase (string item, string developerPayload) {
            amazon.initiatePurchaseRequest(item);
        }

        public void restoreTransactions () {
            amazon.restoreTransactions();
        }

        #endregion

        public void onSDKAvailable (string isSandbox) {
            bool sandbox = bool.Parse (isSandbox);
            logger.Log("Running against {0} Amazon environment", sandbox ? "SANDBOX" : "PRODUCTION");
        }

        public void onGetItemDataFailed() {
            this.callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED);
            callback.onSetupComplete(false);
        }

        private void onUserIdRetrieved (string userId) {
            tDb.UserId = userId;
        }

        public void onPurchaseFailed(string json) {
            callback.onPurchaseFailedEvent(new PurchaseFailureDescription(json));
        }

        public void onPurchaseSucceeded(string json) {
            Dictionary<string, object> response = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(json);

            string productId = (string) response ["productId"];
            string token = (string) response ["purchaseToken"];
            string transactionId = (string)response ["transactionId"];
            callback.onPurchaseSucceeded (productId, token, transactionId);
        }

        public void onPurchaseUpdateFailed () {
            logger.LogWarning("AmazonAppStoreBillingService: onPurchaseUpdate() failed.");
        }

        private bool finishedSetup;
        public void onPurchaseUpdateSuccess (string json) {
            Dictionary<string, object> responseHash = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(json);
            onUserIdRetrieved (responseHash.getString ("userId"));

            if (!finishedSetup) {
                var products = Util.DeserialiseProductList (responseHash.getHash ("products"));
                callback.onSetupComplete (products);
                finishedSetup = true;
            } else {
                // We must be restoring transactions, which is done automatically
                // on startup so report success.
                callback.onTransactionsRestoredSuccess();
            }
        }

        public void finishTransaction (PurchasableItem item, string transactionId)
        {
            amazon.finishTransaction (transactionId);
        }
    }
}
