using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID
public enum VungleGender
{
	None = -1,
	Male = 0,
	Female
}


public enum VungleAdOrientation
{
	AutoRotate,
    MatchVideo
}

public class VungleAndroid
{
	private static AndroidJavaObject _plugin;

	static VungleAndroid()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		VungleManager.noop();

		using( var pluginClass = new AndroidJavaClass( "com.vungle.VunglePlugin" ) )
			_plugin = pluginClass.CallStatic<AndroidJavaObject>( "instance" );
	}


	// Starts up the SDK with the given appId
	public static void init( string appId, string pluginVersion)
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_plugin.Call( "init", appId , pluginVersion);
	}


	// Call this when your application is sent to the background
	public static void onPause()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_plugin.Call( "onPause" );
	}


	// Call this when your application resumes
	public static void onResume()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_plugin.Call( "onResume" );
	}


	// Checks to see if a video is available
	public static bool isVideoAvailable()
	{
		if( Application.platform != RuntimePlatform.Android )
			return false;

		return _plugin.Call<bool>( "isVideoAvailable" );
	}


	// Sets if sound should be enabled or not
	public static void setSoundEnabled( bool isEnabled )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_plugin.Call( "setSoundEnabled", isEnabled );
	}


	// Sets the allowed orientations of any ads that are displayed
	public static void setAdOrientation( VungleAdOrientation orientation )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		_plugin.Call( "setAdOrientation", (int)orientation );
	}


	// Checks to see if sound is enabled
	public static bool isSoundEnabled()
	{
		if( Application.platform != RuntimePlatform.Android )
			return true;

		return _plugin.Call<bool>( "isSoundEnabled" );
	}


	// Plays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAd( bool incentivized = false, string user = "" )
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		if( user == null )
			user = string.Empty;

		_plugin.Call( "playAd", incentivized, user );
	}

	public static void playAdEx( bool incentivized = false, int orientation = 5, bool large = false, string user = "",
	                      string alerTitle = "", string alertText = "", string alertClose = "",
	                      string alertContinue = "")
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
		
		if( user == null )
			user = string.Empty;
		if( alerTitle == null )
			alerTitle = string.Empty;
		if( alertText == null )
			alertText = string.Empty;
		if( alertClose == null )
			alertClose = string.Empty;
		if( alertContinue == null )
			alertContinue = string.Empty;

		_plugin.Call( "playAdEx", incentivized, orientation, large, user,
		             alerTitle, alertText, alertClose, alertContinue );
	}

	public static void playAdEx( Dictionary<string,object> options)
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
		
		_plugin.Call( "playAdEx", MiniJSONV.Json.Serialize(options) );
	}
}
#endif

