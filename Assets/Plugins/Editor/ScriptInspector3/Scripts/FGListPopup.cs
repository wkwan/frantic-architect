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

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;


namespace ScriptInspector
{

public class FGListPopup : FGPopupWindow
{
	private int currentItem = -1;
	private SymbolDefinition[] data;
	private List<SymbolDefinition> filteredData = new List<SymbolDefinition>();
	private BacktrackingStringMatcher matcher;
	
	private Rect scrollViewRect;
	private int scrollPosition;
	private const int maxListItems = 9;
	private static float listItemHeight = 19f;
	private bool isSizeSet;

	public static string NameOf(SymbolDefinition symbol)
	{
		var name = symbol.name;
		
		if (shortAttributeNames && symbol.kind == SymbolKind.Class &&
			symbol.name.EndsWith("Attribute", StringComparison.Ordinal) && symbol.name != "Attribute")
		{
			name = name.Substring(0, name.Length - "Attribute".Length);
		}
		
		return name;
	}
	
	private static string ItemDisplayString(SymbolDefinition symbol, string styledName)
	{
		return ' ' + symbol.CompletionDisplayString(styledName);
	}
	
	private static string topSuggestion;

	private static List<string> _recentCompletions;
	private static List<string> recentCompletions
	{
		get {
			if (_recentCompletions == null)
			{
				_recentCompletions = new List<string>(100);
				var s = EditorPrefs.GetString("ScriptInspectorRecentCompletions", "");
				_recentCompletions.AddRange(s.Split(','));
			}
			return _recentCompletions;
		}
	}

	private FGTextBuffer textBuffer;
	private FGTextEditor textEditor;
	private bool completeOnUpdate;

	private static int startAtCharacterIndex;
	private static int lineIndex;

	public static SyntaxToken tokenLeft;
	public static bool shortAttributeNames;

	class SymbolDefinitionComparer : IComparer<SymbolDefinition>
	{
		public int Compare(SymbolDefinition a, SymbolDefinition b)
		{
			var nameOfA = NameOf(a);
			var nameOfB = NameOf(b);
			var c = string.Compare(nameOfA, nameOfB, StringComparison.OrdinalIgnoreCase);
			if (c == 0)
				c = string.Compare(nameOfA, nameOfB, StringComparison.Ordinal);
			return c;
		}
	}

	private static readonly SymbolDefinitionComparer symbolDefinitionComparer = new SymbolDefinitionComparer();
	
	public bool IdentifiersOnly { get; private set; }

