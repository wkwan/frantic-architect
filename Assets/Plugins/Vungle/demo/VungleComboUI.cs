using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class VungleComboUI : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WSA_10_0
	void OnGUI()
	{
		beginGuiColomn();

		if( GUILayout.Button( "Start" ) )
		{
			// REPLACE WITH YOUR OWN APP IDS!
			Vungle.init( "com.prime31.Vungle", "vungleTest", "vungleTest" );
		}


		if( GUILayout.Button( "Is Ad Available" ) )
		{
			Debug.Log( "is ad available: " + Vungle.isAdvertAvailable() );
		}


		if( GUILayout.Button( "Disable Sound" ) )
		{
			Vungle.setSoundEnabled( false );
		}


		if( GUILayout.Button( "Display Ad" ) )
		{
			Vungle.playAd();
		}


		if( GUILayout.Button( "Display Insentivized Ad" ) )
		{
			Vungle.playAd( true, "user-tag" );
		}

		endGuiColumn();
	}


	#region Optional: Example of Subscribing to All Events

	void OnEnable()
	{
		Vungle.onAdStartedEvent += onAdStartedEvent;
		Vungle.onAdEndedEvent += onAdEndedEvent;
		Vungle.onAdViewedEvent += onAdViewedEvent;
		Vungle.onCachedAdAvailableEvent += onCachedAdAvailableEvent;
	}


	void OnDisable()
	{
		Vungle.onAdStartedEvent -= onAdStartedEvent;
		Vungle.onAdEndedEvent -= onAdEndedEvent;
		Vungle.onAdViewedEvent -= onAdViewedEvent;
		Vungle.onCachedAdAvailableEvent -= onCachedAdAvailableEvent;
	}


	void onAdStartedEvent()
	{
		Debug.Log( "onAdStartedEvent" );
	}


	void onAdEndedEvent()
	{
		Debug.Log( "onAdEndedEvent" );
	}


	void onAdViewedEvent( double watched, double length )
	{
		Debug.Log( "onAdViewedEvent. watched: " + watched + ", length: " + length );
	}


	void onCachedAdAvailableEvent()
	{
		Debug.Log( "onCachedAdAvailableEvent" );
	}

	#endregion


	#region Helpers to Tame GUI

	void beginGuiColomn()
	{
		var buttonHeight = ( Screen.width >= 960 || Screen.height >= 960 ) ? 70 : 30;

		GUI.skin.label.fixedHeight = buttonHeight;
		GUI.skin.label.margin = new RectOffset( 0, 0, 10, 0 );
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.button.margin = new RectOffset( 0, 0, 10, 0 );
		GUI.skin.button.fixedHeight = buttonHeight;
		GUI.skin.button.fixedWidth = Screen.width / 2 - 20;
		GUI.skin.button.wordWrap = true;

		GUILayout.BeginArea( new Rect( 10, 10, Screen.width / 2, Screen.height ) );
		GUILayout.BeginVertical();
	}


	void endGuiColumn( bool hasSecondColumn = false )
	{
		GUILayout.EndVertical();
		GUILayout.EndArea();

		if( hasSecondColumn )
		{
			GUILayout.BeginArea( new Rect( Screen.width - Screen.width / 2, 10, Screen.width / 2, Screen.height ) );
			GUILayout.BeginVertical();
		}
	}

	#endregion

#endif
}
