using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WSA_10_0
public class Vungle
{
	//Change this constant fields when a new version of the plugin or sdk was released
	private const string PLUGIN_VERSION = "3.0.6";
	private const string IOS_SDK_VERSION = "3.2.0";
	private const string WIN_SDK_VERSION = "1.0.17";
	private const string ANDROID_SDK_VERSION = "3.3.3";

	#region Events

	// Fired when a Vungle ad starts
	public static event Action onAdStartedEvent;

	// Fired when a Vungle ad finishes.
	[Obsolete("Please use onAdFinishedEvent event instead this and onAdViewedEvent event.")]
	public static event Action onAdEndedEvent;
	
	// Fired when a Vungle ad is ready to be displayed
	public static event Action<bool> adPlayableEvent;

	// Fired when a Vungle ad is cached and ready to be displayed
	[Obsolete("Please use adPlayableEvent event instead this and onCachedAdAvailableEvent event.")]
	public static event Action onCachedAdAvailableEvent;

	// Fired when a Vungle video is dismissed and provides the time watched and total duration in that order.
	[Obsolete("Please use onAdFinishedEvent event instead this and onAdEndedEvent event.")]
	public static event Action<double,double> onAdViewedEvent;
	
	// Fired log event from sdk.
	public static event Action<string> onLogEvent;

	//Fired when a Vungle ad finished and provides the entire information about this event.
	public static event Action<AdFinishedEventArgs> onAdFinishedEvent; 


	static void adStarted()
	{
		if( onAdStartedEvent != null )
			onAdStartedEvent();
	}

	static void adEnded()
	{
		if( onAdEndedEvent != null )
			onAdEndedEvent();
	}

	static void videoViewed(double timeWatched, double totalDuration)
	{
		if( onAdViewedEvent != null )
			onAdViewedEvent(timeWatched, totalDuration);
	}

	static void cachedAdAvailable()
	{
		if( onCachedAdAvailableEvent != null )
			onCachedAdAvailableEvent();
	}

	static void adPlayable(bool playable)
	{
		if( adPlayableEvent != null )
			adPlayableEvent(playable);
	}
	
	static void onLog(string log)
	{
		if( onLogEvent != null )
			onLogEvent(log);
	}

	static void adFinished(AdFinishedEventArgs args)
	{
		if(onAdFinishedEvent != null)
			onAdFinishedEvent(args);
	}

	#endregion

	public static string VersionInfo
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder("unity-");
			#if UNITY_IPHONE
			return stringBuilder.Append(PLUGIN_VERSION).Append("/iOS-").Append(IOS_SDK_VERSION).ToString();
			#elif UNITY_ANDROID
			return stringBuilder.Append(PLUGIN_VERSION).Append("/android-").Append(ANDROID_SDK_VERSION).ToString();
			#elif UNITY_WSA_10_0
			return stringBuilder.Append(PLUGIN_VERSION).Append("/android-").Append(WIN_SDK_VERSION).ToString();
			#else
			return stringBuilder.Append(PLUGIN_VERSION).ToString();
			#endif
		}
	}

	static Vungle()
	{
		VungleManager.OnAdStartEvent += adStarted;
		VungleManager.OnAdEndEvent += adEnded;
		VungleManager.OnCachedAdAvailableEvent += cachedAdAvailable;
		VungleManager.OnAdPlayableEvent += adPlayable;
		VungleManager.OnVideoViewEvent += videoViewed;
		VungleManager.OnSDKLogEvent += onLog;
		VungleManager.OnAdFinishedEvent += adFinished;
	}


	// Initializes the Vungle SDK. Pass in your Android and iOS app ID's from the Vungle web portal.
	public static void init( string androidAppId, string iosAppId, string winAppId = "" )
	{
#if UNITY_IPHONE
		VungleBinding.startWithAppId( iosAppId , PLUGIN_VERSION);
#elif UNITY_ANDROID
		VungleAndroid.init( androidAppId , PLUGIN_VERSION);
#elif UNITY_WSA_10_0
		VungleWin.init(winAppId  , PLUGIN_VERSION);
#endif
	}


	// Sets if sound should be enabled or not
	public static void setSoundEnabled( bool isEnabled )
	{
#if UNITY_IPHONE
		VungleBinding.setSoundEnabled( isEnabled );
#elif UNITY_ANDROID
		VungleAndroid.setSoundEnabled( isEnabled );
#elif UNITY_WSA_10_0
		VungleWin.setSoundEnabled( isEnabled );
#endif
	}


	// Checks to see if a video is available
	public static bool isAdvertAvailable()
	{
#if UNITY_IPHONE
		return VungleBinding.isAdAvailable();
#elif UNITY_ANDROID
		return VungleAndroid.isVideoAvailable();
#elif UNITY_WSA_10_0
		return VungleWin.isVideoAvailable();
#else
		return false;
#endif
	}


	// Displays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAd( bool incentivized = false, string user = "", int orientation = 6)
	{
#if UNITY_IPHONE
		VungleBinding.playAd( incentivized, user, (VungleAdOrientation)orientation);
#elif UNITY_ANDROID
		VungleAndroid.playAd( incentivized, user );
#elif UNITY_WSA_10_0
		VungleWin.playAd( incentivized, user );
#endif
	}

	// Displays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAdWithOptions( Dictionary<string,object> options )
	{
		if(options == null)
		{
			throw new ArgumentException("You can not call this method with null parameter");
		}
		#if UNITY_IPHONE
		VungleBinding.playAdEx( options );
		#elif UNITY_ANDROID
		VungleAndroid.playAdEx( options );
		#elif UNITY_WSA_10_0
		VungleWin.playAdEx( options );
		#endif
	}
	
	// Clear cache
	public static void clearCache()
	{
		#if UNITY_IPHONE
		VungleBinding.clearCache();
		#elif UNITY_ANDROID
		//VungleAndroid.clearCache();
		#elif UNITY_WSA_10_0
		#else
		return;
		#endif
	}

	// Clear sleep
	public static void clearSleep()
	{
		#if UNITY_IPHONE
		VungleBinding.clearSleep();
		#elif UNITY_ANDROID
		#elif UNITY_WSA_10_0
		#else
		#endif
	}
	
	public static void setEndPoint(string endPoint)
	{
		#if UNITY_IPHONE
		VungleBinding.setEndPoint(endPoint);
		#elif UNITY_ANDROID
		#elif UNITY_WSA_10_0
		#else
		return;
		#endif
	}

	public static void setLogEnable(bool enable)
	{
		#if UNITY_IPHONE
		VungleBinding.enableLogging(enable);
		#elif UNITY_ANDROID
		#elif UNITY_WSA_10_0
		#else
		return;
		#endif
	}
	
	public static string getEndPoint()
	{
		#if UNITY_IPHONE
		return VungleBinding.getEndPoint();
		#elif UNITY_ANDROID
		return "";
		#elif UNITY_WSA_10_0
		return "";
		#else
		return "";
		#endif
	}
	
	public static void onResume()
	{
		#if UNITY_ANDROID
		VungleAndroid.onResume();
		#endif
	}

	public static void onPause()
	{
		#if UNITY_ANDROID
		VungleAndroid.onPause();
		#endif
	}
}

#endif