	private string typedInPart = "";
	public string TypedInPart
	{
		get { return typedInPart; }
		set
		{
			matcher = null;
			if (data == null)
				return;
			
			value = SymbolDefinition.DecodeId(value);

			var focusRecent = false;
			HashSet<string> bestMatches = null;
			var tempSymbol = new SymbolDefinition { name = value };

			if (value == "")
			{
				typedInPart = value;

				var filteredIndex = currentItem < 0 ? ~currentItem : currentItem;
				var focused = filteredData.Count > 0 && filteredIndex < filteredData.Count ? filteredData[filteredIndex] : null;

				filteredData.Clear();
				filteredData.AddRange(data);
				if (focused != null)
				{
					filteredIndex = filteredData.IndexOf(focused);
					currentItem = currentItem < 0 ? ~filteredIndex : filteredIndex;
					focusRecent = topSuggestion != null;
				}
				else
				{
					currentItem = -1;
					focusRecent = true;
				}
			}
			else
			{
				typedInPart = value;

				var filteredIndex = currentItem < 0 ? ~currentItem : currentItem;
				var focused = filteredData.Count > 0 && filteredIndex < filteredData.Count ? filteredData[filteredIndex] : null;

				matcher = new BacktrackingStringMatcher(value);
				bestMatches = new HashSet<string>();
				var bestMatch = -1;
				var bestRank = -1;
				var newFilteredData = new List<SymbolDefinition>();
				for (var i = 0; i < data.Length; ++i)
				{
					var name = NameOf(data[i]);
					int rank;
					if (!matcher.CalcMatchRank(name, out rank))
						continue;
					if (rank > bestRank)
					{
						bestMatch = newFilteredData.Count;
						bestRank = rank;
						bestMatches.Clear();
						bestMatches.Add(name);
						if (rank >= int.MinValue - value.Length -1)
							focused = null;
					}
					else if (rank == bestRank)
						bestMatches.Add(name);
					newFilteredData.Add(data[i]);
				}

				//var newFilteredData =
				//    from symbol in data
				//    where symbol.name.StartsWith(value, System.StringComparison.InvariantCultureIgnoreCase)
				//    select symbol;
				if (newFilteredData.FirstOrDefault() == null)
				{
					if (currentItem >= 0)
						currentItem = ~currentItem;
				}
				else
				{
					//filteredData.Clear();
					//filteredData.AddRange(newFilteredData);
					filteredData = newFilteredData;

					if (focused != null)
					{
						if (focused.name.StartsWith(value, System.StringComparison.OrdinalIgnoreCase))
						{
							currentItem = filteredData.IndexOf(focused);
						}
						else
						{
							currentItem = bestMatch; // filteredData.BinarySearch(tempSymbol, new SymbolDefinitionComparer());
							if (bestMatches.Count > 1)
								focusRecent = true;
							//if (currentItem < 0)
							//{
							//	if (filteredData[~currentItem].name.StartsWith(value, System.StringComparison.OrdinalIgnoreCase))
							//		currentItem = ~currentItem;

							//	focusRecent = true;
							//}
						}
					}
					else
					{
						currentItem = bestMatch; // currentItem = filteredData.BinarySearch(tempSymbol, new SymbolDefinitionComparer());
						focusRecent = bestMatches.Count > 1 || currentItem < 0 || topSuggestion != null;
					}
				}
			}
			
			if (focusRecent)
			{
				if (topSuggestion != null)
				{
					tempSymbol.name = topSuggestion;
					var suggestedItemIndex = filteredData.BinarySearch(tempSymbol, symbolDefinitionComparer);
					if (suggestedItemIndex >= 0)
					{
						currentItem = suggestedItemIndex;
						focusRecent = false;
					}
				}
				
				if (focusRecent && bestMatches != null && bestMatches.Count > 1)
				{
					for (var i = recentCompletions.Count; i-- > 0; )
					{
						if (bestMatches.Contains(recentCompletions[i]))
						{
							tempSymbol.name = recentCompletions[i];
							currentItem = filteredData.BinarySearch(tempSymbol, symbolDefinitionComparer);
							focusRecent = currentItem < 0;
							break;
						}
					}
				}
				
				if (focusRecent)
				{
					for (var i = recentCompletions.Count; i-- > 0; )
					{
						tempSymbol.name = recentCompletions[i];
						var recentItemIndex = filteredData.BinarySearch(tempSymbol, symbolDefinitionComparer);
						if (recentItemIndex >= 0 && (value == "" || value[0] == '@' || value[0] == '_' || char.IsLower(value[0]) || !char.IsLower(tempSymbol.name[0])))
						{
							currentItem = recentItemIndex;
							break;
						}
					}
				}
			}
			CenterScrollCurrentItem();
		}
	}

	public void UpdateTypedInPart()
	{
		var caretPosition = textEditor.caretPosition;
		var line = textBuffer.lines[caretPosition.line];

		var charIndex = caretPosition.characterIndex;
		while (charIndex > 0)
		{
			if (!char.IsLetterOrDigit(line, charIndex - 1) &&
				line[charIndex - 1] != '_' &&
				line[charIndex - 1] != '\\')
			{
				if (line[charIndex - 1] == '@')
				{
					IdentifiersOnly = true;
					--charIndex;
				}
				break;
			}
			--charIndex;
		}
		var wordAtLeft = line.Substring(charIndex, caretPosition.characterIndex - charIndex);
		TypedInPart = wordAtLeft;
	}

	private static GUIStyle listItemStyle;
	private static Texture2D[,] symbolIcons;
	private static Texture2D keywordIcon;
	private static Texture2D inactiveListItem;
	private static Texture2D selectedListItem;

