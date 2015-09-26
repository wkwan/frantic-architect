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
using System.Collections.Generic;
using System.IO;


namespace ScriptInspector
{
	
public static class FGFindInFiles
{
	public static List<string> assets;
	
	public static List<SymbolDeclaration> FindDeclarations(SymbolDefinition symbol)
	{
		var candidates = FindDefinitionCandidates(symbol);
		foreach (var c in candidates)
		{
			var asset = AssetDatabase.LoadAssetAtPath(c, typeof(TextAsset)) as TextAsset;
			if (!asset)
				continue;
			var buffer = FGTextBufferManager.GetBuffer(asset);
			buffer.LoadImmediately();
		}
		
		var newSymbol = symbol.Rebind();
		var declarations = newSymbol == null ? null : newSymbol.declarations;
		return declarations ?? symbol.declarations;
	}
	
	static List<string> FindDefinitionCandidates(SymbolDefinition symbol)
	{
		var result = new List<string>();
		if (assets != null)
			assets.Clear();
		
		var symbolType = symbol;
		if (symbol.kind == SymbolKind.Namespace)
			return result;
		
		while (symbolType != null &&
			symbolType.kind != SymbolKind.Class && symbolType.kind != SymbolKind.Struct &&
			symbolType.kind != SymbolKind.Enum && symbolType.kind != SymbolKind.Interface &&
			symbolType.kind != SymbolKind.Delegate)
		{
			symbolType = symbolType.parentSymbol;
		}
		
		var assembly = symbolType.Assembly;
		var assemblyId = assembly.assemblyId;
		FindAllAssemblyScripts(assemblyId);
		for (int i = assets.Count; i --> 0; )
			assets[i] = AssetDatabase.GUIDToAssetPath(assets[i]);
		//assets.RemoveAll(x => FGTextBufferManager.TryGetBuffer(x) != null);
		
		string keyword;
		string typeName = symbolType.name;
		switch (symbolType.kind)
		{
			case SymbolKind.Class: keyword = "class"; break;
			case SymbolKind.Struct: keyword = "struct"; break;
			case SymbolKind.Interface: keyword = "interface"; break;
			case SymbolKind.Enum: keyword = "enum"; break;
			case SymbolKind.Delegate: keyword = "delegate"; break;
			default: return result;
		}
		
		for (int i = assets.Count; i --> 0; )
			if (ContainsWordsSequence(assets[i], keyword, typeName))
				result.Add(assets[i]);
		
		return result;
	}
	
	public static IList<string> GetOrReadAllLines(string assetGuid)
	{
		string[] lines;
		try
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
			var textBuffer = FGTextBufferManager.TryGetBuffer(assetPath);
			if (textBuffer != null)
				return textBuffer.lines;
			
			lines = File.ReadAllLines(assetPath);
		}
		catch (IOException e)
		{
			Debug.LogError(e);
			return null;
		}
		return lines;
	}
	
	internal static IEnumerable<TextPosition> FindAll(IList<string> lines, SearchOptions search)
	{
		var length = search.text.Length;
		if (length == 0)
			yield break;
		
		var comparison = search.matchCase ? System.StringComparison.Ordinal : System.StringComparison.OrdinalIgnoreCase;
		
		char firstChar = search.text[0];
		bool startsAsWord = firstChar == '_' || char.IsLetterOrDigit(firstChar);
		char lastChar = search.text[search.text.Length - 1];
		bool endsAsWord = lastChar == '_' || char.IsLetterOrDigit(lastChar);
		
		int skipThisWord = search.text.IndexOf(firstChar.ToString(), 1, comparison);
		if (skipThisWord < 0)
			skipThisWord = search.text.Length;
		
		var l = 0;
		var c = 0;
		while (l < lines.Count)
		{
			var line = lines[l];
			
			if (c > line.Length - length)
			{
				c = 0;
				++l;
				continue;
			}
			
			c = line.IndexOf(search.text, c, comparison);
			if (c < 0)
			{
				c = 0;
				++l;
				continue;
			}
			
			if (search.matchWord)
			{
				if (startsAsWord && c > 0)
				{
					char prevChar = line[c - 1];
					if (prevChar == '_' || char.IsLetterOrDigit(prevChar))
					{
						c += skipThisWord;
						continue;
					}
				}
				if (endsAsWord && c + length < line.Length)
				{
					char nextChar = line[c + length];
					if (nextChar == '_' || char.IsLetterOrDigit(nextChar))
					{
						c += skipThisWord;
						continue;
					}
				}
			}
			
			yield return new TextPosition(l, c);
			c += length;
		}
	}
	
