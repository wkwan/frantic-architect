//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Uniject;
using Unibill;
using Unibill.Impl;

/// <summary>
/// Consumable PurchaseTypes may be purchased more than once.
/// They are therefore suitable for implementing virtual currencies.
/// 
/// The number of times a Consumable PurchasableItem has been
/// purchased is tracked by Unibill using local storage,
/// but not tracked by Google, Apple or Amazon.
/// 
/// Thus, Consumable PurchaseTypes cannot be transferred automatically
/// between devices by Unibill using restoreTransactions.
/// 
/// 
/// NonConsumable PurchaseTypes cannot be purchased more than once.
/// They are suitable for one off purchases, such as unlocking a specific
/// item or level.
/// 
/// Purchased NonConsumables are tracked by Unibill using local storage.
/// They are also linked to User's accounts by Apple, Google and Amazon,
/// and may be restored using Unibiller.restorePurchases().
/// 
/// 
/// Subscription PurchasableItems are as the name suggests; they may be purchased
/// for a finite quantity of time.
/// </summary>
public enum PurchaseType {
    Consumable,
    NonConsumable,
    Subscription,
}

/// <summary>
/// Represents an item that may be purchased as an In App Purchase.
/// </summary>
public partial class PurchasableItem : IEquatable<PurchasableItem> {

    /// <summary>
    /// Determine if this item is available to purchase according to 
    /// the billing subsystem.
    /// 
    /// This will be false if the product's identifier is unknown,
    /// incorrect or otherwise disabled with the billing provider
    /// (ie Apple, Google et al).
    /// 
    /// If this is false, purchase attempts will immediately fail.
    /// </summary>
    public bool AvailableToPurchase { get; internal set; }

    /// <summary>
    /// A platform independent unique identifier for this PurchasableItem.
    /// 
    /// Ids should be structured similarly to bundle identifiers,
    /// eg com.companyname.productname.
    /// </summary>
    public string Id { get; internal set; }

    ///
    /// The type of this PurchasableItem; Consumable, Non-Consumable or Subscription.
    ///
    public PurchaseType PurchaseType { get; internal set; }

    /// <summary>
    /// Name of the item as displayed to users.
    /// </summary>
    public string name { get; internal set; }
    
    /// <summary>
    /// Description of the item as displayed to users.
    /// </summary>
    public string description { get; internal set; }

    /// <summary>
    /// !!!!DEPRECATED!!!!
    /// This property is not supported on Google Play and will be NULL on that platform.
    /// Use localizedPriceString instead.
    /// !!!!DEPRECATED!!!!
    /// Gets the localized price in local currency units, as retrieved from the billing subsystem;
    /// Apple, Google etc.
    /// </summary>
    public decimal localizedPrice { get; internal set; }

    /// <summary>
    /// Gets the localized price.
	/// This is the price formatted with currency symbol.
    /// </summary>
    /// <value>The localized price string.</value>
    public string localizedPriceString { get; internal set; }

    /// <summary>
    /// Gets the localized title, as retrieved from the billing subsystem;
    /// Apple, Google etc.
    /// </summary>
    public string localizedTitle { get; internal set; }

    /// <summary>
    /// Gets the localized description, as retrieved from the billing subsystem;
    /// Apple, Google etc.
    /// </summary>
    public string localizedDescription { get; internal set; }
	
	/// <summary>
	/// The item's currency in ISO 4217 format eg GBP, USD etc.
	/// </summary>
    public string isoCurrencySymbol { get; internal set; }
	
	/// <summary>
	/// The item's price, denominated in the currency
	/// indicated by <c>isoCurrencySymbol</c>.
	/// </summary>
    public decimal priceInLocalCurrency { get; internal set; }

    /// <summary>
    /// The platform specific identifier of the item, which is either the Id or 
    /// the overridden value if configured in the inventory editor.
    /// </summary>
    public string LocalId {
        get {
    		if (string.IsNullOrEmpty (LocalIds [platform])) {
    			return Id;
    		}

            return LocalIds[platform];
        }
    }

    /// <summary>
    /// The purchase receipt for this item, if owned.
    /// For consumable purchases, this will be the most recent purchase receipt.
    /// Consumable receipts are not saved between app restarts.
    /// Receipts in in JSON format.
    /// </summary>
    public string receipt { get; internal set; }

    /// <summary>
    /// The platform specific identifiers per billing platform, where specified.
    /// </summary>
    public Dictionary<BillingPlatform, string> LocalIds { get; private set; }