	private static void LoadSymbolIcons()
	{
		SymbolKind[] kinds = { SymbolKind.Namespace, SymbolKind.Interface, SymbolKind.Enum, SymbolKind.Struct,
			SymbolKind.Class, SymbolKind.Delegate, SymbolKind.Field, SymbolKind.ConstantField, SymbolKind.LocalConstant,
			SymbolKind.EnumMember, SymbolKind.Property, SymbolKind.Event, SymbolKind.Indexer,
			SymbolKind.Method, SymbolKind.Constructor, SymbolKind.Destructor, SymbolKind.Operator,
			SymbolKind.Accessor, SymbolKind.Parameter, SymbolKind.CatchParameter, SymbolKind.Variable,
			SymbolKind.ForEachVariable, SymbolKind.FromClauseVariable, SymbolKind.TypeParameter,
			SymbolKind.Label };
		var oneForAll = new HashSet<SymbolKind> { SymbolKind.Namespace, SymbolKind.EnumMember, SymbolKind.Parameter,
			SymbolKind.CatchParameter, SymbolKind.Variable, SymbolKind.ForEachVariable, SymbolKind.FromClauseVariable,
			SymbolKind.TypeParameter, SymbolKind.Label, SymbolKind.LocalConstant, SymbolKind.Constructor, SymbolKind.Destructor };
		symbolIcons = new Texture2D[System.Enum.GetNames(typeof(SymbolKind)).Length, 3];
		for (var i = 0; i < kinds.Length; i++)
		{
			var kind = kinds[i].ToString();
			if (kind == "ConstantField" || kind == "LocalConstant")
				kind = "Constant";
			else if (kind == "EnumMember")
				kind = "EnumItem";
			var index = (int) kinds[i];
			symbolIcons[index, 0] = FGTextEditor.LoadEditorResource<Texture2D>("Symbol Icons/VSObject_" + kind + ".png");
			if (oneForAll.Contains(kinds[i]))
			{
				symbolIcons[index, 1] = symbolIcons[index, 0];
				symbolIcons[index, 2] = symbolIcons[index, 0];
			}
			else
			{
				symbolIcons[index, 1] = FGTextEditor.LoadEditorResource<Texture2D>("Symbol Icons/VSObject_" + kind + "_Protected.png");
				symbolIcons[index, 2] = FGTextEditor.LoadEditorResource<Texture2D>("Symbol Icons/VSObject_" + kind + "_Private.png");
			}
			
#if SI3_WARNINGS
			if (symbolIcons[index, 0] == null)
				Debug.LogWarning("No icon for " + kind);
			if (symbolIcons[index, 1] == null)
				Debug.LogWarning("No icon for protected " + kind);
			if (symbolIcons[index, 2] == null)
				Debug.LogWarning("No icon for private " + kind);
#endif
		}
		symbolIcons[(int) SymbolKind._Keyword, 0] = keywordIcon = FGTextEditor.LoadEditorResource<Texture2D>("Symbol Icons/Keyword.png");
		symbolIcons[(int) SymbolKind._Keyword, 1] = keywordIcon;
		symbolIcons[(int) SymbolKind._Keyword, 2] = keywordIcon;
		var snippetIcon = FGTextEditor.LoadEditorResource<Texture2D>("Symbol Icons/Snippet.png");
		symbolIcons[(int) SymbolKind._Snippet, 0] = snippetIcon;
		symbolIcons[(int) SymbolKind._Snippet, 1] = snippetIcon;
		symbolIcons[(int) SymbolKind._Snippet, 2] = snippetIcon;

		inactiveListItem = FGTextEditor.LoadEditorResource<Texture2D>("inactiveListItem.png");
		selectedListItem = FGTextEditor.LoadEditorResource<Texture2D>("selectedListItem.png");
	}
	
	private static void LoadResources(FGTextEditor editor)
	{
		if (symbolIcons == null)
			LoadSymbolIcons();
		
		listItemStyle = new GUIStyle
		{
			fixedHeight = 0,//listItemHeight,
			padding = { left = 2, top = 2, bottom = 4, right = 2 },
			border = new RectOffset(2, 2, 2, 2),
			margin = new RectOffset(3, 3, 0, 0),
			overflow = { left = -20 },

			normal = { textColor = editor.styles.normalStyle.normal.textColor },
			onFocused = { background = selectedListItem },
			onNormal = { background = inactiveListItem, textColor = editor.styles.normalStyle.normal.textColor },
		};
	}