	public static bool ContainsWordsSequence(string assetPath, params string[] words)
	{
		try
		{
			var lines = File.ReadAllLines(assetPath);
			var l = 0;
			var w = 0;
			var s = 0;
			while (l < lines.Length)
			{
				if (s >= lines[l].Length - words[0].Length)
				{
					s = 0;
					++l;
					continue;
				}
				
				s = lines[l].IndexOf(words[0], s, System.StringComparison.Ordinal);
				if (s < 0)
				{
					s = 0;
					++l;
					continue;
				}
				
				if (s > 0)
				{
					var c = lines[l][s - 1];
					if (c == '_' || char.IsLetterOrDigit(c))
					{
						s += words[0].Length;
						continue;
					}
				}
				
				s += words[0].Length;
				if (s < lines[l].Length)
				{
					var c = lines[l][s];
					s++;
					if (c != ' ' && c != '\t')
						continue;
				}
				else
				{
					s = 0;
					++l;
					if (l == lines.Length)
						break;
				}
				
				w = 1;
				while (w < words.Length)
				{
					// Skip additional whitespaces
					while (s < lines[l].Length)
					{
						var c = lines[l][s];
						if (c == ' ' || c == '\t')
							++s;
						else
							break;
					}
					
					if (s == lines[l].Length)
					{
						s = 0;
						++l;
						if (l == lines.Length)
							break;
						continue;
					}
					
					if (!lines[l].Substring(s).StartsWith(words[w], System.StringComparison.Ordinal))
					{
						w = 0;
						break;
					}
					
					s += words[w].Length;
					if (s < lines[l].Length)
					{
						var c = lines[l][s];
						if (c == '_' || char.IsLetterOrDigit(c))
						{
							w = 0;
							break;
						}
					}
					
					++w;
				}
				
				if (w == words.Length)
				{
					return true;
				}
			}
		}
		catch (IOException e)
		{
			Debug.LogError(e);
		}
		return false;
	}
	
	public static void Reset()
	{
		if (assets != null)
			assets.Clear();
	}
	
	public static void FindAllAssemblyScripts(AssemblyDefinition.UnityAssembly assemblyId)
	{
		var editor = false;
		var firstPass = false;
		var pattern = "";
		
		switch (assemblyId)
		{
		case AssemblyDefinition.UnityAssembly.CSharpFirstPass:
		case AssemblyDefinition.UnityAssembly.UnityScriptFirstPass:
		case AssemblyDefinition.UnityAssembly.BooFirstPass:
		case AssemblyDefinition.UnityAssembly.CSharpEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.UnityScriptEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.BooEditorFirstPass:
			firstPass = true;
			break;
		}
		
		switch (assemblyId)
		{
		case AssemblyDefinition.UnityAssembly.CSharpFirstPass:
		case AssemblyDefinition.UnityAssembly.CSharpEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.CSharp:
		case AssemblyDefinition.UnityAssembly.CSharpEditor:
			pattern = ".cs";
			break;
		case AssemblyDefinition.UnityAssembly.UnityScriptFirstPass:
		case AssemblyDefinition.UnityAssembly.UnityScriptEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.UnityScript:
		case AssemblyDefinition.UnityAssembly.UnityScriptEditor:
			pattern = ".js";
			break;
		case AssemblyDefinition.UnityAssembly.BooFirstPass:
		case AssemblyDefinition.UnityAssembly.BooEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.Boo:
		case AssemblyDefinition.UnityAssembly.BooEditor:
			pattern = ".boo";
			break;
		}
		
		switch (assemblyId)
		{
		case AssemblyDefinition.UnityAssembly.CSharpEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.UnityScriptEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.BooEditorFirstPass:
		case AssemblyDefinition.UnityAssembly.CSharpEditor:
		case AssemblyDefinition.UnityAssembly.UnityScriptEditor:
		case AssemblyDefinition.UnityAssembly.BooEditor:
			editor = true;
			break;
		}
		
		//var scripts = FindAssets("t:MonoScript");
		var scripts = Directory.GetFiles("Assets", "*" + pattern, SearchOption.AllDirectories);
		var count = scripts.Length;
		
		if (assets == null)
			assets = new List<string>(count);
		
		for (var i = count; i --> 0; )
		{
			var path = scripts[i];// AssetDatabase.GUIDToAssetPath(scripts[i]);
			scripts[i] = path = path.Replace('\\', '/');
			string lowerPath = path.ToLowerInvariant();
			
			if (path.Contains("/.") || lowerPath.StartsWith("assets/webplayertemplates/", System.StringComparison.Ordinal))
			{
				scripts[i] = scripts[--count];
				continue;
			}
			
			scripts[i] = AssetDatabase.AssetPathToGUID(scripts[i]);
			
			var extension = Path.GetExtension(lowerPath);
			if (extension != pattern)
			{
				scripts[i] = scripts[--count];
				continue;
			}
			
			var isFirstPass = lowerPath.StartsWith("assets/standard assets/", System.StringComparison.Ordinal) ||
				lowerPath.StartsWith("assets/pro standard assets/", System.StringComparison.Ordinal) ||
				lowerPath.StartsWith("assets/plugins/", System.StringComparison.Ordinal);
			if (firstPass != isFirstPass)
			{
				scripts[i] = scripts[--count];
				continue;
			}
			
			var isEditor = false;
			if (lowerPath.StartsWith("assets/plugins/", System.StringComparison.Ordinal))
				isEditor = lowerPath.StartsWith("assets/plugins/editor/", System.StringComparison.Ordinal);
			else
				isEditor = lowerPath.Contains("/editor/");
			if (editor != isEditor)
			{
				scripts[i] = scripts[--count];
				continue;
			}
			
			assets.Add(scripts[i]);
		}
		//var joined = string.Join(", ", scripts, 0, count);
		//Debug.Log(joined);
	}
}
	
}
