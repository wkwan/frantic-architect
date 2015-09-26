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


namespace ScriptInspector
{

[CustomEditor(typeof(TextAsset))]
public class FGTextInspector : ScriptInspector
{
	protected override void DoGUI()
	{
		var currentInspector = GetCurrentInspector();

#if UNITY_4_3
		textEditor.OnInspectorGUI(false, new RectOffset(0, -4, 14, -13), currentInspector);
#else
		textEditor.OnInspectorGUI(false, new RectOffset(0, 0, 14, -13), currentInspector);
#endif
	}
}

}
