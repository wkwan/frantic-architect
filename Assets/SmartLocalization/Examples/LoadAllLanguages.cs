//
//  LoadAllLanguages.cs
//
//
// Written by Niklas Borglund and Jakob Hillerström
//

namespace SmartLocalization
{
using UnityEngine;
using System.Collections.Generic;

public class LoadAllLanguages : MonoBehaviour 
{
	private List<string> currentLanguageKeys;
	private List<SmartCultureInfo> availableLanguages;
	private LanguageManager languageManager;
	private Vector2 valuesScrollPosition = Vector2.zero;
	private Vector2 languagesScrollPosition = Vector2.zero;

	void Start () 
	{
		languageManager = LanguageManager.Instance;
		
		SmartCultureInfo deviceCulture = languageManager.GetDeviceCultureIfSupported();
		if(deviceCulture != null)
		{
			languageManager.ChangeLanguage(deviceCulture);	
		}
		else
		{
			Debug.Log("The device language is not available in the current application. Loading default."); 
		}
		
		if(languageManager.NumberOfSupportedLanguages > 0)
		{
			currentLanguageKeys = languageManager.GetAllKeys();
			availableLanguages = languageManager.GetSupportedLanguages();
		}
		else
		{
			Debug.LogError("No languages are created!, Open the Smart Localization plugin at Window->Smart Localization and create your language!");
		}

		LanguageManager.Instance.OnChangeLanguage += OnLanguageChanged;
	}

	void OnDestroy()
	{
		if(LanguageManager.HasInstance)
		{
			LanguageManager.Instance.OnChangeLanguage -= OnLanguageChanged;
		}
	}

	void OnLanguageChanged(LanguageManager languageManager)
	{
		currentLanguageKeys = languageManager.GetAllKeys();
	}
	
	void OnGUI() 
	{
		if(languageManager.NumberOfSupportedLanguages > 0)
		{
			if(languageManager.CurrentlyLoadedCulture != null)
			{
				GUILayout.Label("Current Language:" + languageManager.CurrentlyLoadedCulture.ToString());
			}
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Keys:", GUILayout.Width(460));
			GUILayout.Label("Values:", GUILayout.Width(460));
			GUILayout.EndHorizontal();
			
			valuesScrollPosition = GUILayout.BeginScrollView(valuesScrollPosition);
			foreach(var languageKey in currentLanguageKeys)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(languageKey, GUILayout.Width(460));
				GUILayout.Label(languageManager.GetTextValue(languageKey), GUILayout.Width(460));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndScrollView();
			
			languagesScrollPosition = GUILayout.BeginScrollView (languagesScrollPosition);
			foreach(SmartCultureInfo language in availableLanguages)
			{
				if(GUILayout.Button(language.nativeName, GUILayout.Width(960)))
				{
					languageManager.ChangeLanguage(language);
				}
			}

			GUILayout.EndScrollView();
		}
	}
}
}//namespace SmartLocalization
