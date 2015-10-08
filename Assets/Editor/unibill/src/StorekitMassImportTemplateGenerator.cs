//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using Uniject.Editor;
using Uniject;

namespace Unibill.Impl {
    public class StorekitMassImportTemplateGenerator {

        private IEditorUtil util;
        private UnibillConfiguration config;

        public StorekitMassImportTemplateGenerator(UnibillConfiguration config, IEditorUtil util) {
            this.config = config;
            this.util = util;
        }

        public void writeFile (BillingPlatform platform) {
            string directory = Path.Combine (util.getAssetsDirectoryPath (), "Plugins/unibill/generated/storekit");
            if (!Directory.Exists (directory)) {
                Directory.CreateDirectory(directory);
            }
            string path = Path.Combine (directory, string.Format("{0}.MassImportTemplate.txt", platform));
            using (StreamWriter writer = new StreamWriter(path, false)) {
                writer.WriteLine (getHeaderLine ());
                foreach (PurchasableItem item in config.AllPurchasableItems) {
                    if (PurchaseType.Subscription != item.PurchaseType) {
                        writer.WriteLine(serialisePurchasable(item, platform));
                    }
                }
            }
        }

        public string getHeaderLine () {
            string[] headers = new string[] {
                "SKU",
                "Product ID",
                "Reference Name",
                "Type",
                "Cleared For Sale",
                "Wholesale Price Tier",
                "Displayed Name",
                "Description",
                "Screenshot Path",
            };
            return string.Join("\t", headers);
        }

        public string serialisePurchasable (PurchasableItem item, BillingPlatform platform) {
            string screenshotPath = item.platformBundles[BillingPlatform.AppleAppStore].get<string>("screenshotPath");
            if (!string.IsNullOrEmpty (screenshotPath)) {
                string assetPath = util.guidToAssetPath((string)screenshotPath);
                if (!string.IsNullOrEmpty(assetPath)) {
                    screenshotPath = new FileInfo(assetPath).FullName;
                }
            }
            var records = new string[] {
                platform == BillingPlatform.AppleAppStore ? config.iOSSKU : config.macAppStoreSKU,
                item.LocalIds[platform],
                item.name, // This is the 'reference' field that is used to refer to the product within iTunes connect.
                item.PurchaseType == PurchaseType.Consumable ? "Consumable" : "Non-Consumable",
                "yes",
                item.platformBundles[BillingPlatform.AppleAppStore].getString("appleAppStorePriceTier"),
                item.name,
                item.description,
                screenshotPath,
            };

            return string.Join("\t", records);
        }
    }
}