	public static FGListPopup Create(FGTextEditor editor, Rect buttonRect, bool flipped)
	{
		if (listItemStyle == null)
		{
			LoadResources(editor);
		}
		listItemStyle.fontSize = SISettings.fontSizeDelta + 10;
		listItemHeight = Mathf.Max(19f, listItemStyle.CalcHeight(new GUIContent(symbolIcons[0,0], "W"), 100f));
		listItemStyle.onNormal.textColor = editor.styles.normalStyle.normal.textColor;
		listItemStyle.normal.textColor = editor.styles.normalStyle.normal.textColor;
		
		topSuggestion = null;
		
		string typedInPart = "";
		int tokenIndex;
		bool atTokenEnd;
		var onToken = editor.TextBuffer.GetTokenAt(editor.caretPosition, out lineIndex, out tokenIndex, out atTokenEnd);
		if (onToken != null && /*!atTokenEnd &&*/ onToken.tokenKind >= SyntaxToken.Kind.Keyword)
		{
			// eat
			var textSpan = editor.TextBuffer.GetTokenSpan(lineIndex, tokenIndex);
			startAtCharacterIndex = textSpan.StartPosition.index;
			
			tokenLeft = editor.TextBuffer.GetNonTriviaTokenLeftOf(lineIndex, startAtCharacterIndex);

			typedInPart = onToken.text.Substring(0, editor.caretPosition.characterIndex - textSpan.index);
		//	Debug.Log("typedInPart " + typedInPart);
		}
		else
		{
			if (!atTokenEnd && onToken.tokenKind == SyntaxToken.Kind.Comment)
			{
				return null;
			}
			if (onToken != null && (
				onToken.tokenKind == SyntaxToken.Kind.StringLiteral ||
				onToken.tokenKind == SyntaxToken.Kind.VerbatimStringLiteral ||
				onToken.tokenKind == SyntaxToken.Kind.CharLiteral ||
				onToken.tokenKind == SyntaxToken.Kind.CharLiteral ||
				onToken.tokenKind >= SyntaxToken.Kind.Preprocessor &&
				onToken.tokenKind <= SyntaxToken.Kind.PreprocessorUnexpectedDirective))
			{
				return null;
			}
			//Debug.Log(onToken.tokenKind);
			
			startAtCharacterIndex = editor.caretPosition.characterIndex;
			lineIndex = editor.caretPosition.line;

			tokenLeft = editor.TextBuffer.GetNonTriviaTokenLeftOf(lineIndex, startAtCharacterIndex);
		}

		FGListPopup window = CreatePopup<FGListPopup>();
		window.Flipped = flipped;
		window.minSize = new Vector2(1f, 1f);
		window.textEditor = editor;
		window.textBuffer = editor.TextBuffer;
		window.owner = EditorWindow.focusedWindow;
		startAtCharacterIndex = editor.caretPosition.characterIndex;
		lineIndex = editor.caretPosition.line;
		//tokenLeft = null;

		Vector2 screenPoint = GUIUtility.GUIToScreenPoint(
			new Vector2(buttonRect.x, flipped ? buttonRect.y : buttonRect.yMax));
		Rect position = new Rect(screenPoint.x - 24f - editor.charSize.x * typedInPart.Length,
			flipped ? screenPoint.y - 21f : screenPoint.y, 1f, 21f);
		window.dropDownRect = new Rect(position.x, flipped ? position.y + 21f : position.y - editor.charSize.y, 1f, editor.charSize.y);
		window.position = position;
		window.TypedInPart = typedInPart;

		window.ShowPopup();

		//if (window.owner != null)
		//	window.owner.Focus();
		return window;
	}

	private struct IndexNameTupple
	{
		public string name;
		public int index;
	}

	public void SetCompletionData(HashSet<SymbolDefinition> data)
	{
		this.data = data.ToArray();
		System.Array.Sort(this.data, symbolDefinitionComparer);
		UpdateTypedInPart();
		if (filteredData.Count == 0)
		{
			lineIndex = -1;
		}
		var localSymbols = new List<IndexNameTupple>();
		for (var i = this.data.Length; i -- > 0;)
		{
			var symbol = this.data[i];
			var kind = symbol.kind;
			if (kind == SymbolKind.Variable || kind == SymbolKind.Parameter || kind == SymbolKind.LocalConstant ||
				kind == SymbolKind.Label ||
				kind == SymbolKind.CatchParameter || kind == SymbolKind.FromClauseVariable || kind == SymbolKind.ForEachVariable)
			{
				var nameOf = NameOf(symbol);
				if (nameOf.StartsWith(typedInPart, StringComparison.OrdinalIgnoreCase))
				{
					var recentIndex = recentCompletions.IndexOf(nameOf);
					localSymbols.Add(new IndexNameTupple
					{
						name = nameOf,
						index = recentIndex < 0 ? recentCompletions.Capacity : recentIndex
					});
				}
			}
		}
		if (localSymbols.Count > 0)
		{
			localSymbols.Sort((a, b) => a.index.CompareTo(b.index));
			for (var i = 0; i < localSymbols.Count; ++i)
				AddRecentCompletion(localSymbols[i].name);
			currentItem = localSymbols[localSymbols.Count - 1].index;
			TypedInPart = typedInPart;
		}
	}
	
	public static void SetTopSuggestion(SymbolDefinition suggestion)
	{
		topSuggestion = suggestion.GetName();
	}

