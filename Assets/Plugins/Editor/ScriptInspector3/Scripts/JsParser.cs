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

public class JsParser : FGParser
{
	public override string[] Keywords { get { return keywords; } }
	public override string[] BuiltInLiterals { get { return scriptLiterals; } }

	public override bool IsBuiltInType(string word)
	{
		return Array.BinarySearch(builtInTypes, word, StringComparer.Ordinal) >= 0;
	}

	private static readonly string[] keywords = new string[] {
		"abstract", "else", "instanceof", "super", "enum", "switch", "break", "static", "export",
		"interface", "synchronized", "extends", "let", "this", "case", "with", "throw",
		"catch", "final", "native", "throws", "finally", "new", "transient", "class",
		"const", "for", "package", "try", "continue", "private", "typeof", "debugger", "goto",
		"protected", "default", "if", "public", "delete", "implements", "return", "volatile", "do",
		"import", "while", "in", "function"
	};

	//private static readonly string[] jsPunctsAndOps = {
	//	"{", "}", ";", "#", ".", "(", ")", "[", "]", "++", "--", "->", "+", "-",
	//	"!", "~", "++", "--", "&", "*", "/", "%", "+", "-", "<<", ">>", "<", ">",
	//	"<=", ">=", "==", "!=", "&", "^", "|", "&&", "||", "??", "?", "::", ":",
	//	"=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=>"
	//};

	private static readonly HashSet<string> jsOperators = new HashSet<string>{
		"++", "--", "->", "+", "-", "!", "~", "++", "--", "&", "*", "/", "%", "+", "-", "<<", ">>", "<", ">",
		"<=", ">=", "==", "!=", "&", "^", "|", "&&", "||", "??", "?", "::", ":",
		"=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=>"
	};

	private static readonly string[] preprocessorKeywords = new string[] {
		"elif", "else", "endif", "if", "pragma"
	};

	private static readonly string[] builtInTypes = new string[] {
		"boolean", "byte", "char", "double", "float", "int", "long", "short", "var", "void"
	};

