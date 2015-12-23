//  HeyzapAds.cs
//
//  Copyright 2015 Heyzap, Inc. All Rights Reserved
//
//  Permission is hereby granted, free of charge, to any person
//  obtaining a copy of this software and associated documentation
//  files (the "Software"), to deal in the Software without
//  restriction, including without limitation the rights to use,
//  copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following
//  conditions:
//
//  The above copyright notice and this permission notice shall be
//  included in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//  OTHER DEALINGS IN THE SOFTWARE.
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace Heyzap {
    /// <summary>
    /// Heyzap wrapper for iOS and Android via Unity. For more information, see https://developers.heyzap.com/docs/unity_sdk_setup_and_requirements .
    /// </summary>
    public class HeyzapAds : MonoBehaviour {
        public delegate void NetworkCallbackListener(string network, string callback);

        private static NetworkCallbackListener networkCallbackListener;
        private static HeyzapAds _instance = null;
        
        #region Flags for the call to HeyzapAds.StartWithOptions()
        /// <summary>
        /// Use this flag to start the Heyzap SDK with no extra configuration options. This is the default behavior if no options are passed when the SDK is started.
        /// </summary>
        public const int FLAG_NO_OPTIONS = 0 << 0; // 0
        /// <summary>
        /// Use this flag to disable automatic prefetching of ads. You must call a `Fetch` method for every ad unit before a matching call to a `Show` method.
        /// </summary>
        public const int FLAG_DISABLE_AUTOMATIC_FETCHING = 1 << 0; // 1
        /// <summary>
        /// Use this flag to disable all advertising functionality of the Heyzap SDK. This should only be used if you're integrating the SDK solely as an install tracker.
        /// </summary>
        public const int FLAG_INSTALL_TRACKING_ONLY = 1 << 1; // 2
        /// <summary>
        /// (Android only) Use this flag to tell the Heyzap SDK that this app is being distributed on the Amazon App Store.
        /// </summary>
        public const int FLAG_AMAZON = 1 << 2; // 4
        /// <summary>
        /// Use this flag to disable the mediation features of the Heyzap SDK. Only Heyzap ads will be available.
        /// You should set this flag if you are using Heyzap through another mediation tool to avoid potential conflicts.
        /// </summary>
        public const int FLAG_DISABLE_MEDIATION = 1 << 3; // 8
        /// <summary>
        /// (iOS only) Use this flag to stop the Heyzap SDK from automatically recording in-app purchases.
        /// </summary>
        public const int FLAG_DISABLE_AUTOMATIC_IAP_RECORDING = 1 << 4; // 16

        [Obsolete("Use FLAG_AMAZON instead - we refactored the flags to be consistently named.")]
        public const int AMAZON = FLAG_AMAZON;
        [Obsolete("Use FLAG_DISABLE_MEDIATION instead - we refactored the flags to be consistently named.")]
        public const int DISABLE_MEDIATION = FLAG_DISABLE_MEDIATION;
        #endregion

        public const string DEFAULT_TAG = "default";
        
        #region String constants to expect in network callbacks
        public static class NetworkCallback {
            public const string INITIALIZED = "initialized";
            public const string SHOW = "show";
            public const string AVAILABLE = "available";
            public const string HIDE = "hide";
            public const string FETCH_FAILED = "fetch_failed";
            public const string CLICK = "click";
            public const string DISMISS = "dismiss";
            public const string INCENTIVIZED_RESULT_COMPLETE = "incentivized_result_complete";
            public const string INCENTIVIZED_RESULT_INCOMPLETE = "incentivized_result_incomplete";
            public const string AUDIO_STARTING = "audio_starting";
            public const string AUDIO_FINISHED = "audio_finished";

            // currently sent in Android, but they were removed for iOS
            public const string BANNER_LOADED = "banner-loaded";
            public const string BANNER_CLICK = "banner-click";
            public const string BANNER_HIDE = "banner-hide";
            public const string BANNER_DISMISS = "banner-dismiss";
            public const string BANNER_FETCH_FAILED = "banner-fetch_failed";

            public const string LEAVE_APPLICATION = "leave_application";

            // Facebook Specific
            public const string FACEBOOK_LOGGING_IMPRESSION = "logging_impression";

            // Chartboost Specific
            public const string CHARTBOOST_MOREAPPS_FETCH_FAILED = "moreapps-fetch_failed";
            public const string CHARTBOOST_MOREAPPS_HIDE = "moreapps-hide";
            public const string CHARTBOOST_MOREAPPS_DISMISS = "moreapps-dismiss";
            public const string CHARTBOOST_MOREAPPS_CLICK = "moreapps-click";
            public const string CHARTBOOST_MOREAPPS_SHOW = "moreapps-show";
            public const string CHARTBOOST_MOREAPPS_AVAILABLE = "moreapps-available";
            public const string CHARTBOOST_MOREAPPS_CLICK_FAILED = "moreapps-click_failed";
        }
        #endregion

        #region Network names
        public static class Network {
            public const string HEYZAP = "heyzap";
            public const string HEYZAP_CROSS_PROMO = "heyzap_cross_promo";
            public const string HEYZAP_EXCHANGE = "heyzap_exchange";
            public const string FACEBOOK = "facebook";
            public const string UNITYADS = "unityads";
            public const string APPLOVIN = "applovin";
            public const string VUNGLE = "vungle";
            public const string CHARTBOOST = "chartboost";
            public const string ADCOLONY = "adcolony";
            public const string ADMOB = "admob";
            public const string IAD = "iad";
            public const string LEADBOLT = "leadbolt";
            public const string INMOBI = "inmobi";
        }
        #endregion

        #region Public API
        /// <summary>
        /// Starts the Heyzap SDK. Call this method as soon as possible in your app to ensure Heyzap has time to initialize before you want to show an ad.
        /// </summary>
        /// <param name="publisher_id">Your publisher ID. This can be found on your Heyzap dashboards - see https://developers.heyzap.com/docs/unity_sdk_setup_and_requirements for more information.</param>
        /// <param name="options">A bitmask of options you can pass to this call to change the way Heyzap will work.</param>
        public static void Start(string publisher_id, int options) {
            #if !UNITY_EDITOR

            #if UNITY_ANDROID
            HeyzapAdsAndroid.Start(publisher_id, options);
            #endif

            #if UNITY_IPHONE
            HeyzapAdsIOS.Start(publisher_id, options);
            #endif

            HeyzapAds.InitReceiver();
            HZInterstitialAd.InitReceiver();
            HZVideoAd.InitReceiver();
            HZIncentivizedAd.InitReceiver();
            HZBannerAd.InitReceiver();

            #endif
        }
        
        /// <summary>
        /// Returns the remote data you've set on the Heyzap Dashboards, which will be a JSON dictionary in string format.
        /// </summary>
        public static string GetRemoteData(){
            #if UNITY_ANDROID
            return HeyzapAdsAndroid.GetRemoteData();
            #elif UNITY_IPHONE && !UNITY_EDITOR
            return HeyzapAdsIOS.GetRemoteData();
            #else 
            return "{}";
            #endif
        }

        /// <summary>
        /// Shows the mediation test suite.
        /// </summary>
        public static void ShowMediationTestSuite() {
            #if UNITY_ANDROID
            HeyzapAdsAndroid.ShowMediationTestSuite();
            #endif

            #if UNITY_IPHONE && !UNITY_EDITOR
            HeyzapAdsIOS.ShowMediationTestSuite();
            #endif
        }
        
        /// <summary>
        /// (Android only) Call this method in your back button pressed handler to make sure the back button does what the user should expect when ads are showing.
        /// </summary>
        /// <returns><c>true</c>, if Heyzap handled the back button press (in which case your code should not do anything else), and <c>false</c> if Heyzap did not handle the back button press (in which case your app may want to do something).</returns>
        public static Boolean OnBackPressed() {
            #if UNITY_ANDROID
            return HeyzapAdsAndroid.OnBackPressed();

            #elif UNITY_IPHONE && !UNITY_EDITOR
            return HeyzapAdsIOS.OnBackPressed();

            #else
            return false;
            #endif
        }

        /// <summary>
        /// Returns whether or not the given network has been initialized by Heyzap yet.
        /// </summary>
        /// <returns><c>true</c> if is network initialized the specified network; otherwise, <c>false</c>.</returns>
        /// <param name="network">The name of the network in question. Use the strings in HeyzapAds.Network to ensure the name matches what we expect.</param>
        public static Boolean IsNetworkInitialized(string network) {
            #if UNITY_ANDROID
            return HeyzapAdsAndroid.IsNetworkInitialized(network);

            #elif UNITY_IPHONE && !UNITY_EDITOR
            return HeyzapAdsIOS.IsNetworkInitialized(network);

            #else
            return false;
            #endif
        }
        
        /// <summary>
        /// Sets the NetworkCallbackListener, which receives messages about specific networks, such as when a specific network fetches an ad.
        /// </summary>
        public static void SetNetworkCallbackListener(NetworkCallbackListener listener) {
            networkCallbackListener = listener;
        }
        
        /// <summary>
        /// (iOS only) Pauses expensive work, like ad fetches, until ResumeExpensiveWork() is called. Note that calling this method will affect ad availability.
        /// </summary>
        public static void PauseExpensiveWork() {
            #if UNITY_IPHONE && !UNITY_EDITOR
            HeyzapAdsIOS.PauseExpensiveWork();
            #endif
        }

        /// <summary>
        /// (iOS only) Unpauses expensive work, like ad fetches. Only relevant after a call to PauseExpensiveWork().
        /// </summary>
        public static void ResumeExpensiveWork() {
            #if UNITY_IPHONE && !UNITY_EDITOR
            HeyzapAdsIOS.ResumeExpensiveWork();
            #endif
        }

        /// <summary>
        /// Enables verbose debug logging for the Heyzap SDK.
        /// </summary>
        public static void ShowDebugLogs() {
            #if UNITY_ANDROID
            HeyzapAdsAndroid.ShowDebugLogs();
            #endif
            
            #if UNITY_IPHONE && !UNITY_EDITOR
            HeyzapAdsIOS.ShowDebugLogs();
            #endif
        }

        /// <summary>
        /// Hides all debug logs coming from the Heyzap SDK.
        /// </summary>
        public static void HideDebugLogs() {
            #if UNITY_ANDROID
            HeyzapAdsAndroid.HideDebugLogs();
            #endif
            
            #if UNITY_IPHONE && !UNITY_EDITOR
            HeyzapAdsIOS.HideDebugLogs();
            #endif
        }

        /// <summary>
        /// (iOS only currently) Enables verbose debug logging for all the mediated SDKs we can.
        /// You should call this method before starting the Heyzap SDK if you wish to enable these logs, since some SDK implementations only consider this parameter when initialized.
        /// </summary>
        public static void ShowThirdPartyDebugLogs() {
            #if UNITY_ANDROID
            #endif
            
            #if UNITY_IPHONE && !UNITY_EDITOR
            HeyzapAdsIOS.ShowThirdPartyDebugLogs();
            #endif
        }

        /// <summary>
        /// (iOS only currently) Disables verbose debug logging for all the mediated SDKs we can.
        /// Only some networks' logs will be turned off, since some SDK implementations only consider this parameter when initialized.
        /// </summary>
        public static void HideThirdPartyDebugLogs() {
            #if UNITY_ANDROID
            #endif
            
            #if UNITY_IPHONE && !UNITY_EDITOR
            HeyzapAdsIOS.HideThirdPartyDebugLogs();
            #endif
        }
        #endregion

        #region Internal methods
        public void SetNetworkCallbackMessage(string message) {
            string[] networkStateParams = message.Split(',');
            SetNetworkCallback(networkStateParams[0], networkStateParams[1]); 
        }

        protected static void SetNetworkCallback(string network, string callback) {
            if (networkCallbackListener != null) {
                networkCallbackListener(network, callback);
            }
        }

        public static void InitReceiver(){
            if (_instance == null) {
                GameObject receiverObject = new GameObject("HeyzapAds");
                DontDestroyOnLoad(receiverObject);
                _instance = receiverObject.AddComponent<HeyzapAds>();
            }
        }

        public static string TagForString(string tag) {
            if (tag == null) {
                tag = HeyzapAds.DEFAULT_TAG;
            }
            
            return tag;
        }
        #endregion

        #region Deprecated methods
        //-------- Deprecated methods - will be removed in a future version of the SDK -------- //
        
        [Obsolete("Use the Start() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void start(string publisher_id, int options) {
            Start(publisher_id, options);
        }

        [Obsolete("Use the GetRemoteData() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static string getRemoteData(){
            return GetRemoteData();
        }

        [Obsolete("Use the ShowMediationTestSuite() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void showMediationTestSuite() {
            ShowMediationTestSuite();
        }

        [Obsolete("Use the IsNetworkInitialized(String) method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static Boolean isNetworkInitialized(string network) {
            return IsNetworkInitialized(network);
        }

        [Obsolete("Use the SetNetworkCallbackListener(NetworkCallbackListener) method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void setNetworkCallbackListener(NetworkCallbackListener listener) {
            SetNetworkCallbackListener(listener);
        }

        [Obsolete("Use the PauseExpensiveWork() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void pauseExpensiveWork() {
            PauseExpensiveWork();
        }

        [Obsolete("Use the ResumeExpensiveWork() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void resumeExpensiveWork() {
            ResumeExpensiveWork();
        }

        [Obsolete("Use the ShowDebugLogs() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void showDebugLogs() {
            ShowDebugLogs();
        }

        [Obsolete("Use the HideDebugLogs() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void hideDebugLogs() {
            HideDebugLogs();
        }

        [Obsolete("Use the OnBackPressed() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static Boolean onBackPressed() {
            return OnBackPressed();
        }
        #endregion
    }

    #region Platform-specific translations
    #if UNITY_IPHONE && !UNITY_EDITOR
    public class HeyzapAdsIOS : MonoBehaviour {

        [DllImport ("__Internal")]
        private static extern void hz_ads_start_app(string publisher_id, int flags);

        [DllImport ("__Internal")]
        private static extern void hz_ads_show_mediation_debug_view_controller();

        [DllImport ("__Internal")]
        private static extern string hz_ads_get_remote_data();

        [DllImport ("__Internal")]
        private static extern bool hz_ads_is_network_initialized(string network);

        [DllImport ("__Internal")]
        private static extern void hz_pause_expensive_work();

        [DllImport ("__Internal")]
        private static extern void hz_resume_expensive_work();

        [DllImport ("__Internal")]
        private static extern void hz_ads_hide_debug_logs();

        [DllImport ("__Internal")]
        private static extern void hz_ads_show_debug_logs();

        [DllImport ("__Internal")]
        private static extern void hz_ads_hide_third_party_debug_logs();

        [DllImport ("__Internal")]
        private static extern void hz_ads_show_third_party_debug_logs();


        public static void Start(string publisher_id, int options=0) {
            hz_ads_start_app(publisher_id, options);
        }
        
        public static void ShowMediationTestSuite() {
            hz_ads_show_mediation_debug_view_controller();
        }

        public static Boolean OnBackPressed(){
            return false;
        }

        public static bool IsNetworkInitialized(string network) {
            return hz_ads_is_network_initialized(network);
        }

        public static string GetRemoteData(){
            return hz_ads_get_remote_data();
        }

        public static void PauseExpensiveWork() {
            hz_pause_expensive_work();
        }

        public static void ResumeExpensiveWork() {
            hz_resume_expensive_work();
        }

        public static void ShowDebugLogs() {
            hz_ads_show_debug_logs();
        }
        
        public static void HideDebugLogs() {
            hz_ads_hide_debug_logs();
        }

        public static void ShowThirdPartyDebugLogs() {
            hz_ads_show_third_party_debug_logs();
        }
        
        public static void HideThirdPartyDebugLogs() {
            hz_ads_hide_third_party_debug_logs();
        }
    }
    #endif

    #if UNITY_ANDROID
    public class HeyzapAdsAndroid : MonoBehaviour {
        public static void Start(string publisher_id, int options=0) {
            if(Application.platform != RuntimePlatform.Android) return;

            AndroidJNIHelper.debug = false; 
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                jc.CallStatic("start", publisher_id, options);
            }
        }

        public static Boolean IsNetworkInitialized(string network) {
            if (Application.platform != RuntimePlatform.Android) return false;

            AndroidJNIHelper.debug = false; 
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                return jc.CallStatic<Boolean>("isNetworkInitialized", network);
            }
        }

        public static Boolean OnBackPressed(){
            if(Application.platform != RuntimePlatform.Android) return false;

            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) {
                return jc.CallStatic<Boolean>("onBackPressed");
            }
        }

        public static void ShowMediationTestSuite() {
            if(Application.platform != RuntimePlatform.Android) return;

            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) {
                jc.CallStatic("showNetworkActivity");
            }
        }

        public static string GetRemoteData() {
            if(Application.platform != RuntimePlatform.Android) return "{}";
            AndroidJNIHelper.debug = false;

            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) {
                return jc.CallStatic<String>("getCustomPublisherData");
            }
        }

        public static void ShowDebugLogs() {
            if(Application.platform != RuntimePlatform.Android) return;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) {
                jc.CallStatic("showDebugLogs");
            }
        }

        public static void HideDebugLogs() {
            if(Application.platform != RuntimePlatform.Android) return;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) {
                jc.CallStatic("hideDebugLogs");
            }
        }
    }
    #endif
    #endregion
}
