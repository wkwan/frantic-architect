using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unibill;
using Unibill.Impl;
using UnityEngine;
using Uniject;
using Uniject.Impl;
using unibill.Dummy;

namespace Unibill.Impl {
    public class BillerFactory {

        private IResourceLoader loader;
        private ILogger logger;
        private IStorage storage;
        private IRawBillingPlatformProvider platformProvider;
        private IUtil util;
        private UnibillConfiguration config;
        private IUnityAnalytics analytics;

        public BillerFactory(IResourceLoader resourceLoader, ILogger logger,
                             IStorage storage, IRawBillingPlatformProvider platformProvider,
                             UnibillConfiguration config, IUtil util, IUnityAnalytics analytics) {
            this.loader = resourceLoader;
            this.logger = logger;
            this.storage = storage;
            this.platformProvider = platformProvider;
            this.config = config;
            this.util = util;
            this.analytics = analytics;
        }

        public Biller instantiate() {
            IBillingService svc = instantiateBillingSubsystem();

            var biller = new Biller(config, getTransactionDatabase(), svc, getLogger(), getHelp(), getMapper(), getCurrencyManager());
            instantiateAnalytics (biller);
            return biller;
        }

        public void instantiateAnalytics(Biller biller) {
            var analyticsReporter = new AnalyticsReporter (biller.InventoryDatabase.CurrentPlatform, analytics);
            biller.onPurchaseComplete += x => {
                if (x.IsNewPurchase) {
                    analyticsReporter.onPurchaseSucceeded (x.PurchasedItem);
                }
            };
        }

        private IBillingService instantiateBillingSubsystem() {
            switch (config.CurrentPlatform) {
                case BillingPlatform.AppleAppStore:
                    return new AppleAppStoreBillingService(getMapper(), platformProvider.getStorekit(), util, getLogger());
                case BillingPlatform.AmazonAppstore:
                    return new AmazonAppStoreBillingService(platformProvider.getAmazon(), getMapper(), getTransactionDatabase(), getLogger());
                case BillingPlatform.GooglePlay:
                    return new GooglePlayBillingService(platformProvider.getGooglePlay(), config, getMapper(), getLogger());
                case BillingPlatform.MacAppStore:
                    return new AppleAppStoreBillingService(getMapper(), platformProvider.getStorekit(), util, getLogger());
                #if UNITY_WINRT
                case BillingPlatform.WindowsPhone8:
                    var eventHook = new GameObject().AddComponent<WP8Eventhook>();
                    var result = new WP8BillingService(unibill.Dummy.Factory.Create(config.WP8SandboxEnabled, GetDummyProducts()), getMapper(), getTransactionDatabase(), util, getLogger());
                    eventHook.callback = result;
                    return result;
                #endif
                #if UNITY_METRO
                case BillingPlatform.Windows8_1:
                    var win8 = new Win8_1BillingService(unibill.Dummy.Factory.Create(config.UseWin8_1Sandbox, GetDummyProducts()), getMapper(), getTransactionDatabase(), getLogger());
                    new GameObject().AddComponent<Win8Eventhook>().callback = win8;
                    return win8;
                #endif

                case BillingPlatform.SamsungApps:
					return new SamsungAppsBillingService (config, platformProvider.getSamsung ());
            }
            return new Tests.FakeBillingService(getMapper());
        }

        private CurrencyManager _currencyManager;
        private CurrencyManager getCurrencyManager() {
            if (null == _currencyManager) {
                _currencyManager = new CurrencyManager(config, getStorage());
            }
            return _currencyManager;
        }

        private ProductDescription[] GetDummyProducts() {
            return config.AllPurchasableItems.Where((x) => x.PurchaseType != PurchaseType.Subscription).Select((x) => {
                var prod = new ProductDescription(x.LocalId,
                    "$123.45",
                    x.name,
                    x.description,
                    null,
                    123.45m);
                prod.Consumable = x.PurchaseType == PurchaseType.Consumable;
                return prod;
                }).ToArray();
        }

        private TransactionDatabase _tDb;
        private TransactionDatabase getTransactionDatabase() {
            if (null == _tDb) {
                _tDb = new TransactionDatabase(getStorage(), getLogger());
            }
            return _tDb;
        }

        private IStorage getStorage() {
            return storage;
        }

        private HelpCentre _helpCentre;
        private HelpCentre getHelp() {
            if (null == _helpCentre) {
                _helpCentre = new HelpCentre(loader.openTextFile("unibillStrings.json").ReadToEnd());
            }

            return _helpCentre;
        }

        private ProductIdRemapper _remapper;
        private ProductIdRemapper getMapper() {
            if (null == _remapper) {
                _remapper = new ProductIdRemapper(config);
            }

            return _remapper;
        }

        private ILogger getLogger() {
            return logger;
        }

        private IResourceLoader getResourceLoader() {
            return loader;
        }
    }
}
