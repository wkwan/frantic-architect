
//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unibill;
using Unibill.Demo;
using Unibill.Impl;

/// <summary>
/// An example of basic Unibill functionality.
/// </summary>
[AddComponentMenu("Unibill/UnibillDemo")]
public class UnibillDemo : MonoBehaviour {

    private Unibill.Demo.ComboBox box;
    private GUIContent[] comboBoxList;
    private GUIStyle listStyle;
    private int selectedItemIndex;
    private PurchasableItem[] items;
    private const string DLC_ID = "episode2";
    public Font font;

    void Start () {
        if (UnityEngine.Resources.Load ("unibillInventory.json") == null) {
            Debug.LogError("You must define your purchasable inventory within the inventory editor!");
            this.gameObject.SetActive(false);
            return;
        }

        // We must first hook up listeners to Unibill's events.
        Unibiller.onBillerReady += onBillerReady;
        Unibiller.onTransactionsRestored += onTransactionsRestored;
	    Unibiller.onPurchaseFailed += onFailed;
		Unibiller.onPurchaseCompleteEvent += onPurchased;
        Unibiller.onPurchaseDeferred += onDeferred;

        // Now we're ready to initialise Unibill.
        Unibiller.Initialise();

        initCombobox();

        // iOS includes additional functionality around App receipts.
        #if UNITY_IOS
        var appleExtensions = Unibiller.getAppleExtensions();
        appleExtensions.onAppReceiptRefreshed += x => {
            Debug.Log(x);
            Debug.Log("Refreshed app receipt!");
        };

        appleExtensions.onAppReceiptRefreshFailed += () => {
            Debug.Log("Failed to refresh app receipt.");
        };
        #endif
    }

    /// <summary>
    /// This will be called when Unibill has finished initialising.
    /// </summary>
    private void onBillerReady(UnibillState state) {
        UnityEngine.Debug.Log("onBillerReady:" + state);
        if (state != UnibillState.CRITICAL_ERROR) {
            Debug.Log ("Available items:");
            foreach (var item in Unibiller.AllPurchasableItems) {
                if (item.AvailableToPurchase) {
                    Debug.Log (string.Join (" - ",
                        new string[] {
                            item.localizedTitle,
                            item.localizedDescription,
                            item.isoCurrencySymbol,
                            item.priceInLocalCurrency.ToString (),
                            item.localizedPriceString
                        }));
                }
            }
        }
    }

    /// <summary>
    /// This will be called after a call to Unibiller.restoreTransactions().
    /// </summary>
    private void onTransactionsRestored (bool success) {
        Debug.Log("Transactions restored.");
    }

    /// <summary>
    /// This will be called when a purchase completes.
    /// </summary>
	private void onPurchased(PurchaseEvent e) {
		Debug.Log("Purchase OK: " + e.PurchasedItem.Id);
        Debug.Log ("Receipt: " + e.Receipt);
        Debug.Log(string.Format ("{0} has now been purchased {1} times.",
								 e.PurchasedItem.name,
								 Unibiller.GetPurchaseCount(e.PurchasedItem)));
    }

    /// <summary>
    /// iOS Specific.
    /// This is called as part of Apple's 'Ask to buy' functionality,
    /// when a purchase is requested by a minor and referred to a parent
    /// for approval.
    /// 
    /// When the purchase is approved or rejected, the normal purchase events
    /// will fire.
    /// </summary>
    /// <param name="item">Item.</param>
    private void onDeferred(PurchasableItem item) {
        Debug.Log ("Purchase deferred blud: " + item.Id);
    }
    
    /// <summary>
    /// This will be called is an attempted purchase fails.
    /// </summary>
    private void onFailed(PurchaseFailedEvent e) {
        Debug.Log("Purchase failed: " + e.PurchasedItem.Id);
        Debug.Log (e.Reason);
    }

    private void initCombobox() {
        box = new Unibill.Demo.ComboBox();
        items = Unibiller.AllPurchasableItems;
        comboBoxList = new GUIContent[items.Length];
        for (int t = 0; t < items.Length; t++) {
            comboBoxList[t] = new GUIContent(string.Format("{0} - {1}", items[t].localizedTitle, items[t].localizedPriceString));
        }
        
        listStyle = new GUIStyle();
        listStyle.font = font;
        listStyle.normal.textColor = Color.white; 
        listStyle.onHover.background =
            listStyle.hover.background = new Texture2D(2, 2);
        listStyle.padding.left =
            listStyle.padding.right =
                listStyle.padding.top =
                listStyle.padding.bottom = 4;
    }

    public void Update() {
        for (int t = 0; t < items.Length; t++) {
            comboBoxList[t] = new GUIContent(string.Format("{0} - {1} - {2}", items[t].name, items[t].localizedTitle, items[t].localizedPriceString));
        }
    }

    void OnGUI () {
        selectedItemIndex = box.GetSelectedItemIndex ();
        selectedItemIndex = box.List (new Rect (0, 0, Screen.width, Screen.width / 20.0f), comboBoxList [selectedItemIndex].text, comboBoxList, listStyle);
        if (GUI.Button (new Rect (0, Screen.height - Screen.width / 6.0f, Screen.width / 2.0f, Screen.width / 6.0f), "Buy")) {
            Unibiller.initiatePurchase(items[selectedItemIndex]);
        }

        if (GUI.Button (new Rect (Screen.width / 2.0f, Screen.height - Screen.width / 6.0f, Screen.width / 2.0f, Screen.width / 6.0f), "Restore transactions")) {
            Unibiller.restoreTransactions();
        }

        // Draw the purchase names for our various purchasables.
        int start = (int) (Screen.height - 2 * Screen.width / 6.0f) - 50;
        foreach (PurchasableItem item in Unibiller.AllNonConsumablePurchasableItems) {
            GUI.Label(new Rect(0, start, 500, 50), item.Id, listStyle);
			GUI.Label(new Rect(Screen.width - Screen.width * 0.1f, start, 500, 50), Unibiller.GetPurchaseCount(item).ToString(), listStyle);
            start -= 30;
        }
		
		foreach (string currencyId in Unibiller.AllCurrencies) {
            GUI.Label(new Rect(0, start, 500, 50), currencyId, listStyle);
			GUI.Label(new Rect(Screen.width - Screen.width * 0.1f, start, 500, 50), Unibiller.GetCurrencyBalance(currencyId).ToString(), listStyle);
            start -= 30;
        }

		foreach (var subscription in Unibiller.AllSubscriptions) {
			GUI.Label(new Rect(0, start, 500, 50), subscription.localizedTitle, listStyle);
			GUI.Label(new Rect(Screen.width - Screen.width * 0.1f, start, 500, 50), Unibiller.GetPurchaseCount(subscription).ToString(), listStyle);
			start -= 30;
		}

        GUI.Label(new Rect(0, start - 10, 500, 50), "Item", listStyle);


        GUI.Label(new Rect(Screen.width - Screen.width * 0.2f, start - 10, 500, 50), "Count", listStyle);
    }
}
