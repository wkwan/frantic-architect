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
using System.Linq;


namespace ScriptInspector
{

class SnippetCompletion : SymbolDefinition
{
	protected string displayFormat = "{0}";
	protected string expandedText;
	
	public SnippetCompletion(string name)
	{
		this.name = name;
		kind = SymbolKind._Snippet;
	}
	
	public SnippetCompletion(string name, string displayFormat)
		: this(name)
	{
		this.displayFormat = displayFormat;
	}
	
	public SnippetCompletion(string name, string displayFormat, string expandTo)
		: this(name, displayFormat)
	{
		this.expandedText = expandTo;
	}
	
	public override string CompletionDisplayString(string styledName)
	{
		return string.Format(displayFormat, styledName);
	}
	
	public virtual string Expand()
	{
		return expandedText;
	}
}

interface ISnippetProvider
{
	IEnumerable<SnippetCompletion> EnumSnippets(
		SymbolDefinition context,
		FGGrammar.TokenSet expectedTokens,
		SyntaxToken tokenLeft,
		Scope scope
	);
	
	string Get(
		string shortcut,
		SymbolDefinition context,
		FGGrammar.TokenSet expectedTokens,
		Scope scope
	);
}

class CodeSnippets : AssetPostprocessor
{
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		if (snippets == null)
			return;
		
		var path = GetSnippetsPath();
		if (path == null)
			return;
		
		System.Predicate<string> snippet = x => x.StartsWith(path, System.StringComparison.OrdinalIgnoreCase);
		
		if (System.Array.Exists(importedAssets, snippet) ||
			System.Array.Exists(deletedAssets, snippet) ||
			System.Array.Exists(movedAssets, snippet) ||
			System.Array.Exists(movedFromAssetPaths, snippet))
		{
			Reload();
		}
	}
	
	private static List<ISnippetProvider> snippetsProviders;
	
	private static Dictionary<string, string> _snippets;
	private static Dictionary<string, string> snippets
	{
		get {
			if (_snippets == null)
				Reload();
			return _snippets;
		}
		set {
			_snippets = value;
		}
	}

	public static string Get(string shortcut, SymbolDefinition context, FGGrammar.TokenSet expected)
	{
		string text;
		if (!snippets.TryGetValue(shortcut, out text))
			return null;
		
		if (!IsValid(ref text, context, expected))
			return null;
		
		return text;
	}
	
