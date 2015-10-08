#if UNITY_WINRT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using unibill.Dummy;
using Uniject;
using System.Xml.Linq;

namespace Unibill.Impl {
    /// <summary>
    /// Handles Windows Phone 8.
    /// </summary>
    public class WP8BillingService : IBillingService, IWindowsIAPCallback {

        private IWindowsIAP wp8;
        private IBillingServiceCallback callback;
        private TransactionDatabase tDb;
        private ProductIdRemapper remapper;
        private ILogger logger;
        private IUtil util;

        public WP8BillingService(IWindowsIAP wp8,
                                 ProductIdRemapper remapper,
                                 TransactionDatabase tDb,
                                 IUtil util,
                                 ILogger logger) {
            this.wp8 = wp8;
            this.tDb = tDb;
            this.remapper = remapper;
            this.util = util;
            this.logger = logger;
        }

        public void initialise(IBillingServiceCallback biller) {
            this.callback = biller;
            init(0);
        }

        private void init(int delay) {
            wp8.Initialise(this, delay);
        }

        public void log(string message) {
            logger.Log(message);
        }

        public void purchase(string item, string developerPayload) {
            wp8.Purchase(item);
        }

        public void restoreTransactions() {
            enumerateLicenses();
            callback.onTransactionsRestoredSuccess();
        }

        public void enumerateLicenses() {
            wp8.EnumerateLicenses();
        }

        public void logError (string error)
        {
            // Uncomment to get diagnostics printed on screen.
            logger.LogError (error);
        }

        public void OnProductListReceived(ProductDescription[] products) {
            util.RunOnMainThread (() => {
                // Extract transaction IDs from receipts where available.
                foreach (var product in products) {
                    if (!string.IsNullOrEmpty(product.Receipt)) {
                        product.TransactionID = readTransactionIdFromReceipt(product.Receipt);
                    }
                }
                callback.onSetupComplete(new List<ProductDescription>(products));
            });
        }

        public void RunOnUIThread(Action act) {
            util.RunOnMainThread(act);
        }

        public void OnPurchaseFailed(string productId, string error) {
            util.RunOnMainThread(() => {
                logger.LogError("Purchase failed: {0}, {1}", productId, error);
                callback.onPurchaseFailedEvent(new PurchaseFailureDescription(productId, PurchaseFailureReason.UNKNOWN, error));
            });
        }

        private static int count;
        public void OnPurchaseSucceeded(string productId, string receipt, string tranId) {
            util.RunOnMainThread(() => {
                // Our implementations don't currently set the transaction Id,
                // so we extract it from the receipt.
                tranId = readTransactionIdFromReceipt(receipt);
                if (!remapper.canMapProductSpecificId(productId)) {
                    logger.LogError("Purchased unknown product: {0}. Ignoring!", productId);
                    return;
                }
                var details = remapper.getPurchasableItemFromPlatformSpecificId(productId);
                switch (details.PurchaseType) {
                    case PurchaseType.Consumable:
                        callback.onPurchaseSucceeded(productId, receipt, tranId);
                        break;
                    case PurchaseType.NonConsumable:
                    case PurchaseType.Subscription:
                        var item = remapper.getPurchasableItemFromPlatformSpecificId(productId);
                        // We should only provision non consumables if they're not owned.
                        if (0 == tDb.getPurchaseHistory(item)) {
                            callback.onPurchaseSucceeded(productId, receipt, tranId);
                        }
                        break;
                }
            });
        }

        // When using an incorrect product id:
        // "Exception from HRESULT: 0x805A0194"
        public void OnProductListError(string message) {
            util.RunOnMainThread(() => {
                if (message.Contains("0x805A0194")) {
                    callback.logError(UnibillError.WP8_APP_ID_NOT_KNOWN);
                    callback.onSetupComplete(false);
                }
                else {
                    logError("Unable to retrieve product listings. Unibill will automatically retry...");
                    logError(message);
                    init(3000);
                }
            });
        }

        public void finishTransaction (PurchasableItem item, string transactionId)
        {
            if (item.PurchaseType == PurchaseType.Consumable) {
                this.wp8.FinaliseTransaction(item.LocalId);
            }
        }

        private string readTransactionIdFromReceipt(string receipt) {
            try {
                var doc = XDocument.Parse(receipt);
                XNamespace ns = "http://schemas.microsoft.com/windows/2012/store/receipt";

                var first = doc.Root.Element(ns + "ProductReceipt");
                var second = first.Attribute("Id");
                return second.Value;
            }
            catch (Exception e) {
                // If our receipt is missing or nonconformant...
                logError(e.ToString());

                // Attempt to fingerprint the receipt.
                // Receipts seem to have varying trailing whitespace,
                // are typically 600 characters long
                // so take up to the first 300 characters.
                return receipt.Substring(0, Math.Min(receipt.Length, 300)).GetHashCode().ToString();
            }
        }
    }
}
#endif
