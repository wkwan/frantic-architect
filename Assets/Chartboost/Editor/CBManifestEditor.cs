using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System;


/*
 * The only thing we want to make sure here is to make sure that unityplayer.ForwardNativeEventsToDalvik is true
 * and the uses-sdk tag is there for the manifest
 */
namespace ChartboostSDK
{
	public class CBManifest
	{
		private static string unityNativeActivityName = "com.unity3d.player.UnityPlayerNativeActivity";

		public static void GenerateManifest()
		{
			var outputFile = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
			
			// only copy over a fresh copy of the AndroidManifest if one does not exist
			if (!File.Exists(outputFile))
			{
				var inputFile = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/androidplayer/AndroidManifest.xml");
				File.Copy(inputFile, outputFile);
			}
			CheckAndFixManifest();
		}
		
		public static bool CheckAndFixManifest()
		{
			var outputFile = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
			if (!File.Exists(outputFile))
			{
				GenerateManifest();
				return true;
			}
			
			XmlDocument doc = new XmlDocument();
			doc.Load(outputFile);
			
			if (doc == null)
			{
				Debug.LogError("Couldn't load " + outputFile);
				return false;
			}
			
			XmlNode manNode = FindChildNode(doc, "manifest");
			string ns = manNode.GetNamespaceOfPrefix("android");

			XmlNode dict = FindChildNode(manNode, "application");
			
			if (dict == null)
			{
				Debug.LogError("Error parsing " + outputFile);
				return false;
			}

			XmlElement unityActivityElement = FindElementWithAndroidName("activity", "name", ns, unityNativeActivityName, dict);
			if (unityActivityElement == null)
			{
				Debug.LogError(string.Format("{0} activity is missing from your android manifest. Add \"<meta-data android:name=\"unityplayer.ForwardNativeEventsToDalvik\" android:value=\"true\" />\" to your activity tag so that Chartboost can forward touch events to the advertisements.", unityNativeActivityName));
				return false;
			}

			XmlElement usesSDKElement = FindChildNode(manNode, "uses-sdk") as XmlElement;
			if (usesSDKElement == null)
			{
				usesSDKElement = doc.CreateElement("uses-sdk");
				usesSDKElement.SetAttribute("minSdkVersion",ns,"9");
				manNode.InsertBefore(usesSDKElement, dict);
			}
			else if (Convert.ToInt32(usesSDKElement.GetAttribute("minSdkVersion",ns)) < 9)
			{
				Debug.Log("Chartboost SDK requires the minSdkVersion to be atleast 9 (Android 2.3 and up), please update the same in the manifest file for chartboost sdk to work properly");
				return false;
			}

			XmlElement forwardNativeEventsToDalvikElement = FindElementWithAndroidName("meta-data", "name", ns, "unityplayer.ForwardNativeEventsToDalvik", unityActivityElement);
			if (forwardNativeEventsToDalvikElement == null)
			{
				// Add the forwardNativesToDalvik meta tag with the value true
				forwardNativeEventsToDalvikElement = doc.CreateElement("meta-data");
				forwardNativeEventsToDalvikElement.SetAttribute("name", ns, "unityplayer.ForwardNativesToDalvik");
				forwardNativeEventsToDalvikElement.SetAttribute("value", ns,"true");
				unityActivityElement.AppendChild(forwardNativeEventsToDalvikElement);
				doc.Save(outputFile);
			}
			else if(forwardNativeEventsToDalvikElement.GetAttribute("value",ns).Equals("false"))
			{
				// Set the value of this tag to true
				forwardNativeEventsToDalvikElement.SetAttribute("value", ns,"true");
				doc.Save(outputFile);
			}
			return true;
		}
		
		private static XmlNode FindChildNode(XmlNode parent, string name)
		{
			XmlNode curr = parent.FirstChild;
			while (curr != null)
			{
				if (curr.Name.Equals(name))
				{
					return curr;
				}
				curr = curr.NextSibling;
			}
			return null;
		}
		
		private static XmlElement FindElementWithAndroidName(string name, string androidName, string ns, string value, XmlNode parent)
		{
			var curr = parent.FirstChild;
			while (curr != null)
			{
				if (curr.Name.Equals(name) && curr is XmlElement && ((XmlElement)curr).GetAttribute(androidName, ns) == value)
				{
					return curr as XmlElement;
				}
				curr = curr.NextSibling;
			}
			return null;
		}
	}
}

