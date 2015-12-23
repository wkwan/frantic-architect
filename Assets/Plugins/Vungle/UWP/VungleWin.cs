using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_WSA_10_0
using VungleSDKProxy;

public enum VungleAdOrientation
{
	AutoRotate,
	MatchVideo
}

public class VungleWin
{
	private static VungleAd sdk;
	private static AdConfig cfg;
	private static bool _isSoundEnabled = true;
	private static VungleAdOrientation _orientation = VungleAdOrientation.AutoRotate;
	
	static VungleWin()
	{
	}

	// Fired when a Vungle ad starts
	public static event Action onAdStartEvent;
	
	// Fired when a Vungle ad finishes
	public static event Action onAdEndEvent;
	
	// Fired when a Vungle ad is cached and ready to be displayed
	public static event Action onCachedAdAvailableEvent;
	
	// Fired when a Vungle video is dismissed. Includes the watched and total duration in milliseconds.
	public static event Action<double,double> onVideoViewEvent;

	public static event Action<string> vungleSDKlogEvent;

	// Starts up the SDK with the given appId
	public static void init( string appId, string version )
	{
		VungleSDKConfig config = new VungleSDKConfig ();
		config.SetPluginName ("unity");
		config.SetPluginVersion (version);
		sdk = AdFactory.GetInstance(appId, config);
		sdk.addOnEvent(VungleManager.onEvent);
	}

	// Call this when your application is sent to the background
	public static void onPause()
	{
	}

	// Call this when your application resumes
	public static void onResume()
	{
	}

	// Checks to see if a video is available
	public static bool isVideoAvailable()
	{
		if (sdk != null)
			return sdk.AdPlayable;
		return false;
	}
	
	
	// Sets if sound should be enabled or not
	public static void setSoundEnabled( bool isEnabled )
	{
		_isSoundEnabled = isEnabled;
	}
	
	
	// Sets the allowed orientations of any ads that are displayed
	public static void setAdOrientation( VungleAdOrientation orientation )
	{
		_orientation = orientation;
	}
	
	
	// Checks to see if sound is enabled
	public static bool isSoundEnabled()
	{
		return _isSoundEnabled;
	}


	
	// Plays an ad with the given options. The user option is only supported for incentivized ads.
	public static void playAd( bool incentivized = false, string user = "" )
	{
		if (sdk != null && sdk.AdPlayable) {
			cfg = new AdConfig ();
			cfg.SetIncentivized (incentivized);
			cfg.SetUserId (user);
			cfg.SetSoundEnabled (_isSoundEnabled);
			cfg.SetOrientation((_orientation == VungleAdOrientation.AutoRotate)?VungleSDKProxy.DisplayOrientations.AutoRotate:VungleSDKProxy.DisplayOrientations.Landscape);
			sdk.PlayAd (cfg);
		}
	}
	
	public static void playAdEx( bool incentivized = false, int orientation = 5, bool large = false, string user = "",
	                            string alertTitle = "", string alertText = "", string closeText = "",
	                            string continueText = "")
	{
		Dictionary<string,object> options = new Dictionary<string,object> ();
		options.Add ("incentivized", incentivized);
		options.Add ("orientation", orientation);
		options.Add ("large", large);
		if (user != "")
			options.Add ("userTag", user);
		if (alertTitle != "")
			options.Add ("alertTitle", alertTitle);
		if (alertText != "")
			options.Add ("alertText", alertText);
		if (closeText != "")
			options.Add ("closeText", closeText);
		if (continueText != "")
			options.Add ("continueText", continueText);
		playAdEx(options);
	}
	
	public static void playAdEx( Dictionary<string,object> options)
	{
		if (sdk != null && sdk.AdPlayable) {
			cfg = new AdConfig ();
			if (options.ContainsKey("incentivized") && options["incentivized"] is bool)
				cfg.SetIncentivized ((bool) options["incentivized"]);
			if (options.ContainsKey("userTag") && options["userTag"] is string)
				cfg.SetUserId ((string) options["userTag"]);
			cfg.SetSoundEnabled (_isSoundEnabled);
			if (options.ContainsKey("orientation") && options["orientation"] is bool)
				cfg.SetOrientation(((bool)options["orientation"])?VungleSDKProxy.DisplayOrientations.AutoRotate:VungleSDKProxy.DisplayOrientations.Landscape);
			else
				cfg.SetOrientation((_orientation == VungleAdOrientation.AutoRotate)?VungleSDKProxy.DisplayOrientations.AutoRotate:VungleSDKProxy.DisplayOrientations.Landscape);
			if (options.ContainsKey("placement") && options["placement"] is string)
				cfg.SetPlacement ((string) options["placement"]);
			if (options.ContainsKey("alertText") && options["alertText"] is string)
				cfg.SetIncentivizedDialogBody ((string) options["alertText"]);
			if (options.ContainsKey("alertTitle") && options["alertTitle"] is string)
				cfg.SetIncentivizedDialogTitle ((string) options["alertTitle"]);
			if (options.ContainsKey("closeText") && options["closeText"] is string)
				cfg.SetIncentivizedDialogCloseButton ((string) options["closeText"]);
			if (options.ContainsKey("continueText") && options["continueText"] is string)
				cfg.SetIncentivizedDialogContinueButton ((string) options["continueText"]);
			if (options.ContainsKey("backImmediately") && options["backImmediately"] is string)
				cfg.SetBackButtonImmediatelyEnabled ((bool) options["backImmediately"]);
			string[] extra = cfg.GetExtra();
			if (options.ContainsKey("key1") && options["key1"] is string)
				extra[0] = (string) options["key1"];
			if (options.ContainsKey("key2") && options["key2"] is string)
				extra[1] = (string) options["key2"];
			if (options.ContainsKey("key3") && options["key3"] is string)
				extra[2] = (string) options["key3"];
			if (options.ContainsKey("key4") && options["key4"] is string)
				extra[3] = (string) options["key4"];
			if (options.ContainsKey("key5") && options["key5"] is string)
				extra[4] = (string) options["key5"];
			if (options.ContainsKey("key6") && options["key6"] is string)
				extra[5] = (string) options["key6"];
			if (options.ContainsKey("key7") && options["key7"] is string)
				extra[6] = (string) options["key7"];
			if (options.ContainsKey("key8") && options["key8"] is string)
				extra[7] = (string) options["key8"];
			sdk.PlayAd (cfg);
		}
	}
}
#endif

