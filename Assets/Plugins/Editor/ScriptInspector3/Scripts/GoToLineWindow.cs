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

public class GoToLineWindow : EditorWindow
{
	private string text;
	private FGTextEditor editor;
	
	public static GoToLineWindow Create(FGTextEditor editor)
	{
		var owner = EditorWindow.focusedWindow;
		var wnd = EditorWindow.GetWindow<GoToLineWindow>(true);
		wnd.editor = editor;
		wnd.text = (editor.caretPosition.line + 1).ToString();
		if (owner != null)
		{
			var center = owner.position.center;
			wnd.position = new Rect((int)(center.x - 0.5f * 256f), (int)(center.y - 0.5f * 100f), 256f, 100f);
		}
		wnd.ShowAuxWindow();
		return wnd;
	}

	private void OnEnable()
	{
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		title = "Go To Line";
#else
		titleContent.text = "Go To Line";
#endif
		minSize = new Vector2(265f, 100f);
		maxSize = new Vector2(265f, 100f);
		Repaint();
	}
	
	private void OnDisable()
	{
		editor.OwnerWindow.Focus();
	}

	private void OnGUI()
	{
		int lineToGoTo;
		var validLineNumber = int.TryParse(text, out lineToGoTo) &&
			lineToGoTo >= 1 && lineToGoTo <= editor.TextBuffer.lines.Count;
		
		if (validLineNumber && Event.current.type == EventType.KeyDown && Event.current.character == '\n')
		{
			editor.SetCursorPosition(lineToGoTo - 1, 0);
			Close();
			editor.OwnerWindow.Focus();
			return;
		}
		else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
		{
			Close();
			editor.OwnerWindow.Focus();
			return;
		}
		
		GUILayout.BeginVertical();
		GUILayout.Space(20f);
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(10f);
		
		GUI.SetNextControlName("text field");
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
		EditorGUI.FocusTextInControl("text field");
#endif
		text = EditorGUILayout.TextField(text);
		GUI.FocusControl("text field");
		
		GUILayout.Space(10f);
		GUILayout.EndHorizontal();
		GUILayout.Space(20f);
		
		GUI.enabled = int.TryParse(text, out lineToGoTo) &&
			lineToGoTo >= 1 && lineToGoTo <= editor.TextBuffer.lines.Count;
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("OK"))
		{
			editor.SetCursorPosition(lineToGoTo - 1, 0);
			Close();
			editor.OwnerWindow.Focus();
		}
		GUILayout.Space(6f);
		
		GUI.enabled = true;
		
		if (GUILayout.Button("Cancel"))
		{
			Close();
			editor.OwnerWindow.Focus();
		}
		GUILayout.Space(10f);
		GUILayout.EndHorizontal();
		
		GUILayout.Space(10f);
		GUILayout.EndVertical();
	}
}

}
