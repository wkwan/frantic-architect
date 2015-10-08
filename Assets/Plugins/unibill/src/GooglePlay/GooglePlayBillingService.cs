//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Uniject;
using Unibill;
using Unibill.Impl;
using UnityEngine;


namespace Unibill.Impl {
    public class GooglePlayBillingService : IBillingService {

        private string publicKey;
        private IRawGooglePlayInterface rawInterface;
        private IBillingServiceCallback callback;
        private ProductIdRemapper remapper;
        private UnibillConfiguration db;
        private ILogger logger;
        #if UNITY_ANDROID
        private RSACryptoServiceProvider cryptoProvider;
        #endif

        public GooglePlayBillingService (IRawGooglePlayInterface rawInterface,
                                         UnibillConfiguration config,
                                         ProductIdRemapper remapper,
                                         ILogger logger) {
            this.rawInterface = rawInterface;
            this.publicKey = config.GooglePlayPublicKey;
            this.remapper = remapper;
            this.db = config;
            this.logger = logger;
            #if UNITY_ANDROID
            this.cryptoProvider = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(publicKey);
            #endif
        }

        public void initialise (IBillingServiceCallback callback) {
            this.callback = callback;
            if (null == publicKey || publicKey.Equals ("[Your key]")) {
                callback.logError (UnibillError.GOOGLEPLAY_PUBLICKEY_NOTCONFIGURED, publicKey);
                callback.onSetupComplete (false);
                return;
            }

            var encoder = new Dictionary<string, object>();
            encoder.Add ("publicKey", this.publicKey);
            var productIds = new List<string>();
            List<object> products = new List<object>();
            foreach (var item in db.AllPurchasableItems) {
                Dictionary<string, object> product = new Dictionary<string, object>();
                var id = remapper.mapItemIdToPlatformSpecificId(item);
                productIds.Add(id);
                product.Add ("productId", id);
                product.Add ("consumable", item.PurchaseType == PurchaseType.Consumable);
                products.Add (product);
            }
            encoder.Add("products", products);

            var json = encoder.toJson();
            rawInterface.initialise(this, json, productIds.ToArray());
        }

        public void restoreTransactions () {
            rawInterface.restoreTransactions();
        }

        public void purchase (string item, string developerPayload) {
            var args = new Dictionary<string, object>();
            args ["productId"] = item;
            args ["developerPayload"] = developerPayload;

            rawInterface.purchase(MiniJSON.jsonEncode(args));
        }


        // Callbacks
        public void onBillingNotSupported() {
            callback.logError(UnibillError.GOOGLEPLAY_BILLING_UNAVAILABLE);
            callback.onSetupComplete(false);
        }

        public void onPurchaseSucceeded(string json) {
            Dictionary<string, object> response = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(json);
            var signature = (string) response ["signature"];
            var productId = (string) response ["productId"];
            var transactionId = (string)response ["transactionId"];
            if (!verifyReceipt (signature)) {
                logger.Log ("Signature is invalid!");
                callback.onPurchaseFailedEvent(new PurchaseFailureDescription(productId, PurchaseFailureReason.SIGNATURE_INVALID, "mono"));
                return;
            }
            callback.onPurchaseSucceeded (productId, signature, transactionId);
        }

        public void onPurchaseRefunded(string item) {
            callback.onPurchaseRefundedEvent(item);
        }

        public void onPurchaseFailed(string json) {
            callback.onPurchaseFailedEvent(new PurchaseFailureDescription(json));
        }

        public void onTransactionsRestored (string success) {
            if (bool.Parse (success)) {
                callback.onTransactionsRestoredSuccess ();
            } else {
                callback.onTransactionsRestoredFail("");
            }
        }

        public void onInvalidPublicKey(string key) {
            callback.logError(UnibillError.GOOGLEPLAY_PUBLICKEY_INVALID, key);
            callback.onSetupComplete(false);
        }

        public void onProductListReceived (string json) {
			logger.Log("Received product list, completing setup...");
            callback.onSetupComplete(Util.DeserialiseProductList(json));
        }

        public void finishTransaction (PurchasableItem item, string transactionId) {
            // Only Consumable purchases should be 'finished' by making the consume call in Google Play.
            if (item.PurchaseType == PurchaseType.Consumable) {
                rawInterface.finishTransaction (transactionId);
            }
        }

        private bool verifyReceipt(string receipt) {
            #if UNITY_ANDROID
            try {
                var fields = (Dictionary<string, object>) MiniJSON.jsonDecode (receipt);
                if (null == fields) {
                    return false;
                }

                var base64Signature = fields.getString ("signature");
                var json = fields.getString ("json");

                if (null == base64Signature || null == json) {
                    return false;
                }

                byte[] signature = Convert.FromBase64String(base64Signature);
                SHA1Managed sha = new SHA1Managed();
                byte[] data = Encoding.UTF8.GetBytes(json);

                return cryptoProvider.VerifyData(data, sha, signature);
            } catch (Exception e) {
                logger.Log ("Validation exception");
                logger.Log (e.Message);
                logger.Log (e.StackTrace.ToString ());
                return false;
            }
            #else
            return true;
            #endif
        }
    }
}
	