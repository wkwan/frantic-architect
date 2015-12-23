//  HZIncentivizedAd.cs
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
    /// Use this class to show incentivized (rewarded) video ads.
    /// </summary>
    public class HZIncentivizedAd : MonoBehaviour {
        
        public delegate void AdDisplayListener(string state, string tag);
        private static AdDisplayListener adDisplayListener;
        private static HZIncentivizedAd _instance = null;

        //provided since JS can't use default parameters
        /// <summary>
        /// Fetches an ad for the default ad tag.
        /// </summary>
        public static void Fetch() {
            HZIncentivizedAd.Fetch(null);
        }
        /// <summary>
        /// Fetches an ad for the given ad tag.
        /// </summary>
        /// <param name="tag">The ad tag to fetch an ad for.</param>
        public static void Fetch(string tag) {
            tag = HeyzapAds.TagForString(tag);

            #if UNITY_ANDROID
            HZIncentivizedAdAndroid.Fetch(tag);
            #endif
            
            #if UNITY_IPHONE && !UNITY_EDITOR
            HZIncentivizedAdIOS.Fetch(tag);
            #endif
        }

        //provided since JS can't use default parameters
        /// <summary>
        /// Shows an ad with the default options.
        /// </summary>
        public static void Show() {
            HZIncentivizedAd.ShowWithOptions(null);
        }
        /// <summary>
        /// Shows an ad with the given options.
        /// </summary>
        /// <param name="showOptions"> The options to show the ad with, or the default options if <c>null</c></param>
        public static void ShowWithOptions(HZIncentivizedShowOptions showOptions) {
            if (showOptions == null) {
                showOptions = new HZIncentivizedShowOptions();
            }
            
            #if UNITY_ANDROID
            HZIncentivizedAdAndroid.ShowWithOptions(showOptions);
            #endif
            
            #if UNITY_IPHONE && !UNITY_EDITOR
            HZIncentivizedAdIOS.ShowWithOptions(showOptions);
            #endif
        }

        //provided since JS can't use default parameters
        /// <summary>
        /// Returns whether or not an ad is available for the default ad tag.
        /// </summary>
        /// <returns><c>true</c>, if an ad is available, <c>false</c> otherwise.</returns>
        public static bool IsAvailable() {
            return HZIncentivizedAd.IsAvailable(null);
        }
        /// <summary>
        /// Returns whether or not an ad is available for the given ad tag.
        /// </summary>
        /// <returns><c>true</c>, if an ad is available, <c>false</c> otherwise.</returns>
        public static bool IsAvailable(string tag) {
            tag = HeyzapAds.TagForString(tag);

            #if UNITY_ANDROID
            return HZIncentivizedAdAndroid.IsAvailable(tag);
            #elif UNITY_IPHONE && !UNITY_EDITOR
            return HZIncentivizedAdIOS.IsAvailable(tag);
            #else
            return false;
            #endif
        }
        
        /// <summary>
        /// Sets the AdDisplayListener for incentivized ads, which will receive callbacks regarding the state of incentivized ads.
        /// </summary>
        public static void SetDisplayListener(AdDisplayListener listener) {
            HZIncentivizedAd.adDisplayListener = listener;
        }

        #region Internal methods
        public static void InitReceiver(){
            if (_instance == null) {
                GameObject receiverObject = new GameObject("HZIncentivizedAd");
                DontDestroyOnLoad(receiverObject);
                _instance = receiverObject.AddComponent<HZIncentivizedAd>();
            }
        }

        public void SetCallback(string message) {
            string[] displayStateParams = message.Split(',');
            HZIncentivizedAd.SetCallbackStateAndTag(displayStateParams[0], displayStateParams[1]); 
        }
        
        protected static void SetCallbackStateAndTag(string state, string tag) {
            if (HZIncentivizedAd.adDisplayListener != null) {
                HZIncentivizedAd.adDisplayListener(state, tag);
            }
        }
        #endregion

        #region Deprecated methods
        //-------- Deprecated methods - will be removed in a future version of the SDK -------- //

        [Obsolete("Use the Fetch() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void fetch() {
            HZIncentivizedAd.Fetch();
        }
        [Obsolete("Use the Fetch(string) method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void fetch(string tag) {
            HZIncentivizedAd.Fetch(tag);
        }

        [Obsolete("Use the Show() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void show() {
            HZIncentivizedAd.Show();
        }
        [Obsolete("Use ShowWithOptions() to show ads instead of this deprecated method.")]
        public static void show(string tag) {
            HZIncentivizedShowOptions showOptions = new HZIncentivizedShowOptions();
            showOptions.Tag = tag;
            HZIncentivizedAd.ShowWithOptions(showOptions);
        }

        [Obsolete("Use the IsAvailable() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static bool isAvailable() {
            return HZIncentivizedAd.IsAvailable();
        }
        [Obsolete("Use the IsAvailable(tag) method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static bool isAvailable(string tag) {
            return HZIncentivizedAd.IsAvailable(tag);
        }

        [Obsolete("Use the SetDisplayListener(AdDisplayListener) method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void setDisplayListener(AdDisplayListener listener) {
            HZIncentivizedAd.SetDisplayListener(listener);
        }
        #endregion
    }

    #region Platform-specific translations
    #if UNITY_IPHONE && !UNITY_EDITOR
    public class HZIncentivizedAdIOS : MonoBehaviour {
        [DllImport ("__Internal")]
        private static extern void hz_ads_show_incentivized_with_custom_info(string tag, string incentivizedInfo);
        [DllImport ("__Internal")]
        private static extern void hz_ads_fetch_incentivized(string tag);
        [DllImport ("__Internal")]
        private static extern bool hz_ads_incentivized_is_available(string tag);
        

        public static void ShowWithOptions(HZIncentivizedShowOptions showOptions) {
            hz_ads_show_incentivized_with_custom_info(showOptions.Tag, showOptions.IncentivizedInfo);
        }
        
        public static void Fetch(string tag) {
            hz_ads_fetch_incentivized(tag);
        }
        
        public static bool IsAvailable(string tag) {
            return hz_ads_incentivized_is_available(tag);
        }
    }
    #endif

    #if UNITY_ANDROID
    public class HZIncentivizedAdAndroid : MonoBehaviour {
        
        public static void ShowWithOptions(HZIncentivizedShowOptions showOptions) {
            if(Application.platform != RuntimePlatform.Android) return;
            
            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                jc.CallStatic("showIncentivized", showOptions.Tag, showOptions.IncentivizedInfo); 
            }
        }
        
        public static void Fetch(string tag) {
            if(Application.platform != RuntimePlatform.Android) return;
            
            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                jc.CallStatic("fetchIncentivized", tag); 
            }
        }
        
        public static Boolean IsAvailable(string tag) {
            if(Application.platform != RuntimePlatform.Android) return false;
            
            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                return jc.CallStatic<Boolean>("isIncentivizedAvailable", tag);
            }
        }
    }
    #endif
    #endregion
}