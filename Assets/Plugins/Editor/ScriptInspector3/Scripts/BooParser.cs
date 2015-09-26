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


namespace ScriptInspector
{
using System;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public class BooParser : FGParser
{
	public override string[] Keywords { get { return keywords; } }
	public override string[] BuiltInLiterals { get { return scriptLiterals; } }

	public override bool IsBuiltInType(string word)
	{
		return Array.BinarySearch(builtInTypes, word, StringComparer.Ordinal) >= 0;
	}

	private static readonly string[] keywords = new string[] {
		"abstract", "and", "as", "break", "callable", "cast", "class", "const", "constructor", "destructor", "continue",
		"def", "do", "elif", "else", "enum", "ensure", "event", "except", "final", "for", "from", "given", "get", "goto",
		"if", "interface", "in", "include", "import", "is", "isa", "mixin", "namespace", "not", "or", "otherwise",
		"override", "pass", "raise", "retry", "self", "struct", "return", "set", "success", "try", "transient", "virtual",
		"while", "when", "unless", "yield", 
		"public", "protected", "private", "internal", "static", 
		
		// builtin
		"len", "__addressof__", "__eval__", "__switch__", "array", "matrix", "typeof", "assert", "print", "gets", "prompt", 
		"enumerate", "zip", "filter", "map",
	};

	private static readonly HashSet<string> operators = new HashSet<string>{
		"++", "--", "->", "+", "-", "!", "~", "++", "--", "&", "*", "/", "%", "+", "-", "<<", ">>", "<", ">",
		"<=", ">=", "==", "!=", "&", "^", "|", "&&", "||", "??", "?", "::", ":",
		"=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=>"
	};

	private static readonly string[] builtInTypes = new string[] {
		"bool", "byte", "char", "date", "decimal", "double", "int", "long", "object", "sbyte", "short", "single", "string",
		"timespan", "uint", "ulong", "ushort", "void"
	};

	static BooParser()
	{
		Array.Sort(keywords);
	}

	public override void LexLine(int currentLine, FGTextBuffer.FormatedLine formatedLine)
	{
		formatedLine.index = currentLine;

		if (parserThread != null)
			parserThread.Join();
		parserThread = null;

		string textLine = textBuffer.lines[currentLine];

		//Stopwatch sw1 = new Stopwatch();
		//Stopwatch sw2 = new Stopwatch();
		
		if (currentLine == 0)
		{
			var defaultScriptDefines = UnityEditor.EditorUserBuildSettings.activeScriptCompilationDefines;
			if (scriptDefines == null || !scriptDefines.SetEquals(defaultScriptDefines))
			{
				if (scriptDefines == null)
				{
					scriptDefines = new HashSet<string>(defaultScriptDefines);
				}
				else
				{
					scriptDefines.Clear();
					scriptDefines.UnionWith(defaultScriptDefines);
				}
			}
		}
		
		//sw2.Start();
		Tokenize(textLine, formatedLine);

		var lineTokens = formatedLine.tokens;

		if (textLine.Length == 0)
		{
			formatedLine.tokens.Clear();
		}
		else if (textBuffer.styles != null)
		{
			var lineWidth = textBuffer.CharIndexToColumn(textLine.Length, currentLine);
			if (lineWidth > textBuffer.longestLine)
				textBuffer.longestLine = lineWidth;

			for (var i = 0; i < lineTokens.Count; ++i)
			{
				var token = lineTokens[i];
				switch (token.tokenKind)
				{
					case SyntaxToken.Kind.Whitespace:
					case SyntaxToken.Kind.Missing:
						token.style = textBuffer.styles.normalStyle;
						break;

					case SyntaxToken.Kind.Punctuator:
						token.style = IsOperator(token.text) ? textBuffer.styles.operatorStyle : textBuffer.styles.normalStyle;
						break;

					case SyntaxToken.Kind.Keyword:
						if (IsBuiltInType(token.text))
						{
							if (token.text == "string" || token.text == "object")
								token.style = textBuffer.styles.builtInRefTypeStyle;
							else
								token.style = textBuffer.styles.builtInValueTypeStyle;
						}
						else
						{
							token.style = textBuffer.styles.keywordStyle;
						}
						break;

					case SyntaxToken.Kind.Identifier:
						if (IsBuiltInLiteral(token.text))
						{
							token.style = textBuffer.styles.builtInLiteralsStyle;
							token.tokenKind = SyntaxToken.Kind.BuiltInLiteral;
						}
						else if (IsUnityType(token.text))
						{
							token.style = textBuffer.styles.referenceTypeStyle;
						}
						else
						{
							token.style = textBuffer.styles.normalStyle;
						}
						break;

					case SyntaxToken.Kind.IntegerLiteral:
					case SyntaxToken.Kind.RealLiteral:
						token.style = textBuffer.styles.constantStyle;
						break;

					case SyntaxToken.Kind.Comment:
						token.style = textBuffer.styles.commentStyle;
						break;

					case SyntaxToken.Kind.CharLiteral:
					case SyntaxToken.Kind.StringLiteral:
					case SyntaxToken.Kind.VerbatimStringBegin:
					case SyntaxToken.Kind.VerbatimStringLiteral:
						token.style = textBuffer.styles.stringStyle;
						break;
				}
				lineTokens[i] = token;
			}
		}
	}
	
	protected override void Tokenize(string line, FGTextBuffer.FormatedLine formatedLine)
	{
		var tokens = new List<SyntaxToken>();
		formatedLine.tokens = tokens;

		int startAt = 0;
		int length = line.Length;
		SyntaxToken token;

		SyntaxToken ws = ScanWhitespace(line, ref startAt);
		if (ws != null)
		{
			tokens.Add(ws);
			ws.formatedLine = formatedLine;
		}

		var inactiveLine = formatedLine.regionTree.kind > FGTextBuffer.RegionTree.Kind.LastActive;
		
		while (startAt < length)
		{
			switch (formatedLine.blockState)
			{
				case FGTextBuffer.BlockState.None:
					ws = ScanWhitespace(line, ref startAt);
					if (ws != null)
					{
						tokens.Add(ws);
						ws.formatedLine = formatedLine;
						continue;
					}
					
					if (inactiveLine)
					{
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, line.Substring(startAt)) { formatedLine = formatedLine });
						startAt = length;
						break;
					}

					if (line[startAt] == '#')
					{
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, "#") { formatedLine = formatedLine });
						++startAt;
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, line.Substring(startAt)) { formatedLine = formatedLine });
						startAt = length;
						break;
					}
					
					if (line[startAt] == '/' && startAt < length - 1)
					{
						if (line[startAt + 1] == '/')
						{
							tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, "//") { formatedLine = formatedLine });
							startAt += 2;
							tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, line.Substring(startAt)) { formatedLine = formatedLine });
							startAt = length;
							break;
						}
						else if (line[startAt + 1] == '*')
						{
							tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, "/*") { formatedLine = formatedLine });
							startAt += 2;
							formatedLine.blockState = FGTextBuffer.BlockState.CommentBlock;
							break;
						}
					}

					if (line[startAt] == '\'')
					{
						token = ScanCharLiteral(line, ref startAt);
						tokens.Add(token);
						token.formatedLine = formatedLine;
						break;
					}

					if (line[startAt] == '\"')
					{
						if (startAt < length - 2 && line[startAt + 1] == '\"' && line[startAt + 2] == '\"')
						{
							token = new SyntaxToken(SyntaxToken.Kind.VerbatimStringBegin, line.Substring(startAt, 3)) { formatedLine = formatedLine };
							tokens.Add(token);
							startAt += 3;
							formatedLine.blockState = FGTextBuffer.BlockState.StringBlock;
							break;
						}
	
						token = ScanStringLiteral(line, ref startAt);
						tokens.Add(token);
						token.formatedLine = formatedLine;
						break;
					}

					if (line[startAt] >= '0' && line[startAt] <= '9'
					    || startAt < length - 1 && line[startAt] == '.' && line[startAt + 1] >= '0' && line[startAt + 1] <= '9')
					{
						token = ScanNumericLiteral_JS(line, ref startAt);
						tokens.Add(token);
						token.formatedLine = formatedLine;
						break;
					}

					token = ScanIdentifierOrKeyword(line, ref startAt);
					if (token != null)
					{
						tokens.Add(token);
						token.formatedLine = formatedLine;
						break;
					}

					// Multi-character operators / punctuators
					// "++", "--", "<<", ">>", "<=", ">=", "==", "!=", "&&", "||", "??", "+=", "-=", "*=", "/=", "%=",
					// "&=", "|=", "^=", "<<=", ">>=", "=>", "::", "->", "^="
					var punctuatorStart = startAt++;
					if (startAt < line.Length)
					{
						switch (line[punctuatorStart])
						{
							case '?':
								if (line[startAt] == '?')
									++startAt;
								break;
							case '+':
								if (line[startAt] == '+' || line[startAt] == '=' || line[startAt] == '>')
									++startAt;
								break;
							case '-':
								if (line[startAt] == '-' || line[startAt] == '=')
									++startAt;
								break;
							case '<':
								if (line[startAt] == '=')
									++startAt;
								else if (line[startAt] == '<')
								{
									++startAt;
									if (startAt < line.Length && line[startAt] == '=')
										++startAt;
								}
								break;
							case '>':
								if (line[startAt] == '=')
									++startAt;
								else if (startAt < line.Length && line[startAt] == '>')
								{
								    ++startAt;
								    if (line[startAt] == '=')
								        ++startAt;
								}
								break;
							case '=':
								if (line[startAt] == '=' || line[startAt] == '>')
									++startAt;
								break;
							case '&':
								if (line[startAt] == '=' || line[startAt] == '&')
									++startAt;
								break;
							case '|':
								if (line[startAt] == '=' || line[startAt] == '|')
									++startAt;
								break;
							case '*':
							case '/':
							case '%':
							case '^':
							case '!':
								if (line[startAt] == '=')
									++startAt;
								break;
							case ':':
								if (line[startAt] == ':')
									++startAt;
								break;
						}
					}
					tokens.Add(new SyntaxToken(SyntaxToken.Kind.Punctuator, line.Substring(punctuatorStart, startAt - punctuatorStart)) { formatedLine = formatedLine });
					break;

				case FGTextBuffer.BlockState.CommentBlock:
					int commentBlockEnd = line.IndexOf("*/", startAt, StringComparison.Ordinal);
					if (commentBlockEnd == -1)
					{
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, line.Substring(startAt)) { formatedLine = formatedLine });
						startAt = length;
					}
					else
					{
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, line.Substring(startAt, commentBlockEnd + 2 - startAt)) { formatedLine = formatedLine });
						startAt = commentBlockEnd + 2;
						formatedLine.blockState = FGTextBuffer.BlockState.None;
					}
					break;

				case FGTextBuffer.BlockState.StringBlock:
					int i = startAt;
					int closingQuote = line.IndexOf('\"', startAt);
					while (closingQuote != -1 && closingQuote < length - 2 &&
						(line[closingQuote + 1] != '\"' || line[closingQuote + 2] != '\"'))
					{
						i = closingQuote + 2;
						closingQuote = line.IndexOf('\"', i);
					}
					if (closingQuote == -1)
					{
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.VerbatimStringLiteral, line.Substring(startAt)) { formatedLine = formatedLine });
						startAt = length;
					}
					else
					{
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.VerbatimStringLiteral, line.Substring(startAt, closingQuote - startAt)) { formatedLine = formatedLine });
						startAt = closingQuote;
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.VerbatimStringLiteral, line.Substring(startAt, 3)) { formatedLine = formatedLine });
						startAt += 3;
						formatedLine.blockState = FGTextBuffer.BlockState.None;
					}
					break;
			}
		}
	}

	private new SyntaxToken ScanIdentifierOrKeyword(string line, ref int startAt)
	{
		bool identifier = false;
		int i = startAt;
		if (i >= line.Length)
			return null;
		
		char c = line[i];

		if (!char.IsLetter(c) && c != '_')
			return null;	
		++i;

		while (i < line.Length)
		{
			if (char.IsLetterOrDigit(line, i) || line[i] == '_')
				++i;
			else
				break;
		}
		
		var word = line.Substring(startAt, i - startAt);
		startAt = i;
		var token = new SyntaxToken(identifier ? SyntaxToken.Kind.Identifier : SyntaxToken.Kind.Keyword, word);
		
		if (token.tokenKind == SyntaxToken.Kind.Keyword && !IsKeyword(token.text) && !IsBuiltInType(token.text))
			token.tokenKind = SyntaxToken.Kind.Identifier;
		return token;
	}

	private bool IsKeyword(string word)
	{
		return Array.BinarySearch(Keywords, word, StringComparer.Ordinal) >= 0;
	}

	private bool IsOperator(string text)
	{
		return operators.Contains(text);
	}
}

}
