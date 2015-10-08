#if UNITY_IOS || UNITY_STANDALONE
//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Uniject;
using AOT;

namespace Unibill.Impl {
    public class StoreKitPluginImpl : MonoBehaviour, IStoreKitPlugin {

        private delegate void UnibillSendMessageCallback(string subject, string payload, string receipt);

        #if UNITY_STANDALONE_OSX
        [DllImport("unibillosx")]
        #else
        [DllImport("__Internal")]
        #endif
        private static extern bool _unibillPaymentsAvailable();

#if UNITY_STANDALONE_OSX
        [DllImport("unibillosx")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void _unibillInitialise (UnibillSendMessageCallback AsyncCallback);
        
#if UNITY_STANDALONE_OSX
        [DllImport("unibillosx")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void _unibillRequestProductData (string productIdentifiers);
        
#if UNITY_STANDALONE_OSX
        [DllImport("unibillosx")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void _unibillPurchaseProduct (string productId);
        
#if UNITY_STANDALONE_OSX
        [DllImport("unibillosx")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void _unibillRestoreTransactions();

#if UNITY_STANDALONE_OSX
        [DllImport("unibillosx")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void _unibillAddTransactionObserver();

#if UNITY_STANDALONE_OSX
        [DllImport("unibillosx")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void _unibillFinishTransaction(string transactionId);

#if UNITY_STANDALONE_OSX
[DllImport("unibillosx")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void _unibillRefreshAppReceipt();

        private static StoreKitPluginImpl instance;
        private AppleAppStoreBillingService service;
        private static IUtil util;

        [MonoPInvokeCallback(typeof(UnibillSendMessageCallback))]
        private static void MessageCallback(string subject, string payload, string receipt) {
            util.RunOnMainThread(() => {   
                instance.processMessage (subject, payload, receipt);
            });
        }

        public void initialise(AppleAppStoreBillingService service, IUtil util) {
            instance = this;
            this.service = service;
            StoreKitPluginImpl.util = util;
            _unibillInitialise (MessageCallback);
        }

        public bool unibillPaymentsAvailable () {
            return _unibillPaymentsAvailable();
        }
        public void unibillRequestProductData (string payload) {
            _unibillRequestProductData(payload);
        }
        public void unibillPurchaseProduct (string productId) {
            _unibillPurchaseProduct(productId);
        }
        public void unibillRestoreTransactions () {
            _unibillRestoreTransactions();
        }

        public void addTransactionObserver () {
            _unibillAddTransactionObserver();
        }

        public void finishTransaction (string transactionIdentifier) {
            _unibillFinishTransaction(transactionIdentifier);
        }

        public void refreshAppReceipt () {
            _unibillRefreshAppReceipt ();
        }

        private void processMessage(string subject, string payload, string receipt) {
            switch (subject) {
            case "onProductListReceived":
                service.onProductListReceived (payload);
                break;
            case "onProductPurchaseSuccess":
                service.onPurchaseSucceeded (payload, receipt);
                break;
            case "onProductPurchaseFailed":
                service.onPurchaseFailed (payload);
                break;
            case "onProductPurchaseDeferred":
                service.onPurchaseDeferred (payload);
                break;
            case "onTransactionsRestoredSuccess":
                service.onTransactionsRestoredSuccess ();
                break;
            case "onTransactionsRestoredFail":
                service.onTransactionsRestoredFail (payload);
                break;
            case "onFailedToRetrieveProductList":
                service.onFailedToRetrieveProductList ();
                break;
            case "onAppReceiptRefreshed":
                service.onAppReceiptRetrieved (payload);
                break;
            case "onAppReceiptRefreshFailed":
                service.onAppReceiptRefreshedFailed ();
                break;
            }
        }
    }
}
#endif
