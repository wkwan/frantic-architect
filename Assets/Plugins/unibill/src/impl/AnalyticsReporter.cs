using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uniject;
using Unibill.Impl;

namespace Unibill.Impl
{
	public class AnalyticsReporter
	{
        private BillingPlatform platform;
        private IUnityAnalytics analytics;

        public AnalyticsReporter(BillingPlatform platform, IUnityAnalytics analytics) {
            this.platform = platform;
            this.analytics = analytics;
		}

        public void onPurchaseSucceeded(PurchasableItem item) {
            string receipt, signature;
            extractReceiptAndSignature (item, out receipt, out signature);
            analytics.Transaction(item.LocalId,
                item.priceInLocalCurrency,
                item.isoCurrencySymbol,
                receipt,
                signature);
		}

        private void extractReceiptAndSignature(PurchasableItem item, out string receipt, out string signature) {
            receipt = null;
            signature = null;

            switch (platform) {
            case BillingPlatform.AppleAppStore:
                receipt = item.receipt;
                signature = null;
                break;
            case BillingPlatform.GooglePlay:
                var dic = item.receipt.hashtableFromJson ();
                if (null != dic) {
                    if (dic.ContainsKey ("json")) {
                        receipt = (string)dic ["json"];
                    }

                    if (dic.ContainsKey ("signature")) {
                        signature = (string)dic ["signature"];
                    }
                }
                break;
            }
        }
	}
}
