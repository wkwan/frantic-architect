//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using Unibill.Impl;
using Uniject;

/// <summary>
/// Records purchase history for each <see cref="PurchasableItem"/>.
/// </summary>
public class TransactionDatabase {
    private IStorage storage;
    private ILogger logger;
    private const string TRANSACTION_SET_KEY = "com.outlinegames.unibill.transactionlog";
    private List<object> recentTransactionIdentifiers;

    public string UserId { get; set; }

    public TransactionDatabase(IStorage storage, ILogger logger) {
        this.storage = storage;
        this.logger = logger;
        this.UserId = "default";
        var transactionSet = storage.GetString (TRANSACTION_SET_KEY, "[]");
        recentTransactionIdentifiers = Unibill.Impl.MiniJsonExtensions.arrayListFromJson (transactionSet);
    }

    public int getPurchaseHistory (PurchasableItem item) {
        return storage.GetInt(getKey(item.Id), 0);
    }

    public bool recordPurchase (PurchasableItem item, string transactionId) {
        int previousCount = getPurchaseHistory (item);
        if (item.PurchaseType == PurchaseType.NonConsumable && previousCount != 0) {
            logger.LogWarning("Apparently multi purchased a non consumable:{0}", item.Id);
            return false;
        }

        // Consumables have additional de duplication logic.
        if (item.PurchaseType == PurchaseType.Consumable && transactionId != null) {
            // If we've seen this before, we shouldn't record it again.
            if (recentTransactionIdentifiers.Contains (transactionId)) {
                logger.Log ("Transaction {0} already recorded.", transactionId);
                return false;
            }
                
            if (recentTransactionIdentifiers.Count > 20) {
                recentTransactionIdentifiers.RemoveAt (0);
            }
            recentTransactionIdentifiers.Add (transactionId);
            storage.SetString (TRANSACTION_SET_KEY, Unibill.Impl.MiniJSON.jsonEncode (recentTransactionIdentifiers));
        }

        storage.SetInt(getKey(item.Id), previousCount + 1);
        return true;
    }

    public void clearPurchases(PurchasableItem item) {
        storage.SetInt (getKey (item.Id), 0);
    }

    public void onRefunded(PurchasableItem item) {
        int previousCount = getPurchaseHistory(item);
        previousCount = Math.Max(0, previousCount - 1);
        storage.SetInt(getKey(item.Id), previousCount);
    }

    private string getKey(string fragment) {
        return string.Format("{0}.{1}", UserId, fragment);
    }
}