	private static FieldInfo scrollStepSizeField;
	
	private void Update()
	{
		if (completeOnUpdate)
		{
			completeOnUpdate = false;
			owner.SendEvent(Event.KeyboardEvent("\n"));
		}
	}
	
	public static Texture2D GetSymbolIcon(SymbolDefinition symbol)
	{
		if (symbolIcons == null)
			LoadSymbolIcons();
		
		var icon = symbol.cachedIcon;
		if (icon)
			return icon;
		
		icon = symbolIcons[
			(int) symbol.kind,
			symbol.IsPublic | symbol.IsInternal ? 0 : symbol.IsProtected ? 1 : 2] ?? keywordIcon;
		
		var asReflectedType = symbol as ReflectedType;
		if (asReflectedType != null && typeof(Component).IsAssignableFrom(asReflectedType.GetReflectedType()))
		{
			var unityContent = EditorGUIUtility.ObjectContent(null, asReflectedType.GetReflectedType());
			if (unityContent.image != null)
				icon = unityContent.image as Texture2D ?? icon;
		}
		
		symbol.cachedIcon = icon;
		return icon;
	}

	private static void AddRecentCompletion(string completion)
	{
		if (!recentCompletions.Remove(completion) && recentCompletions.Count == recentCompletions.Capacity)
			recentCompletions.RemoveAt(0);
		recentCompletions.Add(completion);
	}

	private void OnGUI()
	{
		if (data == null)
			return;
		
		if (Event.current.type == EventType.layout)
			return;
		
		if (Event.current.type == EventType.ScrollWheel)
		{
			scrollPosition = Mathf.Clamp(scrollPosition + (int) Event.current.delta.y, 0, filteredData.Count - maxListItems);
			scrollPosition = Mathf.Clamp(scrollPosition, 0, Mathf.Max(0, filteredData.Count - maxListItems));
			Event.current.Use();
		}

		scrollViewRect = new Rect(0f, 0f, position.width, position.height);
		GUI.Label(scrollViewRect, GUIContent.none, textEditor.styles.lineNumbersSeparator);

		scrollViewRect.xMin++;
		scrollViewRect.yMin++;
		scrollViewRect.xMax--;
		scrollViewRect.yMax--;

		GUI.Label(scrollViewRect, GUIContent.none, textEditor.styles.listBgStyle);

		var rcScrollBar = new Rect(scrollViewRect);
		rcScrollBar.xMin = rcScrollBar.xMax - 15f;
		if (filteredData.Count > maxListItems)
		{
			var oldValue = 10f;
			if (Event.current.isMouse)
			{
				if (scrollStepSizeField == null)
					scrollStepSizeField = typeof(GUI).GetField("scrollStepSize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
				if (scrollStepSizeField != null)
				{
					oldValue = (float)scrollStepSizeField.GetValue(null);
					scrollStepSizeField.SetValue(null, 1f);
				}
			}

			scrollPosition = (int)GUI.VerticalScrollbar(rcScrollBar, scrollPosition, maxListItems, 0, filteredData.Count);
			scrollPosition = Mathf.Clamp(scrollPosition, 0, Mathf.Max(0, filteredData.Count - maxListItems));

			if (Event.current.isMouse && scrollStepSizeField != null)
				scrollStepSizeField.SetValue(null, oldValue);
		}
		
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
			(filteredData.Count <= maxListItems || Event.current.mousePosition.x < rcScrollBar.x))
		{
			var itemIndex = Mathf.Clamp((int) (Event.current.mousePosition.y / listItemHeight), 0, maxListItems - 1);
			currentItem = Mathf.Clamp(itemIndex + scrollPosition, 0, filteredData.Count - 1);
			//owner.Focus();

			Event.current.Use();

			if (Event.current.clickCount == 2)
			{
				completeOnUpdate = true;
				//owner.SendEvent(EditorGUIUtility.CommandEvent("ScriptInspector.Autocomplete=" + filteredData[currentItem].name));
			}

			//Repaint();
			return;
		}
		
		if (Event.current.type == EventType.Repaint)
		{
			var width = 100f;
			for (var i = Mathf.Min(filteredData.Count, scrollPosition + maxListItems) - 1; i >= scrollPosition; --i)
			{
				var itemContent = new GUIContent(ItemDisplayString(filteredData[i], "<b>" + filteredData[i].GetName() + "</b>"));
				if (filteredData[i].GetTypeParameters() != null)
					itemContent.text += "<>";
				width = Mathf.Max(width, listItemStyle.CalcSize(itemContent).x);
			}
			var rc = position;
			rc.width = Mathf.Max(rc.width, width + 24f + (filteredData.Count > maxListItems ? 21f : 2f));
			rc.height = listItemHeight * Mathf.Min(maxListItems, filteredData.Count) + 2f;
			if (!isSizeSet || rc != position)
			{
				SetSize(rc.width, rc.height);
				isSizeSet = true;
			}

			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			
			System.Text.StringBuilder sb = null;
			for (var i = Mathf.Min(filteredData.Count, scrollPosition + maxListItems + 1); --i >= scrollPosition; )
			{
				var rcItem = new Rect(2f, 1f + listItemHeight * (i - scrollPosition), position.width - 3f, listItemHeight);
				if (filteredData.Count > maxListItems)
					rcItem.xMax -= 15f;

				var on = i == currentItem;
				var focus = on || ~i == currentItem;
				var symbol = filteredData[i];
				var icon = GetSymbolIcon(symbol);
				var styledName = NameOf(filteredData[i]);
				if (matcher != null)
				{
					const string markerStart = "<b>";
					const string markerEnd = "</b>";
					
					var matches = matcher.GetMatch(styledName);
					if (matches != null && matches.Length > 0)
					{
						sb = sb ?? new System.Text.StringBuilder();
						sb.Length = 0;

						var isMatch = false;
						var m = 0;
						for (var s = 0; s < styledName.Length; ++s)
						{
							if (m == matches.Length)
							{
								if (isMatch)
								{
									isMatch = false;
									sb.Append(markerEnd);
								}
								sb.Append(styledName, s, styledName.Length - s);
								break;
							}
							if (s == matches[m])
							{
								++m;
								if (!isMatch)
								{
									sb.Append(markerStart);
									isMatch = true;
								}
							}
							else if (isMatch)
							{
								sb.Append(markerEnd);
								isMatch = false;
							}
							sb.Append(styledName[s]);
						}
						if (isMatch)
							sb.Append(markerEnd);
						styledName = sb.ToString();
					}
				}
				var displayString = ItemDisplayString(filteredData[i], styledName);
				var itemContent = new GUIContent(displayString, icon);
				if (filteredData[i].GetTypeParameters() != null)
					itemContent.text += "<>";
				listItemStyle.Draw(rcItem, itemContent, false, focus, focus, on);
			}
			
			EditorGUIUtility.SetIconSize(Vector2.zero);
		}
		
		//if (focusedWindow == this && owner != null)
		//	owner.Focus();
		if (Event.current.type == EventType.ExecuteCommand || Event.current.type == EventType.ValidateCommand || Event.current.isKey)
			owner.SendEvent(Event.current);
	}

