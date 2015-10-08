//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using Unibill.Impl;

namespace Unibill.Impl {
    public interface IRawGooglePlayInterface {
        void initialise(GooglePlayBillingService callback, string publicKey, string[] productIds);
        void purchase(string json);
        void finishTransaction(string transactionId);
        void restoreTransactions();
    }
}
