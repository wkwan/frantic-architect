using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;



#if UNITY_IPHONE
public enum VungleAdOrientation
{
	Portrait = 1,
    LandscapeLeft = 2,
    LandscapeRight = 3,
    PortraitUpsideDown = 4,
    Landscape = 5,
    All = 6,
    AllButUpsideDown = 7
}

public class VungleBinding
{
	static VungleBinding()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			VungleManager.noop();
	}


	[DllImport("__Internal")]
	private static extern void _vungleStartWithAppId( string appId, string pluginVersion );

	// Starts up the SDK with the given appId
	public static void startWithAppId( string appId, string pluginVersion )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleStartWithAppId( appId, pluginVersion );
	}


	[DllImport("__Internal")]
	private static extern void _vungleSetSoundEnabled( bool enabled );

	// Enables/disables sound
	public static void setSoundEnabled( bool enabled )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleSetSoundEnabled( enabled );
	}


	[DllImport("__Internal")]
	private static extern void _vungleEnableLogging( bool shouldEnable );

	// Enables/disables verbose logging
	public static void enableLogging( bool shouldEnable )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleEnableLogging( shouldEnable );
	}

	[DllImport("__Internal")]
	private static extern bool _vungleIsAdAvailable();

	// Checks to see if a video ad is available
	public static bool isAdAvailable()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _vungleIsAdAvailable();
		return false;
	}


	[DllImport("__Internal")]
	private static extern void _vunglePlayAdWithOptions( bool incentivized, int orientation, string user );

	[DllImport("__Internal")]
	private static extern void _vunglePlayAdEx( bool incentivized, int orientation, bool large, string user,
	                                                      string alerTitle, string alertText, string alertClose,
	                                                      string alertContinue );
	
	[DllImport("__Internal")]
	private static extern void _vunglePlayAdWithOptionsEx( string options );
	
	// Plays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAd( bool incentivized = false, string user = "", VungleAdOrientation orientation = VungleAdOrientation.All )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vunglePlayAdWithOptions( incentivized, (int)orientation, user );
	}

	// Plays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAdEx( bool incentivized = false, int orientation = 5, bool large = false, string user = "",
	                             string alerTitle = "", string alertText = "", string alertClose = "",
	                             string alertContinue = "" )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vunglePlayAdEx( incentivized, orientation, large, user,
			                            alerTitle, alertText, alertClose,
			                            alertContinue );
	}
	
	// Plays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAdEx( Dictionary<string,object> options )
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vunglePlayAdWithOptionsEx( MiniJSONV.Json.Serialize(options) );
	}
	
	[DllImport("__Internal")]
	private static extern void _vungleClearCache();
	
	public static void clearCache()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleClearCache();
	}
	
	[DllImport("__Internal")]
	private static extern void _vungleClearSleep();
	
	public static void clearSleep()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleClearSleep();
	}

	[DllImport("__Internal")]
	private static extern void _vungleSetEndPoint(string endPoint);
	
	public static void setEndPoint(string endPoint)
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			_vungleSetEndPoint(endPoint);
	}

	[DllImport("__Internal")]
	private static extern string _vungleGetEndPoint();
	
	public static string getEndPoint()
	{
		if( Application.platform == RuntimePlatform.IPhonePlayer )
			return _vungleGetEndPoint();
		return "";
	}
}
#endif