	public static IEnumerable<SnippetCompletion> EnumSnippets(SymbolDefinition context, FGGrammar.TokenSet expected, SyntaxToken tokenLeft, Scope scope)
	{
		foreach (var snippet in snippets)
		{
			var text = snippet.Value;
			if (IsValid(ref text, context, expected))
				yield return new SnippetCompletion(snippet.Key + "...");
		}
		
		if (snippetsProviders == null)
		{
			snippetsProviders = new List<ISnippetProvider>();
			var types = typeof(CodeSnippets).Assembly.GetTypes();
			foreach (var type in types)
				if (typeof(ISnippetProvider).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
					try
					{
						var instance = System.Activator.CreateInstance(type) as ISnippetProvider;
						snippetsProviders.Add(instance);
					}
					catch (System.Exception e)
					{
						Debug.LogException(e);
					}
				
		}
		
		foreach (var snippetsProvider in snippetsProviders)
		{
			foreach (var snippet in snippetsProvider.EnumSnippets(context, expected, tokenLeft, scope))
				yield return snippet;
		}
	}
	
	public static void Substitute(ref string part, SymbolDefinition context)
	{
		var methodDef = context as MethodDefinition;
		
		int index = 0;
		while ((index = part.IndexOf('$', index)) != -1)
		{
			var end = part.IndexOf('$', index + 1);
			if (end > index)
			{
				var macro = part.Substring(index + 1, end - index - 1);
				part = part.Remove(index, end - index + 1);
				switch (macro)
				{
				case "MethodName":
					if (methodDef == null)
						goto default;
					part = part.Insert(index, methodDef.name);
					break;
				case "ArgumentList":
					if (methodDef == null)
						goto default;
					part = part.Insert(index, string.Join(", ",
						(
							from p in methodDef.GetParameters()
							select (p.IsOut ? "out " : p.IsRef ? "ref " : "") + p.GetName()
						).ToArray()));
					break;
				default:
					part = part.Insert(index, macro);
					break;
				}
			}
		}
	}
	
	public static bool IsValid(ref string expanded, SymbolDefinition context, FGGrammar.TokenSet expected)
	{
		var methodDef = context as MethodDefinition;
		var checkKeyword = false;
		
		for (var index = 0; (index = expanded.IndexOf('$', index)) != -1; )
		{
			var end = expanded.IndexOf('$', index + 1);
			if (end < index)
				break;
			
			var macro = expanded.Substring(index + 1, end - index - 1);
			switch (macro)
			{
			case "MethodName":
			case "ArgumentList":
				if (methodDef == null)
					return false;
				break;

			case "ValidIfKeywordExpected":
				checkKeyword = true;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "ValidAsStatement":
				if (expected != null && !expected.Matches(CsGrammar.Instance.tokenStatement))
					return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "NotValidAsStatement":
				if (expected != null && expected.Matches(CsGrammar.Instance.tokenStatement))
					return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "ValidAsClassMember":
				if (expected != null && !expected.Matches(CsGrammar.Instance.tokenClassBody))
					return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "ValidAsStructMember":
				if (expected != null && !expected.Matches(CsGrammar.Instance.tokenStructBody))
					return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "ValidAsInterfaceMember":
				if (expected != null && !expected.Matches(CsGrammar.Instance.tokenInterfaceBody))
					return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "ValidAsNamespaceMember":
				if (expected != null && !expected.Matches(CsGrammar.Instance.tokenNamespaceBody))
					return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "ValidAsTypeDeclaration":
				if (expected != null && !(
					expected.Matches(CsGrammar.Instance.tokenNamespaceBody) ||
					expected.Matches(CsGrammar.Instance.tokenClassBody) ||
					expected.Matches(CsGrammar.Instance.tokenStructBody)
				))
						return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
			
			case "ValidInMethodBody":
				if (methodDef == null)
					return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
				
			case "ValidInPropertyBody":
				if (context == null ||
					context.kind != SymbolKind.Property &&
					(context.parentSymbol == null || context.parentSymbol.kind != SymbolKind.Property))
						return false;
				expanded = expanded.Remove(index, end - index + 2);
				continue;
			}
			index = end + 1;
		}
		
		if (checkKeyword && expected != null)
		{
			var index = 0;
			while (expanded[index] >= 'a' && expanded[index] <= 'z')
				++index;
			if (index == 0)
			{
				if (expanded.StartsWith("!=", System.StringComparison.Ordinal) || expanded.StartsWith("==", System.StringComparison.Ordinal))
					index = 2;
				else
					return false;
			}
			var keyword = expanded.Substring(0, index);
			var tokenId = CsGrammar.Instance.TokenToId(keyword);
			if (!expected.Matches(tokenId))
				return false;
		}
		
		return true;
	}
	
	private static string GetSnippetsPath()
	{
		var managerScript = MonoScript.FromScriptableObject(FGTextBufferManager.instance);
		if (!managerScript)
			return null;
		var path = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(managerScript)));
		return path + "/CodeSnippets/";
	}
	
	private static void Reload()
	{
		snippets = new Dictionary<string, string>();
		
		var path = GetSnippetsPath();
		if (path == null)
			return;
		
		var all = System.IO.Directory.GetFiles(path, "*.txt");
		foreach (var asset in all)
		{
			var snippet = AssetDatabase.LoadAssetAtPath(asset, typeof(TextAsset)) as TextAsset;
			if (snippet == null)
				continue;
			snippets[snippet.name] = snippet.text.Replace("\r\n", "\n").Replace('\r', '\n');
		}
	}
}

}
