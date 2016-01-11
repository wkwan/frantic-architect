using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace ChartboostSDK {
	public class CBExternal {
		private static bool initialized = false;
		private static string _logTag = "ChartboostSDK";
		
		public static void Log (string message) {
			if(CBSettings.isLogging() && Debug.isDebugBuild)
				Debug.Log(_logTag + "/" + message);
		}

		private static bool checkInitialized() {
			if (initialized) {
				return true;
			} else {
				Debug.LogError("The Chartboost SDK needs to be initialized before we can show any ads");
				return false;
			}
		}

#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IPHONE)

		/// Initializes the Chartboost plugin.
		/// This must be called before using any other Chartboost features.
		public static void init() {
			Log("Unity : init with version " + Application.unityVersion);
			
			// Will verify all the id and signatures against example ones.
			CBSettings.getIOSAppId ();
			CBSettings.getIOSAppSecret ();
			CBSettings.getAndroidAppId ();
			CBSettings.getAndroidAppSecret ();
			CBSettings.getAmazonAppId ();
			CBSettings.getAmazonAppSecret ();
		}
		
		/// Caches an interstitial. 
		public static void cacheInterstitial(CBLocation location) {
			Log("Unity : cacheInterstitial at location = " + location.ToString());
		}
		
		/// Checks for a cached an interstitial. 
		public static bool hasInterstitial(CBLocation location) {
			Log("Unity : hasInterstitial at location = " + location.ToString());
			return false;
		}
		
		/// Loads an interstitial. 
		public static void showInterstitial(CBLocation location) {
			Log("Unity : showInterstitial at location = " + location.ToString());
		}
		
		/// Caches the more apps screen. 
		public static void cacheMoreApps(CBLocation location) {
			Log("Unity : cacheMoreApps at location = " + location.ToString());
		}
		
		/// Checks to see if the more apps screen is cached. 
		public static bool hasMoreApps(CBLocation location) {
			Log("Unity : hasMoreApps at location = " + location.ToString());
			return false;
		}
		
		/// Shows the more apps screen. 
		public static void showMoreApps(CBLocation location) {
			Log("Unity : showMoreApps at location = " + location.ToString());
		}
		
		public static void cacheInPlay(CBLocation location) {
			Log("Unity : cacheInPlay at location = " + location.ToString());
		}
		
		public static bool hasInPlay(CBLocation location) {
			Log("Unity : hasInPlay at location = " + location.ToString());
			return false;
		}
		
		public static CBInPlay getInPlay(CBLocation location) {
			Log("Unity : getInPlay at location = " + location.ToString());
			return null;
		}
		
		/// Caches a rewarded video. 
		public static void cacheRewardedVideo(CBLocation location) {
			Log("Unity : cacheRewardedVideo at location = " + location.ToString());
		}
		
		
		/// Checks for a cached a rewarded video. 
		public static bool hasRewardedVideo(CBLocation location) {
			Log("Unity : hasRewardedVideo at location = " + location.ToString());
			return false;
		}
		
		/// Loads a rewarded video. 
		public static void showRewardedVideo(CBLocation location) {
			Log("Unity : showRewardedVideo at location = " + location.ToString());
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayInterstitial
		public static void chartBoostShouldDisplayInterstitialCallbackResult(bool result) {
			Log("Unity : chartBoostShouldDisplayInterstitialCallbackResult");
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayRewardedVideo
		public static void chartBoostShouldDisplayRewardedVideoCallbackResult(bool result) {
			Log("Unity : chartBoostShouldDisplayRewardedVideoCallbackResult");
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayMoreApps
		public static void chartBoostShouldDisplayMoreAppsCallbackResult(bool result) {
			Log("Unity : chartBoostShouldDisplayMoreAppsCallbackResult");
		}
		
		/// Sets the name of the game object to be used by the Chartboost iOS SDK
		public static void setGameObjectName(string name) {
			Log("Unity : Set Game object name for callbacks to = " + name);
		}
		
		/// Set the custom id used for rewarded video call
		public static void setCustomId(string id) {
			Log("Unity : setCustomId to = " + id);
		}
		
		/// Get the custom id used for rewarded video call
		public static string getCustomId() {
			Log("Unity : getCustomId");
			return "";
		}
		
		/// Confirm if an age gate passed or failed. When specified
		/// Chartboost will wait for this call before showing the ios app store
		public static void didPassAgeGate(bool pass) {
			Log("Unity : didPassAgeGate with value = " + pass);
		}
		
		/// Open a URL using a Chartboost Custom Scheme
		public static void handleOpenURL(string url, string sourceApp) {
			Log("Unity : handleOpenURL at url = " + url + " for app = " + sourceApp);
		}
		
		/// Set to true if you would like to implement confirmation for ad clicks, such as an age gate.
		/// If using this feature, you should call CBBinding.didPassAgeGate() in your didClickInterstitial.
		public static void setShouldPauseClickForConfirmation(bool pause) {
			Log("Unity : setShouldPauseClickForConfirmation with value = " + pause);
		}
		
		/// Set to false if you want interstitials to be disabled in the first user session
		public static void setShouldRequestInterstitialsInFirstSession(bool request) {
			Log("Unity : setShouldRequestInterstitialsInFirstSession with value = " + request);
		}
		
		public static bool getAutoCacheAds() {
			Log("Unity : getAutoCacheAds");
			return false;
		}
		
		public static void setAutoCacheAds(bool autoCacheAds) {
			Log("Unity : setAutoCacheAds with value = " + autoCacheAds);
		}

		public static void setStatusBarBehavior(CBStatusBarBehavior statusBarBehavior) {
			Log("Unity : setStatusBarBehavior with value = " + statusBarBehavior);
		}
		
		public static void setShouldDisplayLoadingViewForMoreApps(bool shouldDisplay) {
			Log("Unity : setShouldDisplayLoadingViewForMoreApps with value = " + shouldDisplay);
		}
		
		public static void setShouldPrefetchVideoContent(bool shouldPrefetch) {
			Log("Unity : setShouldPrefetchVideoContent with value = " + shouldPrefetch);
		}

		public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, int subLevel, String description) {
			int levelType = (int)type;
			Log(String.Format("Unity : PIA Level Tracking:\n\teventLabel = {0}\n\ttype = {1}\n\tmainLevel = {2}\n\tsubLevel = {3}\n\tdescription = {4}", eventLabel, levelType, mainLevel, subLevel, description));
		}

        public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, String description) {
        	int levelType = (int)type;
            Log(String.Format("Unity : PIA Level Tracking:\n\teventLabel = {0}\n\ttype = {1}\n\tmainLevel = {2}\n\tdescription = {3}", eventLabel, levelType, mainLevel, description));
        }

		public static void pause(bool paused) {
			Log("Unity : pause");
		}
		
		/// Shuts down the Chartboost plugin
		public static void destroy() {
			Log("Unity : destroy");
		}
		
		/// Used to notify Chartboost that the Android back button has been pressed
		/// Returns true to indicate that Chartboost has handled the event and it should not be further processed
		public static bool onBackPressed() {
			Log("Unity : onBackPressed");
			return true;
		}

		public static void trackInAppGooglePlayPurchaseEvent(string title, string description, string price, string currency, string productID, string purchaseData, string purchaseSignature) {
			Log("Unity: trackInAppGooglePlayPurchaseEvent");
		}
		
		public static void trackInAppAmazonStorePurchaseEvent(string title, string description, string price, string currency, string productID, string userID, string purchaseToken) {
			Log("Unity: trackInAppAmazonStorePurchaseEvent");
		}
		
		public static void trackInAppAppleStorePurchaseEvent(string receipt, string productTitle, string productDescription, string productPrice, string productCurrency, string productIdentifier) {
			Log("Unity : trackInAppAppleStorePurchaseEvent");
		}

		public static bool isAnyViewVisible() {
			Log("Unity : isAnyViewVisible");
			return false;
		}
		
#elif UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern void _chartBoostInit(string appId, string appSignature, string unityVersion);
		[DllImport("__Internal")]
		private static extern bool _chartBoostIsAnyViewVisible();
		[DllImport("__Internal")]
		private static extern void _chartBoostCacheInterstitial(string location);
		[DllImport("__Internal")]
		private static extern bool _chartBoostHasInterstitial(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostShowInterstitial(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostCacheRewardedVideo(string location);
		[DllImport("__Internal")]
		private static extern bool _chartBoostHasRewardedVideo(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostShowRewardedVideo(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostCacheMoreApps(string location);
		[DllImport("__Internal")]
		private static extern bool _chartBoostHasMoreApps(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostShowMoreApps(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostCacheInPlay(string location);
		[DllImport("__Internal")]
		private static extern bool _chartBoostHasInPlay(string location);
		[DllImport("__Internal")]
		private static extern IntPtr _chartBoostGetInPlay(string location);
		[DllImport("__Internal")]
		private static extern void _chartBoostSetCustomId(string id);
		[DllImport("__Internal")]
		private static extern void _chartBoostDidPassAgeGate(bool pass);
		[DllImport("__Internal")]
		private static extern string _chartBoostGetCustomId();
		[DllImport("__Internal")]
		private static extern void _chartBoostHandleOpenURL(string url, string sourceApp);
		[DllImport("__Internal")]
		private static extern void _chartBoostSetShouldPauseClickForConfirmation(bool pause);
		[DllImport("__Internal")]
		private static extern void _chartBoostSetShouldRequestInterstitialsInFirstSession(bool request);
		[DllImport("__Internal")]
		private static extern void _chartBoostShouldDisplayInterstitialCallbackResult(bool result);
		[DllImport("__Internal")]
		private static extern void _chartBoostShouldDisplayRewardedVideoCallbackResult(bool result);
		[DllImport("__Internal")]
		private static extern void _chartBoostShouldDisplayMoreAppsCallbackResult(bool result);
		[DllImport("__Internal")]
		private static extern bool _chartBoostGetAutoCacheAds();
		[DllImport("__Internal")]
		private static extern void _chartBoostSetAutoCacheAds(bool autoCacheAds);
		[DllImport("__Internal")]
		private static extern void _chartBoostSetShouldDisplayLoadingViewForMoreApps(bool shouldDisplay);		
		[DllImport("__Internal")]
		private static extern void _chartBoostSetShouldPrefetchVideoContent(bool shouldDisplay);
		[DllImport("__Internal")]
		private static extern void _chartBoostTrackInAppPurchaseEvent(string receipt, string productTitle, string productDescription, string productPrice, string productCurrency, string productIdentifier);
		[DllImport("__Internal")]
		private static extern void _chartBoostSetGameObjectName(string name);
		[DllImport("__Internal")]
		private static extern void _chartBoostSetStatusBarBehavior(CBStatusBarBehavior statusBarBehavior);
		[DllImport("__Internal")]
		private static extern void _chartBoostTrackLevelInfo(String eventLabel, int levelType, int mainLevel, int subLevel, String description);
		
		/// Initializes the Chartboost plugin.
		/// This must be called before using any other Chartboost features.
		public static void init() {
			// get the AppID and AppSecret from CBSettings
			string appID = CBSettings.getIOSAppId ();
			string appSecret = CBSettings.getIOSAppSecret ();
			
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				_chartBoostInit(appID, appSecret, Application.unityVersion);
			initialized = true;
		}

		/// Shuts down the Chartboost plugin
		public static void destroy() {
			Log("Unity : destroy");
		}
		

		/// Check to see if any chartboost ad or view is visible
		public static bool isAnyViewVisible() {
			bool handled = false;
			if (!checkInitialized())
				return handled;
			
			handled = _chartBoostIsAnyViewVisible();
			Log("iOS : isAnyViewVisible = " + handled );
			
			return handled;
		}

		/// Caches an interstitial. 
		public static void cacheInterstitial(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_chartBoostCacheInterstitial(location.ToString());
			Log("iOS : cacheInterstitial at location = " + location.ToString());
		}
		
		/// Checks for a cached an interstitial. 
		public static bool hasInterstitial(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("iOS : hasInterstitial at location = " + location.ToString());
			return _chartBoostHasInterstitial(location.ToString());
		}
		
		/// Loads an interstitial.
		public static void showInterstitial(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_chartBoostShowInterstitial(location.ToString());
			Log("iOS : showInterstitial at location = " + location.ToString());
		}
		
		/// Caches the more apps screen.
		public static void cacheMoreApps(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_chartBoostCacheMoreApps(location.ToString());
			Log("iOS : cacheMoreApps at location = " + location.ToString());
		}
		
		/// Checks to see if the more apps screen is cached. 
		public static bool hasMoreApps(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("iOS : hasMoreApps at location = " + location.ToString());
			return _chartBoostHasMoreApps(location.ToString());
		}
		
		/// Shows the more apps screen.
		public static void showMoreApps(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_chartBoostShowMoreApps(location.ToString());
			Log("iOS : showMoreApps at location = " + location.ToString());
		}
		
		public static void cacheInPlay(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_chartBoostCacheInPlay(location.ToString());
			Log("iOS : cacheInPlay at location = " + location.ToString());
		}
		
		public static bool hasInPlay(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("iOS : hasInPlay at location = " + location.ToString());
			return _chartBoostHasInPlay(location.ToString());
		}
		
		public static CBInPlay getInPlay(CBLocation location) {
			if (!checkInitialized())
				return null;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return null;
			}
			
			IntPtr inPlayId = _chartBoostGetInPlay(location.ToString());
			// No Inplay was available right now
			if(inPlayId == IntPtr.Zero) {
				return null;
			}
			
			CBInPlay inPlayAd = new CBInPlay(inPlayId);
			Log("iOS : getInPlay at location = " + location.ToString());
			return inPlayAd;
		}
		
		/// Caches a rewarded video. 
		public static void cacheRewardedVideo(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_chartBoostCacheRewardedVideo(location.ToString());
			Log("iOS : cacheRewardedVideo at location = " + location.ToString());
		}
		
		
		/// Checks for a cached a rewarded video. 
		public static bool hasRewardedVideo(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("iOS : hasRewardedVideo at location = " + location.ToString());
			return _chartBoostHasRewardedVideo(location.ToString());
		}
		
		/// Loads a rewarded video. 
		public static void showRewardedVideo(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_chartBoostShowRewardedVideo(location.ToString());
			Log("iOS : showRewardedVideo at location = " + location.ToString());
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayInterstitial
		public static void chartBoostShouldDisplayInterstitialCallbackResult(bool result) {
			if (!checkInitialized())
				return;
			_chartBoostShouldDisplayInterstitialCallbackResult(result);
			Log("iOS : chartBoostShouldDisplayInterstitialCallbackResult");
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayRewardedVideo
		public static void chartBoostShouldDisplayRewardedVideoCallbackResult(bool result) {
			if (!checkInitialized())
				return;
			_chartBoostShouldDisplayRewardedVideoCallbackResult(result);
			Log("iOS : chartBoostShouldDisplayRewardedVideoCallbackResult");
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayMoreApps
		public static void chartBoostShouldDisplayMoreAppsCallbackResult(bool result) {
			if (!checkInitialized())
				return;
			_chartBoostShouldDisplayMoreAppsCallbackResult(result);
			Log("iOS : chartBoostShouldDisplayMoreAppsCallbackResult");
		}
		
		/// Set the custom id used for rewarded video call
		public static void setCustomId(string id) {
			if (!checkInitialized())
				return;
			_chartBoostSetCustomId(id);
			Log("iOS : setCustomId to = " + id);
		}
		
		/// Get the custom id used for rewarded video call
		public static string getCustomId() {
			if (!checkInitialized())
				return null;
			Log("iOS : getCustomId");
			return _chartBoostGetCustomId();
		}
		
		/// Confirm if an age gate passed or failed. When specified
		/// Chartboost will wait for this call before showing the ios app store
		public static void didPassAgeGate(bool pass) {
			if (!checkInitialized())
				return;
			_chartBoostDidPassAgeGate(pass);
			Log("iOS : didPassAgeGate with value = " + pass);
		}
		
		/// Open a URL using a Chartboost Custom Scheme
		public static void handleOpenURL(string url, string sourceApp) {
			if (!checkInitialized())
				return;
			_chartBoostHandleOpenURL(url, sourceApp);
			Log("iOS : handleOpenURL at url = " + url + " for app = " + sourceApp);
		}
		
		/// Set to true if you would like to implement confirmation for ad clicks, such as an age gate.
		/// If using this feature, you should call CBBinding.didPassAgeGate() in your didClickInterstitial.
		public static void setShouldPauseClickForConfirmation(bool pause) {
			if (!checkInitialized())
				return;
			_chartBoostSetShouldPauseClickForConfirmation(pause);
			Log("iOS : setShouldPauseClickForConfirmation with value = " + pause);
		}
		
		/// Set to false if you want interstitials to be disabled in the first user session
		public static void setShouldRequestInterstitialsInFirstSession(bool request) {
			if (!checkInitialized())
				return;
			_chartBoostSetShouldRequestInterstitialsInFirstSession(request);
			Log("iOS : setShouldRequestInterstitialsInFirstSession with value = " + request);
		}
		
		/// Sets the name of the game object to be used by the Chartboost iOS SDK
		public static void setGameObjectName(string name) {
			_chartBoostSetGameObjectName(name);
			Log("iOS : Set Game object name for callbacks to = " + name);
		}
		
		/// Check if we are autocaching ads after every show call
		public static bool getAutoCacheAds() {
			Log("iOS : getAutoCacheAds");
			return _chartBoostGetAutoCacheAds();
		}
		
		/// Sets whether to autocache after every show call
		public static void setAutoCacheAds(bool autoCacheAds) {
			_chartBoostSetAutoCacheAds(autoCacheAds);
			Log("iOS : Set AutoCacheAds to = " + autoCacheAds);
		}

		/// Sets whether to autocache after every show call
		public static void setStatusBarBehavior(CBStatusBarBehavior statusBarBehavior) {
			_chartBoostSetStatusBarBehavior(statusBarBehavior);
			Log("iOS : Set StatusBarBehavior to = " + statusBarBehavior);
		}
		
		/// Sets whether to display loading view for moreapps
		public static void setShouldDisplayLoadingViewForMoreApps(bool shouldDisplay) {
			_chartBoostSetShouldDisplayLoadingViewForMoreApps(shouldDisplay);
			Log("iOS : Set Should Display Loading View for More Apps to = " + shouldDisplay);
		}
		
		/// Sets whether to prefetch videos or not
		public static void setShouldPrefetchVideoContent(bool shouldPrefetch) {
			_chartBoostSetShouldPrefetchVideoContent(shouldPrefetch);
			Log("iOS : Set setShouldPrefetchVideoContent to = " + shouldPrefetch);
		}

		/// PIA Level Tracking info call
		public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, int subLevel, String description) {
			int levelType = (int)type;
			_chartBoostTrackLevelInfo(eventLabel, levelType, mainLevel, subLevel, description);
			
			Log(String.Format("iOS : PIA Level Tracking:\n\teventLabel = {0}\n\ttype = {1}\n\tmainLevel = {2}\n\tsubLevel = {3}\n\tdescription = {4}", eventLabel, levelType, mainLevel, subLevel, description));
		}

        /// PIA Level Tracking info call
        public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, String description) {
            int levelType = (int)type;
            _chartBoostTrackLevelInfo(eventLabel, levelType, mainLevel, 0, description);
            
            Log(String.Format("iOS : PIA Level Tracking:\n\teventLabel = {0}\n\ttype = {1}\n\tmainLevel = {2}\n\tdescription = {3}", eventLabel, levelType, mainLevel, description));
        }

		/// IAP Tracking call
		public static void trackInAppAppleStorePurchaseEvent(string receipt, string productTitle, string productDescription, string productPrice, string productCurrency, string productIdentifier) {
			_chartBoostTrackInAppPurchaseEvent(receipt, productTitle, productDescription, productPrice, productCurrency, productIdentifier);
			Log("iOS : trackInAppAppleStorePurchaseEvent");
		}
		
		
#elif UNITY_ANDROID
		private static AndroidJavaObject _plugin;
		
		/// Initialize the android sdk
		public static void init() {
			// get the AppID and AppSecret from CBSettings
			string appID = CBSettings.getSelectAndroidAppId ();
			string appSecret = CBSettings.getSelectAndroidAppSecret ();
			string unityVersion = Application.unityVersion;
			
			// find the plugin instance
			using (var pluginClass = new AndroidJavaClass("com.chartboost.sdk.unity.CBPlugin"))
				_plugin = pluginClass.CallStatic<AndroidJavaObject>("instance");
			_plugin.Call("init", appID, appSecret, unityVersion);
			initialized = true;
		}

		/// Check to see if any chartboost ad or view is visible
		public static bool isAnyViewVisible() {
			bool handled = false;
			if (!checkInitialized())
				return handled;

			handled = _plugin.Call<bool>("isAnyViewVisible");
			Log("Android : isAnyViewVisible = " + handled );

			return handled;
		}

		/// Caches an interstitial. 
		public static void cacheInterstitial(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_plugin.Call("cacheInterstitial", location.ToString());
			Log("Android : cacheInterstitial at location = " + location.ToString());
		}
		
		/// Checks for a cached an interstitial. 
		public static bool hasInterstitial(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("Android : hasInterstitial at location = " + location.ToString());
			return _plugin.Call<bool>("hasInterstitial", location.ToString());
		}
		
		/// Loads an interstitial. 
		public static void showInterstitial(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_plugin.Call("showInterstitial", location.ToString());
			Log("Android : showInterstitial at location = " + location.ToString());
		}
		
		/// Caches the more apps screen. 
		public static void cacheMoreApps(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			};
			_plugin.Call("cacheMoreApps", location.ToString());
			Log("Android : cacheMoreApps at location = " + location.ToString());
		}
		
		/// Checks to see if the more apps screen is cached. 
		public static bool hasMoreApps(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("Android : hasMoreApps at location = " + location.ToString());
			return _plugin.Call<bool>("hasMoreApps", location.ToString());
		}
		
		/// Shows the more apps screen. 
		public static void showMoreApps(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_plugin.Call("showMoreApps", location.ToString());
			Log("Android : showMoreApps at location = " + location.ToString());
		}
		
		public static void cacheInPlay(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_plugin.Call("cacheInPlay", location.ToString());
			Log("Android : cacheInPlay at location = " + location.ToString());
		}
		
		public static bool hasInPlay(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("Android : hasInPlay at location = " + location.ToString());
			return _plugin.Call<bool>("hasCachedInPlay", location.ToString());
		}
		
		public static CBInPlay getInPlay(CBLocation location) {
			Log("Android : getInPlay at location = " + location.ToString());
			if (!checkInitialized())
				return null;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return null;
			}
			try 
			{
				AndroidJavaObject androidInPlayAd = _plugin.Call<AndroidJavaObject>("getInPlay", location.ToString());
				CBInPlay inPlayAd = new CBInPlay(androidInPlayAd, _plugin);
				return inPlayAd;
			}
			catch
			{
				return null;
			}
		}
		
		/// Caches a rewarded video. 
		public static void cacheRewardedVideo(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_plugin.Call("cacheRewardedVideo", location.ToString());
			Log("Android : cacheRewardedVideo at location = " + location.ToString());
		}
		
		/// Checks for a cached a rewarded video. 
		public static bool hasRewardedVideo(CBLocation location) {
			if (!checkInitialized())
				return false;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return false;
			}
			Log("Android : hasRewardedVideo at location = " + location.ToString());
			return _plugin.Call<bool>("hasRewardedVideo", location.ToString());
		}
		
		/// Loads a rewarded video. 
		public static void showRewardedVideo(CBLocation location) {
			if (!checkInitialized())
				return;
			else if(location == null) {
				Debug.LogError("Chartboost SDK: location passed is null cannot perform the operation requested");
				return;
			}
			_plugin.Call("showRewardedVideo", location.ToString());
			Log("Android : showRewardedVideo at location = " + location.ToString());
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayInterstitial
		public static void chartBoostShouldDisplayInterstitialCallbackResult(bool result) {
			if (!checkInitialized())
				return;
			_plugin.Call("chartBoostShouldDisplayInterstitialCallbackResult", result);
			Log("Android : chartBoostShouldDisplayInterstitialCallbackResult");
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayRewardedVideo
		public static void chartBoostShouldDisplayRewardedVideoCallbackResult(bool result) {
			if (!checkInitialized())
				return;
			_plugin.Call("chartBoostShouldDisplayRewardedVideoCallbackResult", result);
			Log("Android : chartBoostShouldDisplayRewardedVideoCallbackResult");
		}
		
		// Sends back the reponse by the delegate call for ShouldDisplayMoreApps
		public static void chartBoostShouldDisplayMoreAppsCallbackResult(bool result) {
			if (!checkInitialized())
				return;
			_plugin.Call("chartBoostShouldDisplayMoreAppsCallbackResult", result);
			Log("Android : chartBoostShouldDisplayMoreAppsCallbackResult");
		}
		
		public static void didPassAgeGate(bool pass) {
			_plugin.Call ("didPassAgeGate",pass);
		}
		
		public static void setShouldPauseClickForConfirmation(bool shouldPause) {
			_plugin.Call ("setShouldPauseClickForConfirmation",shouldPause);
		}
		
		public static String getCustomId() {
			return _plugin.Call<String>("getCustomId");
		}
		
		public static void setCustomId(String customId) {
			_plugin.Call("setCustomId", customId);
		}
		
		public static bool getAutoCacheAds() {
			return _plugin.Call<bool>("getAutoCacheAds");
		}
		
		public static void setAutoCacheAds(bool autoCacheAds) {
			_plugin.Call ("setAutoCacheAds", autoCacheAds);
		}
		
		public static void setShouldRequestInterstitialsInFirstSession(bool shouldRequest) {
			_plugin.Call ("setShouldRequestInterstitialsInFirstSession", shouldRequest);
		}
		
		public static void setShouldDisplayLoadingViewForMoreApps(bool shouldDisplay) {
			_plugin.Call ("setShouldDisplayLoadingViewForMoreApps", shouldDisplay);
		}
		
		public static void setShouldPrefetchVideoContent(bool shouldPrefetch) {
			_plugin.Call ("setShouldPrefetchVideoContent", shouldPrefetch);
		}

		/// PIA Level Tracking info call
		public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, int subLevel, String description) {;
			int levelType = (int)type;
			_plugin.Call ("trackLevelInfo", eventLabel, levelType, mainLevel, subLevel, description);

			Log(String.Format("Android : PIA Level Tracking:\n\teventLabel = {0}\n\ttype = {1}\n\tmainLevel = {2}\n\tsubLevel = {3}\n\tdescription = {4}", eventLabel, levelType, mainLevel, subLevel, description));
		}

        /// PIA Level Tracking info call
        public static void trackLevelInfo(String eventLabel, CBLevelType type, int mainLevel, String description) {;
            int levelType = (int)type;
            _plugin.Call ("trackLevelInfo", eventLabel, levelType, mainLevel, description);

            Log(String.Format("Android : PIA Level Tracking:\n\teventLabel = {0}\n\ttype = {1}\n\tmainLevel = {2}\n\tdescription = {3}", eventLabel, levelType, mainLevel, description));
        }

		/// Sets the name of the game object to be used by the Chartboost Android SDK
		public static void setGameObjectName(string name) {
			_plugin.Call("setGameObjectName", name);
		}
		
		/// Informs the Chartboost SDK about the lifecycle of your app
		public static void pause(bool paused) {
			if (!checkInitialized())
				return;
			
			_plugin.Call("pause", paused);
			Log("Android : pause");
		}
		
		/// Shuts down the Chartboost plugin
		public static void destroy() {
			if (!checkInitialized())
				return;
			
			_plugin.Call("destroy");
			initialized = false;
			Log("Android : destroy");
		}
		
		/// Used to notify Chartboost that the Android back button has been pressed
		/// Returns true to indicate that Chartboost has handled the event and it should not be further processed
		public static bool onBackPressed() {
			bool handled = false;
			if (!checkInitialized())
				return false;
			
			handled = _plugin.Call<bool>("onBackPressed");
			Log("Android : onBackPressed");
			return handled;
		}

		public static void trackInAppGooglePlayPurchaseEvent(string title, string description, string price, string currency, string productID, string purchaseData, string purchaseSignature) {
			Log("Android: trackInAppGooglePlayPurchaseEvent");
			_plugin.Call("trackInAppGooglePlayPurchaseEvent", title,description,price,currency,productID,purchaseData,purchaseSignature);
		}
		
		public static void trackInAppAmazonStorePurchaseEvent(string title, string description, string price, string currency, string productID, string userID, string purchaseToken) {
			Log("Android: trackInAppAmazonStorePurchaseEvent");
			_plugin.Call("trackInAppAmazonStorePurchaseEvent", title,description,price,currency,productID,userID,purchaseToken);
		}
		
#endif
	}
}

