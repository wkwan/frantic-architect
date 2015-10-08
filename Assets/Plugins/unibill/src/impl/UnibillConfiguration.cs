//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Uniject;
using UnityEngine;

namespace Unibill.Impl {

    /// <summary>
    /// The underlying platform be it from Google, Amazon, Apple etc.
    /// </summary>
    public enum BillingPlatform {
        GooglePlay,
		AmazonAppstore,
		SamsungApps,
        AppleAppStore,
		MacAppStore,
        WindowsPhone8,
        Windows8_1,
        UnityEditor
    }

	public enum SamsungAppsMode {
		PRODUCTION,
		ALWAYS_SUCCEED,
		ALWAYS_FAIL
	}

    public class UnibillConfiguration {

        private ILogger logger;

        public BillingPlatform CurrentPlatform { get; set; }

        public string iOSSKU { get; set; }
        public string macAppStoreSKU { get; set; }

        public BillingPlatform AndroidBillingPlatform { get; set; }

        public string GooglePlayPublicKey { get; set; }
        public bool AmazonSandboxEnabled { get; set; }
        public bool WP8SandboxEnabled { get; set; }
        public bool UseHostedConfig { get; set; }
        public string HostedConfigUrl { get; set; }

        public bool UseWin8_1Sandbox { get; set; }
		public SamsungAppsMode SamsungAppsMode { get; set; }
		public string SamsungItemGroupId { get; set; }

        public List<PurchasableItem> inventory = new List<PurchasableItem>();
        public List<VirtualCurrency> currencies = new List<VirtualCurrency>();

        public UnibillConfiguration (string json, RuntimePlatform runtimePlatform, ILogger logger, List<ProductDefinition> runtimeProducts = null) {
            this.logger = logger;
            var root = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(json);
            this.iOSSKU = root.getString("iOSSKU");
            this.macAppStoreSKU = root.getString ("macAppStoreSKU");
            this.AndroidBillingPlatform = root.getEnum<BillingPlatform>("androidBillingPlatform");
            this.GooglePlayPublicKey = root.get<string>("GooglePlayPublicKey");
            this.AmazonSandboxEnabled = root.getBool("useAmazonSandbox");
            this.WP8SandboxEnabled = root.getBool("UseWP8MockingFramework");
            this.UseHostedConfig = root.getBool("useHostedConfig");
            this.HostedConfigUrl = root.get<string>("hostedConfigUrl");
            this.UseWin8_1Sandbox = root.getBool("UseWin8_1Sandbox");
			this.SamsungAppsMode = root.getEnum<SamsungAppsMode> ("samsungAppsMode");
			this.SamsungItemGroupId = root.getString ("samsungAppsItemGroupId");

            switch (runtimePlatform) {
                case RuntimePlatform.Android:
			        CurrentPlatform = AndroidBillingPlatform;
                break;
                case RuntimePlatform.IPhonePlayer:
                    CurrentPlatform = BillingPlatform.AppleAppStore;
                break;
                case RuntimePlatform.OSXPlayer:
                    CurrentPlatform = BillingPlatform.MacAppStore;
                break;
				#if UNITY_3_5_0
				#else
                case RuntimePlatform.WP8Player:
                    CurrentPlatform = BillingPlatform.WindowsPhone8;
                break;
                    #if UNITY_5
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                    #else
                case RuntimePlatform.MetroPlayerARM:
                case RuntimePlatform.MetroPlayerX64:
                case RuntimePlatform.MetroPlayerX86:
                #endif
                CurrentPlatform = BillingPlatform.Windows8_1;
                break;
				#endif
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                    CurrentPlatform = BillingPlatform.UnityEditor;
                break;
                default:
                    CurrentPlatform = BillingPlatform.UnityEditor;
                break;
            }

            var items = (Dictionary<string, object>)root["purchasableItems"];
            foreach (var kvp in items) {
                var item = new PurchasableItem(kvp.Key, (Dictionary<string, object>)kvp.Value, CurrentPlatform);
                inventory.Add(item);
            }

            if (null != runtimeProducts) {
                foreach (var def in runtimeProducts) {
                    var hash = new Dictionary<string, object> ();
                    hash.Add ("@purchaseType", def.Type.ToString());
                    var item = new PurchasableItem (def.PlatformSpecificId, hash, CurrentPlatform);
                    if (!inventory.Exists (x => x.Id == item.Id)) {
                        inventory.Add (item);
                    }
                }
            }

            loadCurrencies(root);
        }

