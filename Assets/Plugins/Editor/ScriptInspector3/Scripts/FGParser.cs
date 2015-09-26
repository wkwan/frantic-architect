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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;


namespace ScriptInspector
{

using FormatedLine = FGTextBuffer.FormatedLine;
using Debug = UnityEngine.Debug;

public struct TextPosition
{
	public int line;
	public int index;

	public TextPosition(int line, int index)
	{
		this.line = line;
		this.index = index;
	}

	public static TextPosition operator + (TextPosition other, int offset)
	{
		return new TextPosition { line = other.line, index = other.index + offset };
	}

	public static bool operator == (TextPosition lhs, TextPosition rhs)
	{
		return lhs.line == rhs.line && lhs.index == rhs.index;
	}
	
	public static bool operator != (TextPosition lhs, TextPosition rhs)
	{
		return lhs.line != rhs.line || lhs.index != rhs.index;
	}
	
	public static bool operator < (TextPosition lhs, TextPosition rhs)
	{
		return lhs.line < rhs.line || lhs.line == rhs.line && lhs.index < rhs.index;
	}

	public static bool operator <= (TextPosition lhs, TextPosition rhs)
	{
		return lhs.line < rhs.line || lhs.line == rhs.line && lhs.index <= rhs.index;
	}
	
	public static bool operator > (TextPosition lhs, TextPosition rhs)
	{
		return lhs.line > rhs.line || lhs.line == rhs.line && lhs.index > rhs.index;
	}

	public static bool operator >= (TextPosition lhs, TextPosition rhs)
	{
		return lhs.line > rhs.line || lhs.line == rhs.line && lhs.index >= rhs.index;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TextPosition))
			return false;

		var rhs = (TextPosition) obj;
		return line == rhs.line && index == rhs.index;
	}

	public override int GetHashCode()
	{
		unchecked // Overflow is fine, just wrap
		{
			var hash = (int)2166136261;
			hash = hash * 16777619 ^ line.GetHashCode();
			hash = hash * 16777619 ^ index.GetHashCode();
			return hash;
		}
	}

	public bool Move(FGTextBuffer textBuffer, int offset)
	{
		while (offset > 0)
		{
			var lineLength = textBuffer.lines[line].Length;
			if (index + offset <= lineLength)
			{
				index += offset;
				if (index == lineLength)
				{
					index = 0;
					++line;
				}
				return true;
			}

			offset -= lineLength - index;
			++line;
			index = 0;

			if (line >= textBuffer.lines.Count)
			{
				line = textBuffer.lines.Count;
				index = 0;
				return false;
			}
		}

		while (offset < 0)
		{
			if (index + offset >= 0)
			{
				index += offset;
				return true;
			}

			offset += index;
			--line;
			if (line < 0)
			{
				line = 0;
				index = 0;
				return false;
			}
			index = textBuffer.lines[line].Length;
		}

		return true;
	}

	public override string ToString()
	{
		return "TextPosition (line: " + line + ", index: " + index + ")";
	}
}

public struct TextOffset
{
	public int lines;
	public int indexOffset;
}

public struct TextSpan
{
	public int line;
	public int index;
	public int lineOffset;
	public int indexOffset;

	public override string ToString()
	{
		return "TextSpan{ line = " + (line+1) + ", fromChar = " + index + ", lineOffset = " + lineOffset + ", toChar = " + indexOffset + " }";
	}

	public static TextSpan CreateEmpty(TextPosition position)
	{
		return new TextSpan { line = position.line, index = position.index };
	}

	public static TextSpan Create(TextPosition from, TextPosition to)
	{
		return new TextSpan
		{
			line = from.line,
			index = from.index,
			lineOffset = to.line - from.line,
			indexOffset = to.index - (to.line == from.line ? from.index : 0)
		};
	}

	public static TextSpan CreateBetween(TextSpan from, TextSpan to)
	{
		return Create(from.EndPosition, to.StartPosition);
	}

	public static TextSpan CreateEnclosing(TextSpan from, TextSpan to)
	{
		return Create(from.StartPosition, to.EndPosition);
	}

