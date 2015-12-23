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
* Modified for use with the Heyzap Unity plugin
*/
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Heyzap {
    public class HZAndroidSetupUI : EditorWindow {
        
        // minimum version of the Google Play Services library project
        public const long MinGmsCoreVersionCode = 4030530;
        
        [MenuItem("Heyzap/Android Setup...", false, 0)]
        public static void MenuItemHZAndroidSetup() {
            EditorWindow.GetWindow(typeof(HZAndroidSetupUI));
        }
        
        public const string sTitle = "Heyzap - Android Configuration";
        public const string sBlurb = "To configure Heyzap in this project,\n" +
            "please click on the Setup button.";
        public const string sSetupButton = "Setup";
        
        public const string sError = "Error";
        public const string sOk = "OK";
        public const string sCancel = "Cancel";
        public const string sYes = "Yes";
        public const string sNo = "No";
        public const string sSuccess = "Success";
        public const string sWarning = "Warning";
        
        public const string sSdkNotFound = "Android SDK Not found";
        public const string sSdkNotFoundBlurb = "The Android SDK path was not found. " +
            "Please configure it in the Unity preferences window (under External Tools).";
        
        public const string sLibProjNotFound = "Google Play Services Library Project Not Found";
        public const string sLibProjNotFoundBlurb = "Google Play Services library project " +
            "could not be found your SDK installation. Make sure it is installed (open " +
                "the SDK manager and go to Extras, and select Google Play Services).";
        
        public const string sLibProjVerNotFound = "The version of your copy of the Google Play " +
            "Services Library Project could not be determined. Please make sure it is " +
                "at least version {0}. Continue?";
        
        public const string sLibProjVerTooOld = "Your copy of the Google Play " +
            "Services Library Project is out of date. Please launch the Android SDK manager " +
                "and upgrade your Google Play Services bundle to the latest version (your version: " +
                "{0}; required version: {1}). Proceeding may cause problems. Proceed anyway?";
        
        public const string sSetupComplete = "Heyzap configured successfully.";
        
        public HZAndroidSetupUI() {

        }
        
        void OnGUI() {
            GUILayout.BeginArea(new Rect(20, 20, position.width - 40, position.height - 40));
            GUILayout.Label(sTitle, EditorStyles.boldLabel);
            GUILayout.Label(sBlurb);
            
            GUILayout.Space(10);
            if (GUILayout.Button(sSetupButton)) {
                DoSetup();
            }
            GUILayout.EndArea();
        }
        
        void DoSetup() {
            string sdkPath = GetAndroidSdkPath();
            string libProjPath = sdkPath +
                FixSlashes("/extras/google/google_play_services/libproject/google-play-services_lib");
            string lobProjVersionFile = libProjPath + FixSlashes("/res/values/version.xml");
            string libProjDestDir = FixSlashes("Assets/Plugins/Android/google-play-services_lib");
            
            // check that Android SDK is there
            if (!HasAndroidSdk()) {
                Debug.LogError("Android SDK not found.");
                EditorUtility.DisplayDialog(sSdkNotFound,
                                            sSdkNotFoundBlurb, sOk);
                return;
            }
            
            // check that the Google Play Services lib project is there
            if (!System.IO.Directory.Exists(libProjPath) || !System.IO.File.Exists(lobProjVersionFile)) {
                Debug.LogError("Google Play Services lib project not found at: " + libProjPath);
                EditorUtility.DisplayDialog(sLibProjNotFound,
                                            sLibProjNotFoundBlurb, sOk);
                return;
            }
            
            // check lib project version
            if (!CheckAndWarnAboutGmsCoreVersion(lobProjVersionFile)) {
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
        
        private bool CheckAndWarnAboutGmsCoreVersion(string lobProjVersionFile) {
            string fileContents = ReadFile(lobProjVersionFile);
            long vercode = 0;
            // looking for version number like this: `<integer name="google_play_services_version">7571000</integer>`
            MatchCollection matches = Regex.Matches(fileContents, "google_play_services_version\">([\\d]+)<");

            foreach (Match match in matches) {
                vercode = System.Convert.ToInt64(match.Groups[1].Value);
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
        
        public static string FixSlashes(string path) {
            return path.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());
        }
        
        public static string ReadFile(string filePath) {
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
        
        public static string GetAndroidSdkPath() {
            string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            if (sdkPath != null && (sdkPath.EndsWith("/") || sdkPath.EndsWith("\\"))) {
                sdkPath = sdkPath.Substring(0, sdkPath.Length - 1);
            }
            return sdkPath;
        }
        
        public static bool HasAndroidSdk() {
            string sdkPath = GetAndroidSdkPath();
            return sdkPath != null && sdkPath.Trim() != "" && System.IO.Directory.Exists(sdkPath);
        }
    }
}