        private void loadCurrencies(Dictionary<string, object> root) {
            currencies = new List<VirtualCurrency>();
            var currencyTable = root.getHash("currencies");
			if (null != currencyTable) {
				foreach (var currency in currencyTable) {
					var mappings = new Dictionary<string, decimal> ();
					foreach (var kvp in (Dictionary<string, object>) currency.Value) {
						mappings.Add (kvp.Key, decimal.Parse (kvp.Value.ToString ()));
					}
					currencies.Add (new VirtualCurrency (currency.Key, mappings));
				}
			}
        }

        public PurchasableItem AddItem() {
            var item = new PurchasableItem();
            inventory.Add(item);
            return item;
        }

        public Dictionary<string, object> Serialize() {
            var result = new Dictionary<string, object>();
            result.Add("iOSSKU", iOSSKU);
            result.Add ("macAppStoreSKU", macAppStoreSKU);
            result.Add("androidBillingPlatform", AndroidBillingPlatform.ToString());
            result.Add("GooglePlayPublicKey", GooglePlayPublicKey);
            result.Add("useAmazonSandbox", AmazonSandboxEnabled);
            result.Add("UseWP8MockingFramework", WP8SandboxEnabled);
            result.Add("useHostedConfig", UseHostedConfig);
            result.Add("hostedConfigUrl", HostedConfigUrl);
            result.Add("UseWin8_1Sandbox", UseWin8_1Sandbox);
			result.Add ("samsungAppsMode", SamsungAppsMode.ToString ());
			result.Add ("samsungAppsItemGroupId", SamsungItemGroupId);

            {
                var items = new Dictionary<string, object>();
                foreach (var item in inventory) {
                    items.Add(item.Id, item.Serialize());
                }
                result.Add("purchasableItems", items);
            }
            {
                var hash = new Dictionary<string, object>();
                foreach (var currency in currencies) {
                    // MiniJson conversion.
                    var convertedMappings = new Dictionary<string, object>();
                    foreach (var item in currency.mappings) {
                        convertedMappings.Add(item.Key, item.Value);
                    }
                    hash.Add(currency.currencyId, convertedMappings);
                }
                result.Add("currencies", hash);
            }


            return result;
        }

        public PurchasableItem getItemById(string id) {
            PurchasableItem result = inventory.Find(x => x.Id == id);
            if (null == result) {
                logger.LogWarning("Unknown purchasable item:{0}. Check your Unibill inventory configuration.", id);
            }
            return result;
        }

        public VirtualCurrency getCurrency(string id) {
            return currencies.Find(x => x.currencyId == id);
        }

        public List<PurchasableItem> AllPurchasableItems {
            get { return new List<PurchasableItem>(inventory); } // Defensive copy to prevent modification.
        }

        public List<PurchasableItem> AllNonConsumablePurchasableItems {
            get { return inventory.FindAll(x => x.PurchaseType == PurchaseType.NonConsumable); }
        }

        public List<PurchasableItem> AllConsumablePurchasableItems {
            get { return inventory.FindAll(x => x.PurchaseType == PurchaseType.Consumable); }
        }

        public List<PurchasableItem> AllSubscriptions {
            get { return inventory.FindAll(x => x.PurchaseType == PurchaseType.Subscription); }
        }

        public List<PurchasableItem> AllNonSubscriptionPurchasableItems {
            get { return inventory.FindAll(x => x.PurchaseType != PurchaseType.Subscription); }
        }


        private bool tryGetBoolean(string name, Dictionary<string, object> root) {
            if (root.ContainsKey(name)) {
                return Boolean.Parse(root[name].ToString());
            }
            return false;
		}
    }
}