	public static TextSpan Create(TextPosition start, TextOffset length)
	{
		return new TextSpan
		{
			line = start.line,
			index = start.index,
			lineOffset = length.lines,
			indexOffset = length.indexOffset
		};
	}

	public TextPosition StartPosition
	{
		get { return new TextPosition { line = line, index = index }; }
		set
		{
			if (value.line == line + lineOffset)
			{
				line = value.line;
				lineOffset = 0;
				indexOffset = index + indexOffset - value.index;
				index = value.index;
			}
			else
			{
				lineOffset = line + lineOffset - value.line;
				line = value.line;
				index = value.index;
			}
		}
	}

	public TextPosition EndPosition
	{
		get { return new TextPosition { line = line + lineOffset, index = indexOffset + (lineOffset == 0 ? index : 0) }; }
		set
		{
			if (value.line == line)
			{
				lineOffset = 0;
				indexOffset = value.index - index;
			}
			else
			{
				lineOffset = value.line - line;
				indexOffset = value.index;
			}
		}
	}

	public void Offset(int deltaLines, int deltaIndex)
	{
		line += deltaLines;
		index += deltaIndex;
	}

	public bool Contains(TextPosition position)
	{
		return !(position.line < line
			|| position.line == line && (position.index < index || lineOffset == 0 && position.index > index + indexOffset)
			|| position.line > line + lineOffset
			|| position.line == line + lineOffset && position.index > indexOffset);
	}
}

public class SyntaxToken //: IComparable<SyntaxToken>
{
	public enum Kind
	{
		Missing,
		Whitespace,
		Comment,
		Preprocessor,
		PreprocessorArguments,
		PreprocessorSymbol,
		PreprocessorDirectiveExpected,
		PreprocessorCommentExpected,
		PreprocessorUnexpectedDirective,
		VerbatimStringLiteral,
		
		LastWSToken, // Marker only
		
		VerbatimStringBegin,
		BuiltInLiteral,
		CharLiteral,
		StringLiteral,
		IntegerLiteral,
		RealLiteral,
		Punctuator,
		Keyword,
		Identifier,
		ContextualKeyword,
		EOF,
	}

	public Kind tokenKind;
	public GUIStyle style;
	public ParseTree.Leaf parent;
	//public TextSpan textSpan;
	public string text;
	public int tokenId;

	public FormatedLine formatedLine;

	public int Line { get { return formatedLine.index; } }
	public int TokenIndex { get { return formatedLine.tokens.IndexOf(this); } }

	public static SyntaxToken CreateMissing()
	{
		return new SyntaxToken(Kind.Missing, string.Empty) { parent = null };
	}

	public SyntaxToken(Kind kind, string text)
	{
		parent = null;
		tokenKind = kind;
		this.text = string.Intern(text);
		tokenId = -1;
		style = null;
	}

	public bool IsMissing()
	{
		return tokenKind == Kind.Missing;
	}

	public override string ToString() { return tokenKind +"(\"" + text + "\")"; }

	public string Dump() { return "[Token: " + tokenKind + " \"" + text + "\"]"; }

//	public int CompareTo(SyntaxToken other)
//	{
//		var t = tokenKind.GetHashCode().CompareTo(tokenKind.GetHashCode());
//		return t != 0 ? t : text.CompareTo(other.text);
//	}
}

[UnityEditor.InitializeOnLoad]
public abstract class FGParser
{
	protected static readonly char[] whitespaces = { ' ', '\t' };
	//protected static readonly Regex emailRegex = new Regex(@"\b([A-Z0-9._%-]+)@([A-Z0-9.-]+\.[A-Z]{2,6})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static readonly Dictionary<string, Type> parserTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

	public static FGParser Create(FGTextBuffer textBuffer, string path)
	{
		Type parserType;
		FGParser parser;
		var extension = Path.GetExtension(path) ?? String.Empty;
		if (!path.StartsWith("assets/webplayertemplates/", StringComparison.OrdinalIgnoreCase)
			&& parserTypes.TryGetValue(extension, out parserType))
		{
			parser = (FGParser) Activator.CreateInstance(parserType);
		}
		else
		{
			parser = new TextParser();
		}
		
		parser.textBuffer = textBuffer;
		parser.assetPath = path;
		return parser;
	}