    public Dictionary<BillingPlatform, Dictionary<string, object>> platformBundles;

    private BillingPlatform platform;

    public PurchasableItem() {
        this.Id = new Random().Next().ToString();
        this.description = "Describe me!";
        this.name = "Name me!";
        this.PurchaseType = global::PurchaseType.NonConsumable;
        platformBundles = new Dictionary<BillingPlatform, Dictionary<string, object>>();
        LocalIds = new Dictionary<BillingPlatform, string>();
        foreach (BillingPlatform billingPlatform in Enum.GetValues(typeof(BillingPlatform))) {
            platformBundles[billingPlatform] = new Dictionary<string, object>();
            LocalIds[billingPlatform] = string.Empty;
        }
    }

    public PurchasableItem(string id, Dictionary<string, object> hash, BillingPlatform platform) {
        this.Id = id;
        this.platform = platform;
        Deserialize(hash);
    }

    private void Deserialize(Dictionary<string, object> hash) {
        this.PurchaseType = hash.getEnum<PurchaseType>("@purchaseType");
        this.name = hash.get<string>("name");
        this.description = hash.get<string>("description");
        // These localized details will be overwritten when loaded from the app store.
        // They are set here for testing purposes.
        this.localizedTitle = name;
        this.localizedDescription = description;
        this.priceInLocalCurrency = 1;
        this.isoCurrencySymbol = "USD";
        LocalIds = new Dictionary<BillingPlatform, string>();
        platformBundles = new Dictionary<BillingPlatform, Dictionary<string, object>>();
        Dictionary<string, object> platforms;
        if (hash.ContainsKey ("platforms")) {
            platforms = (Dictionary<string, object>)hash ["platforms"];
        } else {
            platforms = new Dictionary<string, object>();
        }
    
        foreach (BillingPlatform billingPlatform in Enum.GetValues(typeof(BillingPlatform))) {
            if (platforms.ContainsKey (billingPlatform.ToString ())) {
                Dictionary<string, object> platformData = (Dictionary<string, object>)platforms [billingPlatform.ToString ()];
                string key = string.Format ("{0}.Id", billingPlatform);
                if (platformData != null && platformData.ContainsKey (key)) {
                    LocalIds.Add (billingPlatform, (string)platformData [key]);
                }

                if (platformData != null) {
                    platformBundles [billingPlatform] = platformData;
                }
            }

            if (!LocalIds.ContainsKey (billingPlatform)) {
                LocalIds [billingPlatform] = Id;
            }
            if (!platformBundles.ContainsKey (billingPlatform)) {
                platformBundles [billingPlatform] = new Dictionary<string, object> ();
            }
        }
    }

    public Dictionary<string, object> Serialize() {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result.Add("@id", Id);
        result.Add("@purchaseType", PurchaseType.ToString());
        result.Add("name", name);
        result.Add("description", description);
        // MiniJson doesn't handle enum dictionary keys.
        var stringPlatforms = new Dictionary<string, object>();
        foreach (var item in platformBundles) {
            stringPlatforms.Add(item.Key.ToString(), item.Value);
        }
        result.Add("platforms", stringPlatforms);
        return result;
    }

    public bool Equals (PurchasableItem other) {
        return other.Id == this.Id;
    }
}

namespace Unibill.Impl {
    public class WritablePurchasable {
        public PurchasableItem item { get; private set; }
        public WritablePurchasable(PurchasableItem item) {
            this.item = item;
        }

        public string Id {
            get { return item.Id; }
            set { item.Id = value; }
        }

        public PurchaseType PurchaseType {
            get { return item.PurchaseType; }
            set { item.PurchaseType = value; }
        }

        public string description {
            get { return item.description; }
            set { item.description = value; }
        }

        public string name {
            get { return item.name; }
            set { item.name = value; }
        }
    }
}

public class VirtualCurrency {
    public string currencyId { get; set; }

    public Dictionary<string, decimal> mappings { get; private set; }
    public VirtualCurrency(string id, Dictionary<string, decimal> mappings) {
        this.currencyId = id;
        this.mappings = mappings;
    }

    public Dictionary<string, object> Serialize() {
        var result = new Dictionary<string, object>();
        result.Add("currencyId", currencyId);
        var mapList = new List<Dictionary<string, object>>();
        foreach (var kvp in mappings) {
            var dic = new Dictionary<string, object>();
            dic.Add("id", kvp.Key);
            dic.Add("amount", kvp.Value);
            mapList.Add(dic);
        }
        result.Add("mappings", mapList);
        return result;
    }
}