	public SymbolDefinition OnOwnerGUI()
	{
		int focusedItem = currentItem;

		if (Event.current.type == EventType.ScrollWheel)
		{
			if (filteredData.Count <= maxListItems)
				return new SymbolDefinition();

			scrollPosition = Mathf.Clamp(scrollPosition + (int) Event.current.delta.y, 0, filteredData.Count - maxListItems);
			Event.current.Use();
			Repaint();
			return null;
		}

		if (Event.current.type == EventType.keyDown)
		{
			var acceptWith = "\t\n {}[]().,:;+-*/%&|^!~=<>?@#\'\"\\";
			if (topSuggestion != null && currentItem >= 0 && typedInPart == "")
			{
				if (topSuggestion == filteredData[currentItem].name)
					acceptWith = "\t\n.";
			}
			if (!(Event.current.alt || Event.current.command || Event.current.control) &&
				currentItem >= 0 && (acceptWith.IndexOf(Event.current.character) >= 0 || Event.current.keyCode == KeyCode.KeypadEnter))
			{
				if (Event.current.shift && Event.current.character == '\t')
					return new SymbolDefinition();

				var completion = NameOf(filteredData[currentItem]);
				AddRecentCompletion(completion);
				var s = string.Join(",", recentCompletions.ToArray());
				EditorPrefs.SetString("ScriptInspectorRecentCompletions", s);
				return filteredData[currentItem];
			}

			switch (Event.current.keyCode)
			{
				//case KeyCode.Return:
				//    Event.current.Use();
				//    break;

				case KeyCode.Escape:
					Event.current.Use();
					return new SymbolDefinition();

				case KeyCode.UpArrow:
					Event.current.Use();
					if (currentItem >= 0)
						currentItem = Mathf.Max(0, currentItem - 1);
					else
						currentItem = Mathf.Max(0, -1-currentItem);
					break;

				case KeyCode.DownArrow:
					Event.current.Use();
					if (currentItem >= 0)
						currentItem = Mathf.Min(filteredData.Count - 1, currentItem + 1);
					else
						currentItem = Mathf.Min(filteredData.Count - 1, ~currentItem);
					break;

				case KeyCode.PageUp:
					Event.current.Use();
					if (currentItem >= 0)
						currentItem = Mathf.Max(0, currentItem - maxListItems);
					else
						currentItem = Mathf.Max(0, -10-currentItem);
					break;

				case KeyCode.PageDown:
					Event.current.Use();
					if (currentItem >= 0)
						currentItem = Mathf.Min(filteredData.Count - 1, currentItem + maxListItems);
					else
						currentItem = Mathf.Max(0, maxListItems + 1 - currentItem);
					break;

				//case KeyCode.Home:
				//	Event.current.Use();
				//	currentItem = 0;
				//	break;

				//case KeyCode.End:
				//	Event.current.Use();
				//	currentItem = filteredData.Count - 1;
				//	break;
			}
		}

		if (currentItem != focusedItem)
		{
			ScrollToCurrentItem();
			if (currentItem != -1)
			{
				scrollPosition = Mathf.Min(scrollPosition, currentItem);
				scrollPosition = Mathf.Max(scrollPosition, currentItem - maxListItems + 1);
			}
			Repaint();
		}

		if (textEditor.caretPosition.line != lineIndex)
			return new SymbolDefinition();
		if (textEditor.caretPosition.characterIndex < startAtCharacterIndex)
			return new SymbolDefinition();
		if (textEditor.caretPosition.characterIndex > startAtCharacterIndex + typedInPart.Length)
			return new SymbolDefinition();

		return null;
	}

