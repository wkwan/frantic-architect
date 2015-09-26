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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Debug = UnityEngine.Debug;


using ScriptInspector;


[System.Serializable, StructLayout(LayoutKind.Sequential)]
public class FGTextBuffer : ScriptableObject
{
	public enum BlockState : byte
	{
		None = 0,
		CommentBlock = 1,
		StringBlock = 2,
	}

	public class RegionTree
	{
		public enum Kind
		{
			None,
			Region,
			If,
			Elif,
			Else,

			LastActive,
			
			InactiveRegion,
			InactiveIf,
			InactiveElif,
			InactiveElse
		}
		public Kind kind;
		public FormatedLine line;
		public RegionTree parent;
		public List<RegionTree> children;
	}
	public RegionTree rootRegion = new RegionTree();

	[System.Serializable, StructLayout(LayoutKind.Sequential)]
	public class FormatedLine
	{
		[System.NonSerialized]
		public BlockState blockState;
		[System.NonSerialized]
		public RegionTree regionTree;
		[SerializeField, HideInInspector]
		public int lastChange = -1;
		[SerializeField, HideInInspector]
		public int savedVersion = -1;
		[System.NonSerialized]
		public List<SyntaxToken> tokens;
		[System.NonSerialized]
		public int laLines;
		[System.NonSerialized]
		public int index;
	}
	[SerializeField, HideInInspector]
	public FormatedLine[] formatedLines = new FormatedLine[0];
	[SerializeField, HideInInspector]
	public List<string> lines = new List<string>();
	[SerializeField, HideInInspector]
	private string lineEnding = "\n";
	[System.NonSerialized]
	public HashSet<string> hyperlinks = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
	[System.NonSerialized]
	private StreamReader streamReader;
	[SerializeField, HideInInspector]
	public int codePage = Encoding.UTF8.CodePage;
	public Encoding fileEncoding
	{
		get
		{
			return isShader || (isText && codePage == Encoding.UTF8.CodePage) ? new UTF8Encoding(false) : Encoding.GetEncoding(codePage);
		}
	}
	[System.NonSerialized]
	public int numParsedLines = 0;
	[SerializeField, HideInInspector]
	public int longestLine = 0;

	[SerializeField, HideInInspector]
	public bool isJsFile = false;
	[SerializeField, HideInInspector]
	public bool isCsFile = false;
	[SerializeField, HideInInspector]
	public bool isBooFile = false;
	[SerializeField, HideInInspector]
	public bool isText = false;
	[SerializeField, HideInInspector]
	public bool isShader = false;
	
	[System.NonSerialized]
	public FGTextEditor.Styles styles = null;

	[SerializeField, HideInInspector]
	public string guid = "";
	public string assetPath { get; private set; }
	[SerializeField, HideInInspector]
	public bool justSavedNow = false;
	[SerializeField, HideInInspector]
	public bool needsReload = true;
	[System.NonSerializedAttribute]
	public System.DateTime lastModifiedTime;

	[System.NonSerialized]
	private List<FGTextEditor> editors = new List<FGTextEditor>();

	public void AddEditor(FGTextEditor editor)
	{
		editors.Add(editor);
		if (!IsLoading && lines.Count > 0)
			editor.ValidateCarets();
	}

	public void RemoveEditor(FGTextEditor editor)
	{
		editors.Remove(editor);
		if (IsModified && editors.Count == 0)
		{
			EditorApplication.update -= CheckSaveOnUpdate;
			EditorApplication.update += CheckSaveOnUpdate;
		}
	}

	public void CheckSaveOnUpdate()
	{
		EditorApplication.update -= CheckSaveOnUpdate;

		if (IsModified && editors.Count == 0 && !IsAnyWindowMaximized())
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);

