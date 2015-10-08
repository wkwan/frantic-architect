//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEngine;
using Unibill.Impl;

/// <summary>
/// Bridge from Java to Unity for the Google Play billing platform.
/// </summary>
[AddComponentMenu("")] // Hide it from the component menu
public class GooglePlayCallbackMonoBehaviour : MonoBehaviour {
    public void Awake() {
        gameObject.name = this.GetType().ToString();
        DontDestroyOnLoad(this);
    }

    private GooglePlayBillingService callback;
    public void Initialise(GooglePlayBillingService callback) {
        this.callback = callback;
    }

    public void onProductListReceived(string json) {
        callback.onProductListReceived (json);
    }

    public void onBillingNotSupported () {
        callback.onBillingNotSupported();
    }

    public void onPurchaseSucceeded(string productId) {
        callback.onPurchaseSucceeded(productId);
    }

    public void onPurchaseRefunded(string productId) {
        callback.onPurchaseRefunded(productId);
    }

    public void onPurchaseFailed(string productId) {
        callback.onPurchaseFailed(productId); 
    }

    public void onTransactionsRestored (string successString) {
        callback.onTransactionsRestored(successString);
    }

    public void onInvalidPublicKey(string publicKey) {
        callback.onInvalidPublicKey(publicKey);
    }
}
