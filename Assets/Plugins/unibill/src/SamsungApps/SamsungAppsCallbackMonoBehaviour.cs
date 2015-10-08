//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEngine;
using Unibill.Impl;

[AddComponentMenu("")]
public class SamsungAppsCallbackMonoBehaviour : MonoBehaviour {

	public void Start() {
		gameObject.name = this.GetType().ToString();
		GameObject.DontDestroyOnLoad(this);
	}

	private SamsungAppsBillingService samsung;
	public void initialise(SamsungAppsBillingService samsung) {
		this.samsung = samsung;
	}

	public void onProductListReceived(string productCSVString) {
		samsung.onProductListReceived(productCSVString);
	}

	public void onPurchaseFailed(string item) {
		samsung.onPurchaseFailed(item);
	}

	public void onPurchaseSucceeded (string item) {
		samsung.onPurchaseSucceeded(item);
	}

    public void onPurchaseCancelled(string item) {
        samsung.onPurchaseCancelled (item);
    }

	public void onTransactionsRestored (string success) {
		samsung.onTransactionsRestored(success);
	}

	public void onInitFail() {
		samsung.onInitFail ();
	}
}
