using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VungleManager : MonoBehaviour
{
	//for android we grab the infromation for OnAdFinishedEvent from two internal SDK events: onAdEnd and onVideoView.
	private AdFinishedEventArgs adFinishedEventArgs = null;
	private static AdFinishedEventArgs adWinFinishedEventArgs = null;

	#region Constructor and Lifecycle

	static VungleManager()
	{
		// try/catch this so that we can warn users if they try to stick this script on a GO manually
		try
		{
			// create a new GO for our manager
			var go = new GameObject( "VungleManager" );
			go.AddComponent<VungleManager>();
			DontDestroyOnLoad( go );
		}
		catch( UnityException )
		{
			Debug.LogWarning( "It looks like you have the VungleManager on a GameObject in your scene. Please remove the script from your scene." );
		}
	}


	// used to ensure the VungleManager will always be in the scene to avoid SendMessage logs if the user isn't using any events
	public static void noop(){}

	#endregion
	
	// Fired when the video is shown
	public static event Action OnAdStartEvent;

	// Fired when a Vungle ad is cached and ready to be displayed
	[Obsolete("Please use OnAdPlayable instead.")]
	public static event Action OnCachedAdAvailableEvent;

	// Fired when a Vungle ad is ready to be displayed
	public static event Action<bool> OnAdPlayableEvent;

	// Fired when a Vungle ad finishes.
	[Obsolete("Please use OnAdFinishedEvent instead.")]
	public static event Action OnAdEndEvent;
	
	// Fired when a video has finished playing
	[Obsolete("Please use OnAdFinishedEvent instead.")]
	public static event Action<double,double> OnVideoViewEvent;

	// Fired when a Vungle write log (implemented only for iOS)
	public static event Action<string> OnSDKLogEvent;

	//Fired when a Vungle ad finished and provides the entire information about this event.
	public static event Action<AdFinishedEventArgs> OnAdFinishedEvent;


	public static void onEvent(string e, string arg) {
		if (e == "OnAdStart") {
			OnAdStartEvent();
		}
		if (e == "OnAdEnd") {
			bool fireRightNow = adWinFinishedEventArgs != null;
			
			if(!fireRightNow)
			{
				adWinFinishedEventArgs = new AdFinishedEventArgs();
			}
			
			adWinFinishedEventArgs.WasCallToActionClicked = "1".Equals (arg);
			
			if(fireRightNow)
			{
				OnAdFinishedEvent(adWinFinishedEventArgs);
				adWinFinishedEventArgs = null;
			}
			
			OnAdEndEvent();
		}
		if (e == "OnAdPlayableChanged") {
			if ("1".Equals (arg))
				OnCachedAdAvailableEvent();
		}
		if (e == "OnVideoView") {
			var parts = arg.Split( new char[] { ':' } );
			if(parts.Length == 3 )
			{
				double timeWatched = double.Parse( parts[1] );
				double totalDuration = double.Parse( parts[2] );
				
				bool fireRightNow = adWinFinishedEventArgs != null;
				
				if(!fireRightNow)
				{
					adWinFinishedEventArgs = new AdFinishedEventArgs();
				}
				
				adWinFinishedEventArgs.IsCompletedView = bool.Parse( parts[0] );
				adWinFinishedEventArgs.TimeWatched = timeWatched;
				adWinFinishedEventArgs.TotalDuration = totalDuration;
				
				if(fireRightNow)
				{
					OnAdFinishedEvent(adWinFinishedEventArgs);
					adWinFinishedEventArgs = null;
				}
				
				OnVideoViewEvent(timeWatched, totalDuration);
			}
		}
		if (e == "Diagnostic") {
			OnSDKLogEvent(arg);
		}
	}

	#region Native code will call these methods

	//methods for both platforms

	void OnAdStart( string empty )
	{
		OnAdStartEvent();
	}

	void OnCachedAdAvailable(string empty)
	{
		OnCachedAdAvailableEvent();
	}

	void OnAdPlayable(string playable)
	{
		OnAdPlayableEvent("1".Equals(playable));
	}

	void OnVideoView( string param )
	{
		#if UNITY_ANDROID
		//param is not json string
		var parts = param.Split( new char[] { '-' } );
		if(parts.Length == 3 )
		{
			double timeWatched = double.Parse( parts[1] ) / 1000;
			double totalDuration = double.Parse( parts[2] ) / 1000;

			bool fireRightNow = adFinishedEventArgs != null;

			if(!fireRightNow)
			{
				adFinishedEventArgs = new AdFinishedEventArgs();
			}

			adFinishedEventArgs.IsCompletedView = bool.Parse( parts[0] );
			adFinishedEventArgs.TimeWatched = timeWatched;
			adFinishedEventArgs.TotalDuration = totalDuration;

			if(fireRightNow)
			{
				OnAdFinishedEvent(adFinishedEventArgs);
				adFinishedEventArgs = null;
			}

			OnVideoViewEvent(timeWatched, totalDuration);
		}
		#elif UNITY_IPHONE
		//param is the json string
		Dictionary<string,object> attrs = (Dictionary<string,object>) MiniJSONV.Json.Deserialize( param );
		bool didDownload = extractBoolValue(attrs,"didDownload");
		bool isCompletedView = extractBoolValue(attrs,"completedView");
		double timeWatched = double.Parse( attrs["playTime"].ToString() );
		// we fake the totalDuration and make it accurate only as far as if they completed it or not for iOS
		double totalDuration = isCompletedView ? timeWatched : timeWatched * 2;

		AdFinishedEventArgs args = new AdFinishedEventArgs();
		args.WasCallToActionClicked = didDownload;
		args.IsCompletedView = isCompletedView;
		args.TimeWatched = timeWatched;
		args.TotalDuration = totalDuration;

		OnAdFinishedEvent(args);

		OnAdEndEvent();
		OnVideoViewEvent(timeWatched,totalDuration);
		#elif UNITY_WSA_10_0
		#endif
	}

	//methods only for ios

	void OnCloseProductSheet( string empty )
	{
		//OnAdEndEvent(false);
	}

	void OnSDKLog(string log)
	{
		OnSDKLogEvent(log);
	}

	//methods only for android

	void OnAdEnd(string param)
	{
		bool fireRightNow = adFinishedEventArgs != null;

		if(!fireRightNow)
		{
			adFinishedEventArgs = new AdFinishedEventArgs();
		}

		adFinishedEventArgs.WasCallToActionClicked = param.Equals ("1");

		if(fireRightNow)
		{
			OnAdFinishedEvent(adFinishedEventArgs);
			adFinishedEventArgs = null;
		}

		OnAdEndEvent();
	}
	
	#endregion

	#region util methods

	private bool extractBoolValue(string json, string key)
	{
		Dictionary<string,object> attrs = (Dictionary<string,object>)MiniJSONV.Json.Deserialize( json );
		return extractBoolValue (attrs, key);
	}

	private bool extractBoolValue(Dictionary<string,object> attrs, string key)
	{
		return bool.Parse( attrs[key].ToString() );
	}

	#endregion
}


