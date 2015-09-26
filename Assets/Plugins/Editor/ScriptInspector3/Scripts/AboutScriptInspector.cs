/* SCRIPT INSPECTOR 3
 * version 3.0.6, September 2015
 * Copyright © 2012-2015, Flipbook Games
 * 
 * Unity's legendary editor for C#, UnityScript, Boo, Shaders, and text,
 * now transformed into an advanced C# IDE!!!
 * 
 * Follow me on http://twitter.com/FlipbookGames
 * Like Flipbook Games on Facebook http://facebook.com/FlipbookGames
 * Join discussion in Unity forums http://forum.unity3d.com/threads/138329
 * Contact info@flipbookgames.com for feedback, bug reports, or suggestions.
 * Visit http://flipbookgames.com/ for more info.
 */

using UnityEditor;
using UnityEngine;

namespace ScriptInspector
{

public class AboutScriptInspector : EditorWindow
{
	private GUIStyle textStyle;
	private GUIStyle bigTextStyle;
	private GUIStyle miniTextStyle;
	private Texture2D flipbookLogo;

	private void OnEnable()
	{
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		title = "About Script Inspector 3";
#else
		titleContent.text = "About Script Inspector 3";
#endif
		minSize = new Vector2(265f, 166f);
		maxSize = new Vector2(265f, 166f);
	}

	void Initialize()
	{
		bigTextStyle = new GUIStyle(EditorStyles.boldLabel);
		bigTextStyle.fontSize = 24;
		bigTextStyle.alignment = TextAnchor.UpperCenter;
		
		miniTextStyle = new GUIStyle(EditorStyles.miniLabel);
		miniTextStyle.alignment = TextAnchor.UpperCenter;

		textStyle = new GUIStyle(EditorStyles.label);
		textStyle.alignment = TextAnchor.UpperCenter;
		textStyle.normal.textColor = miniTextStyle.normal.textColor;
		
		flipbookLogo = FGTextEditor.LoadEditorResource<Texture2D>("CreatedByFlipbookGames.png");
	}

	private void OnGUI()
	{
		if (textStyle == null)
			Initialize();

		GUILayout.Box("Script Inspector 3", bigTextStyle);
		GUILayout.Label("Version " + ScriptInspector.GetVersionString(), textStyle);
		GUILayout.Label("\xa9 Flipbook Games. All Rights Reserved.", miniTextStyle);

		GUILayout.FlexibleSpace();		
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(20f);
		if (GUILayout.Button(flipbookLogo, GUIStyle.none))
		{
			Application.OpenURL("http://www.flipbookgames.com/");
		}
		if (Event.current.type == EventType.repaint)
		{
			EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
		}
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Close"))
		{
			Close();
		}
		GUILayout.Space(2f);
		GUILayout.EndVertical();
		GUILayout.Space(10f);
		GUILayout.EndHorizontal();
		
		GUILayout.Space(10f);
	}
}

}
