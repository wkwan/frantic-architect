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

#if !UNITY3_5 && !UNITY_4_0 && !UNITY_4_1

namespace ScriptInspector.Extensions.FlipbookGames
{

using UnityEngine;
using UnityEditor;

static class Si3OpenAnyFile
{
	static TextAsset selected;
	
#if UNITY_EDITOR_OSX
	[MenuItem("Window/Script Inspector 3/Open Any File... _&%o", false, 501)] // Alt-Cmd-O
#else
	[MenuItem("Window/Script Inspector 3/Open Any File... _#&o", false, 501)] // Alt-Shift+O
#endif
	static void Si3_OpenAnyFile()
	{
		EditorGUIUtility.ShowObjectPicker<TextAsset>(null, false, null, 0x51309);
		EditorApplication.update += WaitForObjectPicker;
	}
	
	static void WaitForObjectPicker()
	{
		var objectPickerId = EditorGUIUtility.GetObjectPickerControlID();
		if (objectPickerId == 0x51309)
		{
			selected = EditorGUIUtility.GetObjectPickerObject() as TextAsset;
		}
		else
		{
			EditorApplication.update -= WaitForObjectPicker;
			if (objectPickerId == 0 && selected != null)
			{
				var path = AssetDatabase.GetAssetPath(selected);
				if (path.StartsWith("Assets/", System.StringComparison.InvariantCultureIgnoreCase))
				{
					var guid = AssetDatabase.AssetPathToGUID(path);
					FGCodeWindow.OpenAssetInTab(guid);
				}
			}
			
			selected = null;
		}
	}
}
	
}

#endif
