//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using Unibill;
using Unibill.Impl;

namespace Tests {
    public class FakeBillingService : IBillingService, IAppleExtensions {

        private IBillingServiceCallback biller;
        private List<string> purchasedItems = new List<string>();
        private ProductIdRemapper remapper;

        public FakeBillingService (ProductIdRemapper remapper) {
            this.remapper = remapper;
        }

        public void initialise (IBillingServiceCallback biller) {
            this.biller = biller;
            
            var products = new List<ProductDescription>();
            foreach (var id in remapper.getAllPlatformSpecificProductIds()) {
                products.Add(new ProductDescription(id, "$123.45", "Fake title", "Fake description", "USD", 123.45m));
            }
            
            biller.onSetupComplete(products);
        }

        public bool purchaseCalled;
        public void purchase (string item, string developerPayload) {
            purchaseCalled = true;
            // Our billing systems should only keep track of non consumables.
            if (remapper.getPurchasableItemFromPlatformSpecificId (item).PurchaseType == PurchaseType.NonConsumable) {
                purchasedItems.Add (item);
            }
            biller.onPurchaseReceiptRetrieved (item, "fake receipt");
            this.biller.onPurchaseSucceeded(item, "{ \"this\" : \"is a fake receipt\" }", Guid.NewGuid().ToString());
        }

        public bool restoreCalled;
        public void restoreTransactions () {
            restoreCalled = true;
            foreach (var item in purchasedItems) {
                biller.onPurchaseSucceeded(item, "{ \"this\" : \"is a fake receipt\" }", "1");
            }
            this.biller.onTransactionsRestoredSuccess();
        }

        public void finishTransaction (PurchasableItem item, string transactionId) {
        }

        public void registerPurchaseForRestore(string productId) {
            purchasedItems.Add (productId);
        }

        public event Action<string> onAppReceiptRefreshed;

        // Suppress the unused event warning.
        #pragma warning disable 67
        public event Action onAppReceiptRefreshFailed;
        #pragma warning restore 67

        public void refreshAppReceipt ()
        {
            if (null != onAppReceiptRefreshed) {
                onAppReceiptRefreshed("fake!");
            }
        }
    }
}
