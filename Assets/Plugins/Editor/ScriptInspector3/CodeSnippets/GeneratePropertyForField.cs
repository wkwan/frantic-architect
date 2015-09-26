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
using System.Collections.Generic;

namespace ScriptInspector
{
	
class GeneratePropertyForField : ISnippetProvider
{
	class MyCompletion : SnippetCompletion
	{
		SymbolDefinition field;
		bool withSetter;
		
		public MyCompletion(string propertyName, SymbolDefinition field, bool withSetter)
			: base(propertyName, withSetter ? "{0} {{ get {{...}} set {{...}} }}" : "{0} {{ get {{...}} }}")
		{
			this.field = field;
			this.withSetter = withSetter;
		}
		
		public override string Expand()
		{
			var field = this.field.Rebind() ?? this.field;
			var declaration = field.declarations.FirstOrDefault();
			if (declaration == null)
			{
#if SI3_WARNINGS
				Debug.LogError("Could not find declaration for field " + field.name);
#endif
				return "";
			}
			var context = declaration.scope;
			
			var type = field.TypeOf();
			var accessLevelModifier = type.accessLevel.ToCSharpString();
			if (accessLevelModifier == "private")
				accessLevelModifier = "internal";
			var staticModifier = field.IsStatic ? "static " : "";
			var typeName = field.TypeOf().RelativeName(context);
			
			var format =
				"{0} {1}{2} {3} {{\n" +
				"\tget {{ return {4}; }}\n" +
				(withSetter ?
					"\tset {{ {4} = value;$end$ }}\n}}" :
					"}}$end$");
			return string.Format(format, accessLevelModifier, staticModifier, typeName, name, field.name);
		}
	}
	
	public IEnumerable<SnippetCompletion> EnumSnippets(
		SymbolDefinition context,
		FGGrammar.TokenSet expectedTokens,
		SyntaxToken tokenLeft,
		Scope scope)
	{
		if (scope == null)
			yield break;
		
		var bodyScope = scope as BodyScope ?? scope.parentScope as BodyScope;
		if (bodyScope == null)
			yield break;
		
		var contextType = bodyScope.definition as TypeDefinitionBase;
		if (contextType == null || contextType.kind != SymbolKind.Class && contextType.kind != SymbolKind.Struct)
			yield break;
		
		context = contextType;
		
		if (tokenLeft == null || tokenLeft.parent == null || tokenLeft.parent.parent == null)
			yield break;
		
		if (tokenLeft.tokenKind != SyntaxToken.Kind.Punctuator)
			yield break;
		
		if (tokenLeft.text != "{" && tokenLeft.text != "}" && tokenLeft.text != ";")
			yield break;
		
		for (var i = contextType.members.Count; i --> 0; )
		{
			var member = contextType.members[i];
			if (member.kind == SymbolKind.Field)
			{
				var type = member.TypeOf();
				if (type == null || type.kind == SymbolKind.Error || !(type is TypeDefinitionBase))
					continue;
				
				var fieldName = member.name;
				if (string.IsNullOrEmpty(fieldName) || fieldName[0] == '.')
					continue;
				
				fieldName = char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
				var propertyName = fieldName;
				for (var suffix = 1; contextType.members.Contains(propertyName, -1); ++suffix)
					propertyName = fieldName + suffix.ToString();
				
				yield return new MyCompletion(propertyName, member, true);// "{0} {{ get {{...}} set {{...}} }}");
				yield return new MyCompletion(propertyName, member, false);// "{0} {{ get {{...}} }}");
			}
		}
	}
	
	public string Get(
		string shortcut,
		SymbolDefinition context,
		FGGrammar.TokenSet expectedTokens,
		Scope scope)
	{
		return null;
	}
}
	
}