	private void ScrollToCurrentItem()
	{
		int item = currentItem;
		if (item < 0)
			item = ~item;

		scrollPosition = Mathf.Min(scrollPosition, item);
		scrollPosition = Mathf.Max(scrollPosition, item - maxListItems + 1);

		Repaint();
	}

	private void CenterScrollCurrentItem()
	{
		int index = currentItem;
		if (index < 0)
			index = ~index;

		scrollPosition = Mathf.Clamp(index - maxListItems / 2, 0, Mathf.Max(0, filteredData.Count - maxListItems));
		Repaint();
	}
}

class BacktrackingStringMatcher
{
	readonly string filterTextUpperCase;
	readonly ulong filterTextLowerCaseTable;
	readonly ulong filterIsNonLetter;
	readonly ulong filterIsDigit;
	readonly string filterText;
	int[] cachedResult;

	public BacktrackingStringMatcher(string filterText)
	{
		this.filterText = filterText ?? "";
		if (filterText != null)
		{
			for (int i = 0; i < filterText.Length && i < 64; i++)
			{
				filterTextLowerCaseTable |= char.IsLower(filterText[i]) ? 1ul << i : 0;
				filterIsNonLetter |= !char.IsLetterOrDigit(filterText[i]) ? 1ul << i : 0;
				filterIsDigit |= char.IsDigit(filterText[i]) ? 1ul << i : 0;
			}

			filterTextUpperCase = filterText.ToUpper();
		}
		else
		{
			filterTextUpperCase = "";
		}
	}

	public bool CalcMatchRank(string name, out int matchRank)
	{
		if (filterTextUpperCase.Length == 0)
		{
			matchRank = int.MinValue;
			return true;
		}
		var lane = GetMatch(name);
		if (lane != null)
		{
			if (name.Length == filterText.Length)
			{
				matchRank = int.MaxValue;
				for (int n = 0; n < name.Length; n++)
				{
					if (filterText[n] != name[n])
						matchRank--;
				}
				return true;
			}
			// exact named parameter case see discussion in bug #9114
			if (name.Length - 1 == filterText.Length && name[name.Length - 1] == ':')
			{
				matchRank = int.MaxValue - 1;
				for (int n = 0; n < name.Length - 1; n++)
				{
					if (filterText[n] != name[n])
						matchRank--;
				}
				return true;
			}
			int capitalMatches = 0;
			int nonCapitalMatches = 0;
			int matching = 0;
			int fragments = 0;
			int lastIndex = -1;
			for (int n = 0; n < lane.Length; n++)
			{
				var ch = filterText[n];
				var i = lane[n];
				bool newFragment = i > lastIndex + 1;
				if (newFragment)
					fragments++;
				lastIndex = i;
				if (ch == name[i])
				{
					matching += 1000 / (1 + fragments);
					if (char.IsUpper(ch))
						capitalMatches += Mathf.Max(1, 10000 - 1000 * fragments);
				}
				else if (newFragment || i == 0)
				{
					matching += 900 / (1 + fragments);
					if (char.IsUpper(ch))
						capitalMatches += Mathf.Max(1, 1000 - 100 * fragments);
				}
				else
				{
					var x = 600 / (1 + fragments);
					nonCapitalMatches += x;
				}
			}
			matchRank = capitalMatches + matching - fragments + nonCapitalMatches + filterText.Length - name.Length;
			// devalue named parameters.
			if (name[name.Length - 1] == ':')
				matchRank /= 2;
			return true;
		}
		matchRank = int.MinValue;
		return false;
	}

