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

namespace ScriptInspector.Extensions.FlipbookGames
{

using UnityEngine;
using UnityEditor;

static class Si3OpenFile
{
	[MenuItem("Window/Script Inspector 3/Open File... _#%o", false, 500)]
	static void Si3_OpenFile()
	{
		var restoreFocusedWindow = EditorWindow.focusedWindow;
		
		var projectDir = Application.dataPath;
		var currentDir = projectDir;
		var guidHistroy = FGCodeWindow.GetGuidHistory();
		if (guidHistroy.Count >= 1)
		{
			var lastAssetPath = AssetDatabase.GUIDToAssetPath(guidHistroy[0]);
			currentDir = System.IO.Path.GetDirectoryName(lastAssetPath);
		}
		
		try
		{
			var open = EditorUtility.OpenFilePanel("Open in Script Inspector", currentDir,
#if UNITY_EDITOR_OSX
				"");
#else
				"cs;*.js;*.boo;*.shader;*.txt");
#endif
			if (!string.IsNullOrEmpty(open))
			{
				if (open.StartsWith(projectDir, System.StringComparison.InvariantCultureIgnoreCase))
				{
					open = open.Substring(projectDir.Length - "Assets".Length);
					var guid = AssetDatabase.AssetPathToGUID(open);
					FGCodeWindow.OpenAssetInTab(guid);
				}
			}
			else if (restoreFocusedWindow)
			{
				restoreFocusedWindow.Focus();
			}
		}
		finally
		{
			System.IO.Directory.SetCurrentDirectory(projectDir.Substring(0, projectDir.Length - "/Assets".Length));
		}
	}
}

}
