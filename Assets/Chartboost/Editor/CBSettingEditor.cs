/*
* Copyright (C) 2013 Google Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* 
* Modified for use with the Chartboost Unity plugin
*/
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ChartboostSDK {
	[CustomEditor(typeof(CBSettings))]
	public class CBSettingEditor : Editor {
		
		GUIContent iOSAppIdLabel = new GUIContent("App Id [?]:", "Chartboost App Ids can be found at https://dashboard.chartboost.com/app");
		GUIContent iOSAppSecretLabel = new GUIContent("App Signature [?]:", "Chartboost App Signature can be found at https://dashboard.chartboost.com/app");
		GUIContent androidAppIdLabel = new GUIContent("App Id [?]:", "Chartboost App Ids can be found at https://dashboard.chartboost.com/app");
		GUIContent androidAppSecretLabel = new GUIContent("App Signature [?]:", "Chartboost App Signature can be found at https://dashboard.chartboost.com/app");
		GUIContent amazonAppIdLabel = new GUIContent("App Id [?]:", "Chartboost App Ids can be found at https://dashboard.chartboost.com/app");
		GUIContent amazonAppSecretLabel = new GUIContent("App Signature [?]:", "Chartboost App Signature can be found at https://dashboard.chartboost.com/app");
		GUIContent selectorLabel = new GUIContent("Android Platform [?]:", "Select if building for Google Play or Amazon Store");
		GUIContent iOSLabel = new GUIContent("iOS");
		GUIContent androidLabel = new GUIContent("Google Play");
		GUIContent amazonLabel = new GUIContent("Amazon");
		GUIContent enableLoggingLabel = new GUIContent("Enable Logging for Debug Builds");
		GUIContent enableLoggingToggle = new GUIContent("isLoggingEnabled");

		// minimum version of the Google Play Services library project
		private long MinGmsCoreVersionCode = 4030530;

		private string sError = "Error";
	    private string sOk = "OK";
	    private string sCancel = "Cancel";
	    private string sSuccess = "Success";
	    private string sWarning = "Warning";

        private string sSdkNotFound = "Android SDK Not found";
        private string sSdkNotFoundBlurb = "The Android SDK path was not found. " +
                "Please configure it in the Unity preferences window (under External Tools).";

        private string sLibProjNotFound = "Google Play Services Library Project Not Found";
        private string sLibProjNotFoundBlurb = "Google Play Services library project " +
                "could not be found your SDK installation. Make sure it is installed (open " +
                "the SDK manager and go to Extras, and select Google Play Services).";
                
        private string sLibProjVerNotFound = "The version of your copy of the Google Play " +
                "Services Library Project could not be determined. Please make sure it is " +
                "at least version {0}. Continue?";
                
        private string sLibProjVerTooOld = "Your copy of the Google Play " +
            "Services Library Project is out of date. Please launch the Android SDK manager " +
            "and upgrade your Google Play Services bundle to the latest version (your version: " +
            "{0}; required version: {1}). Proceeding may cause problems. Proceed anyway?";
        
        private string sSetupComplete = "Chartboost configured successfully.";
		
		private CBSettings instance;

		public override void OnInspectorGUI() {
			instance = (CBSettings)target;

			SetupUI();
		}

		private void SetupUI() {
			EditorGUILayout.HelpBox("Add the Chartboost App Id and App Secret associated with this game", MessageType.None);

			// iOS
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(iOSLabel);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
	        EditorGUILayout.LabelField(iOSAppIdLabel);
	        EditorGUILayout.LabelField(iOSAppSecretLabel);
	        EditorGUILayout.EndHorizontal();

	        EditorGUILayout.BeginHorizontal();
            instance.SetIOSAppId(EditorGUILayout.TextField(instance.iOSAppId));
            instance.SetIOSAppSecret(EditorGUILayout.TextField(instance.iOSAppSecret));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
			EditorGUILayout.Space();

			// Android
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(androidLabel);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(androidAppIdLabel);
			EditorGUILayout.LabelField(androidAppSecretLabel);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			instance.SetAndroidAppId(EditorGUILayout.TextField(instance.androidAppId));
			instance.SetAndroidAppSecret(EditorGUILayout.TextField(instance.androidAppSecret));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// Amazon
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(amazonLabel);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(amazonAppIdLabel);
			EditorGUILayout.LabelField(amazonAppSecretLabel);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			instance.SetAmazonAppId(EditorGUILayout.TextField(instance.amazonAppId));
			instance.SetAmazonAppSecret(EditorGUILayout.TextField(instance.amazonAppSecret));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// Android Selector
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(selectorLabel);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			instance.SetAndroidPlatformIndex(EditorGUILayout.Popup("Android Platform", instance.SelectedAndroidPlatformIndex, instance.AndroidPlatformLabels));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// Loggin toggle.
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(enableLoggingLabel);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			CBSettings.enableLogging(EditorGUILayout.Toggle(enableLoggingToggle, instance.isLoggingEnabled));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Setup Android SDK"))
	        {
	            DoSetup();
	        }
	        EditorGUILayout.EndHorizontal();
		}
		
        private void DoSetup() {
	        string sdkPath = GetAndroidSdkPath();
	        string libProjPath = sdkPath +
	            FixSlashes("/extras/google/google_play_services/libproject/google-play-services_lib");
	        string libProjAM = libProjPath + FixSlashes("/AndroidManifest.xml");
	        string libProjDestDir = FixSlashes("Assets/Plugins/Android/google-play-services_lib");
	
	        // check that Android SDK is there
	        if (!HasAndroidSdk()) {
	            Debug.LogError("Android SDK not found.");
	            EditorUtility.DisplayDialog(sSdkNotFound,
	                sSdkNotFoundBlurb, sOk);
	            return;
	        }
	
	        // check that the Google Play Services lib project is there
	        if (!System.IO.Directory.Exists(libProjPath) || !System.IO.File.Exists(libProjAM)) {
	            Debug.LogError("Google Play Services lib project not found at: " + libProjPath);
	            EditorUtility.DisplayDialog(sLibProjNotFound,
	                sLibProjNotFoundBlurb, sOk);
	            return;
	        }
	        
	        // check lib project version
	        if (!CheckAndWarnAboutGmsCoreVersion(libProjAM)) {
	            return;
	        }
	
	        // create needed directories
	        EnsureDirExists("Assets/Plugins");
	        EnsureDirExists("Assets/Plugins/Android");
	
	        // clear out the destination library project
	        DeleteDirIfExists(libProjDestDir);
	
	        // Copy Google Play Services library
	        FileUtil.CopyFileOrDirectory(libProjPath, libProjDestDir);
	
	        // refresh assets, and we're done
	        AssetDatabase.Refresh();
	        EditorUtility.DisplayDialog(sSuccess,
	            sSetupComplete, sOk);
	    }
	    
	    private bool CheckAndWarnAboutGmsCoreVersion(string libProjAMFile) {
	        string manifestContents = ReadFile(libProjAMFile);
	        string[] fields = manifestContents.Split('\"');
	        int i;
	        long vercode = 0;
	        for (i = 0; i < fields.Length; i++) {
	            if (fields[i].Contains("android:versionCode") && i + 1 < fields.Length) {
	                vercode = System.Convert.ToInt64(fields[i + 1]);
	            }
	        }
	        if (vercode == 0) {
	            return EditorUtility.DisplayDialog(sWarning, string.Format(
	                sLibProjVerNotFound,
	                MinGmsCoreVersionCode),
	                sOk, sCancel);
	        } else if (vercode < MinGmsCoreVersionCode) {
	            return EditorUtility.DisplayDialog(sWarning, string.Format(
	                sLibProjVerTooOld, vercode,
	                MinGmsCoreVersionCode),
	                sOk, sCancel);
	        }
	        return true;
	    }
	
	    private void EnsureDirExists(string dir) {
	        dir = dir.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
	        if (!System.IO.Directory.Exists(dir)) {
	            System.IO.Directory.CreateDirectory(dir);
	        }
	    }
	
	    private void DeleteDirIfExists(string dir) {
	        if (System.IO.Directory.Exists(dir)) {
	            System.IO.Directory.Delete(dir, true);
	        }
	    }
		
		private string FixSlashes(string path) {
	        return path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
	    }
		
	    private string ReadFile(string filePath) {
	        filePath = FixSlashes(filePath);
	        if (!File.Exists(filePath)) {
	            EditorUtility.DisplayDialog(sError, "Plugin error: file not found: " + filePath, sOk);
	            return null;
	        }
	        StreamReader sr = new StreamReader(filePath);
	        string body = sr.ReadToEnd();
	        sr.Close();
	        return body;
	    }
		
	    private string GetAndroidSdkPath() {
	        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
	        if (sdkPath != null && (sdkPath.EndsWith("/") || sdkPath.EndsWith("\\"))) {
	            sdkPath = sdkPath.Substring(0, sdkPath.Length - 1);
	        }
	        return sdkPath;
	    }
	
	    private bool HasAndroidSdk() {
	        string sdkPath = GetAndroidSdkPath();
	        return sdkPath != null && sdkPath.Trim() != "" && System.IO.Directory.Exists(sdkPath);
	    }
	}
}
