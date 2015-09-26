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
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;


namespace ScriptInspector
{

[Serializable]
public class RecentLocation
{
	public string assetGuid;
	public int line;
	public int index;
}

[InitializeOnLoad, Serializable, StructLayout(LayoutKind.Sequential)]
public class FGTextBufferManager : ScriptableObject
{
	[SerializeField]
	private List<FGTextBuffer> allBuffers = new List<FGTextBuffer>();
	
	[SerializeField]
	public List<RecentLocation> recentLocations = new List<RecentLocation>();
	public static bool insertingTextAfterCaret = false;
	
	[SerializeField]
	public int recentLocationsOffset;
	
	private static bool reloadingAssemblies = false;
	
	public static bool IsReloadingAssemblies {
		get {
			return reloadingAssemblies;
		}
	}
	
	private static HashSet<string> pendingAssetImports = new HashSet<string>();
	
	private static Queue<string> asyncParseBuffers = new Queue<string>();
	private static FGTextBuffer asyncBuffer;

	static FGTextBufferManager()
	{
		EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
		EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
		AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;
		AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
	}

	private void OnEnable()
	{
		hideFlags = HideFlags.HideAndDontSave;
		if (_instance == null)
			_instance = this;
		else if (_instance != this)
			Debug.LogError("Multiple Managers!!!");
	}

	static void CurrentDomain_DomainUnload(object sender, EventArgs e)
	{
		if (!reloadingAssemblies)
		{
			SaveAllModified(true);
		}
	}

	//[PostProcessScene]
	//private static void OnBuild()
	//{
	//    SaveAllModified(false);
	//    //AssetDatabase.SaveAssets();
	//}

	private static void OnPlaymodeStateChanged()
	{
		if (_instance == null)
			return;

		if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			SaveAllModified(false);
	}
	
	public static void AddRecentLocation(string assetGuid, FGTextBuffer.CaretPos caretPosition, bool insert)
	{
		if (_instance == null)
			return;
		
		var newLocation = new RecentLocation {
			assetGuid = assetGuid,
			line = caretPosition.line,
			index = caretPosition.characterIndex
		};
		
		if (insert && _instance.recentLocationsOffset != 0)
		{
			var current = _instance.recentLocations.Count - _instance.recentLocationsOffset;
			_instance.recentLocations.Insert(current, newLocation);
		}
		else
		{
			if (_instance.recentLocationsOffset != 0)
			{
				var current = _instance.recentLocations.Count - _instance.recentLocationsOffset;
				_instance.recentLocations.RemoveRange(current, _instance.recentLocationsOffset);
			}
			_instance.recentLocations.Add(newLocation);
			_instance.recentLocationsOffset = 0;
		}
		
		if (_instance.recentLocations.Count > 100 + _instance.recentLocationsOffset)
		{
			_instance.recentLocations.RemoveRange(0, _instance.recentLocations.Count - 100 - _instance.recentLocationsOffset);
		}
	}
	
	public static void OnInsertedText(FGTextBuffer buffer, FGTextBuffer.CaretPos fromPos, FGTextBuffer.CaretPos toPos)
	{
		var guid = buffer.guid;
		var from = new TextPosition(fromPos.line, fromPos.characterIndex);
		var to = new TextPosition(toPos.line, toPos.characterIndex);
		
		var recentLocations = _instance.recentLocations;
		for (var i = recentLocations.Count; i --> 0; )
		{
			var entry = recentLocations[i];
			if (entry.assetGuid != guid)
				continue;
			
			var location = new TextPosition(entry.line, entry.index);
			if (from > location || insertingTextAfterCaret && from == location)
				continue;
			
			if (from.line == location.line)
				entry.index += to.index - from.index;
			entry.line += to.line - from.line;
		}
	}
	
	public static void OnRemovedText(FGTextBuffer buffer, FGTextBuffer.CaretPos fromPos, FGTextBuffer.CaretPos toPos)
	{
		var guid = buffer.guid;
		var from = new TextPosition(fromPos.line, fromPos.characterIndex);
		var to = new TextPosition(toPos.line, toPos.characterIndex);
		
		var recentLocations = _instance.recentLocations;
		for (var i = recentLocations.Count; i --> 0; )
		{
			var entry = recentLocations[i];
			if (entry.assetGuid != guid)
				continue;
			
			var location = new TextPosition(entry.line, entry.index);
			if (from >= location)
				continue;
			
			if (to >= location)
			{
				entry.line = from.line;
				entry.index = from.index;
				continue;
			}
			
			if (to.line == location.line)
				entry.index -= to.index - from.index;
			entry.line -= to.line - from.line;
		}
		
		if (recentLocations.Count > 1)
		{
			var prevLocation = recentLocations[0];
			for (var i = 1; i < recentLocations.Count; ++i)
			{
				var entry = recentLocations[i];
				if (entry.assetGuid != prevLocation.assetGuid || entry.line != prevLocation.line || entry.index != prevLocation.index)
				{
					prevLocation = entry;
				}
				else
				{
					if (_instance.recentLocationsOffset >= recentLocations.Count - i)
						--_instance.recentLocationsOffset;
					recentLocations.RemoveAt(i);
					--i;
				}
			}
		}
	}
	