			switch (EditorUtility.DisplayDialogComplex(
				"Script Inspector",
				"Save changes to the following asset?          \n\n" + path,
				"Save",
				"Discard Changes",
				"Keep in Memory"))
			{
				case 0:
					if (Save())
						AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
					//EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript)));
					break;
				case 1:
					FGTextBufferManager.DestroyBuffer(this);
					break;
				case 2:
					break;
			}
		}
	}

	public static FGTextBuffer GetBuffer(UnityEngine.Object target)
	{
		return FGTextBufferManager.GetBuffer(target);
	}

	public void OnEnable()
	{
		hideFlags = HideFlags.HideAndDontSave;
		assetPath = AssetDatabase.GUIDToAssetPath(guid);
		
//		try {
//			lastModifiedTime = File.GetLastWriteTime(assetPath);
//		} catch {}

		if (needsReload)
		{
			//Debug.LogError("needsReload == true !!! " + Path.GetFileName(assetPath));
			EditorApplication.update -= ReloadOnNextUpdate;
			EditorApplication.update += ReloadOnNextUpdate;
		}
	}

	public void OnDisable()
	{
		guidsToLoadFirst.Remove(guid);
	}

	public void OnDestroy()
	{
		guidsToLoadFirst.Remove(guid);
	}

	[System.Serializable, StructLayout(LayoutKind.Sequential)]
	public class CaretPos : System.Object, System.IComparable<CaretPos>, System.IEquatable<CaretPos>
	{
		[SerializeField]
		public int virtualColumn;
		[SerializeField]
		public int column;
		[SerializeField]
		public int characterIndex;
		[SerializeField]
		public int line;

		public CaretPos Clone()
		{
			return new CaretPos { virtualColumn = virtualColumn, column = column, characterIndex = characterIndex, line = line };
		}

		public void Set(int line, int characterIndex, int column)
		{
			this.column = column;
			this.characterIndex = characterIndex;
			this.line = line;
		}

		public void Set(int line, int characterIndex, int column, int virtualColumn)
		{
			this.virtualColumn = virtualColumn;
			this.column = column;
			this.characterIndex = characterIndex;
			this.line = line;
		}

		public void Set(CaretPos other)
		{
			virtualColumn = other.virtualColumn;
			column = other.column;
			characterIndex = other.characterIndex;
			line = other.line;
		}

		public bool IsSameAs(CaretPos other)
		{
			return Equals(other) && column == other.column && virtualColumn == other.virtualColumn;
		}

		public int CompareTo(CaretPos other)
		{
			return line == other.line ? characterIndex - other.characterIndex : line - other.line;
		}
		public static bool operator <  (CaretPos A, CaretPos B) { return A.CompareTo(B) < 0; }
		public static bool operator >  (CaretPos A, CaretPos B) { return A.CompareTo(B) > 0; }
		public static bool operator <= (CaretPos A, CaretPos B) { return A.CompareTo(B) <= 0; }
		public static bool operator >= (CaretPos A, CaretPos B) { return A.CompareTo(B) >= 0; }

		public static bool operator == (CaretPos A, CaretPos B)
		{
			if (object.ReferenceEquals(A, B))
				return true;
			if (object.ReferenceEquals(A, null))
				return false;
			if (object.ReferenceEquals(B, null))
				return false;
			return A.Equals(B);
		}
		public static bool operator != (CaretPos A, CaretPos B) { return !(A == B); }

		public bool Equals(CaretPos other)
		{
			return line == other.line && characterIndex == other.characterIndex;
		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null) return base.Equals(obj);

			if (!(obj is CaretPos))
				throw new System.InvalidCastException("The 'obj' argument is not a CaretPos object.");
			else
				return this == (CaretPos) obj;
		}

		public override int GetHashCode()
		{
			return line.GetHashCode() ^ characterIndex.GetHashCode();
		}
	}

	[SerializeField, HideInInspector]
	private bool initialized = false;

	public void Initialize()
	{
		if (!needsReload && initialized && numParsedLines > 0)
			return;
		
		assetPath = AssetDatabase.GUIDToAssetPath(guid);
		if (string.IsNullOrEmpty(assetPath))
			return;
			
		isJsFile = assetPath.EndsWith(".js", System.StringComparison.OrdinalIgnoreCase);
		isCsFile = assetPath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase);
		isBooFile = assetPath.EndsWith(".boo", System.StringComparison.OrdinalIgnoreCase);
		isShader = assetPath.EndsWith(".shader", System.StringComparison.OrdinalIgnoreCase) ||
			assetPath.EndsWith(".cg", System.StringComparison.OrdinalIgnoreCase) ||
			assetPath.EndsWith(".cginc", System.StringComparison.OrdinalIgnoreCase) ||
			assetPath.EndsWith(".hlsl", System.StringComparison.OrdinalIgnoreCase) ||
			assetPath.EndsWith(".hlslinc", System.StringComparison.OrdinalIgnoreCase);
		isText = !(isJsFile || isCsFile || isBooFile || isShader);
			
		styles = isText ? FGTextEditor.stylesText : FGTextEditor.stylesCode;

		if (needsReload || lines == null || lines.Count == 0)
		{
			try {
				lastModifiedTime = File.GetLastWriteTime(assetPath);
			} catch {}
			
			parser = FGParser.Create(this, assetPath);
			//Debug.Log("Parser created: " + parser + " for " + System.IO.Path.GetFileName(assetPath));

			lines = new List<string>();
			lineEnding = "\n";
			try
			{
				Stream stream = new BufferedStream(new FileStream(assetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), 1024);
				if (stream != null)
					streamReader = new StreamReader(stream, true);
				codePage = Encoding.UTF8.CodePage;
			}
			catch (System.Exception error)
			{
				Debug.LogError("Could not read the content of '" + assetPath + "' because of the following error:");
				Debug.LogError(error);
				if (streamReader != null)
				{
					streamReader.Close();
					streamReader.Dispose();
					streamReader = null;
				}
				lastModifiedTime = new System.DateTime();
			}

			formatedLines = new FormatedLine[0];
			hyperlinks.Clear();
			longestLine = 0;
			numParsedLines = 0;
			savedAtUndoPosition = undoPosition;

			EditorApplication.update -= ProgressiveLoadOnUpdate;
			EditorApplication.update += ProgressiveLoadOnUpdate;
		}
		else if (numParsedLines == 0)
		{
			if (parser == null)
			{
				assetPath = AssetDatabase.GUIDToAssetPath(guid);
				parser = FGParser.Create(this, assetPath);
			}

			EditorApplication.update -= ProgressiveLoadOnUpdate;
			EditorApplication.update += ProgressiveLoadOnUpdate;
		}
		else
		{
			initialized = true;
		}
	}
	
	public void LoadImmediately()
	{
		Initialize();
		while (IsLoading)
			ProgressiveLoadOnUpdate();
		ProgressiveLoadOnUpdate();
	}

	public void Reload()
	{
		try
		{
			var lastModifiedTime = File.GetLastWriteTime(assetPath);
			justSavedNow = lastModifiedTime == this.lastModifiedTime;
		} catch {}
		
		needsReload = needsReload || !justSavedNow;
		EditorApplication.update -= ReloadOnNextUpdate;
		EditorApplication.update += ReloadOnNextUpdate;
	}

	private void ReloadOnNextUpdate()
	{
		EditorApplication.update -= ReloadOnNextUpdate;

		if (justSavedNow)
		{
			justSavedNow = false;
			RescanHyperlinks();

			if (parser == null)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				parser = FGParser.Create(this, assetPath);
			}

			UpdateViews();
		}
		else
		{
			FGCodeWindow.CheckAssetRename(guid);

			if (IsModified)
			{
				if (!EditorUtility.DisplayDialog(
					"Script Inspector",
					AssetDatabase.GUIDToAssetPath(guid)
						+ "\n\nThis asset has been modified outside of Unity Editor.\nDo you want to reload it and lose the changes made in Script Inspector?",
					"Reload",
					"Keep changes"))
				{
					needsReload = false;

					savedAtUndoPosition = 0;
					UpdateViews();
					return;
				}
			}

			ReloadNow();
		}
	}

	void ReloadNow()
	{
		formatedLines = new FormatedLine[0];
		lines = new List<string>();
		lineEnding = "\n";
		hyperlinks.Clear();
		if (streamReader != null)
		{
			streamReader.Close();
			streamReader.Dispose();
			streamReader = null;
		}
		codePage = Encoding.UTF8.CodePage;
		numParsedLines = 0;
		longestLine = 0;

		isJsFile = false;
		isCsFile = false;
		isBooFile = false;
		isShader = false;
		isText = false;

		undoBuffer = new List<UndoRecord>();
		undoPosition = 0;
		currentChangeId = 0;
		savedAtUndoPosition = 0;
		//recordUndo = true;
		//beginEditDepth = 0;

		initialized = false;
		Initialize();
	}

	public void RescanHyperlinks()
	{
		hyperlinks.Clear();
		//foreach (var line in formatedLines)
		//{
		//	if (line.tokens == null)
		//		continue;
			
		//	foreach (var token in line.tokens)
		//	{
		//		if (token.style == styles.mailtoStyle || token.style == styles.hyperlinkStyle)
		//		{
		//			string text = token.text;
		//			if (token.style == styles.hyperlinkStyle && text.EndsWith("/"))
		//				text.Remove(text.Length - 1);
		//			hyperlinks.Add(text);
		//		}
		//	}
		//}
	}

	public bool Save()
	{
		if (!TryEdit())
			return false;
		
		var result = true;
		justSavedNow = true;
		
		var assetPath = AssetDatabase.GUIDToAssetPath(guid);
		StreamWriter writer = null;
		try
		{
			using (writer = new StreamWriter(assetPath, false, fileEncoding))
			{
				writer.NewLine = lineEnding;
				int numLines = lines.Count;
				for (int i = 0; i < numLines - 1; ++i)
					writer.WriteLine(lines[i]);
				writer.Write(lines[numLines - 1]);
				writer.Close();
			}

			for (int i = 0; i < numParsedLines; ++i)
				formatedLines[i].savedVersion = formatedLines[i].lastChange;

			savedAtUndoPosition = undoPosition;

			foreach (UndoRecord record in undoBuffer)
				foreach (UndoRecord.TextChange change in record.changes)
					change.savedVersions = null;
		}
		catch
		{
			if (writer != null)
			{
				writer.Close();
				writer.Dispose();
			}
			EditorUtility.DisplayDialog("Error Saving Script", "The script '" + AssetDatabase.GUIDToAssetPath(guid) + "' could not be saved!", "OK");
			justSavedNow = false;
			result = false;
		}

		if (writer != null)
		{
			writer.Close();
			writer.Dispose();
		}
		
		try {
			lastModifiedTime = File.GetLastWriteTime(assetPath);
		} catch {}
			
		UpdateViews();
		
		return result;
	}
	
	private static bool IsAnyWindowMaximized()
	{
		System.Type maximizedType = typeof(EditorWindow).Assembly.GetType("UnityEditor.MaximizedHostView");
		return Resources.FindObjectsOfTypeAll(maximizedType).Length != 0;
	}

	public delegate void ChangeDelegate();
	public ChangeDelegate onChange;

	public void UpdateViews()
	{
		if (onChange != null)
			onChange();
	}

	private static List<string> guidsToLoadFirst = new List<string>();

	public void LoadFaster()
	{
		//if (!guidsToLoadFirst.Contains(guid))
		//	guidsToLoadFirst.Add(guid);
	}

	public void ProgressiveLoadOnUpdate()
	{
		initialized = true;

		if (guidsToLoadFirst.Count > 0 && !guidsToLoadFirst.Contains(guid))
			return;
		
		if (streamReader != null)
		{
			try
			{
				Parse(numParsedLines + 128);
			}
			catch (System.Exception error)
			{
				Debug.LogError("Could not read the content of '" + AssetDatabase.GUIDToAssetPath(guid) + "' because of the following error:");
				Debug.LogError(error);
				if (streamReader != null)
				{
					streamReader.Close();
					streamReader.Dispose();
					streamReader = null;
				}
			}

			if (streamReader == null)
			{
				//if (searchString != string.Empty)
				//    SetSearchText(searchString);
				//focusCodeView = false;
				for (int i = formatedLines.Length; i-- > 0; )
					formatedLines[i].lastChange = -1;
				UpdateViews();
			}
		}
		else if (numParsedLines < lines.Count)
		{
			int toLine = System.Math.Min(numParsedLines + 128, lines.Count - 1);
			ReformatLines(numParsedLines, toLine);
			numParsedLines = toLine + 1;
			UpdateViews();
		}
		else
		{
			needsReload = false;
			EditorApplication.update -= ProgressiveLoadOnUpdate;
			guidsToLoadFirst.Remove(guid);
			ValidateCarets();

			if (parser != null)
				parser.OnLoaded();
		}
	}

	private void ValidateCarets()
	{
		foreach (FGTextEditor editor in editors)
			editor.ValidateCarets();
	}

	public bool CanUndo()
	{
		return undoPosition > 0;
	}

	public bool CanRedo()
	{
		return undoPosition < undoBuffer.Count;
	}

	public void Undo()
	{
		if (!CanUndo())
			return;

		recordUndo = false;
		
		int updateFrom = int.MaxValue;
		int updateTo = -1;

		UndoRecord record = undoBuffer[--undoPosition];
		for (int i = record.changes.Count; i-- != 0; )
		{
			UndoRecord.TextChange change = record.changes[i];

			int changeFromLine = change.from.line;
			int changeToLine = change.to.line;
			if (changeFromLine > changeToLine)
			{
				int temp = changeToLine;
				changeToLine = changeFromLine;
				changeFromLine = temp;
			}

			int[] tempSavedVersions = null;

			CaretPos insertAt = change.from.Clone();
			if (change.newText != string.Empty)
			{
				// Undo inserting text
				string[] textLines = change.newText.Split('\n');
				CaretPos to = change.from.Clone();
				to.characterIndex = textLines.Length > 1 ? textLines[textLines.Length - 1].Length
					: to.characterIndex + change.newText.Length;
				to.line += textLines.Length - 1;
				to.virtualColumn = to.column = CharIndexToColumn(to.characterIndex, to.line);

				int numLinesChanging = 1 + to.line - changeFromLine;

				tempSavedVersions = new int[numLinesChanging];
				for (int j = 0; j < numLinesChanging; ++j)
					tempSavedVersions[j] = formatedLines[changeFromLine + j].savedVersion;

				insertAt = DeleteText(change.from, to);
				
				if (updateFrom > insertAt.line)
					updateFrom = insertAt.line;
				if (updateTo > insertAt.line)
					updateTo -= numLinesChanging - 1;
				if (updateTo < updateFrom)
					updateTo = updateFrom;
			}
			if (change.oldText != string.Empty)
			{
				// Undo deleting text
				CaretPos insertedTo = InsertText(insertAt, change.oldText);
				
				if (updateFrom > insertAt.line)
					updateFrom = insertAt.line;
				if (updateTo < insertAt.line)
					updateTo = insertAt.line;
				//if (updateTo >= insertAt.line)
				updateTo += insertedTo.line - insertAt.line;
			}
			//UpdateHighlighting(changeFromLine, changeToLine);
			for (int j = change.oldLineChanges.Length; j-- > 0; )
			{
				formatedLines[j + changeFromLine].lastChange = change.oldLineChanges[j];
				if (change.savedVersions != null && change.savedVersions.Length == 1 + changeToLine - changeFromLine)
				{
					formatedLines[j + changeFromLine].savedVersion = change.savedVersions[j];
				}
			}

			change.savedVersions = tempSavedVersions;
		}
		activeEditor.caretPosition = record.preCaretPos.Clone();
		if (record.preCaretPos == record.preSelectionPos)
			activeEditor.selectionStartPosition = null;
		else
			activeEditor.selectionStartPosition = record.preSelectionPos.Clone();
		activeEditor.caretMoveTime = Time.realtimeSinceStartup;
		activeEditor.scrollToCaret = true;
		
		//var preserveTo = Mathf.Min(updateTo + 1, formatedLines.Length - 1);
		//var lastChanges = new int[preserveTo - updateFrom + 1];
		//for (int i = updateFrom; i <= preserveTo; ++i)
		//	lastChanges[i - updateFrom] = formatedLines[i].lastChange;
		UpdateHighlighting(updateFrom, updateTo, true);
		//for (int i = updateFrom; i <= preserveTo; ++i)
		//	formatedLines[i].lastChange = lastChanges[i - updateFrom];
		
		recordUndo = true;
	}

	public void Redo()
	{
		if (!CanRedo())
			return;

		recordUndo = false;

		int updateFrom = int.MaxValue;
		int updateTo = -1;

		UndoRecord record = undoBuffer[undoPosition++];

		for (int i = 0; i < record.changes.Count; ++i)
		{
			UndoRecord.TextChange change = record.changes[i];

			int changeFromLine = change.from.line;
			int changeToLine = change.to.line;
			if (changeFromLine > changeToLine)
			{
				int temp = changeToLine;
				changeToLine = changeFromLine;
				changeFromLine = temp;
			}

			int numLinesChanging = 1 + changeToLine - changeFromLine;

			int[] tempSavedVersions = new int[numLinesChanging];
			for (int j = numLinesChanging; j-- > 0; )
				tempSavedVersions[j] = formatedLines[j + changeFromLine].savedVersion;

			CaretPos newPos = change.from.Clone();
			if (change.oldText != string.Empty)
			{
				// Redo deleting text
				newPos = DeleteText(change.from, change.to);
				
				if (updateFrom > newPos.line)
					updateFrom = newPos.line;
				if (updateTo > newPos.line)
					updateTo -= numLinesChanging - 1;
				if (updateTo < updateFrom)
					updateTo = updateFrom;
			}
			if (change.newText != string.Empty)
			{
				// Redo inserting text
				newPos = InsertText(newPos, change.newText);
				
				if (updateFrom > changeFromLine)
					updateFrom = changeFromLine;
				if (updateTo < changeFromLine)
					updateTo = changeFromLine;
				//if (updateTo >= changeFromLine)
				updateTo += newPos.line - changeFromLine;
			}
			//UpdateHighlighting(changeFromLine, newPos.line);
			for (int j = changeFromLine; j <= newPos.line; ++j)
			{
				formatedLines[j].lastChange = record.changeId;
				if (change.savedVersions != null && change.savedVersions.Length != 0)
				{
					formatedLines[j].savedVersion = change.savedVersions[j - changeFromLine];
				}
			}

			change.savedVersions = tempSavedVersions;
		}
		activeEditor.caretPosition = record.postCaretPos.Clone();
		if (record.postCaretPos == record.postSelectionPos)
			activeEditor.selectionStartPosition = null;
		else
			activeEditor.selectionStartPosition = record.postSelectionPos.Clone();
		activeEditor.caretMoveTime = Time.realtimeSinceStartup;
		activeEditor.scrollToCaret = true;

		//var preserveTo = Mathf.Min(updateTo + 1, formatedLines.Length - 1);
		//var lastChanges = new int[preserveTo - updateFrom + 1];
		//for (int i = updateFrom; i <= preserveTo; ++i)
		//	lastChanges[i - updateFrom] = formatedLines[i].lastChange;
		UpdateHighlighting(updateFrom, updateTo, true);
		//for (int i = updateFrom; i <= preserveTo; ++i)
		//	formatedLines[i].lastChange = lastChanges[i - updateFrom];
		
		recordUndo = true;
	}

	public int CharIndexToColumn(int charIndex, int line, int start)
	{
		if (lines.Count == 0 || line >= lines.Count)
			return 0;
		string s = lines[line];
		if (s.Length < charIndex)
			charIndex = s.Length;

		int col = 0;
		for (int i = start; i < charIndex; ++i)
			col += s[i] != '\t' ? 1 : 4 - (col & 3);
		return col;
	}

	public int CharIndexToColumn(int charIndex, int line)
	{
		if (lines.Count == 0 || line >= lines.Count)
			return 0;
		string s = lines[line];
		if (s.Length < charIndex)
			charIndex = s.Length;

		int col = 0;
		for (int i = 0; i < charIndex; ++i)
			col += s[i] != '\t' ? 1 : 4 - (col & 3);
		return col;
	}

	public int ColumnToCharIndex(ref int column, int line, int rowStart)
	{
		line = System.Math.Max(0, System.Math.Min(line, lines.Count - 1));
		column = System.Math.Max(0, column);

		if (lines.Count == 0)
			return 0;
		var s = lines[line];

		var i = rowStart;
		var col = 0;
		while (i < s.Length && col < column)
		{
			if (s[i] == '\t')
				col = (col & ~3) + 4;
			else
				++col;
			++i;
		}
		if (i == s.Length)
		{
			column = col;
		}
		else if (col > column)
		{
			if ((column & 3) < 2)
			{
				--col;
				--i;
				column &= ~3;
			}
			else
			{
				column = (column & ~3) + 4;
			}
		}
		return i;
	}

	public int ColumnToCharIndex(ref int column, int line)
	{
		line = System.Math.Max(0, System.Math.Min(line, numParsedLines - 1));
		column = System.Math.Max(0, column);

		if (lines.Count == 0 || line >= lines.Count)
			return 0;
		string s = lines[line];

		int i = 0;
		int col = 0;
		while (i < s.Length && col < column)
		{
			if (s[i] == '\t')
				col = (col & ~3) + 4;
			else
				++col;
			++i;
		}
		if (i == s.Length)
		{
			column = col;
		}
		else if (col > column)
		{
			if ((column & 3) < 2)
			{
				--col;
				--i;
				column &= ~3;
			}
			else
			{
				column = (column & ~3) + 4;
			}
		}
		return i;
	}

	public string GetTextRange(CaretPos from, CaretPos to)
	{
		int fromCharIndex, fromLine, toCharIndex, toLine;
		if (from < to)
		{
			fromCharIndex = from.characterIndex;
			fromLine = from.line;
			toCharIndex = to.characterIndex;
			toLine = to.line;
		}
		else
		{
			fromCharIndex = to.characterIndex;
			fromLine = to.line;
			toCharIndex = from.characterIndex;
			toLine = from.line;
		}

		StringBuilder buffer = new StringBuilder();
		if (fromLine == toLine)
		{
			buffer.Append(lines[fromLine].Substring(fromCharIndex, toCharIndex - fromCharIndex));
		}
		else
		{
			buffer.Append(lines[fromLine].Substring(fromCharIndex) + '\n');
			for (int i = fromLine + 1; i < toLine; ++i)
				buffer.Append(lines[i] + '\n');
			buffer.Append(lines[toLine].Substring(0, toCharIndex));
		}

		return buffer.ToString();
	}

	public static int GetCharClass(char c)
	{
		if (c == ' ' || c == '\t')
			return 0;
		if (c >= '0' && c <= '9')
			return 1;
		if (c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
			return 2;
		return 3;
	}

	public bool GetWordExtents(int charIndex, int line, out int wordStart, out int wordEnd)
	{
		wordStart = charIndex;
		wordEnd = charIndex;
		if (line >= formatedLines.Length)
			return false;

		string text = lines[line];
		int length = text.Length;
		wordStart = wordEnd = System.Math.Min(charIndex, length - 1);
		if (wordStart < 0)
			return false;

		int cc = GetCharClass(text[wordStart]);
		if (wordStart > 0 && cc == 0)
		{
			--wordStart;
			cc = GetCharClass(text[wordStart]);
			if (cc != 0)
				--wordEnd;
		}
		if (cc == 3)
		{
			++wordEnd;
		}
		else if (cc == 0)
		{
			while (wordStart > 0 && GetCharClass(text[wordStart - 1]) == 0)
				--wordStart;
			while (wordEnd < length && GetCharClass(text[wordEnd]) == 0)
				++wordEnd;
		}
		else
		{
			while (wordStart > 0)
			{
				char ch = text[wordStart - 1];
				int c = GetCharClass(ch);
				if (c == 1 || c == 2 || cc == 1 && ch == '.')
					--wordStart;
				else
					break;
				cc = c;
			}
			while (wordEnd < length)
			{
				int c = GetCharClass(text[wordEnd]);
				if (c == 1 || c == 2 || cc == 1 && text[wordEnd] == '.')
					++wordEnd;
				else
					break;
			}
		}
		return true;
	}

	private static int DigitsAsLetters(int characterClass)
	{
		return characterClass == 2 ? 1 : characterClass;
	}

	public CaretPos WordStopLeft(CaretPos from)
	{
		int column = from.characterIndex;
		int line = from.line;

		if (column == 0)
		{
			if (line == 0)
				return new CaretPos { characterIndex = 0, column = 0, line = 0, virtualColumn = 0 };

			--line;
			column = lines[line].Length;
		}

		string s = lines[line];

		if (column > 0)
		{
			int characterClass = DigitsAsLetters(GetCharClass(s[--column]));

			while (column > 0 && characterClass == 0)
				characterClass = DigitsAsLetters(GetCharClass(s[--column]));

			while (column > 0 && DigitsAsLetters(GetCharClass(s[column - 1])) == characterClass)
				--column;
		}

		return new CaretPos { characterIndex = column, column = CharIndexToColumn(column, line), line = line, virtualColumn = column };
	}

	public CaretPos WordStopRight(CaretPos from)
	{
		int column = from.characterIndex;
		int line = from.line;

		if (column >= lines[line].Length)
		{
			if (line == lines.Count - 1)
				return new CaretPos { characterIndex = column, column = CharIndexToColumn(column, line), line = line, virtualColumn = column };

			++line;
			column = 0;
		}

		string s = lines[line];

		if (column < s.Length)
		{
			int characterClass = DigitsAsLetters(GetCharClass(s[column++]));

			if (characterClass != 0)
			{
				while (column < s.Length)
				{
					int nextClass = DigitsAsLetters(GetCharClass(s[column]));
					if (nextClass != characterClass)
					{
						characterClass = nextClass;
						break;
					}
					else
					{
						++column;
					}
				}
			}

			if (characterClass == 0)
				while (column < s.Length && GetCharClass(s[column]) == 0)
					++column;
		}

		return new CaretPos { characterIndex = column, column = CharIndexToColumn(column, line), line = line, virtualColumn = column };
	}

	[System.Serializable, StructLayout(LayoutKind.Sequential)]
	private class UndoRecord
	{
		[System.Serializable, StructLayout(LayoutKind.Sequential)]
		public class TextChange
		{
			[SerializeField, HideInInspector]
			public CaretPos from;
			[SerializeField, HideInInspector]
			public CaretPos to;
			[SerializeField, HideInInspector]
			public string oldText;
			[SerializeField, HideInInspector]
			public string newText;
			[SerializeField, HideInInspector]
			public int[] oldLineChanges;
			[SerializeField, HideInInspector]
			public int[] savedVersions;
		}
		[SerializeField, HideInInspector]
		public List<TextChange> changes;
		[SerializeField, HideInInspector]
		public int changeId;
		[SerializeField, HideInInspector]
		public CaretPos preCaretPos;
		[SerializeField, HideInInspector]
		public CaretPos preSelectionPos;
		[SerializeField, HideInInspector]
		public CaretPos postCaretPos;
		[SerializeField, HideInInspector]
		public CaretPos postSelectionPos;
		[SerializeField, HideInInspector]
		public string actionType;
	}
	[SerializeField, HideInInspector]
	private List<UndoRecord> undoBuffer = new List<UndoRecord>();
	[System.NonSerialized]
	private UndoRecord tempUndoRecord;
	[SerializeField, HideInInspector]
	public int undoPosition = 0;
	[SerializeField, HideInInspector]
	public int savedAtUndoPosition = 0;
	[SerializeField, HideInInspector]
	public int currentChangeId = 0;
	[System.NonSerialized]
	private bool recordUndo = true;
	[System.NonSerialized]
	private int beginEditDepth = 0;
	[System.NonSerialized]
	private List<FormatedLine> updatedLines = new List<FormatedLine>();

	public bool IsModified { get { return undoPosition != savedAtUndoPosition; } }
	public bool IsLoading { get { return needsReload || streamReader != null || numParsedLines != lines.Count; } }

	public void BeginEdit(string description)
	{
		if (!recordUndo)
			return;

		if (beginEditDepth++ == 0)
		{
			var caretPosition = activeEditor.caretPosition;
			var selectionStartPosition = activeEditor.selectionStartPosition;
			
			tempUndoRecord = tempUndoRecord ?? new UndoRecord();
			tempUndoRecord.changes = tempUndoRecord.changes ?? new List<UndoRecord.TextChange>();
			tempUndoRecord.changeId = currentChangeId + 1;
			tempUndoRecord.actionType = description;
			if (tempUndoRecord.preCaretPos == null)
			{
				tempUndoRecord.preCaretPos = caretPosition.Clone();
			}
			else
			{
				tempUndoRecord.preCaretPos.line = caretPosition.line;
				tempUndoRecord.preCaretPos.characterIndex = caretPosition.characterIndex;
				tempUndoRecord.preCaretPos.column = caretPosition.column;
				tempUndoRecord.preCaretPos.virtualColumn = caretPosition.virtualColumn;
			}
			if (selectionStartPosition != null)
			{
				if (tempUndoRecord.preSelectionPos == null)
				{
					tempUndoRecord.preSelectionPos = selectionStartPosition.Clone();
				}
				else
				{
					tempUndoRecord.preSelectionPos.line = selectionStartPosition.line;
					tempUndoRecord.preSelectionPos.characterIndex = selectionStartPosition.characterIndex;
					tempUndoRecord.preSelectionPos.column = selectionStartPosition.column;
					tempUndoRecord.preSelectionPos.virtualColumn = selectionStartPosition.virtualColumn;
				}
			}
			else
			{
				if (tempUndoRecord.preSelectionPos == null)
				{
					tempUndoRecord.preSelectionPos = caretPosition.Clone();
				}
				else
				{
					tempUndoRecord.preSelectionPos.line = caretPosition.line;
					tempUndoRecord.preSelectionPos.characterIndex = caretPosition.characterIndex;
					tempUndoRecord.preSelectionPos.column = caretPosition.column;
					tempUndoRecord.preSelectionPos.virtualColumn = caretPosition.virtualColumn;
				}
			}

			if (updatedLines != null)
				updatedLines.Clear();
			else
				updatedLines = new List<FormatedLine>();
		}
	}

	private void RegisterUndoText(string actionType, CaretPos from, CaretPos to, string text)
	{
		if (!recordUndo)
			return;

		UndoRecord.TextChange change = new UndoRecord.TextChange();
		if (from < to)
		{
			change.from = from.Clone();
			change.to = to.Clone();
		}
		else
		{
			change.from = to.Clone();
			change.to = from.Clone();
		}
		change.oldText = GetTextRange(change.from, change.to);
		change.newText = text;
		change.oldLineChanges = new int[1 + change.to.line - change.from.line];
		change.savedVersions = new int[1 + change.to.line - change.from.line];
		for (int i = change.oldLineChanges.Length; i-- > 0; )
		{
			change.oldLineChanges[i] = formatedLines[i + change.from.line].lastChange;
			change.savedVersions[i] = formatedLines[i + change.from.line].savedVersion;
		}
		tempUndoRecord.changes.Add(change);

		tempUndoRecord.actionType = actionType;
	}

	public static FGTextEditor activeEditor = null;

	public void EndEdit()
	{
		if (!recordUndo)
			return;

		if (--beginEditDepth > 0)
			return;

		if (tempUndoRecord.changes.Count == 0)
			return;

		tempUndoRecord.postCaretPos = activeEditor.caretPosition.Clone();
		tempUndoRecord.postSelectionPos = activeEditor.selectionStartPosition != null ? activeEditor.selectionStartPosition.Clone() : activeEditor.caretPosition.Clone();

		bool addNewRecord = true;

		if (undoPosition < undoBuffer.Count)
		{
			undoBuffer.RemoveRange(undoPosition, undoBuffer.Count - undoPosition);
			if (savedAtUndoPosition > undoPosition)
				savedAtUndoPosition = -1;
		}
		else
		{
			// Check is it fine to combine with previous record
			if (undoPosition > 0 && tempUndoRecord.changes.Count == 1)
			{
				UndoRecord last = undoBuffer[undoPosition - 1];
				if (IsModified && last.changes.Count == 1 && last.postCaretPos == tempUndoRecord.preCaretPos && last.postSelectionPos == tempUndoRecord.preSelectionPos)
				{
					UndoRecord.TextChange currChange = tempUndoRecord.changes[0];
					UndoRecord.TextChange prevChange = last.changes[0];
					if (currChange.oldText == string.Empty && currChange.newText.Length == 1 && prevChange.newText != string.Empty)
					{
						int currCharClass = GetCharClass(currChange.newText[0]);
						int prevCharClass = GetCharClass(prevChange.newText[prevChange.newText.Length - 1]);
						if (currCharClass == prevCharClass)
						{
							addNewRecord = false;
							prevChange.newText += currChange.newText;
							last.changes[0] = prevChange;
							last.postCaretPos = tempUndoRecord.postCaretPos.Clone();
							last.postSelectionPos = tempUndoRecord.postSelectionPos.Clone();
							//undoBuffer[undoPosition - 1] = prevRecord;
						}
					}
				}
			}
		}

		if (addNewRecord)
		{
			undoBuffer.Add(tempUndoRecord);
			++undoPosition;
			++currentChangeId;
			
			tempUndoRecord = new UndoRecord();
		}
		else
		{
			tempUndoRecord.changes.Clear();
			tempUndoRecord.postCaretPos = null;
			tempUndoRecord.postSelectionPos = null;
		}

		foreach (FormatedLine formatedLine in updatedLines)
			formatedLine.lastChange = currentChangeId;
	}

	public CaretPos DeleteText(CaretPos fromPos, CaretPos toPos)
	{
		CaretPos from = fromPos.Clone();
		CaretPos to = toPos.Clone();

		int fromTo = from.CompareTo(to);
		if (fromTo == 0)
			return from.Clone();

		RegisterUndoText("Delete Text", from, to, string.Empty);

		if (fromTo > 0)
		{
			CaretPos temp = from;
			from = to;
			to = temp;
		}

		if (from.line == to.line)
		{
			lines[from.line] = lines[from.line].Remove(from.characterIndex, to.characterIndex - from.characterIndex);
		}
		else
		{
			lines[from.line] = lines[from.line].Substring(0, from.characterIndex) + lines[to.line].Substring(to.characterIndex);
			lines.RemoveRange(from.line + 1, to.line - from.line);
			for (int i = 1; to.line + i < formatedLines.Length; ++i)
			{
				formatedLines[from.line + i] = formatedLines[to.line + i];
				formatedLines[from.line + i].index = from.line + i;
			}
			System.Array.Resize(ref formatedLines, formatedLines.Length - to.line + from.line);
			numParsedLines -= to.line - from.line;

			NotifyRemovedLines(from.line + 1, to.line - from.line);
		}
		
		NotifyRemovedText(from, to);

		return from;
	}
	
	public bool CanEdit()
	{
		if (IsModified)
			return true;
		
		if (!File.Exists(assetPath) || (File.GetAttributes(assetPath) & FileAttributes.ReadOnly) == 0)
			return true;
		
		return false;
	}
	
	private static System.Type P4Connect_Engine;
	private static System.Reflection.MethodInfo method_CheckoutAssets;
	private static System.Type P4Connect_Queries;
	private static System.Reflection.MethodInfo method_GetFileState;
	
	private void TryP4Checkout()
	{
		if (P4Connect_Engine == null)
		{
			P4Connect_Engine = System.Type.GetType("P4Connect.Engine,P4Connect");
			
			if (P4Connect_Engine != null)
			{
				method_CheckoutAssets = P4Connect_Engine.GetMethod("CheckoutAssets");
				
				P4Connect_Queries = System.Type.GetType("P4Connect.Queries,P4Connect");
				if (P4Connect_Queries != null)
					method_GetFileState = P4Connect_Queries.GetMethod("GetFileState");
			}
		}
		
		if (method_CheckoutAssets == null || method_GetFileState == null)
			return;
		
		//var assetPathsArray = new object[]{ assetPath };
		//var returnedObject = method_GetFileState.Invoke(null, assetPathsArray);
		//var checkout = returnedObject.ToString() == "InDepot";
		//if (checkout)
			method_CheckoutAssets.Invoke(null, new object[]{ new[]{assetPath} });
		EditorApplication.RepaintProjectWindow();
	}
	
	public bool TryEdit()
	{
		if (IsModified || !File.Exists(assetPath) || (File.GetAttributes(assetPath) & FileAttributes.ReadOnly) == 0)
			return true;
		
#if !UNITY_4_0
		var versionControlAsset = UnityEditor.VersionControl.Provider.GetAssetByGUID(guid);
		if (versionControlAsset == null)
			return true;
		
		if ((versionControlAsset.state & UnityEditor.VersionControl.Asset.States.ReadOnly) == 0 ||
			(versionControlAsset.state & UnityEditor.VersionControl.Asset.States.AddedLocal) != 0)
		{
			if ((versionControlAsset.state & UnityEditor.VersionControl.Asset.States.AddedLocal) == 0)
#endif
				if (File.Exists(assetPath) && (File.GetAttributes(assetPath) & FileAttributes.ReadOnly) != 0)
					TryP4Checkout();
			
			if (File.Exists(assetPath) && (File.GetAttributes(assetPath) & FileAttributes.ReadOnly) != 0)
				return EditReadOnly();
		
			return true;
#if !UNITY_4_0
		}
		
		var result = false;
		
		var checkoutTask = UnityEditor.VersionControl.Provider.Checkout(versionControlAsset, UnityEditor.VersionControl.CheckoutMode.Both);
		try
		{
			checkoutTask.Wait();
			foreach (var message in checkoutTask.messages)
				if (message.severity == UnityEditor.VersionControl.Message.Severity.Warning || message.severity == UnityEditor.VersionControl.Message.Severity.Error)
					message.Show();
			result = checkoutTask.success;
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
		}
		finally
		{
			if (checkoutTask != null)
				checkoutTask.Dispose();
		}
		
		return result || EditReadOnly();
#endif
	}
	
	private bool EditReadOnly()
	{
		var focus = EditorWindow.focusedWindow;
		var answer = EditorUtility.DisplayDialog(
			"Script Inspector",
			"This file is read-only! You may not be able to save the changes.\n\n" +
			"Would you still like to edit it?          \n\n",
			"Yes, Edit in Memory",
			"No, Don't Edit");
		if (focus)
			focus.Focus();
		return answer;
	}

	public CaretPos InsertText(CaretPos position, string text)
	{
		RegisterUndoText("Insert Text", position, position, text);

		CaretPos pos = new CaretPos { characterIndex = position.characterIndex, column = position.column, virtualColumn = position.column, line = position.line };
		CaretPos end = new CaretPos { characterIndex = position.characterIndex, column = position.column, virtualColumn = position.column, line = position.line };

		string[] insertLines = text.Split(new char[] { '\n' }, System.StringSplitOptions.None);

		if (insertLines.Length == 1)
		{
			lines[pos.line] = lines[pos.line].Insert(pos.characterIndex, text);

			end.characterIndex += text.Length;
			end.column = end.virtualColumn = CharIndexToColumn(end.characterIndex, end.line);
		}
		else
		{
			lines.Insert(pos.line + 1, insertLines[insertLines.Length - 1] + lines[pos.line].Substring(pos.characterIndex));
			lines[pos.line] = lines[pos.line].Substring(0, pos.characterIndex) + insertLines[0];
			for (int i = 1; i < insertLines.Length - 1; ++i)
				lines.Insert(pos.line + i, insertLines[i]);

			end.characterIndex = insertLines[insertLines.Length - 1].Length;
			end.line = pos.line + insertLines.Length - 1;
			end.column = end.virtualColumn = CharIndexToColumn(end.characterIndex, end.line);

			System.Array.Resize(ref formatedLines, formatedLines.Length + insertLines.Length - 1);
			for (int i = formatedLines.Length - 1; i > end.line; --i)
			{
				formatedLines[i] = formatedLines[i - insertLines.Length + 1];
				formatedLines[i].index = i;
			}
			for (int i = 1; i <= insertLines.Length - 1; ++i)
				formatedLines[pos.line + i] = new FormatedLine { index = pos.line + i };
			numParsedLines = formatedLines.Length;

			NotifyInsertedLines(pos.line + 1, insertLines.Length - 1);
		}
		
		NotifyInsertedText(position, end);

		return end;
	}

	public delegate void InsertedLinesDelegate(int lineIndex, int numLines);
	public InsertedLinesDelegate onInsertedLines;
	
	public delegate void InsertedLinesAllDelegate(string assetGuid, int lineIndex, int numLines);
	public static InsertedLinesAllDelegate onInsertedLinesAll;
	
	private void NotifyInsertedLines(int lineIndex, int numLines)
	{
		if (onInsertedLines != null)
			onInsertedLines(lineIndex, numLines);
		if (onInsertedLinesAll != null)
			onInsertedLinesAll(guid, lineIndex, numLines);
	}

	public delegate void InsertedTextDelegate(CaretPos from, CaretPos to);
	public InsertedTextDelegate onInsertedText;

	private void NotifyInsertedText(CaretPos from, CaretPos to)
	{
		FGTextBufferManager.OnInsertedText(this, from, to);
		
		if (onInsertedText != null)
			onInsertedText(from.Clone(), to.Clone());
	}

	public delegate void RemovedLinesDelegate(int lineIndex, int numLines);
	public RemovedLinesDelegate onRemovedLines;

	public delegate void RemovedLinesAllDelegate(string assetGuid, int lineIndex, int numLines);
	public static RemovedLinesAllDelegate onRemovedLinesAll;

	private void NotifyRemovedLines(int lineIndex, int numLines)
	{
		if (onRemovedLines != null)
			onRemovedLines(lineIndex, numLines);
		if (onRemovedLinesAll != null)
			onRemovedLinesAll(guid, lineIndex, numLines);
	}

	public delegate void RemovedTextDelegate(CaretPos from, CaretPos to);
	public RemovedTextDelegate onRemovedText;
	
	private void NotifyRemovedText(CaretPos from, CaretPos to)
	{
		FGTextBufferManager.OnRemovedText(this, from, to);
		
		if (onRemovedText != null)
			onRemovedText(from.Clone(), to.Clone());
	}
	
	public int FirstNonWhitespace(int atLine)
	{
		int index = 0;
		string line = lines[atLine];
		while (index < line.Length)
		{
			char c = line[index];
			if (c != ' ' && c != '\t')
				break;
			++index;
		}
		return index;
	}
	
	public TextPosition FirstNonWhitespacePos(int fromLine, int fromCharIndex)
	{
		int index = fromCharIndex;
		for (var i = fromLine; i < lines.Count; ++i)
		{
			string line = lines[i];
			while (index < line.Length)
			{
				char c = line[index];
				if (c != ' ' && c != '\t')
				{
					return new TextPosition(i, index);
				}
				++index;
			}
			index = 0;
		}
		return new TextPosition(lines.Count, 0);
	}
	
	private static readonly string[] spaces = { "    ", "   ", "  ", " " };
	private static Dictionary<string, string>[] expandedCache = {
		new Dictionary<string, string>(),
		new Dictionary<string, string>(),
		new Dictionary<string, string>(),
		new Dictionary<string, string>(),
	};
	public static string ExpandTabs(string s, int startAtColumn)
	{
		// Replacing tabs with spaces for proper alignment
		int tabPos = s.IndexOf('\t', 0);
		if (tabPos == -1)
			return s;

		string cached;
		if (expandedCache[startAtColumn & 3].TryGetValue(s, out cached))
			return cached;

		int startFrom = 0;
		var sb = new StringBuilder();
		while ((tabPos = s.IndexOf('\t', startFrom)) != -1)
		{
			sb.Append(s, startFrom, tabPos - startFrom);
			sb.Append(spaces[(sb.Length + startAtColumn) & 3]);
			startFrom = tabPos + 1;
		}
		if (startFrom == 0)
			return s;
		sb.Append(s.Substring(startFrom));
		
		cached = sb.ToString();
		expandedCache[startAtColumn & 3][s] = cached;
		return cached;
	}

	private void Parse(int parseToLine)
	{
		// Is there still anything left for reading/parsing?
		if (streamReader == null)
			return;

		// Reading lines till parseToLine-th line
		for (int i = numParsedLines; i < parseToLine; ++i)
		{
			string line = "";
			if (i == 0)
			{
				var sb = new StringBuilder();
				while (!streamReader.EndOfStream)
				{
					char[] buffer = new char[1];
					streamReader.ReadBlock(buffer, 0, 1);
					if (buffer[0] == '\r' || buffer[0] == '\n')
					{
						lineEnding = buffer[0].ToString();
						if (!streamReader.EndOfStream)
						{
							string next = char.ConvertFromUtf32(streamReader.Peek());
							if (next != lineEnding && (next == "\r" || next == "\n"))
							{
								lineEnding += next;
								streamReader.ReadBlock(buffer, 0, 1);
							}
						}
						break;
					}
					else
					{
						sb.Append(buffer[0]);
					}
				}
				line = sb.ToString();

				if (streamReader != null)
				{
					codePage = streamReader.CurrentEncoding.CodePage;
				}
			}
			else
			{
				line = streamReader.ReadLine();
			}

			if (line == null)
			{
				if (streamReader.BaseStream.Position > 0)
				{
					streamReader.BaseStream.Position -= 1;
					int last = streamReader.BaseStream.ReadByte();
					if (last == 0 && streamReader.BaseStream.Position > 1)
					{
						streamReader.BaseStream.Position -= 2;
						last = streamReader.BaseStream.ReadByte();
					}
					if (last == 10 || last == 13)
					{
						lines.Add(System.String.Empty);
					}
				}

				streamReader.Close();
				streamReader.Dispose();
				streamReader = null;
				needsReload = false;
				break;
			}

			lines.Add(line);
		}
		if (formatedLines.Length == parseToLine)
			return;

		parseToLine = System.Math.Min(parseToLine, lines.Count);
		System.Array.Resize(ref formatedLines, parseToLine);

		for (int currentLine = numParsedLines; currentLine < parseToLine; ++currentLine)
		{
			FormatLine(currentLine);
		}

		numParsedLines = parseToLine;
	}

	System.Func<bool> progressiveParser;

	void ProgressiveParseOnUpdate()
	{
		if (progressiveParser == null || !progressiveParser())
		{
			EditorApplication.update -= ProgressiveParseOnUpdate;
			progressiveParser = null;
		}
	}
	
	public SyntaxToken FirstNonTriviaToken(int line)
	{
		var formatedLine = formatedLines[line];
		if (formatedLine == null)
			return null;
		
		var tokensInLine = formatedLine.tokens;
		if (tokensInLine == null || tokensInLine.Count == 0)
			return null;

		SyntaxToken firstToken = null;
		for (var i = 0; i < tokensInLine.Count; ++i)
		{
			if (tokensInLine[i].tokenKind > SyntaxToken.Kind.LastWSToken)
			{
				firstToken = tokensInLine[i];
				break;
			}
		}
		return firstToken;
	}
	
	private void GetFirstTokens(int line, out SyntaxToken firstToken, out SyntaxToken firstNonTrivia)
	{
		firstToken = null;
		firstNonTrivia = null;
		
		var tokens = formatedLines[line].tokens;
		for (var i = 0; i < tokens.Count; ++i)
		{
			var t = tokens[i];
			if (t.tokenKind > SyntaxToken.Kind.Whitespace)
			{
				firstToken = t;
				do {
					if (tokens[i].tokenKind > SyntaxToken.Kind.LastWSToken &&
						tokens[i].parent != null && tokens[i].parent.parent != null)
					{
						firstNonTrivia = tokens[i];
						break;
					}
					++i;
				} while (i < tokens.Count);
				break;
			}
		}
	}
	
	public SyntaxToken FirstNonWhitespaceToken(int line)
	{
		var tokens = formatedLines[line].tokens;
		if (tokens == null || tokens.Count == 0)
			return null;
		
		for (int i = 0; i < tokens.Count; ++i)
			if (tokens[i].tokenKind != SyntaxToken.Kind.Whitespace)
				return tokens[i];
		
		return null;
	}
	
	public TextPosition GetOpeningBraceLeftOf(int tokenLine, int tokenIndex, int maxLinesDistance)
	{
		var firstLine = maxLinesDistance >= 0 ? Mathf.Max(0, tokenLine - maxLinesDistance) : 0;
		var bracePosition = new TextPosition();
		var tokens = formatedLines[tokenLine].tokens;
		
		var skipOver = 0;
		while (tokenIndex < 0)
		{
			if (--tokenLine < firstLine)
				break;
			tokens = formatedLines[tokenLine].tokens;
			tokenIndex = tokens.Count - 1;
		}
		while (tokenIndex >= 0)
		{
			var tokenLeft = tokens[tokenIndex];
			var text = tokenLeft.text;
			if (tokenLeft.tokenKind == SyntaxToken.Kind.Punctuator && text.Length == 1)
			{
				var c = text[0];
				if ('(' == c || '[' == c || '{' == c)
				{
					if (skipOver > 0)
					{
						--skipOver;
					}
					else
					{
						bracePosition = new TextPosition(tokenLine, tokenIndex);
						break;
					}
				}
				else if (')' == c || ']' == c || '}' == c)
				{
					++skipOver;
				}
			}
			while (--tokenIndex < 0)
			{
				if (--tokenLine < firstLine)
					break;
				tokens = formatedLines[tokenLine].tokens;
				tokenIndex = tokens.Count;
			}
		}
		
		return bracePosition;
	}

	public TextPosition GetClosingBraceRightOf(int tokenLine, int tokenIndex, int maxLinesDistance)
	{
		var lastLine = maxLinesDistance >= 0 ? Mathf.Min(formatedLines.Length - 1, tokenLine + maxLinesDistance) : formatedLines.Length - 1;
		var tokens = formatedLines[tokenLine].tokens;
		if (tokens == null)
			return new TextPosition();

		var skipOver = 0;
		var numTokens = tokens.Count;
		while (tokenLine <= lastLine)
		{
			while (++tokenIndex >= numTokens)
			{
				if (++tokenLine > lastLine)
					break;
				tokens = formatedLines[tokenLine].tokens;
				if (tokens == null)
					return new TextPosition();
				tokenIndex = -1;
				numTokens = tokens.Count;
			}
			if (tokenIndex >= numTokens)
				break;
			var tokenRight = tokens[tokenIndex];
			var text = tokenRight.text;
			if (tokenRight.tokenKind == SyntaxToken.Kind.Punctuator && text.Length == 1)
			{
				var c = text[0];
				if (')' == c || ']' == c || '}' == c)
				{
					if (skipOver > 0)
						--skipOver;
					else
						return new TextPosition(tokenLine, tokenIndex);
				}
				else if ('(' == c || '[' == c || '{' == c)
				{
					++skipOver;
				}
			}
		}

		return new TextPosition();
	}
	
	public string GetAutoIndentAfter(int line)
	{
		//List<SyntaxToken> tokens;
		//SyntaxToken firstNonWSToken = null;
		//int tokenIndex = 0;
		//while (line >= 0)
		//{
		//	firstNonWSToken = FirstNonWhitespaceToken(line);
		//	if (firstNonWSToken == null || firstNonWSToken.tokenKind == SyntaxToken.Kind.Preprocessor)
		//	{
		//		--line;
		//	}
		//	else
		//	{
		//		tokens = formatedLines[line].tokens;
		//		tokenIndex = firstNonWSToken.TokenIndex;
		//		while (firstNonWSToken.tokenKind == SyntaxToken.Kind.VerbatimStringLiteral ||
		//			firstNonWSToken.tokenKind == SyntaxToken.Kind.Comment ||
		//			firstNonWSToken.tokenKind == SyntaxToken.Kind.Missing ||
		//			firstNonWSToken.tokenKind == SyntaxToken.Kind.Whitespace)
		//		{
		//			++tokenIndex;
		//			if (tokenIndex == tokens.Count)
		//			{
		//				--line;
		//				continue;
		//			}
		//			firstNonWSToken = tokens[tokenIndex];
		//		}
		//		break;
		//	}
		//}
		if (line < 0)
			return "";
		
		var lineToken = line;
		var indexToken = formatedLines[line].tokens.Count;
		var token = GetNonTriviaTokenLeftOf(ref lineToken, ref indexToken);
		if (token == null)
			return "";
		
		var tokens = formatedLines[lineToken].tokens;
		
		//var lineTokenLeft = lineToken;
		//var indexTokenLeft = indexToken;
		//var tokenLeft = GetNonTriviaTokenLeftOf(ref lineTokenLeft, ref indexTokenLeft);
		
		return tokens[0].tokenKind == SyntaxToken.Kind.Whitespace ? tokens[0].text : "";
	}
	
	public string CalcAutoIndent(int line)
	{
		SyntaxToken firstToken, firstNonTrivia;
		GetFirstTokens(line, out firstToken, out firstNonTrivia);
		
		if (firstNonTrivia != null && firstNonTrivia.parent.syntaxError != null)
			return null;
		
		var leaf = firstNonTrivia != null ? firstNonTrivia.parent : null;
		if (leaf == null && firstToken != null)
			return null;
		
		string indent = null;
		var delta = 0;
		ParseTree.Leaf reference = null;
		var parent = leaf != null ? leaf.parent : null;
		
		var childIndex = leaf != null ? leaf.childIndex : 0;
		if (leaf == null)
		{
			var previousLeaf = GetNonTriviaTokenLeftOf(line, 0);
			if (previousLeaf == null || previousLeaf.parent == null)
				return null;
			
			ParseTree.BaseNode previousNode = previousLeaf.parent;
			if (previousNode == null)
				return null;

			var scanner = parser.MoveAfterLeaf(previousLeaf.parent);
			if (scanner == null)
				return null;
			var grammarNode = scanner.CurrentGrammarNode;
			grammarNode.parent.NextAfterChild(grammarNode, scanner);
			
			parent = scanner.CurrentParseTreeNode;
			while (previousNode.parent != null && previousNode.parent != parent)
				previousNode = previousNode.parent;
			if (previousNode.parent != parent)
				return null;
		//	Debug.Log(previousLeaf.parent.ToString() + previousNode.childIndex);
			childIndex = previousNode.childIndex + 1;
		}
		
		if (leaf != null && (leaf.IsLit("{") || leaf.IsLit("[") || leaf.IsLit("(")))
		{
			do {
				while (parent != null && parent.childIndex > 0)
				{
					var checkBracket = true;
					var checkParen = true;
					for (var i = parent.childIndex; i --> 0; )
					{
						var leafLeft = parent.parent.LeafAt(i);
						if (leafLeft == null)
							continue;
						if (leafLeft.IsLit("{") ||
							checkBracket && leafLeft.IsLit("[") ||
							checkParen && leafLeft.IsLit("("))
						{
							reference = leafLeft;
							delta = 1;
						//	Debug.Log(parent.RuleName + " " + leafLeft.token.text + " found");
							break;
						}
						else if (leafLeft.IsLit(","))
						{
							parent = leafLeft.FindPreviousNode() as ParseTree.Node;
							reference = parent != null ? parent.GetFirstLeaf() : leafLeft;
							parent = null;
							delta = 0;
							break;
						}
						else if (leafLeft.IsLit(")"))
							checkParen = false;
						else if (leafLeft.IsLit("]"))
							checkBracket = false;
					}
					if (reference != null)
						break;
					parent = parent.parent;
				}
				if (parent != null)
					parent = parent.parent;
			} while (parent != null && parent.GetFirstLeaf() == leaf);
			if (parent != null)
			{
				reference = parent.GetFirstLeaf();
				if (reference.IsLit("{"))
					delta = 1;
			}
		}
		else if (leaf != null && (leaf.IsLit("}") || leaf.IsLit("]") || leaf.IsLit(")")))
		{
			var openParen = leaf.token.text;
			openParen = openParen == "}" ? "{" : openParen == "]" ? "[" : "(";
			for (var i = leaf.childIndex; i --> 0; )
			{
				reference = parent.LeafAt(i);
				if (reference != null)
				{
					if (reference.IsLit(openParen))
						break;
					else
						reference = null;
				}
			}
		}
		else if (parent != null)
		{
			while (parent != null)
			{
				var thisIndex = childIndex;
				childIndex = parent.childIndex;
				var rule = parent.RuleName;
				if (rule == null)
					break;
				
				if (rule == "embeddedStatement")
				{
					if (parent.parent.RuleName != "statement")
					{
						reference = parent.parent.GetFirstLeaf();
						delta = 1;
						break;
					}
				}
				else if (rule == "statement")
				{
					reference = parent.GetFirstLeaf();
					if (reference != leaf)
					{
						delta = 1;
						break;
					}
					else
					{
						reference = null;
					}
				}
				else if (rule == "elseStatement")
				{
					parent = parent.parent;
					reference = parent.GetFirstLeaf();
					break;
				}
				else if (rule == "switchLabel")
				{
					parent = parent.parent;
					if (childIndex == 0)
					{
						if (parent.childIndex >= 2)
							parent = parent.parent.NodeAt(1);
						else
							parent = parent.parent;
					}
					reference = parent.GetFirstLeaf();
					break;
				}
				else if (rule == "switchSection")// && childIndex > 1)
				{
					if (thisIndex > 0)
					{
						reference = parent.GetFirstLeaf();
						delta = 1;
					}
					else
					{
						reference = parent.parent.GetFirstLeaf();
					}
					break;
				}
				else if (rule == "labeledStatement" && childIndex < 2)
				{
					parent = parent.parent.parent.parent;
					reference = parent.GetFirstLeaf();
				//	Debug.Log(reference.ToString() + parent.RuleName + " " + childIndex);
					break;
				}
				else if (rule == "fieldDeclaration" || rule == "eventDeclaration")
				{
					parent = parent.parent;
					reference = parent.GetFirstLeaf();
					delta = 1;
					break;
				}
				else if (rule == "constantDeclaration")
				{
					parent = parent.parent.parent;
					reference = parent.GetFirstLeaf();
					delta = 1;
					break;
				}
				else if (rule == "formalParameterList")
				{
					parent = parent.parent;
					reference = parent.GetFirstLeaf();
					delta = 1;
					break;
				}
				
				for (var i = childIndex; i --> 0; )
				{
					var leafLeft = parent.LeafAt(i);
					if (leafLeft != null && leafLeft.IsLit("{"))
					{
						reference = leafLeft;
						delta = 1;
					//	Debug.Log(parent.RuleName + " { found");
						break;
					}
				}
				if (reference != null)
					break;
				
				parent = parent.parent;
			}
		}
		
		if (reference != null)
		{
			//Debug.Log(reference.ToString() + parent.RuleName);

			indent = lines[reference.line].Substring(0, FirstNonWhitespace(reference.line));
			if (delta > 0)
			{
				indent = new string('\t', delta) + indent;
			}
			else
			{
				var skip = 0;
				while (delta < 0 && skip < indent.Length)
				{
					for (var column = 0; column < 4; ++column)
						if (indent[skip++] == '\t')
							break;
					++delta;
				}
				indent = indent.Substring(skip);
			}
		}
		
		return indent;
	}
	
	public int GetLineIndent(int line)
	{
		var firstNonWhitespace = FirstNonWhitespace(line);
		var s = lines[line].Substring(0, firstNonWhitespace);
		s = ExpandTabs(s, 0);
		return (s.Length + 3) / 4;
	}
	
	public void UpdateHighlighting(int fromLine, int toLineInclusive, bool keepLastChangeId = false)
	{
		if (progressiveParser != null)
		{
			EditorApplication.update -= ProgressiveParseOnUpdate;
			progressiveParser = null;
		}

		//Debug.Log("Updating HL from line " + (fromLine + 1) + " to " + (toLineInclusive + 1));
		parser.CutParseTree(toLineInclusive + 1, formatedLines);

		for (var i = fromLine; i <= toLineInclusive; ++i)
		{
			var line = formatedLines[i];
			if (line == null || line.tokens == null)
				continue;
			var tokens = line.tokens;
			for (var t = 0; t < tokens.Count; t++)
			{
				var token = tokens[t];
				if (token.parent != null)
					token.parent.ReparseToken();
			}
		}
		
		parser.scriptDefinesChanged = false;
		var laLine = UpdateLexer(fromLine, toLineInclusive, keepLastChangeId);
		parser.CutParseTree(laLine, formatedLines);
		if (parser.scriptDefinesChanged)
		{
			if (fromLine != 0)
			{
				parser.scriptDefinesChanged = false;
				var savedRecordUndo = recordUndo;
				recordUndo = false;
				UpdateHighlighting(0, formatedLines.Length - 1, true);
				recordUndo = savedRecordUndo;
				return;
			}
		}

		//Debug.Log("Updated HL from " + (fromLine + 1) + " (" + (laLine + 1) + ") to " + (toLineInclusive + 1));
		//if (laLine <= toLineInclusive)
		{
			for (var i = laLine; i < fromLine; ++i)
			{
				for (var t = 0; t < formatedLines[i].tokens.Count; t++)
				{
					var token = formatedLines[i].tokens[t];
					if (token.parent != null)
						token.parent.ReparseToken();
				}
			}
			//parser.CutParseTree(laLine, formatedLines);
		}
		//var timer = new System.Diagnostics.Stopwatch();
		//timer.Start();
		
		var updater = parser.Update(laLine, toLineInclusive);
		if (updater != null)
		{
			progressiveParser = updater;
			EditorApplication.update += ProgressiveParseOnUpdate;
		}
		
		//timer.Stop();
		//Debug.Log("Parsing - " + timer.Elapsed.TotalMilliseconds.ToString());
		
		UpdateViews();
	}

	private int UpdateLexer(int fromLine, int toLineInclusive, bool keepLastChangeId)
	{
		var laLine = fromLine;
		var line = fromLine;
		while (line <= toLineInclusive)
		{
			//laLine = System.Math.Min(laLine, line - formatedLines[line].laLines);
			laLine = Mathf.Clamp(line - formatedLines[line].laLines, 0, laLine);
			FormatLine(line);
			if (!keepLastChangeId)
				formatedLines[line].lastChange = currentChangeId;
			if (recordUndo)
				updatedLines.Add(formatedLines[line]);
			++line;
		}
		
		if (fromLine != 0 && parser.scriptDefinesChanged)
			return -1;

		while (line < formatedLines.Length)
		{
			var formatedLine = formatedLines[line];
			laLine = Mathf.Clamp(line - formatedLine.laLines, 0, laLine);
			var prevState = formatedLine.blockState;
			var prevRegion = formatedLine.regionTree;
		
			FormatLine(line);
			
			if (fromLine != 0 && parser.scriptDefinesChanged)
				return -1;
			
			formatedLine = formatedLines[line++];
			if (!parser.scriptDefinesChanged && prevState == formatedLine.blockState && prevRegion == formatedLine.regionTree)
				break;
		}

		// TODO: Optimize this!!!
		
		//for (var i = line; i < formatedLines.Length; ++i)
		//{
		//	var formatedLine = formatedLines[i];
		//	if (formatedLine == null)
		//		continue;
				
		//	for (var j = 0; j < formatedLine.tokens.Count; ++j)
		//	{
		//		var token = formatedLine.tokens[j];
		//		if (token.parent != null)
		//		{
		//			//if (token.parent.line == i)
		//			//{
		//			//	i = formatedLines.Length;
		//			//	break;
		//			//}
					
		//			//token.parent.line = i;
		//			if (token.parent.tokenIndex != j)
		//			{
		//				//Debug.Log("Index of token " + token + " on line " + i
		//				//	+ " was " + token.parent.tokenIndex + " instead of " + j);
		//				token.parent.tokenIndex = j;
		//			}
		//		}
		//	}
		//}

	//	if (laLine < fromLine)
	//		Debug.LogWarning("laLine: " + laLine + ", fromLine: " + fromLine);
		return laLine;
	}

	private void ReformatLines(int fromLine, int toLineInclusive)
	{
		var line = fromLine;
		while (line <= toLineInclusive)
		{
			FormatLine(line);
			++line;
		}
	}

	public delegate void LineFormattedDelegate(int line);
	public LineFormattedDelegate onLineFormatted;

	[System.NonSerialized]
	private FGParser parser;
	public FGParser Parser { get { return parser; } }

	private void FormatLine(int currentLine)
	{
		FormatedLine formatedLine = formatedLines[currentLine];
		if (formatedLine == null)
		{
			formatedLine = formatedLines[currentLine] = new FormatedLine { index = currentLine };
			formatedLine.lastChange = currentChangeId;
		}
		else if (formatedLine.tokens != null)
		{
			foreach (var token in formatedLine.tokens)
			{
				if (token.parent != null)
				{
					token.parent.token = null;
					token.parent = null;
				}
			}
		}

		if (currentLine > 0)
		{
			var prevLine = formatedLines[currentLine - 1];
			formatedLine.blockState = prevLine.blockState;
			formatedLine.regionTree = prevLine.regionTree;
		}
		else
		{
			formatedLine.blockState = 0;
			formatedLine.regionTree = rootRegion;
		}

		parser.LexLine(currentLine, formatedLine);
		if (onLineFormatted != null)
			onLineFormatted(currentLine);
	}

	public TextSpan GetTokenSpan(ParseTree.Leaf parseTreeLeaf)
	{
		//var tokens = formatedLines[parseTreeLeaf.line].tokens;
		//var tokenIndex = parseTreeLeaf.tokenIndex;
		//for (var j = 0; j <= tokenIndex; ++j)
		//    if (tokens[j].tokenKind < SyntaxToken.Kind.LastWSToken)
		//        ++tokenIndex;
		//Debug.Log(parseTreeLeaf.tokenIndex + " -> " + tokenIndex);
		return GetTokenSpan(parseTreeLeaf.line, parseTreeLeaf.tokenIndex);
	}

	public TextSpan GetTokenSpan(int lineIndex, int tokenIndex)
	{
		var tokens = formatedLines[lineIndex].tokens;

		var tokenStart = 0;
		for (var i = 0; i < tokenIndex; ++i)
			if (i < tokens.Count)
				tokenStart += tokens[i].text.Length;
			else
				Debug.LogWarning("Token at line " + (lineIndex + 1) + ", index " + i + " is out of range!");
		
		var tokenLength = tokens[tokenIndex].text.Length;
		return TextSpan.Create(new TextPosition { line = lineIndex, index = tokenStart }, new TextOffset { indexOffset = tokenLength });
	}

	public TextSpan GetParseTreeNodeSpan(ParseTree.BaseNode parseTreeNode)
	{
		var leaf = parseTreeNode as ParseTree.Leaf;
		if (leaf != null)
			return GetTokenSpan(leaf);

		var node = (ParseTree.Node) parseTreeNode;
		ParseTree.Leaf from = node.GetFirstLeaf();
		ParseTree.Leaf to = node.GetLastLeaf();

		return TextSpan.Create(GetTokenSpan(from).StartPosition, GetTokenSpan(to).EndPosition);
	}

	public SyntaxToken GetTokenLeftOf(ref int lineIndex, ref int tokenIndex)
	{
		while (lineIndex >= 0)
		{
			var tokens = formatedLines[lineIndex].tokens;
			if (tokenIndex == -1)
				tokenIndex = tokens.Count;
			if (--tokenIndex >= 0)
				return tokens[tokenIndex];
			--lineIndex;
		}
		return null;
	}

	public SyntaxToken GetTokenAt(CaretPos caretPosition, out int lineIndex, out int tokenIndex, out bool atTokenEnd)
	{
		return GetTokenAt(new TextPosition(caretPosition.line, caretPosition.characterIndex), out lineIndex, out tokenIndex, out atTokenEnd);
	}

	public SyntaxToken GetTokenAt(TextPosition position, out int lineIndex, out int tokenIndex, out bool atTokenEnd)
	{
		atTokenEnd = true;
		lineIndex = position.line;
		tokenIndex = 0;
		if (lineIndex >= formatedLines.Length)
			return null;

		var characterIndex = position.index;
		var tokens = formatedLines[lineIndex].tokens;
		if (tokens == null)
			return null;

		while (tokenIndex < tokens.Count && tokens[tokenIndex].IsMissing())
			++tokenIndex;

		if (tokenIndex == tokens.Count)
		{
			tokenIndex = -1;
			return null;
		}

		if (characterIndex == 0)
		{
			atTokenEnd = false;
			return tokens[0];
		}

		SyntaxToken result = null;
		while (characterIndex > 0)
		{
			if (tokens[tokenIndex].IsMissing())
			{
				if (++tokenIndex == tokens.Count)
					break;
				continue;
			}
			result = tokens[tokenIndex];
			if (tokenIndex < tokens.Count)
				characterIndex -= result.text.Length;
			if (characterIndex > 0 && tokenIndex < tokens.Count - 1)
				++tokenIndex;
			else
				break;
		}

		atTokenEnd = characterIndex == 0;
		return result; //tokens[tokenIndex];
	}

	public SyntaxToken GetNonTriviaTokenLeftOf(ref int lineIndex, ref int tokenIndex)
	{
		while (lineIndex > 0)
		{
			var tokens = formatedLines[lineIndex].tokens;
			
			--tokenIndex;
			while (tokenIndex >= 0 && tokens[tokenIndex].tokenKind <= SyntaxToken.Kind.LastWSToken)
				--tokenIndex;
			
			if (tokenIndex >= 0)
				return tokens[tokenIndex];
			
			--lineIndex;
			if (lineIndex >= 0)
				tokenIndex = formatedLines[lineIndex].tokens.Count;
		}
		return null;
	}
	
	public SyntaxToken GetNonTriviaTokenLeftOf(CaretPos position, out int lineIndex, out int tokenIndex)
	{
		lineIndex = position.line;
		tokenIndex = -1;

		var characterIndex = position.characterIndex;
		var tokens = formatedLines[lineIndex].tokens;
		if (tokens == null)
			return null;
		
		if (tokens.Count > 0)
		{
			while (characterIndex > 0)
			{
				if (++tokenIndex == tokens.Count - 1)
					break;
				characterIndex -= tokens[tokenIndex].text.Length;
			}
		}

		while (tokenIndex < 0 || tokens[tokenIndex].tokenKind <= SyntaxToken.Kind.LastWSToken)
		{
			if (tokenIndex >= 0)
			{
				--tokenIndex;
			}
			else if (lineIndex > 0)
			{
				tokens = formatedLines[--lineIndex].tokens;
				tokenIndex = tokens.Count - 1;
			}
			else
			{
				break;
			}
		}

		return tokenIndex >= 0 ? tokens[tokenIndex] : null;
	}

	public SyntaxToken GetNonTriviaTokenLeftOf(int lineIndex, int characterIndex)
	{
		var tokenIndex = -1;

		var tokens = formatedLines[lineIndex].tokens;
		if (tokens == null)
			return null;
		if (tokens.Count > 0)
		{
			while (characterIndex > 0)
			{
				if (++tokenIndex == tokens.Count - 1)
					break;
				characterIndex -= tokens[tokenIndex].text.Length;
			}
		}

		while (tokenIndex < 0 || tokens[tokenIndex].tokenKind <= SyntaxToken.Kind.LastWSToken)
		{
			if (tokenIndex >= 0)
			{
				--tokenIndex;
			}
			else if (lineIndex > 0)
			{
				tokens = formatedLines[--lineIndex].tokens;
				tokenIndex = tokens.Count - 1;
			}
			else
			{
				break;
			}
		}

		return tokenIndex >= 0 ? tokens[tokenIndex] : null;
	}
}
