//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using Unibill;
using Unibill.Impl;

public class AmazonJSONGenerator {

    private UnibillConfiguration config;
    public AmazonJSONGenerator (UnibillConfiguration config) {
        this.config = config;
    }

    public void encodeAll () {
        var result = new Dictionary<string, object>();
        foreach (PurchasableItem item in config.AllPurchasableItems) {
            var localId = item.LocalIds [BillingPlatform.AmazonAppstore];
            result[localId] = purchasableDetailsToDictionary (item);
        }

        var json = MiniJSON.jsonEncode(result);
		using (StreamWriter o = new StreamWriter("Assets/Plugins/unibill/resources/amazon.sdktester.json.txt")) {
			o.Write(json);
		}
    }

    public Dictionary<string, object> purchasableDetailsToDictionary (PurchasableItem item) {
        var dic = new Dictionary<string, object>();
        dic ["itemType"] = item.PurchaseType == PurchaseType.Consumable ? "CONSUMABLE" : item.PurchaseType == PurchaseType.NonConsumable ? "ENTITLED" : "SUBSCRIPTION";
        dic ["title"] = item.name == null ? string.Empty : item.name;
        dic ["description"] = item.description == null ? string.Empty : item.description;
        dic["price"] = 0.99;
        dic ["smallIconUrl"] = "http://example.com";
        if (PurchaseType.Subscription == item.PurchaseType) {
            dic["subscriptionParent"] = "does.not.exist";
        }

        return dic;
    }

}