	private static void RegisterParsers()
	{
		parserTypes.Add(".cs", typeof(CsParser));
		parserTypes.Add(".js", typeof(JsParser));
		parserTypes.Add(".boo", typeof(BooParser));
		
		parserTypes.Add(".shader", typeof(ShaderParser));
		parserTypes.Add(".cg", typeof(ShaderParser));
		parserTypes.Add(".cginc", typeof(ShaderParser));
		parserTypes.Add(".hlsl", typeof(ShaderParser));
		parserTypes.Add(".hlslinc", typeof(ShaderParser));
		
		parserTypes.Add(".txt", typeof(TextParser));
	}

	static FGParser()
	{
		RegisterParsers();
		
		unityTypes = new HashSet<string>();
		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (var assembly in assemblies)
		{
			try
			{
				if (assembly is System.Reflection.Emit.AssemblyBuilder)
					continue;
				
				var takeAllTypes = AssemblyDefinition.IsScriptAssemblyName(assembly.GetName().Name);
				var assemblyTypes = takeAllTypes ? assembly.GetTypes() : assembly.GetExportedTypes();
				foreach (var type in assemblyTypes)
				{
					var name = type.Name;
					var index = name.IndexOf('`');
					if (index >= 0)
						name = name.Remove(index);
					unityTypes.Add(name);
					if (type.IsSubclassOf(typeof(Attribute)) && name.EndsWith("Attribute", StringComparison.Ordinal))
						unityTypes.Add(type.Name.Substring(0, type.Name.Length - "Attribute".Length));
				}
			}
			catch (ReflectionTypeLoadException)
			{
				Debug.LogWarning("Error reading types from assembly " + assembly.FullName);
			}
		}
	}


	// Instance members

	protected string assetPath;

	protected FGTextBuffer textBuffer;
	public ParseTree parseTree { get; protected set; }

	public HashSet<string> scriptDefines;
	public bool scriptDefinesChanged;
	
	public void OnLoaded()
	{
		scriptDefines = new HashSet<string>(UnityEditor.EditorUserBuildSettings.activeScriptCompilationDefines);
		scriptDefinesChanged = false;
		
		ParseAll(assetPath);
	}

	public virtual FGGrammar.IScanner MoveAfterLeaf(ParseTree.Leaf leaf)
	{
		return null;
	}

	public virtual bool ParseLines(int fromLine, int toLineInclusive)
	{
		return true;
	}

	protected Thread parserThread;
	
	public virtual void FullRefresh()
	{
		if (parserThread != null)
			parserThread.Join();
		parserThread = null;
	}

	public virtual void LexLine(int currentLine, FormatedLine formatedLine)
	{
		formatedLine.index = currentLine;

		if (parserThread != null)
			parserThread.Join();
		parserThread = null;

		string textLine = textBuffer.lines[currentLine];
		var lineTokens = new List<SyntaxToken>();

		if (textLine.Length == 0)
		{
			formatedLine.tokens = lineTokens;
		}
		else
		{
			//Tokenize(lineTokens, textLine, ref formatedLine.blockState);
			lineTokens.Add(new SyntaxToken(SyntaxToken.Kind.Comment, textLine) { style = textBuffer.styles.normalStyle, formatedLine = formatedLine });

			formatedLine.tokens = lineTokens;

			var lineWidth = textBuffer.CharIndexToColumn(textLine.Length, currentLine);
			if (lineWidth > textBuffer.longestLine)
				textBuffer.longestLine = lineWidth;
		}
	}
	
	protected virtual void Tokenize(string line, FGTextBuffer.FormatedLine formatedLine)
	{
	}
	
	protected virtual void ParseAll(string bufferName)
	{
	}

