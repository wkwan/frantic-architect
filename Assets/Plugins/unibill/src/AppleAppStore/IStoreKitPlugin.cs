//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using Uniject;

namespace Unibill.Impl {

    /// <summary>
    /// Raw interface for the Unibill native iOS plugin.
    /// </summary>
    public interface IStoreKitPlugin {

        void initialise(AppleAppStoreBillingService callback, IUtil util);

        bool unibillPaymentsAvailable();
        void unibillRequestProductData (string payload);
        void unibillPurchaseProduct (string productId);
        void finishTransaction (string transactionIdentifier);
        void unibillRestoreTransactions();
        void addTransactionObserver();
        void refreshAppReceipt();
    }
}
