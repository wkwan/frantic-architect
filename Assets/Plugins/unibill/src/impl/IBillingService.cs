//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;

namespace Unibill.Impl
{
    /// <summary>
    /// Represents the public interface of the underlying billing system such as Google Play,
    /// or the Apple App store.
    /// </summary>
    public interface IBillingService {

        /// <summary>
        /// Initialise the instance using the specified <see cref="IBillingServiceCallback"/>.
        /// </summary>
        void initialise(IBillingServiceCallback biller);

        /// <summary>
        /// Handle a purchase request from a user.
        /// 
        /// Developer payload is provided for platforms
        /// that define such a concept (Google Play).
        /// </summary>
        void purchase(string item, string developerPayload);

        /// <summary>
        /// Called by Unibill when a transaction has been recorded.
        /// 
        /// Billing systems should perform any housekeeping here,
        /// such as closing transactions or consuming consumables.
        /// </summary>
        void finishTransaction (PurchasableItem item, string transactionId);

        /// <summary>
        /// The user wants to get back any non consumable items the already own,
        /// typically on a reinstall.
        /// </summary>
        void restoreTransactions();
    }
}