	public virtual void CutParseTree(int fromLine, FormatedLine[] formatedLines)
	{
		if (parseTree == null)
			return;

		ParseTree.BaseNode cut = null;
		var prevLine = fromLine;
		while (cut == null && prevLine --> 0)
		{
			var tokens = textBuffer.formatedLines[prevLine].tokens;
			if (tokens != null)
			{
				for (var i = tokens.Count; i --> 0; )
				{
					if (tokens[i].tokenKind > SyntaxToken.Kind.LastWSToken && tokens[i].parent != null &&
						tokens[i].parent.syntaxError == null)
					{
						cut = tokens[i].parent;
						break;
					}
				}
			}
		}

		var cutThis = false;
		if (cut == null)
		{
			cut = parseTree.root.ChildAt(0);
			cutThis = true;
		}

		while (cut != null)
		{
			var cutParent = cut.parent;
			if (cutParent == null)
				break;
			var cutIndex = cutThis ? cut.childIndex : cut.childIndex + 1;
			while (cutIndex > 0)
			{
				var child = cutParent.ChildAt(cutIndex - 1);
				if (child != null && !child.HasLeafs())
					--cutIndex;
				else
					break;
			}
			cutThis = cutThis && cutIndex == 0;
			if (cutIndex < cutParent.numValidNodes)
			{
				cutParent.InvalidateFrom(cutIndex);
			}
			cut = cutParent;
			cut.syntaxError = null;
		}
	}

//int hyperlink = IndexOf3(line, startAt, "http://", "https://", "ftp://");
//if (hyperlink == -1)
//	hyperlink = line.Length;

//while (hyperlink != startAt)
//{
//	Match emailMatch = emailRegex.Match(line, startAt, hyperlink - startAt);
//	if (emailMatch.Success)
//	{
//		if (emailMatch.Index > startAt)
//			blocks.Add(new TextBlock(line.Substring(startAt, emailMatch.Index - startAt), commentStyle));

//		address = line.Substring(emailMatch.Index, emailMatch.Length);
//		blocks.Add(new TextBlock(address, textBuffer.styles.mailtoStyle));
//		address = "mailto:" + address;
//		if (textBuffer.IsLoading)
//		{
//			index = Array.BinarySearch<string>(textBuffer.hyperlinks, address, StringComparer.OrdinalIgnoreCase);
//			if (index < 0)
//				ArrayUtility.Insert(ref textBuffer.hyperlinks, -1 - index, address);
//		}

//		startAt = emailMatch.Index + emailMatch.Length;
//		continue;
//	}

//	blocks.Add(new TextBlock(line.Substring(startAt, hyperlink - startAt), commentStyle));
//	startAt = hyperlink;
//}

//if (startAt == line.Length)
//	break;

//int i = line.IndexOf(':', startAt) + 3;
//while (i < line.Length)
//{
//	char c = line[i];
//	if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '_' || c == '.' ||
//		c == '-' || c == '=' || c == '+' || c == '%' || c == '&' || c == '?' || c == '/' || c == '#')
//		++i;
//	else
//		break;
//}

//address = line.Substring(startAt, i - startAt);
//blocks.Add(new TextBlock(address, textBuffer.styles.hyperlinkStyle));
//if (textBuffer.IsLoading)
//{
//	index = Array.BinarySearch<string>(textBuffer.hyperlinks, address, StringComparer.OrdinalIgnoreCase);
//	if (index < 0)
//		ArrayUtility.Insert(ref textBuffer.hyperlinks, -1 - index, address);
//}

	private string[] emptyStringArray = new string[0];
	public virtual string[] Keywords { get { return emptyStringArray; } }
	public virtual string[] BuiltInLiterals { get { return emptyStringArray; } }
	
	protected static readonly string[] scriptLiterals = new string[] { "false", "null", "true", };

	protected static HashSet<string> unityTypes;

	public bool IsBuiltInLiteral(string word)
	{
		return Array.BinarySearch(BuiltInLiterals, word, textBuffer.isShader ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal) >= 0;
	}
	
	public virtual bool IsBuiltInType(string word)
	{
		return false;
	}

	protected bool IsUnityType(string word)
	{
		return textBuffer.isShader ? false : unityTypes.Contains(word);
	}

