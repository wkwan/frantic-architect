//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace Unibill.Impl {

    /// <summary>
    /// Callback interface for <see cref="IBillingService"/>s.
    /// </summary>
    public interface IBillingServiceCallback {
        /// <summary>
        /// We're ready to go (or not, in which case purchases are disabled).
        /// </summary>
        void onSetupComplete(bool successful);

        /// <summary>
        /// Complete setup by providing a list of available products,
        /// complete with metadata and any associated purchase receipts
        /// and transaction IDs.
        /// 
        /// Any previously unseen purchases will be completed by the Biller.
        /// </summary>
        void onSetupComplete (List<ProductDescription> products);

        void onPurchaseSucceeded(string platformSpecificId, string receipt, string transactionIdentifier);

        /// <summary>
        /// Certain billing systems (storekit) will generate refund transactions.
        /// </summary>
        void onPurchaseRefundedEvent(string item);

        void onPurchaseFailedEvent(PurchaseFailureDescription desc);

        /// <summary>
        /// Only relevant to iOS.
        /// </summary>
        void onPurchaseDeferredEvent(string item);

        /// <summary>
        /// The process of restoring transactions finished succesfully.
        /// </summary>
        void onTransactionsRestoredSuccess();

        void onTransactionsRestoredFail(string error);

        /// <summary>
        /// Identify a product receipt with a product, typically performed on
        /// purchase and on startup.
        /// </summary>
        void onPurchaseReceiptRetrieved (string productId, string receipt);
        void setAppReceipt (string receipt);

        void logError(UnibillError error, params object[] args);
        void logError(UnibillError error);
    }
}
