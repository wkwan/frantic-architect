//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unibill;
using Unibill.Impl;
using System.IO;
using System.Xml;

public class InventoryPostProcessor : AssetPostprocessor {
	
	public const string UNIBILL_XML_INVENTORY_PATH = "Assets/Plugins/unibill/resources/unibillInventory.xml";
    public const string UNIBILL_JSON_INVENTORY_PATH = "Assets/Plugins/unibill/resources/unibillInventory.json.txt";
	private const string UNIBILL_BACKUP_PATH = "Assets/Plugins/unibill/resources/old_inventory_delete_me.xml";
	
    static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath) {
		CreateInventoryIfNecessary ();
        DeleteLegacyFiles ();

        foreach (var s in importedAssets) {
			try {
	            if (s.Contains("unibillInventory")) {
                    UnibillInjector.GetStorekitGenerator ().writeFile (BillingPlatform.AppleAppStore);
                    UnibillInjector.GetStorekitGenerator ().writeFile (BillingPlatform.MacAppStore);
					UnibillInjector.GetGooglePlayCSVGenerator ().writeCSV ();
					UnibillInjector.GetAmazonGenerator ().encodeAll ();
	            }
			} catch (NullReferenceException) {
				// Unity insists on throwing this on first import.
			}
        }
    }

	public static void CreateInventoryIfNecessary() {
		if (!File.Exists(UNIBILL_JSON_INVENTORY_PATH) && ShouldWriteInventory()) {
			AssetDatabase.CopyAsset("Assets/Plugins/unibill/static/InventoryTemplate.json", UNIBILL_JSON_INVENTORY_PATH);
		}
	}

    /// <summary>
    /// You may be wondering what on earth this is for.
    /// This is to finally solve the problem of people's 
    /// inventory being overwritten when they update the plugin.
    /// Given that a unitypackage is a dumb directory of files
    /// that is imported, blatting anything already there, and there
    /// is no way of excluding files when uploading to the asset store,
    /// I have to stop them existing in the directory on my machine only!
    /// 
    /// One day Unity may build a proper package management system.
    /// </summary>
    public static bool ShouldWriteInventory() {
        try {
            if (File.Exists("/tmp/B1R5SxGBA7UnmxSaW5U6qlUdOfVoa7oDV")) {
                return false;
            }
        } catch (Exception) {
        }

        return true;
    }

    private static string[] legacyFiles = new string[] {
        // This is superseded by an aar.
        "Assets/Plugins/Android/unibillSamsung.jar",
        "Assets/Plugins/unibill/src/impl/DownloadManager.cs"
    };

    public static void DeleteLegacyFiles() {
        foreach (var f in legacyFiles) {
            if (File.Exists (f)) {
                AssetDatabase.DeleteAsset (f);
            }
        }
    }
}
