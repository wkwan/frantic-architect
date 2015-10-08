//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;

namespace Unibill.Impl {
    public interface IRawAmazonAppStoreBillingInterface {
        void initialise(AmazonAppStoreBillingService amazon);
        void initiateItemDataRequest(string[] productIds);
        void initiatePurchaseRequest(string productId);
        void finishTransaction(string transactionId);
        void restoreTransactions();
    }
}

