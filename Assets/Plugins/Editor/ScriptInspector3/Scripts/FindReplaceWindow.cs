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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptInspector
{
	
public enum FindReplace_LookIn
{
	AllAssemblies,
	AllGameAssemblies,
	AllEditorAssemblies,
	GameAssemblies,
	EditorAssemblies,
	FirstPassGameAssemblies,
	FirstPassEditorAssemblies,
}

public enum FindReplace_LookFor
{
	AllAssets,
	AllScripts,
	CSharpScripts,
	JSScripts,
	BooScripts,
	Shaders,
	TextAssets,
}

public class FindReplaceWindow : EditorWindow
{
	const float fixedWidth = 320f;
	const float fixedHeightFind = 232f - 30f;
	const float fixedHeightReplace = 278f;
	
	private static FindReplaceWindow instance;
	
	private string findText;
	private string replaceText;
	private string initialFindText;
	private FindReplace_LookIn lookInOption;
	private FindReplace_LookFor lookForOption;
	
	private FGTextEditor editor;
	private bool setInitialFocus = true;
	private bool resetFocus = false;
	
	bool isReplace;
	
	bool matchCase;
	bool matchWholeWord;
	bool listResultsInNewWindow;
	
	private static string[] searchHistory = new string[20];
	
	//private static readonly string[] toolbarTexts = new[] { "Find in Files", "Replace in Files" };
	
	[MenuItem("Window/Script Inspector 3/Find in Files... _#%f", false, 600)]
	public static void ShowFindInFilesWindow()
	{
		if (!SISettings.captureShiftCtrlF)
		{
			if ((FGTextBuffer.activeEditor == null || focusedWindow != FGTextBuffer.activeEditor.OwnerWindow) &&
				!(focusedWindow is FGConsole || focusedWindow is FindResultsWindow))
			{
				if (EditorApplication.ExecuteMenuItem("GameObject/Align With View"))
					return;
			}
		}
		
		EditorApplication.delayCall += () => {
			if (instance != null)
			{
				instance.isReplace = false;
				instance.Repaint();
			}
			else
			{
				Create(false);
			}
		};
	}
	
	//[MenuItem("Edit/Script Inspector 3/Replace Text in Files... _#%h", false, -100)]
	//public static void ShowReplaceInFilesWindow()
	//{
	//	EditorApplication.delayCall += () => {
	//		if (instance != null)
	//		{
	//			instance.isReplace = true;
	//			instance.Repaint();
	//		}
	//		else
	//		{
	//			Create(true);
	//		}
	//	};
	//}
	
	public static void Create(bool replace)
	{
		var owner = EditorWindow.focusedWindow;
		var wnd = EditorWindow.GetWindow<FindReplaceWindow>(true);
		
		var editor = FGTextBuffer.activeEditor;
		wnd.editor = editor;
		
		if (editor != null && editor.selectionStartPosition != null &&
			editor.selectionStartPosition.line == editor.caretPosition.line)
		{
			var from = Mathf.Min(editor.selectionStartPosition.characterIndex, editor.caretPosition.characterIndex);
			var to = Mathf.Max(editor.selectionStartPosition.characterIndex, editor.caretPosition.characterIndex);
			wnd.findText = editor.TextBuffer.lines[editor.caretPosition.line].Substring(from, to - from);
		}
		else
		{
			wnd.findText = editor != null ? editor.GetSearchTextFromSelection() : "";
		}
		wnd.initialFindText = wnd.findText;
		wnd.replaceText = null;
		wnd.isReplace = replace;
		
		if (owner != null)
		{
			var center = owner.position.center;
			wnd.position = new Rect(
				(int)(center.x - 0.5f * fixedWidth),
				(int)(center.y - 0.5f * fixedHeightFind),
				fixedWidth,
				fixedHeightFind);
		}
		wnd.ShowAuxWindow();
	}

	private void OnEnable()
	{
		instance = this;
		
		lookInOption = (FindReplace_LookIn) EditorPrefs.GetInt("FlipbookGames.ScriptInspector.FindReplace.LookIn", 0);
		lookForOption = (FindReplace_LookFor) EditorPrefs.GetInt("FlipbookGames.ScriptInspector.FindReplace.LookFor", 0);
		
		matchCase = EditorPrefs.GetBool("FlipbookGames.ScriptInspector.FindReplace.MatchCase", false);
		matchWholeWord = EditorPrefs.GetBool("FlipbookGames.ScriptInspector.FindReplace.MatchWholeWord", false);
		listResultsInNewWindow = EditorPrefs.GetBool("FlipbookGames.ScriptInspector.FindReplace.ListResultsInNewWindow", false);
		
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		title = "Find Text";
#else
		titleContent.text = "Find Text";
#endif
		minSize = new Vector2(fixedWidth, fixedHeightFind);
		maxSize = new Vector2(fixedWidth, fixedHeightFind);
		Repaint();
		
		for (var i = 0; i < searchHistory.Length; i++)
		{
			searchHistory[i] = EditorPrefs.GetString("FlipbookGames.ScriptInspector.SearchHistory_" + i.ToString());
			if (searchHistory[i] == "")
			{
				searchHistory[i] = null;
				break;
			}
		}
	}
	
	private void OnDisable()
	{
		EditorPrefs.SetInt("FlipbookGames.ScriptInspector.FindReplace.LookIn", (int) lookInOption);
		EditorPrefs.SetInt("FlipbookGames.ScriptInspector.FindReplace.LookFor", (int) lookForOption);
		
		EditorPrefs.SetBool("FlipbookGames.ScriptInspector.FindReplace.MatchCase", matchCase);
		EditorPrefs.SetBool("FlipbookGames.ScriptInspector.FindReplace.MatchWholeWord", matchWholeWord);
		EditorPrefs.SetBool("FlipbookGames.ScriptInspector.FindReplace.ListResultsInNewWindow", listResultsInNewWindow);
		
		instance = null;
		if (editor != null && editor.OwnerWindow)
			editor.OwnerWindow.Focus();
	}
	
	private void SelectFromHistory(object s)
	{
		var str = (string) s;
		for (var i = searchHistory.Length; i --> 0; )
		{
			if (searchHistory[i] == str)
			{
				while (i --> 0)
					searchHistory[i+1] = searchHistory[i];
				searchHistory[0] = str;
				break;
			}
		}
		findText = str;
		delayCounter = 1;
		EditorApplication.update += ReFocusFindField;
	}
	
	private int delayCounter;
	private void ReFocusFindField()
	{
		if (delayCounter > 0)
		{
			--delayCounter;
			resetFocus = true;
		}
		else
		{
			EditorApplication.update -= ReFocusFindField;
			setInitialFocus = true;
		}
		Repaint();
	}
	
	private void ShowFindHistory()
	{
		GenericMenu menu = new GenericMenu();
		if (findText != initialFindText && initialFindText.Trim() != "")
			menu.AddItem(new GUIContent(initialFindText), false, SelectFromHistory, initialFindText);
		for (var i = 0; i < searchHistory.Length && searchHistory[i] != null; i++)
			if (findText != searchHistory[i])
				menu.AddItem(new GUIContent(searchHistory[i]), false, SelectFromHistory, searchHistory[i]);
		if (menu.GetItemCount() > 0)
			menu.DropDown(new Rect(14f, 28f, Screen.width - 10f, 18f));
		Event.current.Use();
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.KeyDown)
		{
			if (findText != "" && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
			{
				Event.current.Use();
				Execute();
				return;
			}
			else if (Event.current.keyCode == KeyCode.Escape)
			{
				Event.current.Use();
				Close();
				if (editor != null && editor.OwnerWindow)
					editor.OwnerWindow.Focus();
				return;
			}
			else if (Event.current.keyCode == KeyCode.DownArrow)
			{
				if (GUI.GetNameOfFocusedControl() == "Find field")
				{
					ShowFindHistory();
				}
			}
		}
		
		// Left margin
		GUILayout.BeginHorizontal();
		GUILayout.Space(10f);
		{
			// Top margin
			GUILayout.BeginVertical();
			GUILayout.Space(10f);
			
			isReplace = false; // 1 == GUILayout.Toolbar(isReplace ? 1 : 0, toolbarTexts);
			//GUILayout.Space(10f);
			
			GUILayout.Label("Find what:");
			
			GUILayout.BeginHorizontal();
			
			GUI.SetNextControlName("Find field");
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
			if (setInitialFocus)
				EditorGUI.FocusTextInControl("Find field");
#endif
			try { findText = EditorGUILayout.TextField(findText); } catch {}
			if (setInitialFocus)
				GUI.FocusControl("Find field");
			setInitialFocus = false;
			
			if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Height(16f), GUILayout.Width(13f)))
				ShowFindHistory();
			
			GUILayout.Space(4f);
			
			GUILayout.EndHorizontal();
			
			if (isReplace)
			{
				GUILayout.Space(10f);
				
				GUILayout.Label("Replace with:");
				
				replaceText = replaceText ?? findText;
				try { replaceText = EditorGUILayout.TextField(replaceText); } catch {}
			}
			
			GUILayout.Space(10f);
			
			GUI.SetNextControlName("Look for");
			lookForOption = (FindReplace_LookFor) EditorGUILayout.EnumPopup("Look for:", lookForOption);
			if (resetFocus)
				GUI.FocusControl("Look for");
			resetFocus = false;
			
			if (lookForOption != FindReplace_LookFor.AllAssets &&
				lookForOption != FindReplace_LookFor.Shaders && lookForOption != FindReplace_LookFor.TextAssets)
			{
				lookInOption = (FindReplace_LookIn) EditorGUILayout.EnumPopup("Look in:", lookInOption);
			}
			else
			{
				GUI.enabled = false;
				EditorGUILayout.EnumPopup("Look in:", FindReplace_LookIn.AllAssemblies);
				GUI.enabled = true;
			}
			
			GUILayout.Space(10f);
			
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
			matchCase = EditorGUILayout.ToggleLeft(" Match case", matchCase);
			matchWholeWord = EditorGUILayout.ToggleLeft(" Match whole word", matchWholeWord);
			listResultsInNewWindow = EditorGUILayout.ToggleLeft(" List results in a new window", listResultsInNewWindow);
#else
			matchCase = GUILayout.Toggle(matchCase, " Match case");
			matchWholeWord = GUILayout.Toggle(matchWholeWord, " Match whole word");
			listResultsInNewWindow = GUILayout.Toggle(listResultsInNewWindow, " List results in new window");
#endif
			
			GUILayout.Space(10f);
			
			GUI.enabled = findText != "";
			
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Find All"))
				{
					Execute();
				}
				GUILayout.Space(6f);
				
				GUI.enabled = true;
				
				if (GUILayout.Button("Cancel"))
				{
					Close();
					if (editor != null && editor.OwnerWindow)
						editor.OwnerWindow.Focus();
				}
			}
			GUILayout.EndHorizontal();
		
			GUILayout.Space(20f);
			GUILayout.EndVertical();
		}
		GUILayout.Space(10f);
		GUILayout.EndHorizontal();
		
		if (isReplace)
		{
			if (position.height != fixedHeightReplace)
			{
				maxSize = new Vector2(fixedWidth, fixedHeightReplace);
				minSize = new Vector2(fixedWidth, fixedHeightReplace);
			}
		}
		else
		{
			if (position.height != fixedHeightFind)
			{
				maxSize = new Vector2(fixedWidth, fixedHeightFind);
				minSize = new Vector2(fixedWidth, fixedHeightFind);
			}
		}
	}
	
	private static List<string> ignoreFileTypes = new List<string> { ".dll", ".a", ".so", ".dylib", ".exe" };
	private static List<string> scriptFileTypes = new List<string> { ".cs", ".js", ".boo" };
	private static List<string> shaderFileTypes = new List<string> { ".shader", ".cg", ".cginc", ".hlsl", ".hlslinc" };
	private static List<string> nonTextFileTypes = new List<string> {
		".dll", ".a", ".so", ".dylib", ".exe", ".cs", ".js", ".boo", ".shader", ".cg", ".cginc", ".hlsl", ".hlslinc"
		};
	
	private void Execute()
	{
		if (findText.Trim() != "")
		{
			var historyIndex = System.Array.IndexOf(searchHistory, findText);
			if (historyIndex < 0)
				historyIndex = searchHistory.Length - 1;
			for (var i = historyIndex; i --> 0; )
				searchHistory[i + 1] = searchHistory[i];
			searchHistory[0] = findText;
		}
		
		for (var i = 0; i < searchHistory.Length; i++)
			if (searchHistory[i] != null)
				EditorPrefs.SetString("FlipbookGames.ScriptInspector.SearchHistory_" + i.ToString(), searchHistory[i]);
		
		Close();
		if (editor != null && editor.OwnerWindow)
			editor.OwnerWindow.Focus();
		
		string[] allTextAssetGuids;
		
		if (lookInOption != FindReplace_LookIn.AllAssemblies &&
			lookForOption != FindReplace_LookFor.AllAssets &&
			lookForOption != FindReplace_LookFor.Shaders &&
			lookForOption != FindReplace_LookFor.TextAssets)
		{
			if (FGFindInFiles.assets != null)
				FGFindInFiles.assets.Clear();
			
			if (lookInOption == FindReplace_LookIn.FirstPassGameAssemblies || lookInOption == FindReplace_LookIn.AllGameAssemblies)
			{
				if (lookForOption == FindReplace_LookFor.CSharpScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.CSharpFirstPass);
				if (lookForOption == FindReplace_LookFor.JSScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.UnityScriptFirstPass);
				if (lookForOption == FindReplace_LookFor.BooScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.BooFirstPass);
			}
			if (lookInOption == FindReplace_LookIn.GameAssemblies || lookInOption == FindReplace_LookIn.AllGameAssemblies)
			{
				if (lookForOption == FindReplace_LookFor.CSharpScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.CSharp);
				if (lookForOption == FindReplace_LookFor.JSScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.UnityScript);
				if (lookForOption == FindReplace_LookFor.BooScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.Boo);
			}
			if (lookInOption == FindReplace_LookIn.FirstPassEditorAssemblies || lookInOption == FindReplace_LookIn.AllEditorAssemblies)
			{
				if (lookForOption == FindReplace_LookFor.CSharpScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.CSharpEditorFirstPass);
				if (lookForOption == FindReplace_LookFor.JSScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.UnityScriptEditorFirstPass);
				if (lookForOption == FindReplace_LookFor.BooScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.BooEditorFirstPass);
			}
			if (lookInOption == FindReplace_LookIn.EditorAssemblies || lookInOption == FindReplace_LookIn.AllEditorAssemblies)
			{
				if (lookForOption == FindReplace_LookFor.CSharpScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.CSharpEditor);
				if (lookForOption == FindReplace_LookFor.JSScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.UnityScriptEditor);
				if (lookForOption == FindReplace_LookFor.BooScripts || lookForOption == FindReplace_LookFor.AllScripts)
					FGFindInFiles.FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly.BooEditor);
			}
			
			allTextAssetGuids = FGFindInFiles.assets.ToArray();
		}
		else
		{
			allTextAssetGuids = FindAllTextAssets().ToArray();

			IEnumerable<string> realTextAssets = null;
			switch (lookForOption)
			{
			case FindReplace_LookFor.AllAssets:
				realTextAssets =
					from guid in allTextAssetGuids
					where ! ignoreFileTypes.Contains(Path.GetExtension(AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant()))
					select guid;
				break;
			case FindReplace_LookFor.AllScripts:
				realTextAssets =
					from guid in allTextAssetGuids
					where scriptFileTypes.Contains(Path.GetExtension(AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant()))
					select guid;
				break;
			case FindReplace_LookFor.CSharpScripts:
				realTextAssets =
					from guid in allTextAssetGuids
					where Path.GetExtension(AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant()) == ".cs"
					select guid;
				break;
			case FindReplace_LookFor.JSScripts:
				realTextAssets =
					from guid in allTextAssetGuids
					where Path.GetExtension(AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant()) == ".js"
					select guid;
				break;
			case FindReplace_LookFor.BooScripts:
				realTextAssets =
					from guid in allTextAssetGuids
					where Path.GetExtension(AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant()) == ".boo"
					select guid;
				break;
			case FindReplace_LookFor.Shaders:
				realTextAssets =
					from guid in allTextAssetGuids
					where shaderFileTypes.Contains(Path.GetExtension(AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant()))
					select guid;
				break;
			case FindReplace_LookFor.TextAssets:
				realTextAssets =
					from guid in allTextAssetGuids
					where ! nonTextFileTypes.Contains(Path.GetExtension(AssetDatabase.GUIDToAssetPath(guid).ToLowerInvariant()))
					select guid;
				break;
			}
			
			allTextAssetGuids = realTextAssets.ToArray();
		}
		
		var searchOptions = new SearchOptions {
			text = findText,
			matchCase = matchCase,
			matchWord = matchWholeWord,
		};
		
		FindResultsWindow.Create(
			"Searching for '" + findText + "'...",
			FindInFile,
			allTextAssetGuids,
			searchOptions,
			listResultsInNewWindow);
	}
	
	private static List<string> FindAllTextAssets()
	{
		var hierarchyProperty = new HierarchyProperty(HierarchyType.Assets);
		hierarchyProperty.SetSearchFilter("t:TextAsset", 0);
		hierarchyProperty.Reset();
		List<string> list = new List<string>();
		while (hierarchyProperty.Next(null))
			list.Add(hierarchyProperty.guid);
		return list;
	}
	
	private static void FindInFile(
		System.Action<string, string, TextPosition, int> addResultAction,
		string assetGuid,
		SearchOptions search)
	{
		var allLines = FGFindInFiles.GetOrReadAllLines(assetGuid);
		foreach (var textPosition in FGFindInFiles.FindAll(allLines, search))
		{
			var line = allLines[textPosition.line];
			addResultAction(line, assetGuid, textPosition, search.text.Length);
		}
	}
}

class SearchOptions
{
	public string text;
	public bool matchCase;
	public bool matchWord;
}

}
