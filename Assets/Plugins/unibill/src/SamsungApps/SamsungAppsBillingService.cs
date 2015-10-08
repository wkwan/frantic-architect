using System;
using System.Collections;
using System.Collections.Generic;
using Unibill;
using Uniject;

namespace Unibill.Impl
{
    public class SamsungAppsBillingService : IBillingService
	{
		private IBillingServiceCallback callback;
		private UnibillConfiguration config;
		private IRawSamsungAppsBillingService rawSamsung;

		private HashSet<string> unknownSamsungProducts = new HashSet<string>();

		public SamsungAppsBillingService (UnibillConfiguration config, IRawSamsungAppsBillingService rawSamsung) {
			this.config = config;
			this.rawSamsung = rawSamsung;
		}

		public void initialise (IBillingServiceCallback biller)
		{
			this.callback = biller;
			rawSamsung.initialise (this);

			var encoder = new Dictionary<string, object>();
			encoder.Add ("mode", config.SamsungAppsMode.ToString());
			encoder.Add ("itemGroupId", config.SamsungItemGroupId);

			rawSamsung.getProductList (encoder.toJson());
		}

        public void purchase (string item, string developerPayload)
		{
			if (unknownSamsungProducts.Contains (item)) {
				callback.logError(UnibillError.SAMSUNG_APPS_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_SAMSUNG, item);
                callback.onPurchaseFailedEvent(new PurchaseFailureDescription(item, PurchaseFailureReason.ITEM_UNAVAILABLE, null));
				return;
			}

			rawSamsung.initiatePurchaseRequest (item);
		}

		public void restoreTransactions ()
		{
			rawSamsung.restoreTransactions ();
		}

		public void onProductListReceived(string productListString) {
            callback.onSetupComplete (Util.DeserialiseProductList (productListString));
		}

		public void onPurchaseFailed(string item) {
            callback.onPurchaseFailedEvent (new PurchaseFailureDescription(item, PurchaseFailureReason.UNKNOWN, null));
		}

        public void onPurchaseCancelled(string item) {
            callback.onPurchaseFailedEvent (new PurchaseFailureDescription (item, PurchaseFailureReason.USER_CANCELLED, null));
        }

		public void onPurchaseSucceeded(string json) {
			Dictionary<string, object> response = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(json);

			callback.onPurchaseSucceeded ((string)response["productId"], (string) response ["signature"], null);
		}

		public void onTransactionsRestored (string success) {
			if (bool.Parse (success)) {
				callback.onTransactionsRestoredSuccess ();
			} else {
				callback.onTransactionsRestoredFail("");
			}
		}

		public void onInitFail() {
			callback.onSetupComplete (false);
		}

        public void finishTransaction (PurchasableItem item, string transactionId) {
            // Samsung has no concept of consumption or fulfilment,
            // so nothing to do here.
        }
	}
}