	public Func<bool> Update(int fromLine, int toLineInclusive)
	{
	//	var t = new Stopwatch();
	//	t.Start();

		var lastLine = textBuffer.formatedLines.Length - 1;
		try
		{
			if (this.parseTree != null)
				ParseLines(fromLine, lastLine);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		//if (toLineInclusive < textBuffer.lines.Count)
		//{
		//    progressiveParseLine = toLineInclusive + 1;
		//    return ProgressiveParser;
		//}
		//else
		{
			// TODO: Temporary solution, discarding all unused invalid parse tree nodes
			if (parseTree != null && parseTree.root != null)
				parseTree.root.CleanUp();
		}
		//ParseAll(assetPath);

	//	t.Stop();
	//	Debug.Log("Updated parser for lines " + (fromLine + 1) + "-" + (toLineInclusive + 1) + " in " + t.ElapsedMilliseconds + " ms");
		return null;
	}

	private int progressiveParseLine = -1;
	private bool ProgressiveParser()
	{
		if (textBuffer == null || textBuffer.lines == null || textBuffer.lines.Count <= progressiveParseLine)
		{
			progressiveParseLine = -1;
			return false;
		}

		if (!ParseLines(progressiveParseLine, progressiveParseLine))
			return false;
		++progressiveParseLine;
		if (progressiveParseLine < textBuffer.lines.Count)
			return true;

		progressiveParseLine = -1;
		return false;
	}

	protected static SyntaxToken ScanWhitespace(string line, ref int startAt)
	{
		int i = startAt;
		while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
			++i;
		if (i == startAt)
			return null;

		var token = new SyntaxToken(SyntaxToken.Kind.Whitespace, line.Substring(startAt, i - startAt));
		startAt = i;
		return token;
	}

	protected static SyntaxToken ScanWord(string line, ref int startAt)
	{
		int i = startAt;
		while (i < line.Length)
		{
			if (!Char.IsLetterOrDigit(line, i) && line[i] != '_')
				break;
			++i;
		}
		var token = new SyntaxToken(SyntaxToken.Kind.Identifier, line.Substring(startAt, i - startAt));
		startAt = i;
		return token;
	}

	protected static bool ScanUnicodeEscapeChar(string line, ref int startAt)
	{
		if (startAt >= line.Length - 5)
			return false;
		if (line[startAt] != '\\')
			return false;
		int i = startAt + 1;
		if (line[i] != 'u' && line[i] != 'U')
			return false;
		var n = line[i] == 'u' ? 4 : 8;
		++i;
		while (n > 0)
		{
			if (!ScanHexDigit(line, ref i))
				break;
			--n;
		}
		if (n == 0)
		{
			startAt = i;
			return true;
		}
		return false;
	}

	protected static SyntaxToken ScanCharLiteral(string line, ref int startAt)
	{
		var i = startAt + 1;
		while (i < line.Length)
		{
			if (line[i] == '\'')
			{
				++i;
				break;
			}
			if (line[i] == '\\' && i < line.Length - 1)
				++i;
			++i;
		}
		var token = new SyntaxToken(SyntaxToken.Kind.CharLiteral, line.Substring(startAt, i - startAt));
		startAt = i;
		return token;
	}

	protected static SyntaxToken ScanStringLiteral(string line, ref int startAt)
	{
		var i = startAt + 1;
		while (i < line.Length)
		{
			if (line[i] == '\"')
			{
				++i;
				break;
			}
			if (line[i] == '\\' && i < line.Length - 1)
				++i;
			++i;
		}
		var token = new SyntaxToken(SyntaxToken.Kind.StringLiteral, line.Substring(startAt, i - startAt));
		startAt = i;
		return token;
	}

	protected static SyntaxToken ScanNumericLiteral(string line, ref int startAt)
	{
		bool hex = false;
		bool point = false;
		bool exponent = false;
		var i = startAt;

		SyntaxToken token;

		char c;
		if (line[i] == '0' && i < line.Length - 1 && (line[i + 1] == 'x' || line[i + 1] == 'X'))
		{
			i += 2;
			hex = true;
			while (i < line.Length)
			{
				c = line[i];
				if (c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F')
					++i;
				else
					break;
			}
		}
		else
		{
			while (i < line.Length && line[i] >= '0' && line[i] <= '9')
				++i;
		}

		if (i > startAt && i < line.Length)
		{
			c = line[i];
			if (c == 'l' || c == 'L' || c == 'u' || c == 'U')
			{
				++i;
				if (i < line.Length)
				{
					if (c == 'l' || c == 'L')
					{
						if (line[i] == 'u' || line[i] == 'U')
							++i;
					}
					else if (line[i] == 'l' || line[i] == 'L')
						++i;
				}
				token = new SyntaxToken(SyntaxToken.Kind.IntegerLiteral, line.Substring(startAt, i - startAt));
				startAt = i;
				return token;
			}
		}

		if (hex)
		{
			token = new SyntaxToken(SyntaxToken.Kind.IntegerLiteral, line.Substring(startAt, i - startAt));
			startAt = i;
			return token;
		}

		while (i < line.Length)
		{
			c = line[i];

			if (!point && !exponent && c == '.')
			{
				if (i < line.Length - 1 && line[i+1] >= '0' && line[i+1] <= '9')
				{
					point = true;
					++i;
					continue;
				}
				else
				{
					break;
				}
			}
			if (!exponent && i > startAt && (c == 'e' || c == 'E'))
			{
				exponent = true;
				++i;
				if (i < line.Length && (line[i] == '-' || line[i] == '+'))
					++i;
				continue;
			}
			if (c == 'f' || c == 'F' || c == 'd' || c == 'D' || c == 'm' || c == 'M')
			{
				point = true;
				++i;
				break;
			}
			if (c < '0' || c > '9')
				break;
			++i;
		}
		token = new SyntaxToken(point || exponent ? SyntaxToken.Kind.RealLiteral : SyntaxToken.Kind.IntegerLiteral,
		                        line.Substring(startAt, i - startAt));
		startAt = i;
		return token;
	}

	protected static SyntaxToken ScanNumericLiteral_JS(string line, ref int startAt)
	{
		bool hex = false;
		bool point = false;
		bool exponent = false;
		var i = startAt;

		SyntaxToken token;

		char c;
		if (line[i] == '0' && i < line.Length - 1 && (line[i + 1] == 'x'))
		{
			i += 2;
			hex = true;
			while (i < line.Length)
			{
				c = line[i];
				if (c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F')
					++i;
				else
					break;
			}
		}
		else
		{
			while (i < line.Length && line[i] >= '0' && line[i] <= '9')
				++i;
		}

		if (i > startAt && i < line.Length)
		{
			c = line[i];
			if (c == 'l' || c == 'L')
			{
				++i;
				token = new SyntaxToken(SyntaxToken.Kind.IntegerLiteral, line.Substring(startAt, i - startAt));
				startAt = i;
				return token;
			}
		}

		if (hex)
		{
			token = new SyntaxToken(SyntaxToken.Kind.IntegerLiteral, line.Substring(startAt, i - startAt));
			startAt = i;
			return token;
		}

		while (i < line.Length)
		{
			c = line[i];

			if (!point && !exponent && c == '.')
			{
				if (i < line.Length - 1 && line[i+1] >= '0' && line[i+1] <= '9')
				{
					point = true;
					++i;
					continue;
				}
				else
				{
					break;
				}
			}
			if (!exponent && i > startAt && (c == 'e' || c == 'E'))
			{
				exponent = true;
				++i;
				if (i < line.Length && (line[i] == '-' || line[i] == '+'))
					++i;
				continue;
			}
			if (c == 'f' || c == 'F' || c == 'd' || c == 'D')
			{
				point = true;
				++i;
				break;
			}
			if (c < '0' || c > '9')
				break;
			++i;
		}
		token = new SyntaxToken(point || exponent ? SyntaxToken.Kind.RealLiteral : SyntaxToken.Kind.IntegerLiteral,
		                        line.Substring(startAt, i - startAt));
		startAt = i;
		return token;
	}

	protected static bool ScanHexDigit(string line, ref int i)
	{
		if (i >= line.Length)
			return false;
		char c = line[i];
		if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f')
		{
			++i;
			return true;
		}
		return false;
	}

	protected static SyntaxToken ScanIdentifierOrKeyword(string line, ref int startAt)
	{
		bool identifier = false;
		int i = startAt;
		if (i >= line.Length)
			return null;
		
		char c = line[i];
		if (c == '@')
		{
			identifier = true;
			++i;
		}
		if (i < line.Length)
		{
			c = line[i];
			if (char.IsLetter(c) || c == '_')
			{
				++i;
			}
			else if (!ScanUnicodeEscapeChar(line, ref i))
			{
				if (i == startAt)
					return null;
				var partialWord = line.Substring(startAt, i - startAt);
				startAt = i;
				return new SyntaxToken(SyntaxToken.Kind.Identifier, partialWord);
			}
			else
			{
				identifier = true;
			}
			
			while (i < line.Length)
			{
				if (char.IsLetterOrDigit(line, i) || line[i] == '_')
					++i;
				else if (!ScanUnicodeEscapeChar(line, ref i))
					break;
				else
					identifier = true;
			}
		}
		
		var word = line.Substring(startAt, i - startAt);
		startAt = i;
		return new SyntaxToken(identifier ? SyntaxToken.Kind.Identifier : SyntaxToken.Kind.Keyword, word);
	}
	
	protected bool ParsePPOrExpression(string line, FGTextBuffer.FormatedLine formatedLine, ref int startAt)
	{
		if (startAt >= line.Length)
		{
			//TODO: Insert missing token
			return true;
		}
		
		var lhs = ParsePPAndExpression(line, formatedLine, ref startAt);
		
		var ws = ScanWhitespace(line, ref startAt);
		if (ws != null)
		{
			formatedLine.tokens.Add(ws);
			ws.formatedLine = formatedLine;
		}
		
		if (startAt + 1 < line.Length && line[startAt] == '|' && line[startAt + 1] == '|')
		{
			formatedLine.tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, "||") { formatedLine = formatedLine });
			startAt += 2;
			
			ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			var rhs = ParsePPOrExpression(line, formatedLine, ref startAt);
			
			ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			return lhs || rhs;
		}
		
		return lhs;
	}
	
	protected bool ParsePPAndExpression(string line, FGTextBuffer.FormatedLine formatedLine, ref int startAt)
	{
		if (startAt >= line.Length)
		{
			//TODO: Insert missing token
			return true;
		}
		
		var lhs = ParsePPEqualityExpression(line, formatedLine, ref startAt);
		
		var ws = ScanWhitespace(line, ref startAt);
		if (ws != null)
		{
			formatedLine.tokens.Add(ws);
			ws.formatedLine = formatedLine;
		}
		
		if (startAt + 1 < line.Length && line[startAt] == '&' && line[startAt + 1] == '&')
		{
			formatedLine.tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, "&&") { formatedLine = formatedLine });
			startAt += 2;
			
			ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			var rhs = ParsePPAndExpression(line, formatedLine, ref startAt);
			
			ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			return lhs && rhs;
		}
		
