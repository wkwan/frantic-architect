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

namespace ScriptInspector
{
	
public class TabSwitcher : FGPopupWindow
{
	public static TabSwitcher instance;
	
	private const float itemHeight = 16f;
		
	private static GUIContent[] items;
	private static List<string> guids;
	private static int selected = 0;
	
	private static GUIStyle itemStyle;
	private static GUIStyle captionStyle;
	private static new EditorWindow owner;
	private static float lastWidth = 200f;
	
	private float itemsWidth = 0f;
	private float captionWidth = 0f;
	private float captionHeight = 0f;
	
	public static TabSwitcher Create(bool selectNext)
	{
		guids = FGCodeWindow.GetGuidHistory();
		if (guids.Count == 0)
			return null;
		
		items = new GUIContent[guids.Count];
		for (var i = 0; i < items.Length; ++i)
		{
			var path = AssetDatabase.GUIDToAssetPath(guids[i]);
			var textBuffer = FGTextBufferManager.TryGetBuffer(path);
			var name = (textBuffer != null && textBuffer.IsModified ? "*" : "") + System.IO.Path.GetFileName(path);
			items[i] = new GUIContent(name, AssetDatabase.GetCachedIcon(path), path);
		}
		selected = selectNext ? (items.Length > 1 ? 1 : 0) : items.Length - 1;
		if (!(EditorWindow.focusedWindow is FGCodeWindow))
			selected = 0;
		
		var numRows = Mathf.Min(items.Length, 10);
		var height = itemHeight * numRows;
		var width = lastWidth;
		
		owner = EditorWindow.focusedWindow;
		var center = owner.position.center;
		Rect position = new Rect((int)(center.x - 0.5f * width), (int)(center.y - height * 0.5f), width, height);
		
		var window = CreatePopup<TabSwitcher>();
		window.hideFlags = HideFlags.HideAndDontSave;
		
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
		window.title = "";
#else
		window.titleContent.text = "";
#endif
		window.minSize = Vector2.one;
		
		window.position = position;
		window.wantsMouseMove = false;
		window.ShowPopup();
		//window.Repaint();
		//window.Focus();
		
		//if (window.owner != null)
		//	window.owner.Focus();
		
		EditorApplication.modifierKeysChanged += window.OnModifierKeysChanged;
		return window;
	}
	
	private void OnModifierKeysChanged()
	{
		if (!this)
			return;
		
		Repaint();
	}
	
	private void OnDisable()
	{
		EditorApplication.modifierKeysChanged -= OnModifierKeysChanged;
	}
	
	private void OnDestroy()
	{
		EditorApplication.modifierKeysChanged -= OnModifierKeysChanged;
	}
	
	private new void SetSize(float width, float height)
	{
		lastWidth = width;
		
		var center = position.center;
		var x = (int)(center.x - 0.5f * width);
		var y = (int)(center.y - 0.5f * height);
		var rc = new Rect(x, y, width, height);
		var fit = FitRectToScreen(rc, this);
		
		minSize = Vector2.one;
		maxSize = new Vector2(4000f, 4000f);
		position = fit;
		maxSize = minSize = new Vector2(width, height);
		
		if (Application.platform == RuntimePlatform.OSXEditor)
			EditorApplication.delayCall += Focus;
	}
	
	public static string GetSelectedGUID()
	{
		return guids[selected];
	}
	
	public void CloseAndSwitch()
	{
		if (!instance)
			return;
		
		var guid = GetSelectedGUID();
		Close();
		DestroyImmediate(this);
		FGCodeWindow.OpenAssetInTab(guid);
		
		instance = null;
	}
	
	public static bool OnGUIGlobal()
	{
		if (instance)
		{
			instance.OnGUI();
			return true;
		}
		else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
		{
			var isOSX = Application.platform == RuntimePlatform.OSXEditor;
			if (isOSX)
			{
				if (Event.current.alt && !EditorGUI.actionKey)
					instance = Create(!Event.current.shift);
				return true;
			}
			if (!Event.current.alt && EditorGUI.actionKey)
			{
				EditorApplication.update -= DelayedCreateWithShiftKey;
				EditorApplication.update -= DelayedCreateNoShiftKey;
				
				var isShift = Event.current.shift;
				if (isShift)
					EditorApplication.update += DelayedCreateWithShiftKey;
				else
					EditorApplication.update += DelayedCreateNoShiftKey;
				return true;
			}
		}
		return false;
	}
	
	private static void DelayedCreateWithShiftKey()
	{
		EditorApplication.update -= DelayedCreateWithShiftKey;
		instance = Create(false);
	}
	
