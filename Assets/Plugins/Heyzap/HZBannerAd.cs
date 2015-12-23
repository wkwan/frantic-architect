//
//  HZBannerAd.cs
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
    /// Use this class to show banner ads.
    /// </summary>
    public class HZBannerAd : MonoBehaviour {
        public delegate void AdDisplayListener(string state, string tag);

        private static AdDisplayListener adDisplayListener;
        private static HZBannerAd _instance = null;

        // these are reproduced here for convenience since they were here in old SDK versions
        /// <summary>
        /// Set `HZBannerShowOptions.Position` to this value to show ads at the top of the screen.
        /// </summary>
        [Obsolete("This constant has been relocated to HZBannerShowOptions")]
        public const string POSITION_TOP = HZBannerShowOptions.POSITION_TOP;
        /// <summary>
        /// Set `HZBannerShowOptions.Position` to this value to show ads at the bottom of the screen.
        /// </summary>
        [Obsolete("This constant has been relocated to HZBannerShowOptions")]
        public const string POSITION_BOTTOM = HZBannerShowOptions.POSITION_BOTTOM;

        /// <summary>
        /// Shows a banner ad with the given options.
        /// </summary>
        /// <param name="showOptions">The options with which to show the banner ad, or the defaults if <c>null</c> </param>
        public static void ShowWithOptions(HZBannerShowOptions showOptions) {
            if (showOptions == null) {
                showOptions = new HZBannerShowOptions();
            }

            #if UNITY_ANDROID
            HZBannerAdAndroid.ShowWithOptions(showOptions);
            #endif
            
            #if UNITY_IPHONE && !UNITY_EDITOR
            HZBannerAdIOS.ShowWithOptions(showOptions);
            #endif
        }

        /// <summary>
        /// Gets the current banner ad's dimensions.
        /// </summary>
        /// <returns><c>true</c>, if the dimensions were successfully retrieved, <c>false</c> otherwise.</returns>
        /// <param name="banner">An out param where the dimensions of the current banner ad will be stored, if they are retrieved successfully.</param>
        public static bool GetCurrentBannerDimensions(out Rect banner){
            #if UNITY_ANDROID
            return HZBannerAdAndroid.GetCurrentBannerDimensions(out banner);

            #elif UNITY_IPHONE && !UNITY_EDITOR
            return HZBannerAdIOS.GetCurrentBannerDimensions(out banner);

            #else
            banner = new Rect(0,0,0,0);
            return false;
            #endif
        }

        /// <summary>
        /// Hides the current banner ad, if there is one, from the view. The next call to ShowWithOptions will unhide the banner ad hidden by this method.
        /// </summary>
        public static void Hide() {
            #if UNITY_ANDROID
            HZBannerAdAndroid.Hide();
            #endif

            #if UNITY_IPHONE && !UNITY_EDITOR
            HZBannerAdIOS.Hide();
            #endif
        }

        /// <summary>
        /// Destroys the current banner ad, if there is one. The next call to ShowWithOptions() will create a new banner ad.
        /// </summary>
        public static void Destroy() {
            #if UNITY_ANDROID
            HZBannerAdAndroid.Destroy();
            #endif

            #if UNITY_IPHONE && !UNITY_EDITOR
            HZBannerAdIOS.Destroy();
            #endif
        }

        /// <summary>
        /// Sets the AdDisplayListener for banner ads, which will receive callbacks regarding the state of banner ads.
        /// </summary>
        public static void SetDisplayListener(AdDisplayListener listener) {
            HZBannerAd.adDisplayListener = listener;
        }

        #region Internal methods
        public static void InitReceiver(){
            if (_instance == null) {
                GameObject receiverObject = new GameObject("HZBannerAd");
                DontDestroyOnLoad(receiverObject);
                _instance = receiverObject.AddComponent<HZBannerAd>();
            }
        }
        
        public void SetCallback(string message) {
            string[] displayStateParams = message.Split(',');
            HZBannerAd.SetCallbackStateAndTag(displayStateParams[0], displayStateParams[1]); 
        }
      
        protected static void SetCallbackStateAndTag(string state, string tag) {
            if (HZBannerAd.adDisplayListener != null) {
                HZBannerAd.adDisplayListener(state, tag);
            }
        }
        #endregion

        #region Deprecated methods
        //-------- Deprecated methods - will be removed in a future version of the SDK -------- //

        [Obsolete("Use ShowWithOptions() to show ads instead of this deprecated method.")]
        /// <summary>
        /// Shows a banner ad with the given ad tag and position.
        /// </summary>
        /// <param name="position">A string describing the screen position to place the banner ad in; should be one of HZBannerShowOptions.POSITION_TOP or HZBannerShowOptions.POSITION_BOTTOM.</param>
        /// <param name="tag">The ad tag for the banner ad to be shown.</param>
        public static void showWithTag(string position, string tag) {
            HZBannerShowOptions showOptions = new HZBannerShowOptions();
            showOptions.Position = position;
            showOptions.Tag = tag;
            HZBannerAd.ShowWithOptions(showOptions);
        }
        
        [Obsolete("Use ShowWithOptions() to show ads instead of this deprecated method.")]
        /// <summary>
        /// Shows a banner ad with the given position and the default ad tag.
        /// </summary>
        /// <param name="position">A string describing the screen position to place the banner ad in; should be one of HZBannerShowOptions.POSITION_TOP or HZBannerShowOptions.POSITION_BOTTOM.</param>
        public static void show(string position) {
            HZBannerShowOptions showOptions = new HZBannerShowOptions();
            showOptions.Position = position;
            HZBannerAd.ShowWithOptions(showOptions);
        }

        [Obsolete("Use the GetCurrentBannerDimensions(out Rect) method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static bool getCurrentBannerDimensions(out Rect banner){
            return HZBannerAd.GetCurrentBannerDimensions(out banner);
        }

        [Obsolete("Use the Hide() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void hide() {
            HZBannerAd.Hide();
        }

        [Obsolete("Use the Destroy() method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void destroy() {
            HZBannerAd.Destroy();
        }

        [Obsolete("Use the SetDisplayListener(AdDisplayListener) method instead - it uses the proper PascalCase for C#. Older versions of our SDK used incorrect casing.")]
        public static void setDisplayListener(AdDisplayListener listener) {
            HZBannerAd.SetDisplayListener(listener);
        }
        #endregion
    }

    #region Platform-specific translations
    #if UNITY_IPHONE && !UNITY_EDITOR
    public class HZBannerAdIOS : MonoBehaviour {
        [DllImport ("__Internal")]
        private static extern void hz_ads_show_banner(string position, string tag);
        [DllImport ("__Internal")]
        private static extern bool hz_ads_hide_banner();
        [DllImport ("__Internal")]
        private static extern bool hz_ads_destroy_banner();
        [DllImport ("__Internal")]
        private static extern string hz_ads_banner_dimensions();


        public static void ShowWithOptions(HZBannerShowOptions showOptions) {
            hz_ads_show_banner(showOptions.Position, showOptions.Tag);
        }

        public static bool Hide() {
            return hz_ads_hide_banner();
        }

        public static void Destroy() {
            hz_ads_destroy_banner();
        }

        public static bool GetCurrentBannerDimensions(out Rect banner){
            banner = new Rect(0,0,0,0); // default value in error cases

            string returnValue = hz_ads_banner_dimensions();
            if(returnValue == null || returnValue.Length == 0){
                return false;
            }

            string[] split = returnValue.Split(' ');
            if(split.Length != 4){
                return false;
            }

            banner = new Rect(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]), float.Parse(split[3]));
            return true;
        }
    }
    #endif

    #if UNITY_ANDROID
    public class HZBannerAdAndroid : MonoBehaviour {

        public static bool GetCurrentBannerDimensions(out Rect banner){
            banner = new Rect(0,0,0,0); // default value in error cases

            if (Application.platform != RuntimePlatform.Android) {
                return false;
            }

            AndroidJNIHelper.debug = false; 
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                string returnValue = jc.CallStatic<string>("getBannerDimensions");
                if(returnValue == null || returnValue.Length == 0){
                    return false;
                }

                string[] split = returnValue.Split(' ');
                if(split.Length != 4){
                    return false;
                }

                banner = new Rect(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3]));
                return true;
            }
        }

        public static void ShowWithOptions(HZBannerShowOptions showOptions) {
            if(Application.platform != RuntimePlatform.Android) return;

            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                jc.CallStatic("showBanner", showOptions.Tag, showOptions.Position);
            }
        }

        public static void Hide() {
            if(Application.platform != RuntimePlatform.Android) return;

            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                jc.CallStatic("hideBanner"); 
            }
        }

        public static void Destroy() {
            if(Application.platform != RuntimePlatform.Android) return;

            AndroidJNIHelper.debug = false;
            using (AndroidJavaClass jc = new AndroidJavaClass("com.heyzap.sdk.extensions.unity3d.UnityHelper")) { 
                jc.CallStatic("destroyBanner"); 
            }
        }

    }
    #endif
    #endregion
}