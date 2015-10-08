//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uniject;

namespace Unibill.Impl {

    /// <summary>
    /// App Store implementation of <see cref="IBillingService"/>.
    /// This class has platform specific logic to handle errors from Storekit,
    /// such as a nil product list being returned, and print helpful information.
    /// </summary>
    public class AppleAppStoreBillingService : IBillingService, IAppleExtensions {

        private IBillingServiceCallback biller;
        private ProductIdRemapper remapper;
        private ILogger logger;
        public IStoreKitPlugin storekit { get; private set; }

        public event Action<string> onAppReceiptRefreshed;
        public event Action onAppReceiptRefreshFailed;

        public AppleAppStoreBillingService(ProductIdRemapper mapper, IStoreKitPlugin storekit, IUtil util, ILogger logger) {
            this.storekit = storekit;
            this.remapper = mapper;
            this.logger = logger;
            storekit.initialise(this, util);
        }

        public void initialise (IBillingServiceCallback biller) {
            this.biller = biller;
            bool available = storekit.unibillPaymentsAvailable ();
            if (available) {
                storekit.unibillRequestProductData (remapper.getAllPlatformSpecificProductIds().toJson());
            } else {
                biller.logError(UnibillError.STOREKIT_BILLING_UNAVAILABLE);
                biller.onSetupComplete(false);
            }
        }

        public void purchase (string item, string developerPayload) {
            storekit.unibillPurchaseProduct(item);
        }

        private bool restoreInProgress;
        public void restoreTransactions () {
            restoreInProgress = true;
            storekit.unibillRestoreTransactions();
        }

        public void refreshAppReceipt () {
            storekit.refreshAppReceipt ();
        }

        public void onProductListReceived (string productListString) {
            if (productListString.Length == 0) {
                biller.logError (UnibillError.STOREKIT_RETURNED_NO_PRODUCTS);
                biller.onSetupComplete (false);
                return;
            }

            var responseObject = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(productListString);
            var productHash = responseObject.getHash ("products");

            var products = Util.DeserialiseProductList (productHash);

            // Register our storekit transaction observer.
            // We must wait until we have initialised to do this.
            storekit.addTransactionObserver ();

            var appReceipt = responseObject.getString ("appReceipt");
            onAppReceiptRetrieved (appReceipt);

            biller.onSetupComplete(products);
        }
        
        public void onPurchaseSucceeded(string data, string receipt) {
            Dictionary<string, object> response = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(data);

            var productId = (string)response ["productId"];
            var transactionId = response.getString ("transactionIdentifier");
            // If we're restoring, make sure it isn't a consumable.
            if (restoreInProgress) {
                if (remapper.canMapProductSpecificId (productId)) {
                    if (remapper.getPurchasableItemFromPlatformSpecificId (productId).PurchaseType == PurchaseType.Consumable) {
                        // Silently ignore this consumable.
                        logger.Log ("Ignoring restore of consumable: " + productId);
                        // We must close the transaction or storekit will keep sending it to us.
                        storekit.finishTransaction (transactionId);
                        return;
                    }
                }
            }

            biller.onPurchaseSucceeded(productId, receipt, transactionId);
        }
        
        public void onPurchaseFailed(string jsonHash) {
            biller.onPurchaseFailedEvent(new PurchaseFailureDescription(jsonHash));
        }

        public void onPurchaseDeferred(string productId) {
            biller.onPurchaseDeferredEvent(productId);
        }
        
        public void onTransactionsRestoredSuccess() {
            restoreInProgress = false;
            biller.onTransactionsRestoredSuccess();
        }
        
        public void onTransactionsRestoredFail(string error) {
            restoreInProgress = false;
            biller.onTransactionsRestoredFail(error);
        }

        public void onFailedToRetrieveProductList() {
            biller.logError(UnibillError.STOREKIT_FAILED_TO_RETRIEVE_PRODUCT_DATA);
            biller.onSetupComplete(true); // We should still be able to buy things, assuming they are correctly setup.
        }

        public void finishTransaction (PurchasableItem item, string transactionId)
        {
            logger.Log ("Finishing transaction " + transactionId);
            storekit.finishTransaction (transactionId);
        }

        public void onAppReceiptRetrieved(string receipt) {
            if (receipt != null) {
                biller.setAppReceipt (receipt);
                if (null != onAppReceiptRefreshed) {
                    onAppReceiptRefreshed (receipt);
                }
            }
        }

        public void onAppReceiptRefreshedFailed() {
            if (null != onAppReceiptRefreshFailed) {
                onAppReceiptRefreshFailed ();
            }
        }
    }
}