		return lhs;
	}
	
	protected bool ParsePPEqualityExpression(string line, FGTextBuffer.FormatedLine formatedLine, ref int startAt)
	{
		if (startAt >= line.Length)
		{
			//TODO: Insert missing token
			return true;
		}
		
		var lhs = ParsePPUnaryExpression(line, formatedLine, ref startAt);
		
		var ws = ScanWhitespace(line, ref startAt);
		if (ws != null)
		{
			formatedLine.tokens.Add(ws);
			ws.formatedLine = formatedLine;
		}
		
		if (startAt + 1 < line.Length && (line[startAt] == '=' || line[startAt + 1] == '!') && line[startAt + 1] == '=')
		{
			var equality = line[startAt] == '=';
			formatedLine.tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, equality ? "==" : "!=") { formatedLine = formatedLine });
			startAt += 2;
			
			ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			var rhs = ParsePPEqualityExpression(line, formatedLine, ref startAt);
			
			ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			return equality ? lhs == rhs : lhs != rhs;
		}
		
		return lhs;
	}
	
	protected bool ParsePPUnaryExpression(string line, FGTextBuffer.FormatedLine formatedLine, ref int startAt)
	{
		if (startAt >= line.Length)
		{
			//TODO: Insert missing token
			return true;
		}
		
		if (line[startAt] == '!')
		{
			formatedLine.tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, "!") { formatedLine = formatedLine });
			++startAt;
			
			var ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			var result = ParsePPUnaryExpression(line, formatedLine, ref startAt);
			
			ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			return !result;
		}
		
		return ParsePPPrimaryExpression(line, formatedLine, ref startAt);
	}
	
	protected bool ParsePPPrimaryExpression(string line, FGTextBuffer.FormatedLine formatedLine, ref int startAt)
	{
		if (line[startAt] == '(')
		{
			formatedLine.tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, "(") { formatedLine = formatedLine });
			++startAt;
			
			var ws = ScanWhitespace(line, ref startAt);
			if (ws != null)
			{
				formatedLine.tokens.Add(ws);
				ws.formatedLine = formatedLine;
			}
			
			var result = ParsePPOrExpression(line, formatedLine, ref startAt);
			
			if (startAt >= line.Length)
			{
				//TODO: Insert missing token
				return result;
			}
			
			if (line[startAt] == ')')
			{
				formatedLine.tokens.Add(new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, ")") { formatedLine = formatedLine });
				++startAt;
				
				ws = ScanWhitespace(line, ref startAt);
				if (ws != null)
				{
					formatedLine.tokens.Add(ws);
					ws.formatedLine = formatedLine;
				}
				
				return result;
			}
			
			//TODO: Insert missing token
			return result;
		}
		
		var symbolResult = ParsePPSymbol(line, formatedLine, ref startAt);
		
		var ws2 = ScanWhitespace(line, ref startAt);
		if (ws2 != null)
		{
			formatedLine.tokens.Add(ws2);
			ws2.formatedLine = formatedLine;
		}
		
		return symbolResult;
	}
	
	protected bool ParsePPSymbol(string line, FGTextBuffer.FormatedLine formatedLine, ref int startAt)
	{
		var word = FGParser.ScanIdentifierOrKeyword(line, ref startAt);
		if (word == null)
			return true;
		
		word.tokenKind = SyntaxToken.Kind.PreprocessorSymbol;
		formatedLine.tokens.Add(word);
		word.formatedLine = formatedLine;
		
		if (word.text == "true")
		{
			return true;
		}
		if (word.text == "false")
		{
			return false;
		}
		
		if (scriptDefines == null)
			scriptDefines = new HashSet<string>(UnityEditor.EditorUserBuildSettings.activeScriptCompilationDefines);
		
		var isDefined = scriptDefines.Contains(word.text);
		return isDefined;
	}
	
	protected void OpenRegion(FGTextBuffer.FormatedLine formatedLine, FGTextBuffer.RegionTree.Kind regionKind)
	{
		var parentRegion = formatedLine.regionTree;
		FGTextBuffer.RegionTree reuseRegion = null;
		
		switch (regionKind)
		{
		case FGTextBuffer.RegionTree.Kind.Else:
		case FGTextBuffer.RegionTree.Kind.Elif:
		case FGTextBuffer.RegionTree.Kind.InactiveElse:
		case FGTextBuffer.RegionTree.Kind.InactiveElif:
			parentRegion = parentRegion.parent;
			break;
		}
		
		if (parentRegion.children != null)
		{
			reuseRegion = parentRegion.children.Find(x => x.line == formatedLine);
		}
		if (reuseRegion != null)
		{
			if (reuseRegion.kind == regionKind)
			{
				formatedLine.regionTree = reuseRegion;
				return;
			}
			
			reuseRegion.parent = null;
			parentRegion.children.Remove(reuseRegion);
		}
		
		formatedLine.regionTree = new FGTextBuffer.RegionTree {
			parent = parentRegion,
			kind = regionKind,
			line = formatedLine,
		};
		
		if (parentRegion.children == null)
			parentRegion.children = new List<FGTextBuffer.RegionTree>();
		parentRegion.children.Add(formatedLine.regionTree);
	}
	
	protected void CloseRegion(FGTextBuffer.FormatedLine formatedLine)
	{
		formatedLine.regionTree = formatedLine.regionTree.parent;
	}
}

internal class TextParser : FGParser
{
}

}
