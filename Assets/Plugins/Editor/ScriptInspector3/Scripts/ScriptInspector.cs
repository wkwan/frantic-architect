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

using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace ScriptInspector
{

[CustomEditor(typeof(MonoScript))]
public class ScriptInspector : Editor
{
	[SerializeField, HideInInspector]
	protected FGTextEditor textEditor = new FGTextEditor();

	[System.NonSerialized]
	protected bool enableEditor = true;

	public static string GetVersionString()
	{
		return "3.0.6, September 2015";
	}

	public void OnDisable()
	{
		textEditor.onRepaint = null;
		textEditor.OnDisable();
	}
	
	public override void OnInspectorGUI()
	{
		if (enableEditor)
		{
			textEditor.onRepaint = Repaint;
			textEditor.OnEnable(target);
			enableEditor = false;
		}

		if (Event.current.type == EventType.ValidateCommand)
		{
			if (Event.current.commandName == "ScriptInspector.AddTab")
			{
				Event.current.Use();
				return;
			}
		}
		else if (Event.current.type == EventType.ExecuteCommand)
		{
			if (Event.current.commandName == "ScriptInspector.AddTab")
			{
				FGCodeWindow.OpenNewWindow(null, null, false);
				Event.current.Use();
				return;
			}
		}
		
		DoGUI();
	}

	protected virtual void DoGUI()
	{
		var currentInspector = GetCurrentInspector();
		textEditor.OnInspectorGUI(true, new RectOffset(0, -4, 0, 0), currentInspector);
	}
	
	private static System.Type inspectorWindowType;
	private static FieldInfo currentInspectorWindowField;
	protected static EditorWindow GetCurrentInspector()
	{
		//EditorWindow wnd = EditorWindow.focusedWindow;
		//if (wnd == null)
		//	return null;

		if (inspectorWindowType == null)
			inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
		if (inspectorWindowType != null)
		{
			if (currentInspectorWindowField == null)
				currentInspectorWindowField = inspectorWindowType.GetField("s_CurrentInspectorWindow",
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (currentInspectorWindowField != null)
			{
				var currentInspector = currentInspectorWindowField.GetValue(null) as EditorWindow;
				//if (currentInspector == wnd)
				//	return wnd;
				return currentInspector;
			}
		}
		
		return null;
	}
}

}