	public bool IsMatch(string text)
	{
		int[] match = GetMatch(text);
		// no need to clear the cache
		cachedResult = cachedResult ?? match;
		return match != null;
	}

	int GetMatchChar(string text, int i, int j, bool onlyWordStart)
	{
		char filterChar = filterTextUpperCase[i];
		char ch;
		// filter char is no letter -> next char should match it - see Bug 674512 - Space doesn't commit generics
		var flag = 1ul << i;
		if ((filterIsNonLetter & flag) != 0)
		{
			for (; j < text.Length; j++)
			{
				if (filterChar == text[j])
					return j;
			}
			return -1;
		}
		// letter case
		ch = text[j];
		bool textCharIsUpper = char.IsUpper(ch);
		if (!onlyWordStart && filterChar == (textCharIsUpper ? ch : char.ToUpper(ch)) && char.IsLetter(ch))
		{
			// cases don't match. Filter is upper char & letter is low, now prefer the match that does the word skip.
			if (!(textCharIsUpper || (filterTextLowerCaseTable & flag) != 0) && j + 1 < text.Length)
			{
				int possibleBetterResult = GetMatchChar(text, i, j + 1, onlyWordStart);
				if (possibleBetterResult >= 0)
					return possibleBetterResult;
			}
			return j;
		}
		// no match, try to continue match at the next word start

		bool lastWasLower = false;
		bool lastWasUpper = false;
		int wordStart = j + 1;
		for (; j < text.Length; j++)
		{
			// word start is either a upper case letter (FooBar) or a char that follows a non letter
			// like foo:bar 
			ch = text[j];
			var category = char.GetUnicodeCategory(ch);
			if (category == System.Globalization.UnicodeCategory.LowercaseLetter)
			{
				if (lastWasUpper && (j - wordStart) > 0)
				{
					if (filterChar == char.ToUpper(text[j - 1]))
						return j - 1;
				}
				lastWasLower = true;
				lastWasUpper = false;
			}
			else if (category == System.Globalization.UnicodeCategory.UppercaseLetter)
			{
				if (lastWasLower)
				{
					if (filterChar == char.ToUpper(ch))
						return j;
				}
				lastWasLower = false;
				lastWasUpper = true;
			}
			else
			{
				if (filterChar == ch)
					return j;
				if (j + 1 < text.Length && filterChar == char.ToUpper(text[j + 1]))
					return j + 1;
				lastWasLower = lastWasUpper = false;
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the match indices.
	/// </summary>
	/// <returns>
	/// The indices in the text which are matched by our filter.
	/// </returns>
	/// <param name='text'>
	/// The text to match.
	/// </param>
	public int[] GetMatch(string text)
	{
		if (string.IsNullOrEmpty(filterTextUpperCase))
			return new int[0];
		if (string.IsNullOrEmpty(text) || filterText.Length > text.Length)
			return null;
		int[] result;
		if (cachedResult != null)
		{
			result = cachedResult;
		}
		else
		{
			cachedResult = result = new int[filterTextUpperCase.Length];
		}
		int j = 0;
		int i = 0;
		bool onlyWordStart = false;
		while (i < filterText.Length)
		{
			if (j >= text.Length)
			{
				if (i > 0)
				{
					j = result[--i] + 1;
					onlyWordStart = true;
					continue;
				}
				return null;
			}

			j = GetMatchChar(text, i, j, onlyWordStart);
			onlyWordStart = false;
			if (j == -1)
			{
				if (i > 0)
				{
					j = result[--i] + 1;
					onlyWordStart = true;
					continue;
				}
				return null;
			}
			else
			{
				result[i] = j++;
			}
			i++;
		}
		cachedResult = null;
		// clear cache
		return result;
	}
}

}