	static JsParser()
	{
		Array.Sort(keywords);

		//var all = new HashSet<string>(jsKeywords);
		//all.UnionWith(jsTypes);
		//all.UnionWith(jsPunctsAndOps);
		//all.UnionWith(scriptLiterals);
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

//		syntaxTree.SetLineTokens(currentLine, lineTokens);
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
						var regionKind = formatedLine.regionTree.kind;
						var inactiveLine = regionKind > FGTextBuffer.RegionTree.Kind.LastActive;
						token.style = inactiveLine ? textBuffer.styles.inactiveCodeStyle : textBuffer.styles.commentStyle;
						break;

					case SyntaxToken.Kind.Preprocessor:
						token.style = textBuffer.styles.preprocessorStyle;
						break;

					case SyntaxToken.Kind.PreprocessorSymbol:
						token.style = textBuffer.styles.defineSymbols;
						break;

					case SyntaxToken.Kind.PreprocessorArguments:
					case SyntaxToken.Kind.PreprocessorCommentExpected:
					case SyntaxToken.Kind.PreprocessorDirectiveExpected:
					case SyntaxToken.Kind.PreprocessorUnexpectedDirective:
						token.style = textBuffer.styles.normalStyle;
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

		if (formatedLine.blockState == FGTextBuffer.BlockState.None && startAt < length && line[startAt] == '#')
		{
			tokens.Add(new SyntaxToken(SyntaxToken.Kind.Preprocessor, "#") { formatedLine = formatedLine });
			++startAt;

			//ws = ScanWhitespace(line, ref startAt);
			//if (ws != null)
			//{
			//	tokens.Add(ws);
			//	ws.formatedLine = formatedLine;
			//}

			var error = false;
			var commentsOnly = false;
			var preprocessorCommentsAllowed = true;
			
			token = ScanWord(line, ref startAt);
			if (Array.BinarySearch(preprocessorKeywords, token.text) < 0)
			{
				token.tokenKind = SyntaxToken.Kind.PreprocessorDirectiveExpected;
				tokens.Add(token);
				token.formatedLine = formatedLine;
				
				error = true;
			}
			else
			{
				token.tokenKind = SyntaxToken.Kind.Preprocessor;
				tokens.Add(token);
				token.formatedLine = formatedLine;
	
				ws = ScanWhitespace(line, ref startAt);
				if (ws != null)
				{
					tokens.Add(ws);
					ws.formatedLine = formatedLine;
				}

				if (token.text == "if")
				{
					bool active = ParsePPOrExpression(line, formatedLine, ref startAt);
					bool inactiveParent = formatedLine.regionTree.kind > FGTextBuffer.RegionTree.Kind.LastActive;
					if (active && !inactiveParent)
					{
						OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.If);
						commentsOnly = true;
					}
					else
					{
						OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.InactiveIf);
						commentsOnly = true;
					}
				}
				else if (token.text == "elif")
				{
					bool active = ParsePPOrExpression(line, formatedLine, ref startAt);
					bool inactiveParent = formatedLine.regionTree.kind > FGTextBuffer.RegionTree.Kind.LastActive;
					if (formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.If ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.Elif ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveElif)
					{
						OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.InactiveElif);
					}
					else if (formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveIf)
					{
						inactiveParent = formatedLine.regionTree.parent.kind > FGTextBuffer.RegionTree.Kind.LastActive;
						if (active && !inactiveParent)
						{
							OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.Elif);
						}
						else
						{
							OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.InactiveElif);
						}
					}
					else
					{
						token.tokenKind = SyntaxToken.Kind.PreprocessorUnexpectedDirective;
					}
				}
				else if (token.text == "else")
				{
					if (formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.If ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.Elif)
					{
						OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.InactiveElse);
					}
					else if (formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveIf ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveElif)
					{
						bool inactiveParent = formatedLine.regionTree.parent.kind > FGTextBuffer.RegionTree.Kind.LastActive;
						if (inactiveParent)
							OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.InactiveElse);
						else
							OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.Else);
					}
					else
					{
						token.tokenKind = SyntaxToken.Kind.PreprocessorUnexpectedDirective;
					}
				}
				else if (token.text == "endif")
				{
					if (formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.If ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.Elif ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.Else ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveIf ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveElif ||
						formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveElse)
					{
						CloseRegion(formatedLine);
					}
					else
					{
						token.tokenKind = SyntaxToken.Kind.PreprocessorUnexpectedDirective;
					}
				}
				//else if (token.text == "region")
				//{
				//	var inactive = formatedLine.regionTree.kind > FGTextBuffer.RegionTree.Kind.LastActive;
				//	if (inactive)
				//	{
				//		OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.InactiveRegion);
				//	}
				//	else
				//	{
				//		OpenRegion(formatedLine, FGTextBuffer.RegionTree.Kind.Region);
				//	}
				//	preprocessorCommentsAllowed = false;
				//}
				//else if (token.text == "endregion")
				//{
				//	if (formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.Region ||
				//		formatedLine.regionTree.kind == FGTextBuffer.RegionTree.Kind.InactiveRegion)
				//	{
				//		CloseRegion(formatedLine);
				//	}
				//	else
				//	{
				//		token.tokenKind = SyntaxToken.Kind.PreprocessorUnexpectedDirective;
				//	}
				//	preprocessorCommentsAllowed = false;
				//}
				//else if (token.text == "define" || token.text == "undef")
				//{
				//	var symbol = FGParser.ScanIdentifierOrKeyword(line, ref startAt);
				//	if (symbol != null && symbol.text != "true" && symbol.text != "false")
				//	{
				//		symbol.tokenKind = SyntaxToken.Kind.PreprocessorSymbol;
				//		formatedLine.tokens.Add(symbol);
				//		symbol.formatedLine = formatedLine;

				//		scriptDefinesChanged = true;
						
				//		var inactive = formatedLine.regionTree.kind > FGTextBuffer.RegionTree.Kind.LastActive;
				//		if (!inactive)
				//		{
				//			if (token.text == "define")
				//			{
				//				if (!scriptDefines.Contains(symbol.text))
				//				{
				//					scriptDefines.Add(symbol.text);
				//					//scriptDefinesChanged = true;
				//				}
				//			}
				//			else
				//			{
				//				if (scriptDefines.Contains(symbol.text))
				//				{
				//					scriptDefines.Remove(symbol.text);
				//					//scriptDefinesChanged = true;
				//				}
				//			}
				//		}
				//	}
				//}
				//else if (token.text == "error" || token.text == "warning")
				//{
				//	preprocessorCommentsAllowed = false;
				//}
			}
			
			if (!preprocessorCommentsAllowed)
			{
				ws = ScanWhitespace(line, ref startAt);
				if (ws != null)
				{
					tokens.Add(ws);
					ws.formatedLine = formatedLine;
				}
				if (startAt < length)
				{
					var textArgument = line.Substring(startAt);
					textArgument.TrimEnd(new [] {' ', '\t'});
					tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, textArgument) { formatedLine = formatedLine });
					startAt = length - textArgument.Length;
					if (startAt < length)
						tokens.Add(new SyntaxToken(SyntaxToken.Kind.Whitespace, line.Substring(startAt)) { formatedLine = formatedLine });
				}
				return;
			}
			
			while (startAt < length)
			{
				ws = ScanWhitespace(line, ref startAt);
				if (ws != null)
				{
					tokens.Add(ws);
					ws.formatedLine = formatedLine;
					continue;
				}
				
				var firstChar = line[startAt];
				if (startAt < length - 1 && firstChar == '/' && line[startAt + 1] == '/')
				{
					tokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, line.Substring(startAt)) { formatedLine = formatedLine });
					break;
				}
				else if (commentsOnly)
				{
					tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorCommentExpected, line.Substring(startAt)) { formatedLine = formatedLine });
					break;						
				}
				
				if (char.IsLetterOrDigit(firstChar) || firstChar == '_')
				{
					token = ScanWord(line, ref startAt);
					token.tokenKind = SyntaxToken.Kind.PreprocessorArguments;
					tokens.Add(token);
					token.formatedLine = formatedLine;
				}
				else if (firstChar == '"')
				{
					token = ScanStringLiteral(line, ref startAt);
					token.tokenKind = SyntaxToken.Kind.PreprocessorArguments;
					tokens.Add(token);
					token.formatedLine = formatedLine;
				}
				else if (firstChar == '\'')
				{
					token = ScanCharLiteral(line, ref startAt);
					token.tokenKind = SyntaxToken.Kind.PreprocessorArguments;
					tokens.Add(token);
					token.formatedLine = formatedLine;
				}
				else
				{
					token = new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, firstChar.ToString()) { formatedLine = formatedLine };
					tokens.Add(token);
					++startAt;
				}
				
				if (error)
				{
					token.tokenKind = SyntaxToken.Kind.PreprocessorDirectiveExpected;
				}
			}
			
			return;
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
						token = ScanStringLiteral(line, ref startAt);
						tokens.Add(token);
						token.formatedLine = formatedLine;
						break;
					}

					//if (startAt < length - 1 && line[startAt] == '@' && line[startAt + 1] == '\"')
					//{
					//	token = new SyntaxToken(SyntaxToken.Kind.VerbatimStringBegin, line.Substring(startAt, 2)) { formatedLine = formatedLine };
					//	tokens.Add(token);
					//	startAt += 2;
					//	formatedLine.blockState = FGTextBuffer.BlockState.StringBlock;
					//	break;
					//}

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

				//case FGTextBuffer.BlockState.StringBlock:
				//	int i = startAt;
				//	int closingQuote = line.IndexOf('\"', startAt);
				//	while (closingQuote != -1 && closingQuote < length - 1 && line[closingQuote + 1] == '\"')
				//	{
				//		i = closingQuote + 2;
				//		closingQuote = line.IndexOf('\"', i);
				//	}
				//	if (closingQuote == -1)
				//	{
				//		tokens.Add(new SyntaxToken(SyntaxToken.Kind.VerbatimStringLiteral, line.Substring(startAt)) { formatedLine = formatedLine });
				//		startAt = length;
				//	}
				//	else
				//	{
				//		tokens.Add(new SyntaxToken(SyntaxToken.Kind.VerbatimStringLiteral, line.Substring(startAt, closingQuote - startAt)) { formatedLine = formatedLine });
				//		startAt = closingQuote;
				//		tokens.Add(new SyntaxToken(SyntaxToken.Kind.VerbatimStringLiteral, line.Substring(startAt, 1)) { formatedLine = formatedLine });
				//		++startAt;
				//		formatedLine.blockState = FGTextBuffer.BlockState.None;
				//	}
				//	break;
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
		return jsOperators.Contains(text);
	}
}

}