	private static void DelayedCreateNoShiftKey()
	{
		EditorApplication.update -= DelayedCreateNoShiftKey;
		instance = Create(true);
	}
	
	public void OnGUI()
	{
		var isOSX = Application.platform == RuntimePlatform.OSXEditor;
		if (isOSX)
		{
			if ((Event.current.modifiers & EventModifiers.Alt) == 0)
			{
				EditorApplication.delayCall -= CloseAndSwitch;
				EditorApplication.delayCall += CloseAndSwitch;
				return;
			}
		}
		else
		{
			if ((Event.current.modifiers & EventModifiers.Control) == 0)
			{
				EditorApplication.delayCall -= CloseAndSwitch;
				EditorApplication.delayCall += CloseAndSwitch;
				return;
			}
		}
		
		if (itemStyle == null)
		{
			itemStyle = new GUIStyle("PR Label");
			itemStyle.padding.left = 2;
			itemStyle.padding.right = 4;
			itemStyle.border = new RectOffset();
			itemStyle.margin = new RectOffset();
			itemStyle.fixedWidth = 0;
				// itemStyle.stretchWidth = false;
				// itemStyle.fixedHeight = itemHeight;
			
			captionStyle = new GUIStyle(EditorStyles.largeLabel);
			captionStyle.fontStyle = FontStyle.Bold;
			captionStyle.fontSize = 18;
			captionStyle.padding = new RectOffset(6, 10, 10, 10);
			captionStyle.normal.textColor = EditorGUIUtility.isProSkin ?
				new Color(0.7f, 0.7f, 0.7f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);
		}
		
		if (items == null)
		{
			EditorApplication.delayCall += Close;
			return;
		}
		
		if (Event.current.type == EventType.KeyDown)
		{
			var newSelected = selected;
			var isTabKey = Event.current.keyCode == KeyCode.Tab;
			if (Event.current.keyCode == KeyCode.DownArrow || isTabKey && !Event.current.shift)
			{
				if (++newSelected == items.Length)
					newSelected = 0;
			}
			else if (Event.current.keyCode == KeyCode.UpArrow || isTabKey && Event.current.shift)
			{
				if (--newSelected < 0)
					newSelected = items.Length - 1;
			}
			else if (items.Length > 10 &&
				(Event.current.keyCode == KeyCode.RightArrow || Event.current.keyCode == KeyCode.LeftArrow))
			{
				newSelected += Event.current.keyCode == KeyCode.RightArrow ? 10 : -10;
				if (newSelected < 0)
					newSelected += ((items.Length + 9) / 10) * 10;
				if (newSelected >= items.Length)
				{
					if (newSelected / 10 == items.Length / 10)
						newSelected = items.Length - 1;
					else
						newSelected = newSelected % 10;
				}
			}
			if (newSelected != selected)
			{
				selected = newSelected;
				Repaint();
				Event.current.Use();
			}
		}
		else if (Event.current.type == EventType.Layout)
		{
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			
			if (itemsWidth == 0f)
			{
				EditorGUIUtility.SetIconSize(new Vector2(32f, 32f));
				for (int i = 0; i < items.Length; ++i)
				{
					var size = itemStyle.CalcSize(items[i]);
					itemsWidth = Mathf.Max(itemsWidth, size.x);
					size = captionStyle.CalcSize(items[i]);
					captionWidth = Mathf.Max(captionWidth, size.x);
				}
				captionHeight = 54f;

				var width = Mathf.Max(captionWidth, 8f + itemsWidth * ((items.Length + 9) / 10));
				var height = 4f + itemHeight * (Mathf.Min(items.Length, 10)) + captionHeight;
				SetSize(width, height);
				Repaint();
			}
			
			EditorGUIUtility.SetIconSize(Vector2.zero);
		}
		
		EditorGUIUtility.SetIconSize(new Vector2(32f, 32f));
		if (captionStyle != null)
			GUI.Label(new Rect(0f, 0f, captionWidth, captionHeight), items[selected], captionStyle);
		
		if (Event.current.type == EventType.Repaint)
		{
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			for (int i = 0; i < items.Length; ++i)
			{
				var pos = new Rect(4f + itemsWidth * (i / 10), captionHeight + itemHeight * (i % 10), itemsWidth, itemHeight);
				itemStyle.Draw(pos, items[i], false, false, i == selected, true);
			}
		}
		
		EditorGUIUtility.SetIconSize(Vector2.zero);
	}
}
	
}
