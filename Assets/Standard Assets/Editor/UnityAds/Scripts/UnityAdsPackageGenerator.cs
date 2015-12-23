using UnityEngine;
using UnityEditor;
using System.Collections;

public class UnityAdsPackageGenerator : MonoBehaviour {

	private static string[] assetPathNames = {
		"Assets/Plugins",
		"Assets/Standard Assets"
	};

	private static string defaultFileName = "UnityAds";
	private static string defaultFileExtension = "unitypackage";

	public static void CreatePackage() {
		string fileNameWithPath = defaultFileName + "." + defaultFileExtension;

		AssetDatabase.ExportPackage(assetPathNames, fileNameWithPath, ExportPackageOptions.Recurse);

		Debug.Log("UnityAds package created");
	}
}
