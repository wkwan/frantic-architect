//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEditor;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEngine;
using Unibill.Impl;
using Uniject;
using Uniject.Impl;


public class AndroidManifestGenerator {

    private const string AndroidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

    public static void mergeManifest() {

        if (!Directory.Exists("Assets/Plugins/Android")) {
            AssetDatabase.CreateFolder("Assets/Plugins", "Android");
        }

		CreateManifestIfNecessary ();
        
		string xml = new UnityResourceLoader ().openTextFile ("unibillInventory.json").ReadToEnd ();
        UnibillConfiguration config = new UnibillConfiguration(xml, Application.platform, new UnityLogger());
        XDocument doc = XDocument.Load(AndroidManifestPath);
        doc = new AndroidManifestMerger().merge(doc, config.AndroidBillingPlatform, config.AmazonSandboxEnabled);
        doc.Save(AndroidManifestPath);
        AssetDatabase.ImportAsset(AndroidManifestPath);
    }

	public static void CreateManifestIfNecessary() {
		if (!File.Exists (AndroidManifestPath) && InventoryPostProcessor.ShouldWriteInventory()) {
			AssetDatabase.CopyAsset("Assets/Plugins/unibill/static/Manifest.xml", AndroidManifestPath);
			AssetDatabase.ImportAsset(AndroidManifestPath);
		}
	}
}