	public static void AddPendingAssetImport(string guid)
	{
		pendingAssetImports.Add(guid);
	}
	
	public static bool HasPendingAssetImports
	{
		get{ return pendingAssetImports.Count > 0; }
	}
	
	public static void ImportPendingAssets()
	{
		foreach (var guid in pendingAssetImports)
			AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid));
		pendingAssetImports.Clear();
	}

	public static void SaveAllModified(bool onQuit)
	{
		if (_instance == null)
			return;
		
		//bool locked = false;
		try
		{
			foreach (FGTextBuffer buffer in instance.allBuffers)
			{
				if (buffer == null)
					continue;

				if (buffer.IsModified)
				{
					string path = AssetDatabase.GUIDToAssetPath(buffer.guid);
					if (onQuit && !EditorUtility.DisplayDialog("Script Inspector", "Save changes to the following asset?\n\n" + path, "Save", "Don't Save"))
						continue;

					//if (!onQuit && !locked)
					//{
					//	EditorApplication.LockReloadAssemblies();
					//	locked = true;
					//}
		
					if (buffer.Save())
					{
						if (!onQuit)
						{
							AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
							//var asset = AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));
							//if (asset != null)
							//	EditorUtility.SetDirty(asset);
							buffer.UpdateViews();
						}
					}
				}
			}
			
			if (!onQuit)
				ImportPendingAssets();
		}
		catch (Exception e) { Debug.LogError(e); }
		//finally
		//{
		//	if (locked)
		//	{
		//		EditorApplication.UnlockReloadAssemblies();
		//	}
		//}
	}

	private static FGTextBufferManager _instance = null;
	public static FGTextBufferManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = ScriptableObject.CreateInstance<FGTextBufferManager>();
				_instance.hideFlags = HideFlags.HideAndDontSave;
			}
			return _instance;
		}
	}
	
	public static FGTextBuffer TryGetBuffer(string assetPath)
	{
		if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
			return null;
		
		string guid = AssetDatabase.AssetPathToGUID(assetPath);
		return instance.allBuffers.Find(x => x != null && guid == x.guid);
	}

	public static FGTextBuffer GetBuffer(UnityEngine.Object target)
	{
		string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(target));
		return GetBuffer(guid);
	}
	
	public static FGTextBuffer GetBuffer(string guid)
	{
		List<FGTextBuffer> buffers = instance.allBuffers.FindAll(x => x != null && guid == x.guid);
		if (buffers.Count > 0)
		{
			if (buffers.Count > 1)
			{
				Debug.Log("Removing " + (buffers.Count - 1) + " duplicates...");
				for (int i = 1; i < buffers.Count; ++i)
					instance.allBuffers.Remove(buffers[i]);
//				EditorUtility.SetDirty(instance);
			}
			return buffers[0];
		}

		FGTextBuffer buffer = CreateInstance<FGTextBuffer>();
		instance.allBuffers.Add(buffer);
		buffer.guid = guid;
//		EditorUtility.SetDirty(instance);
		return buffer;
	}

	public static void DestroyBuffer(FGTextBuffer buffer)
	{
		instance.allBuffers.Remove(buffer);
		DestroyImmediate(buffer);
	}
	
	private static Dictionary<AssemblyDefinition.UnityAssembly, List<string>> scriptsWithPartialClass;
	private static Dictionary<AssemblyDefinition.UnityAssembly, List<string>> scriptsWithPartialStruct;
	private static Dictionary<AssemblyDefinition.UnityAssembly, List<string>> scriptsWithPartialInterface;
	
	public static void FindOtherTypeDeclarationParts(SymbolDeclaration declaration)
	{
		if (scriptsWithPartialClass == null)
		{
			scriptsWithPartialClass = new Dictionary<AssemblyDefinition.UnityAssembly, List<string>>();
			scriptsWithPartialStruct = new Dictionary<AssemblyDefinition.UnityAssembly, List<string>>();
			scriptsWithPartialInterface = new Dictionary<AssemblyDefinition.UnityAssembly, List<string>>();
		}
		
		var typeDefinition = declaration.definition as TypeDefinition;
		if (typeDefinition == null)
			return;
		var assemblyId = typeDefinition.Assembly.assemblyId;
		
		var cachedAssemblyScripts =
			declaration.kind == SymbolKind.Class ? scriptsWithPartialClass :
			declaration.kind == SymbolKind.Struct ? scriptsWithPartialStruct :
			scriptsWithPartialInterface;
		List<string> scriptsWithPartials;
		if (!cachedAssemblyScripts.TryGetValue(assemblyId, out scriptsWithPartials))
		{
			FGFindInFiles.Reset();
			FGFindInFiles.FindAllAssemblyScripts(assemblyId);
			
			cachedAssemblyScripts[assemblyId] = scriptsWithPartials = new List<string>(FGFindInFiles.assets.Count);
			
			var partialTypePhrase = new string[] {
				"partial",
				declaration.kind == SymbolKind.Class ? "class" : declaration.kind == SymbolKind.Struct ? "struct" : "interface"
			};
			for (var i = FGFindInFiles.assets.Count; i --> 0; )
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(FGFindInFiles.assets[i]);
				if (FGFindInFiles.ContainsWordsSequence(assetPath, partialTypePhrase))
					scriptsWithPartials.Add(FGFindInFiles.assets[i]);
			}
		}
		
		var words = new string[] {
			"partial",
			declaration.kind == SymbolKind.Class ? "class" : declaration.kind == SymbolKind.Struct ? "struct" : "interface",
			declaration.Name
		};
		for (var i = scriptsWithPartials.Count; i --> 0; )
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(scriptsWithPartials[i]);
			if (FGFindInFiles.ContainsWordsSequence(assetPath, words))
			{
				asyncParseBuffers.Enqueue(scriptsWithPartials[i]);
				scriptsWithPartials.RemoveAt(i);
			}
		}
		
		EditorApplication.update -= OnUpdate;
		EditorApplication.update += OnUpdate;
	}
	
	private static void OnUpdate()
	{
		if (asyncParseBuffers.Count > 0)
		{
			var guid = asyncParseBuffers.Dequeue();
			var buffer = GetBuffer(guid);
			if (buffer.Parser == null)
				buffer.LoadImmediately();
		}
		else
		{
			EditorApplication.update -= OnUpdate;
		}
	}

	public class FGScriptPostprocessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (_instance == null)
				return;

			foreach (string imported in importedAssets)
			{
				if (imported.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
					imported.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
					imported.EndsWith(".boo", StringComparison.OrdinalIgnoreCase))
				{
					if (!Array.Exists(movedAssets, (string path) => imported == path))
					{
						instance.OnAssetReimported(imported);
					}

					reloadingAssemblies = true;
				}
				else
				{
					instance.OnAssetReimported(imported);
				}
			}

			//foreach (string str in deletedAssets)
			//    Debug.Log("== Deleted Asset: " + str);

			for (int i = 0; i < movedAssets.Length; ++i)
			{
				if (movedAssets[i].EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
					movedAssets[i].EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
					movedAssets[i].EndsWith(".boo", StringComparison.OrdinalIgnoreCase))
				{
					instance.OnAssetMoved(movedAssets[i]);
				}
			}
			
			if (reloadingAssemblies)
			{
				EditorApplication.update -= CompileErrorsCheck;
				EditorApplication.update += CompileErrorsCheck;
				
				if (SISettings.autoFocusConsole == 2)
				{
					var focusedWindow = EditorWindow.focusedWindow;
					var siConsole = FGConsole.FindInstance();
					if (siConsole)
					{
						siConsole.Focus();
						if (focusedWindow)
							focusedWindow.Focus();
					}
				}
			}
			
			EditorApplication.update -= RepaintConsoleAfterUpdate;
			EditorApplication.update += RepaintConsoleAfterUpdate;
		}
	}

	private static void CompileErrorsCheck()
	{
		if (EditorApplication.isCompiling)
			return;

		EditorApplication.update -= CompileErrorsCheck;
		reloadingAssemblies = false;
		//EditorUtility.DisplayDialog("Script Inspector", "Compile errors!", "OK");
		FGTextEditor.RepaintAllInstances();
		FGConsole.repaintOnUpdateCounter = 1;
		
		if (SISettings.autoFocusConsole != 0)
		{
			FGConsole.ShowConsole();
			var siConsole = FGConsole.FindInstance();
			if (siConsole)
				siConsole.SendEvent(Event.KeyboardEvent("%end"));
		}
	}
	
	private static void RepaintConsoleAfterUpdate()
	{
		if (EditorApplication.isUpdating)
			return;

		EditorApplication.update -= RepaintConsoleAfterUpdate;
		FGConsole.repaintOnUpdateCounter = 1;
	}
	
	public void OnAssetReimported(string assetPath)
	{
		if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
			return;
		
		string guid = AssetDatabase.AssetPathToGUID(assetPath);
		FGTextBuffer buffer = allBuffers.Find((FGTextBuffer x) => guid == x.guid);
		if (buffer != null)
		{
			buffer.Reload();
		}
	}

	public void OnAssetMoved(string assetPath)
	{
		if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
			return;
		
		string guid = AssetDatabase.AssetPathToGUID(assetPath);
		FGTextBuffer buffer = allBuffers.Find(x => guid == x.guid);
		if (buffer != null)
		{
			buffer.justSavedNow = true;
		}
	}
}

}
