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

using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;


namespace ScriptInspector
{
	
[Serializable, StructLayout(LayoutKind.Sequential)]
public class FGTextEditor
{
	internal static readonly string[] availableThemes =
	{
		"Visual Studio",
		"Xcode",
		"Tango Dark (Oblivion)",
		"Tango Light",
		"MD Brown",
		"MD Brown - Dark",
		"Monokai",
		"Son of Obsidian",
		"Darcula",
		"Visual Studio Dark",
		"VS Light with VA X",
		"VS Dark with VA X",
		"Solarized Dark",
		"Solarized Light",
		"VS Dark with ReSharper"
	};

	public class Theme
	{
		public Color background = Color.gray;
		public Color text = Color.red;
		public Color hyperlinks = Color.red;
		
		public Color keywords = Color.red;
		public Color constants = Color.red;
		public Color strings = Color.red;
		public Color builtInLiterals = Color.red;
		public Color operators = Color.red;
		
		public Color referenceTypes = Color.red;
		public Color valueTypes = Color.red;
		public Color interfaceTypes = Color.red;
		public Color enumTypes = Color.red;
		public Color delegateTypes = Color.red;
		public Color builtInTypes = Color.red;
		
		public Color namespaces = Color.red;
		public Color methods = Color.red;
		public Color fields = Color.red;
		public Color properties = Color.red;
		public Color events = Color.red;
		
		public Color parameters = Color.red;
		public Color variables = Color.red;
		public Color typeParameters = Color.red;
		public Color enumMembers = Color.red;
		
		public Color preprocessor = Color.red;
		public Color defineSymbols = Color.red;
		public Color inactiveCode = Color.gray;
		public Color comments = Color.red;
		public Color xmlDocs = Color.red;
		public Color xmlDocsTags = Color.red;
		
		public Color lineNumbers = Color.red;
		public Color lineNumbersHighlight = Color.red;
		public Color lineNumbersBackground = Color.gray;
		public Color fold = Color.red;
		
		public Color activeSelection = new Color32(51, 153, 255, 102);
		public Color passiveSelection = new Color32(191, 205, 219, 102);
		public Color searchResults = Color.yellow;
		
		public Color trackSaved = new Color32(108, 226, 108, 255);
		public Color trackChanged = new Color32(255, 238, 98, 255);
		public Color trackReverted = new Color32(246, 201, 60, 255);

		public Color currentLine = Color.green;
		public Color currentLineInactive = Color.magenta;
		
		public Color referenceHighlight = new Color32(0xe0, 0xff, 0xff, 0xff);
		public Color referenceModifyHighlight = new Color32(0xff, 0xdd, 0xdd, 0xff);
		
		public Color tooltipBackground = new Color32(253, 255, 153, 255);
		public Color tooltipFrame = new Color32(128, 128, 128, 255);
		public Color tooltipText = new Color32(22, 22, 22, 255);
		
		public Color listPopupBackground = Color.gray;
			
		public FontStyle commentsStyle = FontStyle.Normal;
		public FontStyle stringsStyle = FontStyle.Normal;
		public FontStyle keywordsStyle = FontStyle.Normal;
		public FontStyle constantsStyle = FontStyle.Normal;
		public FontStyle typesStyle = FontStyle.Normal;
		public FontStyle namespacesStyle = FontStyle.Normal;
		public FontStyle methodsStyle = FontStyle.Normal;
		public FontStyle fieldsStyle = FontStyle.Normal;
		public FontStyle propertiesStyle = FontStyle.Normal;
		public FontStyle eventsStyle = FontStyle.Normal;
		public FontStyle hyperlinksStyle = FontStyle.Normal;
		public FontStyle preprocessorStyle = FontStyle.Normal;
		public FontStyle defineSymbolsStyle = FontStyle.Normal;
		public FontStyle inactiveCodeStyle = FontStyle.Normal;
		public FontStyle parametersStyle = FontStyle.Normal;
		public FontStyle variablesStyle = FontStyle.Normal;
		public FontStyle typeParametersStyle = FontStyle.Normal;
		public FontStyle enumMembersStyle = FontStyle.Normal;
	}
	
	private static Theme[] themes = {
		// Visual Studio
		new Theme {
			background				= Color.white,
			text					= Color.black,
			hyperlinks				= Color.blue,
			
			keywords				= Color.blue,
			constants               = Color.black,
			strings					= new Color32(0x80, 0x00, 0x00, 0xff),
			builtInLiterals         = Color.blue,
			operators               = Color.black,
		
			referenceTypes          = new Color32(0x2b, 0x91, 0xaf, 0xff),
			valueTypes              = new Color32(0x2b, 0x91, 0xaf, 0xff),
			interfaceTypes          = new Color32(0x2b, 0x91, 0xaf, 0xff),
			enumTypes               = new Color32(0x2b, 0x91, 0xaf, 0xff),
			delegateTypes           = new Color32(0x2b, 0x91, 0xaf, 0xff),
			builtInTypes            = Color.blue,
		
			namespaces              = Color.black,
			methods                 = Color.black,
			fields                  = Color.black,
			properties              = Color.black,
			events                  = Color.black,
		
			parameters              = Color.gray,
			variables               = Color.black,
			typeParameters          = new Color32(0x2b, 0x91, 0xaf, 0xff),
			enumMembers             = new Color32(111, 0, 138, 255),
		
			preprocessor            = Color.blue,
			defineSymbols           = new Color32(111, 0, 138, 255),
			inactiveCode            = Color.gray,
			comments				= new Color32(0x00, 0x80, 0x00, 0xff),
			xmlDocs                 = new Color32(0x00, 0x80, 0x00, 0xff),
			xmlDocsTags             = new Color32(0x80, 0x80, 0x80, 0xff),
		
			lineNumbers				= new Color32(0x2b, 0x91, 0xaf, 0xff),
			lineNumbersHighlight	= Color.blue,
			lineNumbersBackground	= Color.white,
			fold					= new Color32(165, 165, 165, 255),
		
			activeSelection			= new Color32(51, 153, 255, 102),
			passiveSelection		= new Color32(191, 205, 219, 102),
			searchResults			= new Color32(244, 167, 33, 255),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = new Color32(213, 213, 241, 255),
			currentLineInactive     = new Color32(228, 228, 228, 255),
		
			referenceHighlight      = new Color32(0xe0, 0xff, 0xff, 0xff),
			referenceModifyHighlight = new Color32(0xff, 0xdd, 0xdd, 0xff),
		
			tooltipBackground       = new Color32(253, 255, 153, 255),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = new Color32(22, 22, 22, 255),
			
			listPopupBackground				= Color.white,
		},
		
		// Xcode (updated to Xcode 5 by inventor2010)
		new Theme {
			background				= Xcode5Theme.Background,
			text					= Xcode5Theme.PlainText,
			hyperlinks				= Xcode5Theme.URLs,
		
			keywords				= Xcode5Theme.Keywords,
			constants               = Xcode5Theme.Numbers,
			strings					= Xcode5Theme.Strings,
			builtInLiterals         = Xcode5Theme.Keywords,
			operators               = Color.black,
		
			referenceTypes			= Xcode5Theme.ProjectTypeNames,
			valueTypes				= Xcode5Theme.OtherTypeNames,
			interfaceTypes			= Xcode5Theme.OtherTypeNames,
			enumTypes				= Xcode5Theme.OtherTypeNames,
			delegateTypes			= Xcode5Theme.OtherTypeNames,
			builtInTypes			= Xcode5Theme.Keywords,
		
			namespaces              = Xcode5Theme.Keywords,
			methods                 = Xcode5Theme.ProjectFunctionandMethodNames,
			fields                  = Color.black,
			properties              = Xcode5Theme.OtherInstanceVariablesandGlobals,
			events                  = Xcode5Theme.ProjectFunctionandMethodNames,
		
			parameters              = Color.black,
			variables               = Color.black,
			typeParameters          = new Color32(0x80, 0x46, 0xb0, 0xff),
			enumMembers             = Xcode5Theme.OtherConstants,
		
			preprocessor            = Xcode5Theme.ProjectPreprocessorMacros,
			defineSymbols           = Xcode5Theme.ProjectPreprocessorMacros,
			inactiveCode			= TangoColors.aluminium4,
			comments				= Xcode5Theme.Comments,
			xmlDocs					= new Color32(0x23, 0x97, 0x2d, 0xff),
			xmlDocsTags				= new Color32(0x23, 0x97, 0x2d, 0xff),
		
			lineNumbers				= Xcode5Theme.lineNumbers,
			lineNumbersHighlight	= Xcode5Theme.lineNumbers,
			lineNumbersBackground	= Xcode5Theme.lineNumberBackground,
			fold					= Xcode5Theme.lineNumberBoarder,
		
			activeSelection			= new Color32(164, 205, 255, 0xff),
			passiveSelection		= new Color32(212, 212, 212, 0x7f),
			searchResults			= new Color32(250, 241, 190, 255),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = new Color32(213, 213, 241, 255),
			currentLineInactive     = new Color32(228, 228, 228, 255),
		
			referenceHighlight      = new Color32(0xe0, 0xff, 0xff, 0xff),
			referenceModifyHighlight = new Color32(0xff, 0xdd, 0xdd, 0xff),
		
			tooltipBackground       = new Color32(255, 254, 205, 255),
			tooltipFrame            = new Color32(210, 210, 210, 255),
			tooltipText             = new Color32(20, 15, 0, 255),
		
			listPopupBackground		= Xcode5Theme.Background,
		},
		
		// Tango Dark (Oblivion)
		new Theme {
			background				= TangoColors.aluminium6,
			text					= TangoColors.aluminium2,
			hyperlinks				= TangoColors.butter2,
		
			keywords				= TangoColors.plum1,
			constants               = TangoColors.butter2,
			strings					= TangoColors.butter2,
			builtInLiterals			= TangoColors.orange3,
			operators				= TangoColors.aluminium2,
		
			referenceTypes			= TangoColors.chameleon1,
			valueTypes				= TangoColors.chameleon1,
			interfaceTypes			= TangoColors.chameleon1,
			enumTypes				= TangoColors.chameleon1,
			delegateTypes			= TangoColors.chameleon1,
			builtInTypes			= TangoColors.plum1,
		
			namespaces				= TangoColors.aluminium2,
			methods					= TangoColors.aluminium2,
			fields					= TangoColors.aluminium2,
			properties				= TangoColors.aluminium2,
			events					= TangoColors.aluminium2,
		
			parameters              = TangoColors.aluminium2,
			variables               = TangoColors.aluminium2,
			typeParameters          = TangoColors.chameleon1,
			enumMembers             = TangoColors.aluminium2,
		
			preprocessor            = TangoColors.skyblue1,
			defineSymbols           = TangoColors.skyblue1,
			inactiveCode			= TangoColors.aluminium4,
			comments				= TangoColors.aluminium4,
			xmlDocs					= TangoColors.aluminium4,
			xmlDocsTags				= TangoColors.aluminium4,
		
			lineNumbers				= TangoColors.aluminium5,
			lineNumbersHighlight	= TangoColors.aluminium3,
			lineNumbersBackground	= TangoColors.aluminium7,
			fold					= TangoColors.aluminium3,
		
			activeSelection			= TangoColors.aluminium5,
			passiveSelection		= TangoColors.aluminium5,
			searchResults			= new Color32(0x00, 0x60, 0x60, 0xff),
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = TangoColors.aluminium7,
			currentLineInactive     = new Color32(0x11, 0x11, 0x11, 0x80),
		
			referenceHighlight      = new Color32(48, 65, 87, 255),
			referenceModifyHighlight = new Color32(105, 48, 49, 192),
			
			tooltipBackground       = (Color)TangoColors.aluminium7 * 0.5f + (Color)TangoColors.aluminium6 * 0.5f,
			tooltipFrame            = TangoColors.aluminium4,
			tooltipText             = TangoColors.aluminium2,
		
			listPopupBackground		= (Color)TangoColors.aluminium7 * 0.5f + (Color)TangoColors.aluminium6 * 0.5f,
		
			preprocessorStyle       = FontStyle.Italic,
		},
		
		// Tango Light
		new Theme {
			background				= Color.white,
			text					= TangoColors.aluminium7,
			hyperlinks				= Color.blue,
		
			keywords				= TangoColors.skyblue3,
			constants               = TangoColors.skyblue3,
			strings					= TangoColors.plum2,
			builtInLiterals			= TangoColors.plum1,
			operators				= TangoColors.aluminium7,
		
			referenceTypes			= TangoColors.skyblue2,
			valueTypes				= TangoColors.chameleon3,
			interfaceTypes			= TangoColors.chameleon3,
			enumTypes				= TangoColors.chameleon3,
			delegateTypes			= TangoColors.skyblue2,
			builtInTypes			= Color.clear,
		
			namespaces				= TangoColors.aluminium7,
			methods					= TangoColors.plum3,
			fields					= TangoColors.plum3,
			properties				= TangoColors.plum3,
			events					= TangoColors.plum3,
		
			parameters              = TangoColors.aluminium7,
			variables               = TangoColors.aluminium7,
			typeParameters          = TangoColors.chameleon3,
			enumMembers             = TangoColors.aluminium7,
		
			preprocessor            = TangoColors.orange3,
			defineSymbols           = TangoColors.orange2,
			inactiveCode			= TangoColors.aluminium3,
			comments				= TangoColors.chameleon3,
			xmlDocs					= TangoColors.chameleon3,
			xmlDocsTags				= TangoColors.chameleon3,
		
			lineNumbers				= TangoColors.aluminium4,
			lineNumbersHighlight	= TangoColors.aluminium5,
			lineNumbersBackground	= Color.white,
			fold					= TangoColors.aluminium3,
		
			tooltipBackground       = new Color32(253, 255, 153, 255),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = new Color32(22, 22, 22, 255),
		
			listPopupBackground		= Color.white,
		
			activeSelection			= new Color32(51, 153, 255, 102),
			passiveSelection		= new Color32(191, 205, 219, 102),
			searchResults			= new Color32(0xff, 0xe2, 0xb9, 0xff),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = TangoColors.aluminium1,
			currentLineInactive     = TangoColors.aluminium1,

			preprocessorStyle       = FontStyle.Italic
		},

		// MD Brown (courtesy of Little Angel)
		new Theme {
            background              = Color.white,
            text                    = Color.black,
			hyperlinks              = Color.blue,                           // Didn't change
		
            keywords                = new Color (0.98f, 0.23f, 0.01f),		// Red
            constants               = new Color (1.0f, 0.14f, 1.0f),
            strings                 = new Color (1.0f, 0.14f, 1.0f),		// Pink
			builtInLiterals         = new Color (0.98f, 0.23f, 0.01f),		// Red
			operators               = Color.black,
		
			referenceTypes          = new Color (0.58f, 0.04f, 0.0f),		// Dark Red
			valueTypes              = new Color (0.58f, 0.04f, 0.0f),		// Dark Red
			interfaceTypes          = new Color (0.58f, 0.04f, 0.0f),		// Dark Red
			enumTypes               = new Color (0.58f, 0.04f, 0.0f),		// Dark Red
			delegateTypes           = new Color (0.58f, 0.04f, 0.0f),		// Dark Red
			builtInTypes            = new Color (0.98f, 0.23f, 0.01f),		// Red
		
			namespaces              = Color.black,
			methods                 = Color.black,
			fields                  = Color.black,
			properties              = Color.black,
			events                  = Color.black,
		
			parameters              = Color.black,
			variables               = Color.black,
			typeParameters          = new Color (0.58f, 0.04f, 0.0f),		// Dark Red
			enumMembers             = Color.black,
		
			preprocessor            = new Color32(0x33, 0x66, 0x99, 0xff),
			defineSymbols           = new Color32(0x33, 0x66, 0x99, 0xff),
			inactiveCode            = new Color (0.20f, 0.60f, 0.0f),		// Green
            comments                = new Color (0.20f, 0.60f, 0.0f),		// Green
			xmlDocs                 = new Color (0.20f, 0.60f, 0.0f),		// Green
			xmlDocsTags             = new Color (0.20f, 0.60f, 0.0f),		// Green
		
			lineNumbers             = new Color (0.50f, 0.40f, 0.28f),		// Tan, Dark
            lineNumbersHighlight    = new Color (0.25f, 0.20f, 0.14f),		// Tan, Very Dark
            lineNumbersBackground   = new Color (1.0f, 0.80f, 0.56f),		// Tan, Light
            fold                    = new Color (0.20f, 0.60f, 0.0f),		// Green
		
			activeSelection			= new Color32(51, 153, 255, 102),
			passiveSelection		= new Color32(191, 205, 219, 102),
			searchResults           = new Color32(0xff, 0xe2, 0xb9, 0xff),  // Didn't change
		
			trackSaved              = new Color32(98, 201, 98, 255),
			trackChanged            = new Color32(255, 243, 158, 255),
			trackReverted           = new Color32(236, 175, 50, 255),
		
			currentLine             = new Color32(253, 255, 153, 255),
			currentLineInactive     = new Color32(253, 255, 153, 192),
		
			tooltipBackground       = new Color32(253, 255, 153, 255),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = new Color32(22, 22, 22, 255),
		
			listPopupBackground		= Color.white,
		},

		// MD Brown - Dark
		new Theme {
			background              = new Color (0.22f, 0.22f, 0.22f),  // Dark Grey (Pro)
			text                    = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
			hyperlinks              = new Color (0.0f, 0.75f, 0.75f),   // Light Blue
		
			keywords                = new Color (1.0f, 0.33f, 0.01f),   // Red for Pro
			constants               = new Color (1.0f, 0.33f, 0.01f),
			strings                 = new Color (0.85f, 0.15f, 0.85f),  // Pink for Pro
			builtInLiterals         = new Color (1.0f, 0.33f, 0.01f),   // Red for Pro
			operators               = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
		
			referenceTypes          = new Color (0.9f, 0.15f, 0.05f),    // Dark Red for Pro
			valueTypes              = new Color (0.9f, 0.15f, 0.05f),    // Dark Red for Pro
			interfaceTypes          = new Color (0.9f, 0.15f, 0.05f),    // Dark Red for Pro
			enumTypes               = new Color (0.9f, 0.15f, 0.05f),    // Dark Red for Pro
			delegateTypes           = new Color (0.9f, 0.15f, 0.05f),    // Dark Red for Pro
			builtInTypes            = new Color (1.0f, 0.33f, 0.01f),   // Red for Pro
		
			namespaces              = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
			methods                 = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
			fields                  = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
			properties              = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
			events                  = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
		
			parameters              = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
			variables               = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
			typeParameters          = new Color (0.9f, 0.15f, 0.05f),    // Dark Red for Pro
			enumMembers             = new Color (0.85f, 0.85f, 0.85f),  // Light Grey
		
			preprocessor            = new Color (1.0f, 0.33f, 0.01f),
			defineSymbols           = new Color (1.0f, 0.33f, 0.01f),
			inactiveCode            = new Color (0.20f, 0.60f, 0.0f),   // Green
			comments                = new Color (0.20f, 0.60f, 0.0f),   // Green
			xmlDocs                 = new Color (0.20f, 0.60f, 0.0f),   // Green
			xmlDocsTags             = new Color (0.20f, 0.60f, 0.0f),   // Green
		
			lineNumbers             = new Color (0.25f, 0.20f, 0.14f),  // Tan, Very Dark
			lineNumbersHighlight    = new Color (1.0f, 0.80f, 0.56f),   // Tan, Light
			lineNumbersBackground   = new Color (0.50f, 0.40f, 0.28f),  // Tan, Dark
			fold                    = new Color (0.20f, 0.60f, 0.0f),   // Green
		
			activeSelection			= new Color (0.30f, 0.40f, 0.48f, 0.7f),
			passiveSelection		= new Color (0.30f, 0.40f, 0.48f, 0.4f),
			searchResults           = new Color (0.50f, 0.45f, 0.14f, 0.5f),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = new Color(0.20f, 0.18f, 0.14f),
			currentLineInactive     = new Color(0.25f, 0.20f, 0.14f),
		
			referenceHighlight      = (Color)TangoColors.skyblue2 * 0.3f + (Color)TangoColors.aluminium6 * 0.7f,
			referenceModifyHighlight = (Color)TangoColors.scarletred1 * 0.3f + (Color)TangoColors.aluminium6 * 0.7f,
			
			tooltipBackground       = new Color(0.25f, 0.20f, 0.14f),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = new Color(1.0f, 0.80f, 0.56f),
		
			listPopupBackground     = new Color(0.20f, 0.18f, 0.14f),
		},

		// Monokai
		new Theme {
			background              = new Color32(39, 40, 34, 255),
			text                    = new Color32(248, 248, 242, 255),
			hyperlinks              = new Color32(127, 74, 129, 255),
		
			keywords                = new Color32(249, 38, 114, 255),
			constants               = new Color32(174, 129, 255, 255),
			strings                 = new Color32(230, 219, 106, 255),
			builtInLiterals         = new Color32(174, 129, 255, 255),
			operators               = new Color32(248, 248, 242, 255),
		
			referenceTypes          = new Color32(102, 218, 236, 255),
			valueTypes              = new Color32(102, 218, 236, 255),
			interfaceTypes          = new Color32(102, 218, 236, 255),
			enumTypes               = new Color32(102, 218, 236, 255),
			delegateTypes           = new Color32(102, 218, 236, 255),
			builtInTypes            = Color.clear,
		
			namespaces              = new Color32(230, 219, 106, 255),
			methods                 = new Color32(166, 226, 46, 255),
			fields                  = new Color32(248, 248, 242, 255),
			properties              = new Color32(248, 248, 242, 255),
			events                  = new Color32(248, 248, 242, 255),
		
			parameters              = new Color32(0xFD, 0x97, 0x1F, 0xFF),
			variables               = new Color32(248, 248, 242, 255),
			typeParameters          = new Color32(0xFD, 0x97, 0x1F, 0xFF),
			enumMembers             = new Color32(174, 129, 255, 255),
		
			preprocessor            = new Color32(166, 226, 46, 255),
			defineSymbols           = new Color32(166, 226, 46, 255),
			inactiveCode            = new Color32(117, 113, 94, 255),
			comments                = new Color32(117, 113, 94, 255),
			xmlDocs                 = new Color32(117, 113, 94, 255),
			xmlDocsTags             = new Color32(117, 113, 94, 255),
		
			lineNumbers             = new Color32(188, 188, 188, 255),
			lineNumbersHighlight    = new Color32(248, 248, 242, 255),
			lineNumbersBackground   = new Color32(39, 40, 34, 255),
			fold                    = new Color32(59, 58, 50, 255),
		
			activeSelection			= new Color32(73, 72, 62, 255),
			passiveSelection		= new Color32(56, 56, 48, 255),
			searchResults           = new Color32(0, 96, 96, 128),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = new Color32(62, 61, 49, 255),
			currentLineInactive     = new Color32(50, 50, 41, 255),
			
			referenceHighlight      = new Color32(48, 65, 87, 144),
			referenceModifyHighlight = new Color32(105, 48, 49, 144),
		
			tooltipBackground       = new Color32(62, 61, 49, 255),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = new Color32(188, 188, 188, 255),
		
			listPopupBackground     = new Color32(50, 50, 41, 255),
		
			typesStyle              = FontStyle.Italic,
			typeParametersStyle     = FontStyle.Italic,
			parametersStyle         = FontStyle.Italic,
		},

		// Son of Obsidian
		new Theme {
			background              = new Color32(0x22, 0x28, 0x2A, 0xFF),
			text                    = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
			hyperlinks              = new Color32(0x99, 0xDA, 0xF9, 0xFF),
		
			keywords                = new Color32(0x93, 0xC7, 0x63, 0xFF),
			constants               = new Color32(0xFF, 0xCD, 0x22, 0xFF),
			strings                 = new Color32(0xEC, 0x76, 0x00, 0xFF),
			builtInLiterals         = new Color32(0x93, 0xC7, 0x63, 0xFF),
			operators               = new Color32(0xE8, 0xE2, 0xB7, 0xFF),
		
			referenceTypes          = new Color32(0x8C, 0x8C, 0xB4, 0xFF),
			valueTypes              = new Color32(0x8C, 0x8C, 0xB4, 0xFF),
			interfaceTypes          = new Color32(0x67, 0x8C, 0xB1, 0xFF),
			enumTypes               = new Color32(0x67, 0x8C, 0xB1, 0xFF),
			delegateTypes           = new Color32(0x67, 0x8C, 0xB1, 0xFF),
			builtInTypes            = new Color32(0x93, 0xC7, 0x63, 0xFF),
		
			namespaces              = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
			methods                 = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
			fields                  = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
			properties              = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
			events                  = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
		
			parameters              = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
			variables               = new Color32(0xF1, 0xF2, 0xF3, 0xFF),
			typeParameters          = new Color32(0x67, 0x8C, 0xB1, 0xFF),
			enumMembers             = new Color32(0xA0, 0x82, 0xBD, 0xFF),
		
			preprocessor            = new Color32(0xA0, 0x82, 0xBD, 0xFF),
			defineSymbols           = new Color32(0xA0, 0x82, 0xBD, 0xFF),
			inactiveCode            = new Color32(0x61, 0x61, 0x61, 0xFF),
			comments                = new Color32(0x66, 0x74, 0x7B, 0xFF),
			xmlDocs                 = new Color32(0x99, 0xA3, 0x8A, 0xFF),
			xmlDocsTags             = new Color32(0x80, 0x80, 0x80, 0xFF),
		
			lineNumbers             = new Color32(0x3F, 0x4E, 0x49, 0xFF),
			lineNumbersHighlight    = new Color32(0x7E, 0x9D, 0x92, 0xFF),
			lineNumbersBackground   = new Color32(0x29, 0x31, 0x34, 0xFF),
			fold                    = new Color32(0x29, 0x31, 0x34, 0xFF),
		
			activeSelection			= new Color32(0x96, 0xAD, 0xB2, 0x44),
			passiveSelection		= new Color32(0x17, 0x1B, 0x1C, 0xFF),
			searchResults           = new Color32(0x57, 0x4E, 0x40, 0xFF),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = new Color32(0x31, 0x3A, 0x3E, 0xFF),
			currentLineInactive     = new Color32(0x29, 0x31, 0x36, 0xFF),
			
			referenceHighlight      = new Color32(48, 65, 87, 144),
			referenceModifyHighlight = new Color32(105, 48, 49, 144),

			tooltipBackground       = new Color32(0x29, 0x31, 0x34, 0xFF),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = TangoColors.aluminium2,
		
			listPopupBackground		= new Color32(0x17, 0x1B, 0x1C, 0xFF),
		
			//typesStyle              = FontStyle.Italic
		},
		
		// Darcula
		new Theme {
			background				= new Color32( 56,  56,  56, 255),
			text					= new Color32(183, 196, 208, 255),
			hyperlinks				= new Color32(0x99, 0xDA, 0xF9, 0xFF),
		
			keywords				= new Color32(215, 139,  54, 255),
			constants               = new Color32(0x68, 0x97, 0xBB, 0xFF),
			strings					= new Color32(0xA5, 0xC2, 0x5C, 255),
			builtInLiterals			= new Color32(215, 139,  54, 255),
			operators				= new Color32(0xE8, 0xE2, 0xB7, 255),
		
			referenceTypes			= new Color32(212, 106, 0, 0xFF),
			valueTypes				= new Color32(212, 106, 0, 0xFF),
			interfaceTypes			= new Color32(0x21, 0x95, 0x98, 0xFF),
			enumTypes				= new Color32(0x76, 0x9A, 0xA5, 0xFF),
			delegateTypes			= new Color32(212, 106, 0, 0xFF),
			builtInTypes			= new Color32(215, 139,  54, 255),
		
			namespaces				= new Color32(183, 196, 208, 255),
			methods					= new Color32(0xC4, 0xB3, 0xA3, 255),
			fields					= new Color32(183, 196, 208, 255),
			properties				= new Color32(183, 196, 208, 255),
			events					= new Color32(183, 196, 208, 255),
		
			parameters              = new Color32(0xC4, 0xB3, 0xA3, 0xFF),
			variables				= new Color32(183, 196, 208, 255),
			typeParameters          = new Color32(0x76, 0x9A, 0xA5, 0xFF),
			enumMembers             = new Color32(0xA0, 0x82, 0xBD, 0xFF),
		
			preprocessor            = new Color32(0xA0, 0x82, 0xBD, 0xFF),
			defineSymbols           = new Color32(0xA0, 0x82, 0xBD, 0xFF),
			inactiveCode			= new Color32(122, 118, 138, 255),
			comments				= new Color32(114, 162, 102, 255),
			xmlDocs					= new Color32(114, 162, 102, 255),
			xmlDocsTags				= new Color32(122, 118, 138, 255),
		
			lineNumbers				= new Color32(0x2B, 0x91, 0xAF, 0xFF),
			lineNumbersHighlight	= new Color32(183, 196, 208, 255),
			lineNumbersBackground	= new Color32( 41,  41,  41, 255),
			fold					= new Color32(0x2B, 0x91, 0xAF, 0xFF),
		
			activeSelection			= new Color32( 68, 134, 244, 80),
			passiveSelection		= new Color32( 72,  72,  72, 255),
			searchResults			= new Color32(0x67, 0x47, 0x07, 0xFF),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238,  98, 255),
			trackReverted           = new Color32(246, 201,  60, 255),
		
			currentLine             = new Color32( 73,  73,  73, 255),
			currentLineInactive     = new Color32( 73,  73,  73, 255),
		
			referenceHighlight      = (Color)TangoColors.skyblue2 * 0.3f + (Color)TangoColors.aluminium6 * 0.7f,
			referenceModifyHighlight = (Color)TangoColors.scarletred1 * 0.3f + (Color)TangoColors.aluminium6 * 0.7f,
			
			tooltipBackground       = new Color32( 64,  64,  64, 255),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = new Color32(183, 196, 208, 255),
			
			listPopupBackground       = new Color32( 48,  48,  48, 255),

			preprocessorStyle       = FontStyle.Italic,
			commentsStyle           = FontStyle.Italic,
		},
		
		// Visual Studio Dark (courtesy of Killcycle)
		new Theme {
			background = new Color32(30, 30, 30, 255),
			text = new Color32(218, 218, 218, 255),
			hyperlinks = new Color32(86, 156, 214, 255),
		
			keywords = new Color32(86, 156, 214, 255),
			constants = new Color32(181, 206, 168, 255),
			strings = new Color32(214, 157, 133, 255),
			builtInLiterals = new Color32(86, 156, 214, 255),
			operators = new Color32(180, 180, 180, 255),
		
			referenceTypes = new Color32(78, 201, 176, 255),
			valueTypes = new Color32(78, 201, 176, 255),
			interfaceTypes = new Color32(184, 215, 163, 255),
			enumTypes = new Color32(184, 215, 163, 255),
			delegateTypes = new Color32(78, 201, 176, 255),
			builtInTypes = new Color32(86, 156, 214, 255),
		
			namespaces = new Color32(200, 200, 200, 255),
			methods = new Color32(200, 200, 200, 255),
			fields = new Color32(218, 218, 218, 255),
			properties = new Color32(200, 200, 200, 255),
			events = new Color32(200, 200, 200, 255),
		
			parameters = new Color32(127, 127, 127, 255),
			variables = new Color32(200, 200, 200, 255),
			typeParameters = new Color32(184, 215, 163, 255),
			enumMembers = new Color32(189, 99, 197, 255),
		
			preprocessor = new Color32(155, 155, 155, 255),
			defineSymbols = new Color32(189, 99, 197, 255),
			inactiveCode = new Color32(155, 155, 155, 255),
			comments = new Color32(87, 166, 74, 255),
			xmlDocs = new Color32(87, 166, 74, 255),
			xmlDocsTags = new Color32(87, 166, 74, 255),
			
			lineNumbers = new Color32(43, 145, 175, 255),
			lineNumbersHighlight = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			lineNumbersBackground = new Color32(30, 30, 30, 255),
			fold = new Color32(165, 165, 165, 255),
		
			activeSelection = new Color32(51, 153, 255, 102),
			passiveSelection = new Color32(86, 86, 86, 102),
			searchResults = new Color32(119, 56, 0, 255),
		
			trackSaved = new Color32(87, 116, 48, 255),
			trackChanged = new Color32(239, 242, 132, 255),
			trackReverted = new Color32(95, 149, 250, 255),
		
			currentLine = new Color32(0, 0, 0, 255),
			currentLineInactive = new Color32(42, 42, 42, 255),
			
			referenceHighlight = new Color32(14, 69, 131, 144),
			referenceModifyHighlight = new Color32(131, 14, 69, 162),
			
			tooltipBackground = new Color32(66, 66, 69, 255),
			tooltipText = new Color32(241, 241, 241, 255),
			tooltipFrame = new Color32(102, 102, 102, 255),
			
			listPopupBackground = new Color32(37, 37, 38, 255),
		},
		
		// Visual Studio with Visual Assist X
		new Theme {
			background				= Color.white,
			text					= Color.black,
			hyperlinks				= Color.blue,
		
			keywords				= Color.blue,
			constants               = Color.black,
			strings					= new Color32(0x80, 0x00, 0x00, 0xff),
			builtInLiterals         = Color.blue,
			operators				= Color.black,
		
			referenceTypes          = new Color32(0x2b, 0x91, 0xaf, 0xff),
			valueTypes              = new Color32(0x2b, 0x91, 0xaf, 0xff),
			interfaceTypes          = new Color32(0x2b, 0x91, 0xaf, 0xff),
			enumTypes               = new Color32(0x2b, 0x91, 0xaf, 0xff),
			delegateTypes           = new Color32(0x2b, 0x91, 0xaf, 0xff),
			builtInTypes			= Color.blue,
		
			namespaces              = new Color32(33, 111, 133, 255),
			methods                 = new Color32(136, 0, 0, 255),
			fields                  = new Color32(0, 0, 128, 255),
			properties              = new Color32(0, 0, 128, 255),
			events                  = new Color32(0, 0, 128, 255),
		
			parameters              = new Color32(0, 0, 128, 255),
			variables               = new Color32(0, 0, 128, 255),
			typeParameters          = new Color32(0x21, 0x6f, 0x85, 0xff),
			enumMembers             = new Color32(111, 0, 138, 255),
		
			preprocessor            = Color.blue,
			defineSymbols           = new Color32(111, 0, 138, 255),
			inactiveCode            = Color.gray,
			comments				= new Color32(0x00, 0x80, 0x00, 0xff),
			xmlDocs					= new Color32(0x80, 0x80, 0x80, 0xff),
			xmlDocsTags				= new Color32(0x80, 0x80, 0x80, 0xff),
			
			lineNumbers				= new Color32(0x2b, 0x91, 0xaf, 0xff),
			lineNumbersHighlight	= Color.blue,
			lineNumbersBackground	= Color.white,
			fold					= new Color32(165, 165, 165, 255),
		
			activeSelection			= new Color32(51, 153, 255, 102),
			passiveSelection		= new Color32(191, 205, 219, 102),
			searchResults			= new Color32(255, 255, 183, 255),
		
			trackSaved              = new Color32(108, 226, 108, 255),
			trackChanged            = new Color32(255, 238, 98, 255),
			trackReverted           = new Color32(246, 201, 60, 255),
		
			currentLine             = new Color32(213, 213, 241, 255),
			currentLineInactive     = new Color32(228, 228, 228, 255),
			
			referenceHighlight      = new Color32(0xe0, 0xff, 0xff, 0xff),
			referenceModifyHighlight = new Color32(0xff, 0xdd, 0xdd, 0xff),
		
			tooltipBackground       = new Color32(253, 255, 153, 255),
			tooltipFrame            = new Color32(128, 128, 128, 255),
			tooltipText             = new Color32(22, 22, 22, 255),
		
			listPopupBackground = Color.white,
		
			//variablesStyle = FontStyle.Bold,
			//parametersStyle = FontStyle.Bold,
			//keywordsStyle = FontStyle.Bold,
		},
		
		// Visual Studio Dark with Visual Assist X
		new Theme {
			background = new Color32(30, 30, 30, 255),
			text = new Color32(218, 218, 218, 255),
			hyperlinks = new Color32(86, 156, 214, 255),
		
			keywords = new Color32(86, 156, 214, 255),
			constants = new Color32(181, 206, 168, 255),
			strings = new Color32(214, 157, 133, 255),
			builtInLiterals = new Color32(86, 156, 214, 255),
			operators = new Color32(180, 180, 180, 255),
		
			referenceTypes = new Color32(78, 201, 176, 255),
			valueTypes = new Color32(78, 201, 176, 255),
			interfaceTypes = new Color32(184, 215, 163, 255),
			enumTypes = new Color32(184, 215, 163, 255),
			delegateTypes = new Color32(78, 201, 176, 255),
			builtInTypes = new Color32(86, 156, 214, 255),
		
			namespaces = new Color32(255, 215, 0, 255),
			methods = new Color32(255, 128, 0, 255),
			fields = new Color32(189, 183, 107, 255),
			properties = new Color32(189, 183, 107, 255),
			events = new Color32(189, 183, 107, 255),
		
			parameters = new Color32(189, 183, 107, 255),
			variables = new Color32(189, 183, 107, 255),
			typeParameters = new Color32(184, 215, 163, 255),
			enumMembers = new Color32(189, 99, 197, 255),
		
			preprocessor = new Color32(155, 155, 155, 255),
			defineSymbols = new Color32(189, 99, 197, 255),
			inactiveCode = new Color32(155, 155, 155, 255),
			comments = new Color32(87, 166, 74, 255),
			xmlDocs = new Color32(87, 166, 74, 255),
			xmlDocsTags = new Color32(87, 166, 74, 255),
		
			lineNumbers = new Color32(43, 145, 175, 255),
			lineNumbersHighlight = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			lineNumbersBackground = new Color32(30, 30, 30, 255),
			fold = new Color32(165, 165, 165, 255),
		
			activeSelection = new Color32(51, 153, 255, 102),
			passiveSelection = new Color32(86, 86, 86, 102),
			searchResults = new Color32(119, 56, 0, 255),
		
			trackSaved = new Color32(87, 116, 48, 255),
			trackChanged = new Color32(239, 242, 132, 255),
			trackReverted = new Color32(95, 149, 250, 255),
		
			currentLine = new Color32(0, 0, 0, 255),
			currentLineInactive = new Color32(42, 42, 42, 255),
			
			referenceHighlight = new Color32(14, 69, 131, 144),
			referenceModifyHighlight = new Color32(111, 14, 69, 162),
			
			tooltipBackground = new Color32(66, 66, 69, 255),
			tooltipText = new Color32(241, 241, 241, 255),
			tooltipFrame = new Color32(102, 102, 102, 255),
		
			listPopupBackground = new Color32(37, 37, 38, 255),
		},
		
		// Solarized Dark
		new Theme {
			background = new Color32(0x00, 0x2B, 0x36, 0xFF),
			text = new Color32(0x83, 0x94, 0x96, 0XFF),
			hyperlinks = new Color32(0x26, 0x8B, 0xD2, 0xFF),
			
			keywords = new Color32(0x71, 0x9A, 0x07, 0xFF),
			constants = new Color32(0x2A, 0xA1, 0x98, 0xFF),
			strings = new Color32(0x2A, 0xA1, 0x98, 0xFF),
			builtInLiterals = new Color32(0x71, 0x9A, 0x07, 0xFF),
			operators = new Color32(0x71, 0x9A, 0x07, 0xFF),
			
			referenceTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			valueTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			interfaceTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			enumTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			delegateTypes = new Color32(0x6C, 0x71, 0xC4, 0xFF),
			builtInTypes = new Color32(0x71, 0x9A, 0x07, 0xFF),
			
			namespaces = new Color32(0x65, 0x7B, 0x83, 0xFF),
			methods = new Color32(0x93, 0xA1, 0xA1, 0xFF),
			fields = new Color32(0x83, 0x94, 0x96, 0xFF),
			properties = new Color32(0x83, 0x94, 0x96, 0xFF),
			events = new Color32(0x6C, 0x71, 0xC4, 0xFF),
			
			parameters = new Color32(0x83, 0x94, 0x96, 0xFF),
			variables = new Color32(0x83, 0x94, 0x96, 0xFF),
			typeParameters = new Color32(0xD3, 0x36, 0x82, 0xFF),
			enumMembers = new Color32(0xD3, 0x36, 0x82, 0xFF),
			
			preprocessor = new Color32(0xCB, 0x4B, 0x16, 0xFF),
			defineSymbols = new Color32(0xD3, 0x36, 0x82, 0xFF),
			inactiveCode = new Color32(0x58, 0x6E, 0x75, 0xFF),
			comments = new Color32(0x58, 0x6E, 0x75, 0xFF),
			xmlDocs = new Color32(0x58, 0x6E, 0x75, 0xFF),
			xmlDocsTags = new Color32(0x58, 0x6E, 0x75, 0xFF),
			
			lineNumbers = new Color32(0x65, 0x7B, 0x83, 0xFF),
			lineNumbersHighlight = new Color32(0x83, 0x94, 0x96, 0xFF),
			lineNumbersBackground = new Color32(0x07, 0x36, 0x42, 0xFF),
			fold = new Color32(0x65, 0x7B, 0x83, 0xFF),
			
			activeSelection = new Color32(0x58, 0x6E, 0x75, 102),
			passiveSelection = new Color32(0x58, 0x6E, 0x75, 102),
			searchResults = new Color32(0x07, 0x36, 0x42, 0xFF),
			
			trackSaved = new Color32(0x71, 0x9A, 0x07, 0xFF),
			trackChanged = new Color32(0xB5, 0x89, 0x00, 0xFF),
			trackReverted = new Color32(95, 149, 250, 255),
			
			currentLine = new Color32(0x07, 0x36, 0x42, 0xFF),
			currentLineInactive = new Color32(0x07, 0x36, 0x42, 0xFF),
			
			referenceHighlight = new Color32(0x00, 0x49, 0x3E, 204),
			referenceModifyHighlight = new Color32(0x79, 0x22, 0x05, 144),
			
			tooltipBackground = new Color32(0x07, 0x36, 0x42, 0xFF),
			tooltipText = new Color32(0x83, 0x94, 0x96, 0XFF),
			tooltipFrame = new Color32(0x65, 0x7B, 0x83, 0xFF),
			
			listPopupBackground = new Color32(0x07, 0x36, 0x42, 0xFF),
		},
		
		// Solarized Light
		new Theme {
			background = new Color32(0xFD, 0xF6, 0xE3, 0xFF),
			text = new Color32(0x65, 0x7B, 0x83, 0XFF),
			hyperlinks = new Color32(0x26, 0x8B, 0xD2, 0xFF),
			
			keywords = new Color32(0xCB, 0x4B, 0x16, 0xFF),
			constants = new Color32(0x2A, 0xA1, 0x98, 0xFF),
			strings = new Color32(0x2A, 0xA1, 0x98, 0xFF),
			builtInLiterals = new Color32(0xCB, 0x4B, 0x16, 0xFF),
			operators = new Color32(0x71, 0x9A, 0x07, 0xFF),
			
			referenceTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			valueTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			interfaceTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			enumTypes = new Color32(0xB5, 0x89, 0x00, 0xFF),
			delegateTypes = new Color32(0x6C, 0x71, 0xC4, 0xFF),
			builtInTypes = new Color32(0xCB, 0x4B, 0x16, 0xFF),
			
			namespaces = new Color32(0x83, 0x94, 0x96, 0xFF),
			methods = new Color32(0x58, 0x6E, 0x75, 0xFF),
			fields = new Color32(0x65, 0x7B, 0x83, 0xFF),
			properties = new Color32(0x65, 0x7B, 0x83, 0xFF),
			events = new Color32(0x6C, 0x71, 0xC4, 0xFF),
			
			parameters = new Color32(0x65, 0x7B, 0x83, 0xFF),
			variables = new Color32(0x65, 0x7B, 0x83, 0xFF),
			typeParameters = new Color32(0xD3, 0x36, 0x82, 0xFF),
			enumMembers = new Color32(0xD3, 0x36, 0x82, 0xFF),
			
			preprocessor = new Color32(0xCB, 0x4B, 0x16, 0xFF),
			defineSymbols = new Color32(0xD3, 0x36, 0x82, 0xFF),
			inactiveCode = new Color32(0x93, 0xA1, 0xA1, 0xFF),
			comments = new Color32(0x93, 0xA1, 0xA1, 0xFF),
			xmlDocs = new Color32(0x93, 0xA1, 0xA1, 0xFF),
			xmlDocsTags = new Color32(0x93, 0xA1, 0xA1, 0xFF),
			
			lineNumbers = new Color32(0x83, 0x94, 0x96, 0xFF),
			lineNumbersHighlight = new Color32(0x58, 0x6E, 0x75, 0xFF),
			lineNumbersBackground = new Color32(0xEE, 0xE8, 0xD5, 0xFF),
			fold = new Color32(0x83, 0x94, 0x96, 0xFF),
			
			activeSelection = new Color32(0x93, 0xA1, 0xA1, 102),
			passiveSelection = new Color32(0x93, 0xA1, 0xA1, 102),
			searchResults = new Color32(0xEE, 0xE8, 0xD5, 0xFF),
			
			trackSaved = new Color32(0x71, 0x9A, 0x07, 0xFF),
			trackChanged = new Color32(0xB5, 0x89, 0x00, 0xFF),
			trackReverted = new Color32(95, 149, 250, 255),
			
			currentLine = new Color32(0xFF, 0xFF, 0xE0, 0xFF),
			currentLineInactive = new Color32(0xFF, 0xFF, 0xE0, 0xFF),
			
			referenceHighlight = new Color32(0x87, 0xCE, 0xFA, 102),
			referenceModifyHighlight = new Color32(0xFF, 0xB6, 0xC1, 102),
			
			tooltipBackground = new Color32(0xEE, 0xE8, 0xD5, 0xFF),
			tooltipText = new Color32(0x58, 0x6E, 0x75, 0XFF),
			tooltipFrame = new Color32(0x83, 0x94, 0x96, 0xFF),
			
			listPopupBackground = new Color32(0xFD, 0xF6, 0xE3, 0xFF),
		},
		
		// Visual Studio Dark with Resharper 9.1 (courtesy of Sarper Soher)
		new Theme {
			background = new Color32(0x1E, 0x1E, 0x1E, 0xFF),
			text = new Color32(0xDC, 0xDC, 0xDC, 0XFF),
			hyperlinks = new Color32(0x00, 0x00, 0xFF, 0xFF),
			
			keywords = new Color32(0x56, 0x9C, 0xD6, 0xFF),
			constants = new Color32(0xB5, 0xCE, 0xA8, 0xFF),
			strings = new Color32(0xD6, 0x9D, 0x85, 0xFF),
			builtInLiterals = new Color32(0xDA, 0xDA, 0xDA, 0xFF),
			operators = new Color32(0xB4, 0xB4, 0xB4, 0xFF),
			
			referenceTypes = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			valueTypes = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			interfaceTypes = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			enumTypes = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			delegateTypes = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			builtInTypes = new Color32(0x56, 0x9C, 0xD6, 0xFF),
			
			namespaces = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			methods = new Color32(0x00, 0xFF, 0xFF, 0xFF),
			fields = new Color32(0xEE, 0x82, 0xEE, 0xFF),
			properties = new Color32(0xEE, 0x82, 0xEE, 0xFF),
			events = new Color32(0xDD, 0xA0, 0xDD, 0xFF),
			
			parameters = new Color32(0xDC, 0xDC, 0xDC, 0xFF),
			variables = new Color32(0xDC, 0xDC, 0xDC, 0xFF),
			typeParameters = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			enumMembers = new Color32(0xEE, 0x82, 0xEE, 0xFF),
			
			preprocessor = new Color32(0xDC, 0xDC, 0xDC, 0xFF),
			defineSymbols = new Color32(0xDA, 0xDA, 0xDA, 0xFF),
			inactiveCode = new Color32(0x93, 0xA1, 0xA1, 0xFF),
			comments = new Color32(0x57, 0xA6, 0x4A, 0xFF),
			xmlDocs = new Color32(0x00, 0x64, 0x00, 0xFF),
			xmlDocsTags = new Color32(0x00, 0x64, 0x00, 0xFF),
			
			lineNumbers = new Color32(43, 145, 175, 255),
			lineNumbersHighlight = new Color32(0xAD, 0xD8, 0xE6, 0xFF),
			lineNumbersBackground = new Color32(30, 30, 30, 255),
			fold = new Color32(165, 165, 165, 255),
		
			activeSelection = new Color32(51, 153, 255, 102),
			passiveSelection = new Color32(86, 86, 86, 102),
			searchResults = new Color32(119, 56, 0, 255),
		
			trackSaved = new Color32(0x71, 0x9A, 0x07, 0xFF),
			trackChanged = new Color32(0xB5, 0x89, 0x00, 0xFF),
			trackReverted = new Color32(95, 149, 250, 255),
			
			currentLine = new Color32(0x0F, 0x0F, 0x0F, 0xFF),
			currentLineInactive = new Color32(0x24, 0x24, 0x24, 0xFF),
			
			referenceHighlight = new Color32(72, 61, 139, 144),
			referenceModifyHighlight = new Color32(128, 0, 0, 144),
			
			tooltipBackground = new Color32(66, 66, 69, 255),
			tooltipText = new Color32(241, 241, 241, 255),
			tooltipFrame = new Color32(102, 102, 102, 255),
			
			listPopupBackground = new Color32(37, 37, 38, 255),
		},
	};
	private static Theme currentThemeCode = themes[0];
	private static Theme currentThemeText = themes[0];
	
	public Theme CurrentTheme {
		get {
			return textBuffer != null && textBuffer.isText ? currentThemeText : currentThemeCode;
		}
	}

	//Xcode 5 theme (courtesy of inventor2010)
	//Xcode 5's actual colors and names for its default theme:
	private static class Xcode5Theme
	{
		//Changable Colors in Xcode:
		public static Color32 PlainText								= new Color32(0,0,0,255);
		public static Color32 Comments								= new Color32(0,116,0,255);
		public static Color32 DocumentationComments					= new Color32(0,116,0,255);
		public static Color32 DocumentationCommentKeywords			= new Color32(2,61,16,255);
		public static Color32 Strings								= new Color32(196,26,22,255);
		public static Color32 Characters							= new Color32(28,0,207,255);
		public static Color32 Numbers								= new Color32(28,0,207,255);
		public static Color32 Keywords								= new Color32(170,13,145,255);
		public static Color32 PreprocessorStatements				= new Color32(100,56,32,255);
		public static Color32 URLs									= new Color32(14,14,255,255);
		public static Color32 Attributes							= new Color32(131,108,40,255);
		public static Color32 ProjectClassNames						= new Color32(63,110,116,255);
		public static Color32 ProjectFunctionandMethodNames			= new Color32(38,71,75,255);
		public static Color32 ProjectConstants						= new Color32(38,71,75,255);
		public static Color32 ProjectTypeNames						= new Color32(63,110,116,255);
		public static Color32 ProjectInstanceVariablesandGlobals	= new Color32(63,110,116,255);
		public static Color32 ProjectPreprocessorMacros				= new Color32(100,56,32,255);
		public static Color32 OtherClassNames						= new Color32(92,38,153,255);
		public static Color32 OtherFunctionandMethodNames			= new Color32(46,13,110,255);
		public static Color32 OtherConstants						= new Color32(46,13,110,255);
		public static Color32 OtherTypeNames						= new Color32(92,38,153,255);
		public static Color32 OtherInstanceVariablesandGlobals		= new Color32(92,38,153,255);
		public static Color32 OtherPreprocessorMacros				= new Color32(100,56,32,255);
		
		public static Color32 Background							= new Color32(255,255,255,255);
		public static Color32 Selection								= new Color32(167,202,255,255);
		public static Color32 Cursor								= new Color32(0,0,0,255);
		public static Color32 Invisibles							= new Color32(127,127,127,255);
		
		//Found with "DigitalColor Meter"
		public static Color32 inactiveSelection						= new Color32(212,212,212,255);
		public static Color32 lineNumberBackground					= new Color32(247,247,247,255);
		public static Color32 lineNumbers							= new Color32(146,146,146,255);
		public static Color32 lineNumberBoarder						= new Color32(231,231,231,255);
		public static Color32 searchResults							= new Color32(250, 241, 190,255);
	}
	
	internal static readonly string[] availableFonts = {
		"Fonts/DejaVu Sans Mono.ttf",
		"Fonts/Inconsolata.otf",
		"Fonts/Monaco.ttf",
		"Fonts/SourceCodePro-Regular.otf",
		"Fonts/SourceCodePro-Semibold.otf",
	};
	private static string currentFont = null;
	private static bool resetCodeFont = true;
	private static bool resetTextFont = true;
	
	[NonSerialized]
	private float marginLeft = 0f;
	[NonSerialized]
	private float marginRight = 1f;
	
	[NonSerialized]
	private bool isTextAsset = true;

	public class Styles
	{
		public GUIStyle scrollViewStyle;
		public GUIStyle normalStyle;
		public GUIStyle hyperlinkStyle;
		public GUIStyle mailtoStyle;

		public GUIStyle keywordStyle;
		public GUIStyle constantStyle;
		public GUIStyle stringStyle;
		public GUIStyle builtInLiteralsStyle;
		public GUIStyle operatorStyle;
		
		public GUIStyle referenceTypeStyle;
		public GUIStyle valueTypeStyle;
		public GUIStyle interfaceTypeStyle;
		public GUIStyle enumTypeStyle;
		public GUIStyle delegateTypeStyle;
		public GUIStyle builtInRefTypeStyle;
		public GUIStyle builtInValueTypeStyle;
		
		public GUIStyle namespaceStyle;
		public GUIStyle methodStyle;
		public GUIStyle fieldStyle;
		public GUIStyle propertyStyle;
		public GUIStyle eventStyle;
		
		public GUIStyle parameterStyle;
		public GUIStyle variableStyle;
		public GUIStyle typeParameterStyle;
		public GUIStyle enumMemberStyle;

		public GUIStyle preprocessorStyle;
		public GUIStyle defineSymbols;
		public GUIStyle inactiveCodeStyle;
		public GUIStyle commentStyle;
		public GUIStyle xmlDocsStyle;
		public GUIStyle xmlDocsTagsStyle;
		
		public GUIStyle lineNumbersStyle;
		public GUIStyle lineNumbersBackground;
		public GUIStyle lineNumbersSeparator;

		public GUIStyle caretStyle;
		public GUIStyle activeSelectionStyle;
		public GUIStyle passiveSelectionStyle;
		public GUIStyle searchResultStyle;
		
		public GUIStyle trackChangesAfterSaveStyle;
		public GUIStyle trackChangesBeforeSaveStyle;
		public GUIStyle trackChangesRevertedStyle;
		
		public GUIStyle currentLineStyle;
		public GUIStyle currentLineInactiveStyle;
		
		public GUIStyle referenceHighlightStyle;
		public GUIStyle referenceModifyHighlightStyle;
		
		public GUIStyle tooltipBgStyle;
		public GUIStyle tooltipFrameStyle;
		public GUIStyle tooltipTextStyle;
		
		public GUIStyle listBgStyle;
		
		public GUIStyle ping;
		public GUIStyle toolbarSearchField;
		public GUIStyle toolbarSearchFieldCancelButton;
		public GUIStyle toolbarSearchFieldCancelButtonEmpty;
		public GUIStyle upArrowStyle;
		public GUIStyle downArrowStyle;
	}
	public static Styles stylesCode = new Styles();
	public static Styles stylesText = new Styles();
	[NonSerialized]
	public Styles styles = stylesCode;
	
	[NonSerialized]
	private GUIStyle orangeBoldLabel;

	[SerializeField, HideInInspector]
	private Vector2 scrollPosition;
	[SerializeField, HideInInspector]
	private int scrollPositionLine;
	[SerializeField, HideInInspector]
	private float scrollPositionOffset;
	[NonSerialized]
	private Vector2 smoothScrollPosition;
	[NonSerialized]
	private Vector2 currentScrollVelocity;
	[NonSerialized]
	private float lastSmoothScrollTime;
	[NonSerialized]
	private bool scrollPositionInitialized;
	
//	[NonSerialized]
//	private float contentWidth = 0;

	[NonSerialized]
	private Rect scrollViewRect;
	[NonSerialized]
	private Rect contentRect;
	[NonSerialized]
	private bool needsRepaint;
	public bool hasCodeViewFocus { get; private set; }
	[NonSerialized]
	private EditorWindow parentWindow;
	public Vector2 charSize { get; private set; }
	
	private bool wordWrapping {
		get {
			if (textBuffer == null)
				return false;
			if (parentWindow != null)
				return textBuffer.isText ? SISettings.wordWrapText : SISettings.wordWrapCode;
			return textBuffer.isText ? SISettings.wordWrapTextInspector : SISettings.wordWrapCodeInspector;
		}
	}

	private bool trackChanges {
		get {
			if (textBuffer == null)
				return false;
			if (parentWindow != null)
				return textBuffer.isText ? SISettings.trackChangesText : SISettings.trackChangesCode;
			return textBuffer.isText ? SISettings.trackChangesTextInspector : SISettings.trackChangesCodeInspector;
		}
	}

	private static readonly int buttonHash = "Button".GetHashCode();
	private static Texture2D wrenchIcon;
	private static Texture2D wavyUnderline;
	private static Texture2D saveIcon;
	private static Texture2D buildIcon;
	private static Texture2D undoIcon;
	private static Texture2D redoIcon;
	//private static Texture2D hyperlinksIcon;
	private static Texture2D popOutIcon;

	[NonSerialized]
	private bool hasSearchBoxFocus;
	[NonSerialized]
	private bool focusSearchBox = false;
	[NonSerialized]
	private bool focusCodeView = true;
	[NonSerialized]
	private bool focusCodeViewOnEscapeUp = false;
	private static string defaultSearchString = string.Empty;
	[SerializeField]
	private string searchString = "";
	[NonSerialized]
	private List<FGTextBuffer.CaretPos> searchResults = new List<FGTextBuffer.CaretPos>();
	[NonSerialized]
	private int currentSearchResult = -1;
	[NonSerialized]
	private int searchResultAge = 0;
	[NonSerialized]
	private bool atLastSearchResult = false;
	[NonSerialized]
	private bool highlightSearchResults;
	[NonSerialized]
	private float pingTimer = 0f;
	[NonSerialized]
	private float pingStartTime = 0f;
	[NonSerialized]
	private GUIContent pingContent = new GUIContent();
	[NonSerialized]
	private Color pingColor = Color.white;
	[NonSerialized]
	private Rect scrollToRect;
	[NonSerialized]
	public bool scrollToCaret = false;
	[NonSerialized]
	private SymbolDefinition highlightedSymbol = null;
	[NonSerialized]
	private string highlightedPPSymbol = null;
	[NonSerialized]
	private float highightReferencesTime = 0f;
	[NonSerialized]
	private TextPosition matchingBraceLeft;
	[NonSerialized]
	private TextPosition matchingBraceRight;
	[NonSerialized]
	private int matchedBracesAtUndoPosition = -1;
	[NonSerialized]
	private FGTextBuffer.CaretPos matchedBracesAtCaretPosition;
	
	private static string[] lineNumberCachedStrings = new string[0];

	// Editor

	[NonSerialized]
	private bool isCaretOn = true;
	[NonSerialized]
	public float caretMoveTime = 0f;
	[SerializeField, HideInInspector]
	public FGTextBuffer.CaretPos caretPosition = new FGTextBuffer.CaretPos();
	[SerializeField, HideInInspector]
	private FGTextBuffer.CaretPos _selectionStartPosition = null;
	[SerializeField, HideInInspector]
	private bool hasSelection = false;

	public FGTextBuffer.CaretPos selectionStartPosition
	{
		get { return hasSelection ? _selectionStartPosition : null; }
		set
		{
			if (value == null)
			{
				_selectionStartPosition = null;
				hasSelection = false;
			}
			else
			{
				_selectionStartPosition = value;
				hasSelection = true;
			}
		}
	}

	[NonSerialized]
	private bool codeViewDragging = false;
	[NonSerialized]
	private bool mouseDownOnSelection = false;
	[NonSerialized]
	private FGTextBuffer.CaretPos mouseDropPosition = new FGTextBuffer.CaretPos();
	[NonSerialized]
	private Vector2 autoScrolling = Vector2.zero;
	[NonSerialized]
	private Vector2 autoScrollDelta = Vector2.zero;
	[NonSerialized]
	private float lastAutoScrollTime = 0f;
	[SerializeField]
	private string fullLineCopied;

	[NonSerialized]
	private bool autoScrollLeft = false;
	[NonSerialized]
	private bool autoScrollRight = false;
	[NonSerialized]
	private bool autoScrollUp = false;
	[NonSerialized]
	private bool autoScrollDown = false;

	[NonSerialized]
	private FGTextBuffer textBuffer = null;
	public FGTextBuffer TextBuffer { get { return textBuffer; } }
	public bool IsModified { get { return textBuffer != null && textBuffer.IsModified; } }
	public bool IsLoading { get { return textBuffer == null || textBuffer.IsLoading; } }
	public bool CanEdit() { return !IsLoading /*&& !EditorApplication.isCompiling /*&& !textBuffer.justSavedNow*/; }

	public string targetGuid { get { return textBuffer != null ? textBuffer.guid : string.Empty; } }
	public string targetPath { get { return textBuffer != null ? textBuffer.assetPath : string.Empty; } }
	
	enum TryEditState { Ask, Yes, No };
	[NonSerialized] TryEditState tryEditState;
	private bool TryEdit()
	{
		if (tryEditState == TryEditState.Ask)
		{
			var editState = textBuffer.TryEdit();
			tryEditState = editState ? TryEditState.Yes : TryEditState.No;
			return editState;
		}
		return tryEditState == TryEditState.Yes;
	}
	
	[SerializeField]
	private List<float> yLineOffsets;
	public float GetLineOffset(int index)
	{
		if (!wordWrapping /*|| !CanEdit()*/)
			return charSize.y * index;
		if (index <= 0)
			return 0f;

		if (yLineOffsets == null || yLineOffsets.Count != textBuffer.lines.Count)
		{
			yLineOffsets = new List<float>(textBuffer.lines.Count);
			float yOffset = 0f;
			for (int i = 0; i < textBuffer.lines.Count; ++i)
			{
				yOffset += charSize.y * (GetSoftLineBreaks(i).Count + 1);
				yLineOffsets.Add(yOffset);
			}
		}

		//if (index <= 0 || index > yLineOffsets.Count)
		//	Debug.Log(index + "/" + yLineOffsets.Count);
		if (index > yLineOffsets.Count)
			return yLineOffsets.Count > 0 ? yLineOffsets[yLineOffsets.Count - 1] : 0;
		return yLineOffsets[index - 1];
	}

	public int GetLineAt(float yOffset)
	{
		if (!wordWrapping /*|| !CanEdit()*/ || textBuffer.lines.Count <= 1)
			return Mathf.Min((int) (yOffset / charSize.y), textBuffer.lines.Count - 1);

		GetLineOffset(textBuffer.lines.Count);

		int line = FindFirstIndexGreaterThanOrEqualTo(yLineOffsets, yOffset + 1f);
		//if (line >= 0 && line < yLineOffsets.Count && yOffset != yLineOffsets[line])
		//	++line;
		return line;
	}
	
	public void FocusCodeView()
	{
		caretMoveTime = 0f;
		focusCodeView = true;
		Repaint();
	}

	public void OnEnable(UnityEngine.Object targetFile)
	{
		if (selectionStartPosition != null && selectionStartPosition.line == -1)
			selectionStartPosition = null;

		if (string.IsNullOrEmpty(currentFont))
		{
			currentFont = SISettings.editorFont;
			if (currentFont == null)
				currentFont = Array.Find(availableFonts, x => x.Contains("SourceCodePro"));
			if (currentFont == null)
				currentFont = availableFonts[0];
			if (currentFont == "VeraMono")
				currentFont = availableFonts[0];
		}

		isTextAsset = !(targetFile is MonoScript) && !(targetFile is Shader);
		
		int themeIndex = Array.IndexOf(availableThemes, SISettings.themeNameCode);
		if (isTextAsset && !string.IsNullOrEmpty(SISettings.themeNameText))
			themeIndex = Array.IndexOf(availableThemes, SISettings.themeNameText);
		if (themeIndex == -1)
		{
			if (EditorGUIUtility.isProSkin)
				themeIndex = Array.IndexOf(availableThemes, "Darcula");
			else
				themeIndex = Array.IndexOf(availableThemes, "Xcode");
		}
		if (isTextAsset)
			currentThemeText = themes[themeIndex];
		else
			currentThemeCode = themes[themeIndex];

		styles = isTextAsset ? stylesText : stylesCode;

		if (textBuffer == null)
		{
			try
			{
				textBuffer = FGTextBuffer.GetBuffer(targetFile);
			}
			catch (Exception e)
			{
				Debug.LogError("Exception while trying to get buffer!!!\n" + e);
				return;
			}
		}
		textBuffer.styles = styles;
		Initialize();
		textBuffer.Initialize();

		//caretMoveTime = Time.realtimeSinceStartup;

		EditorApplication.update -= OnUpdate;
		EditorApplication.update += OnUpdate;

		textBuffer.onChange -= Repaint;
		textBuffer.onChange += Repaint;

		textBuffer.onLineFormatted -= OnLineFormatted;
		textBuffer.onLineFormatted += OnLineFormatted;

		textBuffer.onInsertedLines -= OnInsertedLines;
		textBuffer.onInsertedLines += OnInsertedLines;

		textBuffer.onRemovedLines -= OnRemovedLines;
		textBuffer.onRemovedLines += OnRemovedLines;

		textBuffer.AddEditor(this);
		
		EditorApplication.update -= SearchOnLoaded;
		EditorApplication.update += SearchOnLoaded;

		Repaint();
	}
	
	private void SearchOnLoaded()
	{
		if (CanEdit())
		{
			EditorApplication.update -= SearchOnLoaded;
			
			var s = defaultSearchString;
			SetSearchText(searchString);
			defaultSearchString = s;
		}
	}

	private void OnLineFormatted(int line)
	{
		if (_softLineBreaks != null && line < _softLineBreaks.Count)
			_softLineBreaks[line] = null;
	}

	private void OnInsertedLines(int lineIndex, int numLines)
	{
		if (_softLineBreaks != null && lineIndex <= _softLineBreaks.Count)
		{
			_softLineBreaks.InsertRange(lineIndex, new List<int>[numLines]);
		}
		if (yLineOffsets != null && lineIndex < yLineOffsets.Count)
		{
			yLineOffsets.RemoveRange(lineIndex, yLineOffsets.Count - lineIndex);
		}
		if (lineIndex < scrollPositionLine)
		{
			scrollPositionOffset = 0f;
			scrollPositionLine += numLines;
		}
	}

	private void OnRemovedLines(int lineIndex, int numLines)
	{
		if (_softLineBreaks != null && lineIndex < _softLineBreaks.Count)
		{
			_softLineBreaks.RemoveRange(lineIndex, Math.Min(numLines, _softLineBreaks.Count - lineIndex));
		}
		if (yLineOffsets != null && lineIndex < yLineOffsets.Count)
		{
			yLineOffsets.RemoveRange(lineIndex, yLineOffsets.Count - lineIndex);
		}
		if (lineIndex < scrollPositionLine)
		{
			scrollPositionOffset = 0f;
			if (lineIndex + numLines <= scrollPositionLine)
				scrollPositionLine -= numLines;
			else
				scrollPositionLine = lineIndex;
		}
	}
	
	private void SaveBuffer()
	{
		CloseAllPopups();
			
		if (CanEdit())
		{
			if (IsModified)
			{
				if (!textBuffer.Save())
					return;
				
				if (isTextAsset || textBuffer.isShader)
				{
					AssetDatabase.ImportAsset(targetPath);
				}
				else
				{
					FGTextBufferManager.AddPendingAssetImport(textBuffer.guid);

					if (SISettings.compileOnSave && !SISettings.autoReloadAssemblies)
					{
						//Debug.Log("... Will compile in background");
						holdingAssemblies = 1;
						compilers.Clear();
						EditorApplication.update += HoldReloadingAssemblies;
					}
					if (SISettings.compileOnSave)
						FGTextBufferManager.ImportPendingAssets();
				}
			}
			else
			{
				MenuReloadAssemblies();
				return;
			}
						
			RepaintAllInstances();
			//textBuffer.UpdateViews();
		}
	}
	
	//[MenuItem("Tools/Script Inspector/Reload Assemblies %r", true)]
	//static bool ValidateReloadAssemblies()
	//{
	//	return holdingAssemblies > 0;
	//}
	
	//[MenuItem("Tools/Script Inspector/Reload Assemblies %r")]
	static void MenuReloadAssemblies()
	{
		FGTextBufferManager.SaveAllModified(false);
		if (!EditorApplication.isCompiling && !FGTextBufferManager.IsReloadingAssemblies)
			return;
		
		//if (!SISettings.compileOnSave)
		//	FGTextBufferManager.ImportPendingAssets();
		
		if (holdingAssemblies > 0)
		{
			//Debug.Log("Releasing hold of reload");
			holdingAssemblies = 0;
			EditorApplication.update -= HoldReloadingAssemblies;
			EditorApplication.update += ReloadAssemblies;
			EditorApplication.UnlockReloadAssemblies();
		}
		else if (!FGTextBufferManager.IsReloadingAssemblies)
		{
			//Debug.Log("Reloading is false...");
			if (SISettings.compileOnSave && !SISettings.autoReloadAssemblies)
			{
				//Debug.Log("... Will compile in background");
				holdingAssemblies = 1;
				compilers.Clear();
				EditorApplication.update += HoldReloadingAssemblies;
			}
			if (SISettings.compileOnSave)
				FGTextBufferManager.ImportPendingAssets();
		}
		else
		{
			//Debug.Log("Ignoring...");
		}
	}
	
	static int holdingAssemblies;
	static HashSet<System.Diagnostics.Process> compilers = new HashSet<System.Diagnostics.Process>();
	static void HoldReloadingAssemblies()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
		{
			EditorApplication.update -= HoldReloadingAssemblies;
			EditorApplication.update += ReloadAssemblies;
			holdingAssemblies = 10;
		}
		else
		{
			var compiling = false;
			var errors = false;
			var processes = System.Diagnostics.Process.GetProcesses();
			foreach (var p in processes)
				try {
					if (p.ProcessName == "mono")
					{
						compilers.Add(p);
						if (p.ExitCode != 0)
						{
							errors = true;
							break;
						}
						else
						{
							compiling = true;
						}
					}
				} catch {}
//				where Array.IndexOf(
//					compilerFileNames,
//					Path.GetFileNameWithoutExtension(p.StartInfo.FileName).ToLowerInvariant()
//				) >= 0
			//Debug.Log(processes.Length + " : " + compilers.Count());
			compilers.RemoveWhere(x => x.HasExited && x.ExitCode == 0);
			errors = errors || compilers.Any(x => x.HasExited && x.ExitCode != 0);
			if (errors)
			{
				//Debug.Log("ERRORS!");
				EditorApplication.update -= HoldReloadingAssemblies;
				EditorApplication.update += ReloadAssemblies;
				return;
			}

			if (compiling || holdingAssemblies < 10)
			{
				if (!compiling)
					++holdingAssemblies;
				EditorApplication.LockReloadAssemblies();
			}
			else if (!compiling && compilers.Count == 0)
			{
				holdingAssemblies = 10;
				EditorApplication.update -= HoldReloadingAssemblies;
				RepaintAllInstances();
			}
		}
	}
	
	static bool progressBarShown;
	static EditorWindow restoreFocusToWnd;
	static void ReloadAssemblies()
	{
		holdingAssemblies = 0;
		compilers.Clear();
		for (var i = 0; i < 10; ++i)
			EditorApplication.UnlockReloadAssemblies();

		if (!progressBarShown ||
			EditorApplication.isCompiling && !EditorApplication.isUpdating)
		{
			if (!progressBarShown)
			{
				restoreFocusToWnd = EditorWindow.focusedWindow;
				
				progressBarShown = true;
				EditorUtility.DisplayProgressBar("Script Inspector", "Reloading assemblies...", 0f);
				AppDomain.CurrentDomain.DomainUnload -= HideProgressBarOnUnload;
				AppDomain.CurrentDomain.DomainUnload += HideProgressBarOnUnload;
				
				return;
			}
		}
		
		EditorApplication.update -= ReloadAssemblies;
		
		progressBarShown = false;
		EditorUtility.ClearProgressBar();
		
		if (restoreFocusToWnd)
			restoreFocusToWnd.Focus();
		restoreFocusToWnd = null;
		
		RepaintAllInstances();
	}
	
	static void HideProgressBarOnUnload(object sender, EventArgs args)
	{
		EditorUtility.ClearProgressBar();
	}

	private void CheckFocusDelayed()
	{
		EditorApplication.update -= CheckFocusDelayed;

		if (EditorWindow.focusedWindow == OwnerWindow)
			return;

		if (EditorWindow.focusedWindow == autocompleteWindow)
		{
//			if (parentWindow != null)
//			{
//				parentWindow.Focus();
//			}
//			else
//			{
//				var inspectorWnd = GetFocusedInspector();
//				if (inspectorWnd != null)
//					inspectorWnd.Focus();
//			}
			return;
		}
		
		if (EditorWindow.focusedWindow == argumentsHint)
		{
			CloseAllPopups();
			FocusCodeView();
			OwnerWindow.Focus();
			return;
		}

		Input.imeCompositionMode = IMECompositionMode.Auto;
		
		CloseAllPopups();
		FullRefresh();
	}

	private void FullRefresh()
	{
	//	if (textBuffer != null && textBuffer.Parser != null)
	//		textBuffer.Parser.FullRefresh();
	}
	
	private static bool addRecentLocationOnFocusLost = true;
	
	public void OnLostFocus()
	{
		if (addRecentLocationOnFocusLost)
			AddRecentLocation(0, true);
		addRecentLocationOnFocusLost = true;
		
		if (tokenTooltip != null || autocompleteWindow != null || argumentsHint != null)
		{
			if (tokenTooltip != null)
			{
				tokenTooltip.Hide();
				tokenTooltip = null;
			}
			
			EditorApplication.update += CheckFocusDelayed;
		}
		else
		{
			Input.imeCompositionMode = IMECompositionMode.Auto;
			
			if (CanEdit())
				FullRefresh();
		}
	}

	public void OnDisable()
	{
		if (autocompleteWindow != null)
			CloseAutocomplete();

		if (tokenTooltip != null)
			tokenTooltip.Hide();
		
		if (argumentsHint != null)
			CloseArgumentsHint();

		EditorApplication.update -= OnUpdate;
		EditorApplication.update -= SearchOnLoaded;

		if (textBuffer != null)
		{
			textBuffer.RemoveEditor(this);
			textBuffer.onChange -= Repaint;
			textBuffer.onLineFormatted -= OnLineFormatted;
			textBuffer.onInsertedLines -= OnInsertedLines;
			textBuffer.onRemovedLines -= OnRemovedLines;
		}
		
		if (FGTextBuffer.activeEditor == this)
		{
			AddRecentLocation(1, false);
			FGTextBuffer.activeEditor = null;
		}
	}

	public delegate void NotificationDelegate();
	public NotificationDelegate onRepaint;
	//public NotificationDelegate onChange;

	private void Repaint()
	{
		if (onRepaint != null)
			onRepaint();
	}

	//private void NotifyChange()
	//{
	//    if (onChange != null)
	//        onChange();
	//}

	public void OnUpdate()
	{
		float time = Time.realtimeSinceStartup;

		if (autoScrolling != Vector2.zero || autoScrollLeft || autoScrollRight || autoScrollUp || autoScrollDown)
		{
			float deltaTime = time - lastAutoScrollTime;

			if (!autoScrollLeft && !autoScrollRight)
			{
				autoScrolling.x = autoScrolling.x * 0.9f;
				if (autoScrolling.x != 0f)
					autoScrolling.x = autoScrolling.x > 0f ? Mathf.Max(0f, autoScrolling.x - 50f * deltaTime) : Mathf.Min(0f, autoScrolling.x + 50f * deltaTime);
			}
			else
			{
				autoScrolling.x = Mathf.Clamp(autoScrolling.x + (autoScrollLeft ? -500f : 500f) * deltaTime, -2000f, 2000f);
			}
			if (!autoScrollUp && !autoScrollDown)
			{
				autoScrolling.y = autoScrolling.y * 0.9f;
				if (autoScrolling.y != 0f)
					autoScrolling.y = autoScrolling.y > 0f ? Mathf.Max(0f, autoScrolling.y - 50f * deltaTime) : Mathf.Min(0f, autoScrolling.y + 50f * deltaTime);
			}
			else
			{
				autoScrolling.y = Mathf.Clamp(autoScrolling.y + (autoScrollUp ? -500f : 500f) * deltaTime, -2000f, 2000f);
			}

			autoScrollDelta = autoScrolling * deltaTime;
			if (lastMouseEvent != null)
			{
				simulateLastMouseEvent = codeViewDragging && !mouseDownOnSelection;
			}
			lastAutoScrollTime = time;
			if (EditorWindow.focusedWindow == OwnerWindow)
				EditorWindow.focusedWindow.wantsMouseMove = true;
			Repaint();
		}
		else if (hasCodeViewFocus)
		{
			lastAutoScrollTime = time;

			float caretTime = (time - caretMoveTime) % 1f;

			if (!hasSelection && highightReferencesTime != caretMoveTime)
			{
				int lineIndex, tokenIndex;
				bool atTokenEnd;
				var token = textBuffer.GetTokenAt(caretPosition, out lineIndex, out tokenIndex, out atTokenEnd);
				if (token != null)
				{
					if (atTokenEnd && token.tokenKind != SyntaxToken.Kind.Identifier &&
						token.tokenKind != SyntaxToken.Kind.Keyword &&
						token.tokenKind != SyntaxToken.Kind.ContextualKeyword &&
						token.tokenKind != SyntaxToken.Kind.PreprocessorSymbol)
					{
						var tokens = textBuffer.formatedLines[lineIndex].tokens;
						if (tokenIndex < tokens.Count - 1)
							token = tokens[tokenIndex + 1];
					}
				}

				if (time - caretMoveTime >= 0.35f)
				{
					if (token != null)
					{
						highightReferencesTime = caretMoveTime;

						if (token.parent != null && token.parent.resolvedSymbol != null &&
							(token.tokenKind == SyntaxToken.Kind.Identifier ||
								token.tokenKind == SyntaxToken.Kind.ContextualKeyword ||
								token.tokenKind == SyntaxToken.Kind.Keyword) &&
							token.parent.resolvedSymbol.kind != SymbolKind.Error)
						{
							if (highlightedSymbol != token.parent.resolvedSymbol)
							{
								highlightedSymbol = token.parent.resolvedSymbol.GetGenericSymbol();
								highlightedPPSymbol = null;
								Repaint();
								return;
							}
						}
						else if (token.tokenKind == SyntaxToken.Kind.PreprocessorSymbol)
						{
							highlightedSymbol = null;
							highlightedPPSymbol = token.text;
							Repaint();
							return;
						}
						else if (!SISettings.keepLastHighlight && (highlightedSymbol != null || highlightedPPSymbol != null))
						{
							highlightedSymbol = null;
							highlightedPPSymbol = null;
							Repaint();
							return;
						}
					}
					else if (!SISettings.keepLastHighlight && (highlightedSymbol != null || highlightedPPSymbol != null))
					{
						highlightedSymbol = null;
						highlightedPPSymbol = null;
						Repaint();
						return;
					}
				}
				else if (!SISettings.keepLastHighlight && highlightedSymbol != null)
				{
					if (token == null || token.parent == null || token.parent.resolvedSymbol == null
						|| token.parent.resolvedSymbol.kind == SymbolKind.Error
						|| (token.tokenKind != SyntaxToken.Kind.Identifier
							&& token.tokenKind != SyntaxToken.Kind.ContextualKeyword
							&& token.tokenKind != SyntaxToken.Kind.Keyword)
						|| (highlightedSymbol != token.parent.resolvedSymbol
							&& highlightedSymbol != token.parent.resolvedSymbol.GetGenericSymbol()))
					{
						highlightedSymbol = null;
						Repaint();
						return;
					}
				}
				else if (!SISettings.keepLastHighlight && highlightedPPSymbol != null)
				{
					if (token == null || token.tokenKind != SyntaxToken.Kind.PreprocessorSymbol)
					{
						highlightedPPSymbol = null;
						Repaint();
						return;
					}
				}
			}
			
			//if (GetFocusedInspector() != null)
			//	Debug.Log(GetFocusedInspector().title);
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow == OwnerWindow)
				EditorWindow.focusedWindow.wantsMouseMove = true;

			var shouldCaretBeVisible = caretTime < 0.5f;
			if (isCaretOn != shouldCaretBeVisible)
			{
				Repaint();
			}
		}

		if (showArgumentsHintForLeaf != null && hasCodeViewFocus)
		{
			ShowArgumentsHint(showArgumentsHintForLeaf);
			showArgumentsHintForLeaf = null;
		}
		else if (showArgumentsHintForLeaf != null)
		{
#if SI3_WARNINGS
			Debug.LogWarning(showArgumentsHintForLeaf);
#endif
		}
		else if (tokenTooltip == null && (hasCodeViewFocus || hasSearchBoxFocus))
		{
			if (mouseHoverToken != null && mouseHoverTime != 0 && time - mouseHoverTime > 0.25f)
			{
				if (mouseHoverToken.parent != null && EditorWindow.mouseOverWindow == OwnerWindow)
				{
					var resolvedSymbol = mouseHoverToken.parent.resolvedSymbol;
					if (resolvedSymbol == null ||
						!resolvedSymbol.IsValid() ||
						resolvedSymbol.kind == SymbolKind.Error)
					{
						mouseHoverToken.parent.resolvedSymbol = null; 
						FGResolver.ResolveNode(mouseHoverToken.parent.parent);
					}
					if (mouseHoverToken.parent.resolvedSymbol != null)
					{
						tokenTooltip = FGTooltip.Create(this, mouseHoverTokenRect, mouseHoverToken.parent);
					}
					else if (mouseHoverToken.parent.syntaxError != null || mouseHoverToken.parent.semanticError != null)
					{
						tokenTooltip = FGTooltip.Create(this, mouseHoverTokenRect, mouseHoverToken.parent);
					}
				}
				else
				{
					mouseHoverToken = null;
				}
			}
		}
		else if (mouseHoverTime == 0f && tokenTooltip != null)
		{
			tokenTooltip.Hide();
		}
	}

	private static string editorResourcesPath;

	public static T LoadEditorResource<T>(string indieAndProName) where T : UnityEngine.Object
	{
		return LoadEditorResource<T>(indieAndProName, null);
	}
	
	public static T LoadEditorResource<T>(string indieName, string proName) where T : UnityEngine.Object
	{
		if (editorResourcesPath == null)
		{
			MonoScript managerScript = MonoScript.FromScriptableObject(FGTextBufferManager.instance);
			editorResourcesPath = AssetDatabase.GetAssetPath(managerScript);
			if (string.IsNullOrEmpty(editorResourcesPath))
				editorResourcesPath = "Assets/Plugins/Editor/ScriptInspector3/Scripts/FGTextEditor.cs";
			editorResourcesPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(editorResourcesPath));
			editorResourcesPath = System.IO.Path.Combine(editorResourcesPath, "EditorResources");
		}
		
		string fileName = proName == null ? indieName : EditorGUIUtility.isProSkin ? proName : indieName;
		string path = System.IO.Path.Combine(editorResourcesPath, fileName);
		return AssetDatabase.LoadMainAssetAtPath(path) as T;
	}

	// returns 0 for non-dynamic fonts
	private static int GetDynamicFontSize(Font font)
	{
		if (font == null)
			return 0;
		
		TrueTypeFontImporter fontImporter = TrueTypeFontImporter.GetAtPath(AssetDatabase.GetAssetPath(font)) as TrueTypeFontImporter;
		return fontImporter != null && fontImporter.fontTextureCase == FontTextureCase.Dynamic ? fontImporter.fontSize : 0;
	}

	public void Initialize()
	{
		Vector2 cs = styles.normalStyle != null && styles.normalStyle.font != null ? styles.normalStyle.CalcSize(new GUIContent("W")) : charSize;
		bool isText = textBuffer != null && textBuffer.isText;
		bool reset = isText ? resetTextFont : resetCodeFont;

		if (cs != charSize || styles.normalStyle == null || styles.normalStyle.font == null || undoIcon == null
			|| (SISettings.fontSizeDelta == 0) != (styles.normalStyle.fontSize == 0) || reset // || currentFontSizeDelta != appliedFontSizeDelta
			//|| styles.normalStyle.font.name != Path.GetFileNameWithoutExtension(currentFont)
			)
		{
			_softLineBreaks = null;
			yLineOffsets = null;

			if (isText)
				resetTextFont = false;
			else
				resetCodeFont = false;

			styles.scrollViewStyle = styles.scrollViewStyle ?? new GUIStyle(GUIStyle.none);
			styles.searchResultStyle = styles.searchResultStyle ?? new GUIStyle(GUIStyle.none);

			styles.normalStyle = styles.normalStyle ?? new GUIStyle(GUIStyle.none);
#if !UNITY_3_5
			styles.normalStyle.richText = false;
#endif
			var fontPath = SISettings.fontHinting ? currentFont : "Smooth " + currentFont;
			styles.normalStyle.font = LoadEditorResource<Font>(fontPath);
			for (int i = 0; styles.normalStyle.font == null && i < availableFonts.Length; ++i)
			{
				currentFont = availableFonts[i];
				fontPath = SISettings.fontHinting ? currentFont : "Smooth " + currentFont;
				styles.normalStyle.font = LoadEditorResource<Font>(fontPath);
			}

			int currentFontSize = GetDynamicFontSize(styles.normalStyle.font);
			bool isDynamicFont = currentFontSize != 0;
			if (SISettings.fontSizeDelta != 0)
				styles.normalStyle.fontSize = currentFontSize + SISettings.fontSizeDelta;
			else
				styles.normalStyle.fontSize = 0;
			
			cs = styles.normalStyle.font != null ? styles.normalStyle.CalcSize(new GUIContent("W")) : charSize;
			charSize = cs;

			styles.hyperlinkStyle = styles.hyperlinkStyle ?? new GUIStyle(styles.normalStyle);
			styles.mailtoStyle = styles.mailtoStyle ?? new GUIStyle(styles.hyperlinkStyle);
			styles.keywordStyle = styles.keywordStyle ?? new GUIStyle(styles.normalStyle);
			styles.constantStyle = styles.constantStyle ?? new GUIStyle(styles.normalStyle);
			styles.stringStyle = styles.stringStyle ?? new GUIStyle(styles.normalStyle);
			styles.builtInLiteralsStyle = styles.builtInLiteralsStyle ?? new GUIStyle(styles.keywordStyle);
			styles.operatorStyle = styles.operatorStyle ?? new GUIStyle(styles.normalStyle);
			styles.referenceTypeStyle = styles.referenceTypeStyle ?? new GUIStyle(styles.normalStyle);
			styles.valueTypeStyle = styles.valueTypeStyle ?? new GUIStyle(styles.normalStyle);
			styles.interfaceTypeStyle = styles.interfaceTypeStyle ?? new GUIStyle(styles.normalStyle);
			styles.enumTypeStyle = styles.enumTypeStyle ?? new GUIStyle(styles.normalStyle);
			styles.delegateTypeStyle = styles.delegateTypeStyle ?? new GUIStyle(styles.normalStyle);
			styles.builtInRefTypeStyle = styles.builtInRefTypeStyle ?? new GUIStyle(styles.normalStyle);
			styles.builtInValueTypeStyle = styles.builtInValueTypeStyle ?? new GUIStyle(styles.normalStyle);
			styles.namespaceStyle = styles.namespaceStyle ?? new GUIStyle(styles.normalStyle);
			styles.methodStyle = styles.methodStyle ?? new GUIStyle(styles.normalStyle);
			styles.fieldStyle = styles.fieldStyle ?? new GUIStyle(styles.normalStyle);
			styles.propertyStyle = styles.propertyStyle ?? new GUIStyle(styles.normalStyle);
			styles.eventStyle = styles.eventStyle ?? new GUIStyle(styles.normalStyle);
			styles.parameterStyle = styles.parameterStyle ?? new GUIStyle(styles.normalStyle);
			styles.variableStyle = styles.variableStyle?? new GUIStyle(styles.normalStyle);
			styles.typeParameterStyle = styles.typeParameterStyle ?? new GUIStyle(styles.normalStyle);
			styles.enumMemberStyle = styles.enumMemberStyle ?? new GUIStyle(styles.normalStyle);
			styles.preprocessorStyle = styles.preprocessorStyle ?? new GUIStyle(styles.normalStyle);
			styles.defineSymbols = styles.defineSymbols ?? new GUIStyle(styles.normalStyle);
			styles.inactiveCodeStyle = styles.inactiveCodeStyle ?? new GUIStyle(styles.normalStyle);
			styles.commentStyle = styles.commentStyle ?? new GUIStyle(styles.normalStyle);
			styles.xmlDocsStyle = styles.xmlDocsStyle ?? new GUIStyle(styles.normalStyle);
			styles.xmlDocsTagsStyle = styles.xmlDocsTagsStyle ?? new GUIStyle(styles.normalStyle);
			styles.lineNumbersStyle = styles.lineNumbersStyle ?? new GUIStyle(styles.normalStyle);
			styles.tooltipTextStyle = styles.tooltipTextStyle ?? new GUIStyle(styles.normalStyle);

			styles.hyperlinkStyle.font = styles.normalStyle.font;
			styles.mailtoStyle.font = styles.normalStyle.font;
			styles.keywordStyle.font = styles.normalStyle.font;
			styles.constantStyle.font = styles.normalStyle.font;
			styles.stringStyle.font = styles.normalStyle.font;
			styles.builtInLiteralsStyle.font = styles.normalStyle.font;
			styles.operatorStyle.font = styles.normalStyle.font;
			styles.referenceTypeStyle.font = styles.normalStyle.font;
			styles.valueTypeStyle.font = styles.normalStyle.font;
			styles.interfaceTypeStyle.font = styles.normalStyle.font;
			styles.enumTypeStyle.font = styles.normalStyle.font;
			styles.delegateTypeStyle.font = styles.normalStyle.font;
			styles.builtInRefTypeStyle.font = styles.normalStyle.font;
			styles.builtInValueTypeStyle.font = styles.normalStyle.font;
			styles.namespaceStyle.font = styles.normalStyle.font;
			styles.methodStyle.font = styles.normalStyle.font;
			styles.fieldStyle.font = styles.normalStyle.font;
			styles.propertyStyle.font = styles.normalStyle.font;
			styles.eventStyle.font = styles.normalStyle.font;
			styles.parameterStyle.font = styles.normalStyle.font;
			styles.variableStyle.font = styles.normalStyle.font;
			styles.typeParameterStyle.font = styles.normalStyle.font;
			styles.enumMemberStyle.font = styles.normalStyle.font;
			styles.preprocessorStyle.font = styles.normalStyle.font;
			styles.defineSymbols.font = styles.normalStyle.font;
			styles.inactiveCodeStyle.font = styles.normalStyle.font;
			styles.commentStyle.font = styles.normalStyle.font;
			styles.xmlDocsStyle.font = styles.normalStyle.font;
			styles.xmlDocsTagsStyle.font = styles.normalStyle.font;
			styles.lineNumbersStyle.font = styles.normalStyle.font;
			styles.tooltipTextStyle.font = styles.normalStyle.font;
			styles.tooltipTextStyle.wordWrap = true;

			if (isDynamicFont)
			{
				styles.hyperlinkStyle.fontSize = styles.normalStyle.fontSize;
				styles.mailtoStyle.fontSize = styles.normalStyle.fontSize;
				styles.keywordStyle.fontSize = styles.normalStyle.fontSize;
				styles.constantStyle.fontSize = styles.normalStyle.fontSize;
				styles.stringStyle.fontSize = styles.normalStyle.fontSize;
				styles.builtInLiteralsStyle.fontSize = styles.normalStyle.fontSize;
				styles.operatorStyle.fontSize = styles.normalStyle.fontSize;
				styles.referenceTypeStyle.fontSize = styles.normalStyle.fontSize;
				styles.valueTypeStyle.fontSize = styles.normalStyle.fontSize;
				styles.interfaceTypeStyle.fontSize = styles.normalStyle.fontSize;
				styles.enumTypeStyle.fontSize = styles.normalStyle.fontSize;
				styles.delegateTypeStyle.fontSize = styles.normalStyle.fontSize;
				styles.builtInRefTypeStyle.fontSize = styles.normalStyle.fontSize;
				styles.builtInValueTypeStyle.fontSize = styles.normalStyle.fontSize;
				styles.namespaceStyle.fontSize = styles.normalStyle.fontSize;
				styles.methodStyle.fontSize = styles.normalStyle.fontSize;
				styles.fieldStyle.fontSize = styles.normalStyle.fontSize;
				styles.propertyStyle.fontSize = styles.normalStyle.fontSize;
				styles.eventStyle.fontSize = styles.normalStyle.fontSize;
				styles.parameterStyle.fontSize = styles.normalStyle.fontSize;
				styles.variableStyle.fontSize = styles.normalStyle.fontSize;
				styles.typeParameterStyle.fontSize = styles.normalStyle.fontSize;
				styles.enumMemberStyle.fontSize = styles.normalStyle.fontSize;
				styles.preprocessorStyle.fontSize = styles.normalStyle.fontSize;
				styles.defineSymbols.fontSize = styles.normalStyle.fontSize;
				styles.inactiveCodeStyle.fontSize = styles.normalStyle.fontSize;
				styles.commentStyle.fontSize = styles.normalStyle.fontSize;
				styles.xmlDocsStyle.fontSize = styles.normalStyle.fontSize;
				styles.xmlDocsTagsStyle.fontSize = styles.normalStyle.fontSize;
				styles.lineNumbersStyle.fontSize = styles.normalStyle.fontSize;
				styles.tooltipTextStyle.fontSize = styles.normalStyle.fontSize;
			}
			else
			{
				styles.normalStyle.fontSize = 0;
				styles.hyperlinkStyle.fontSize = 0;
				styles.mailtoStyle.fontSize = 0;
				styles.keywordStyle.fontSize = 0;
				styles.constantStyle.fontSize = 0;
				styles.stringStyle.fontSize = 0;
				styles.builtInLiteralsStyle.fontSize = 0;
				styles.operatorStyle.fontSize = 0;
				styles.referenceTypeStyle.fontSize = 0;
				styles.valueTypeStyle.fontSize = 0;
				styles.interfaceTypeStyle.fontSize = 0;
				styles.enumTypeStyle.fontSize = 0;
				styles.delegateTypeStyle.fontSize = 0;
				styles.builtInRefTypeStyle.fontSize = 0;
				styles.builtInValueTypeStyle.fontSize = 0;
				styles.namespaceStyle.fontSize = 0;
				styles.methodStyle.fontSize = 0;
				styles.fieldStyle.fontSize = 0;
				styles.propertyStyle.fontSize = 0;
				styles.eventStyle.fontSize = 0;
				styles.parameterStyle.fontSize = 0;
				styles.variableStyle.fontSize = 0;
				styles.typeParameterStyle.fontSize = 0;
				styles.enumMemberStyle.fontSize = 0;
				styles.preprocessorStyle.fontSize = 0;
				styles.defineSymbols.fontSize = 0;
				styles.inactiveCodeStyle.fontSize = 0;
				styles.commentStyle.fontSize = 0;
				styles.xmlDocsStyle.fontSize = 0;
				styles.xmlDocsTagsStyle.fontSize = 0;
				styles.lineNumbersStyle.fontSize = 0;
				styles.tooltipTextStyle.fontSize = 0;
			}

			styles.lineNumbersBackground = styles.lineNumbersBackground ?? new GUIStyle();
			styles.lineNumbersSeparator = styles.lineNumbersSeparator ?? new GUIStyle();
			styles.caretStyle = styles.caretStyle ?? new GUIStyle();
			styles.activeSelectionStyle = styles.activeSelectionStyle ?? new GUIStyle();
			styles.passiveSelectionStyle = styles.passiveSelectionStyle ?? new GUIStyle();
			styles.trackChangesAfterSaveStyle = styles.trackChangesAfterSaveStyle ?? new GUIStyle();
			styles.trackChangesBeforeSaveStyle = styles.trackChangesBeforeSaveStyle ?? new GUIStyle();
			styles.trackChangesRevertedStyle = styles.trackChangesRevertedStyle ?? new GUIStyle();
			styles.currentLineStyle = styles.currentLineStyle ?? new GUIStyle();
			styles.currentLineInactiveStyle = styles.currentLineInactiveStyle ?? new GUIStyle();
			styles.referenceHighlightStyle = styles.referenceHighlightStyle ?? new GUIStyle();
			styles.referenceModifyHighlightStyle = styles.referenceModifyHighlightStyle ?? new GUIStyle();
			styles.tooltipBgStyle = styles.tooltipBgStyle ?? new GUIStyle();
			styles.tooltipFrameStyle = styles.tooltipFrameStyle ?? new GUIStyle();
			
			styles.listBgStyle = styles.listBgStyle ?? new GUIStyle();
			
			styles.upArrowStyle = styles.upArrowStyle ?? new GUIStyle();
			styles.downArrowStyle = styles.downArrowStyle ?? new GUIStyle();
			styles.upArrowStyle.normal.background = LoadEditorResource<Texture2D>("upArrowOff.png", "d_upArrowOff.png");
			styles.upArrowStyle.hover.background = styles.upArrowStyle.active.background
				= LoadEditorResource<Texture2D>("upArrow.png", "d_upArrow.png");
			styles.downArrowStyle.normal.background = LoadEditorResource<Texture2D>("downArrowOff.png", "d_downArrowOff.png");
			styles.downArrowStyle.hover.background = styles.downArrowStyle.active.background
				= LoadEditorResource<Texture2D>("downArrow.png", "d_downArrow.png");

			wavyUnderline = LoadEditorResource<Texture2D>("wavyUnderline.png");

			wrenchIcon = LoadEditorResource<Texture2D>("l_wrench.png", "d_wrench.png");

			saveIcon = LoadEditorResource<Texture2D>("saveIconBW.png");
			buildIcon = LoadEditorResource<Texture2D>("buildIconBW.png");
			undoIcon = LoadEditorResource<Texture2D>("editUndoIconBW.png");
			redoIcon = LoadEditorResource<Texture2D>("editRedoIconBW.png");
			//hyperlinksIcon = LoadEditorResource<Texture2D>("hyperlinksIconBW.png");
			popOutIcon = LoadEditorResource<Texture2D>("popOutIconBW.png");

			styles.ping = styles.ping ?? new GUIStyle();
#if !UNITY_3_5
			styles.ping.richText = false;
#endif
			styles.ping.normal.background = LoadEditorResource<Texture2D>("yellowPing.png");
			styles.ping.normal.textColor = Color.black;
			styles.ping.font = styles.normalStyle.font;
			if (isDynamicFont)
				styles.ping.fontSize = styles.normalStyle.fontSize;
			else
				styles.ping.fontSize = 0;
			styles.ping.border = new RectOffset(10, 10, 10, 10);
			styles.ping.overflow = new RectOffset(7, 7, 6, 6);
			styles.ping.stretchWidth = false;
			styles.ping.stretchHeight = false;

			ApplyTheme(styles, isText ? currentThemeText : currentThemeCode);
		}
	}

	private static void ApplyTheme(Styles styles, Theme currentTheme)
	{
		styles.scrollViewStyle.normal.background = FlatColorTexture(currentTheme.background);
		styles.searchResultStyle.normal.background = FlatColorTexture(currentTheme.searchResults);
		styles.caretStyle.normal.background = FlatColorTexture(currentTheme.text);
		styles.activeSelectionStyle.normal.background = FlatColorTexture(currentTheme.activeSelection);
		styles.passiveSelectionStyle.normal.background = FlatColorTexture(currentTheme.passiveSelection);
		styles.trackChangesBeforeSaveStyle.normal.background = FlatColorTexture(currentTheme.trackChanged);
		styles.trackChangesAfterSaveStyle.normal.background = FlatColorTexture(currentTheme.trackSaved);
		styles.trackChangesRevertedStyle.normal.background = FlatColorTexture(currentTheme.trackReverted);
		styles.currentLineStyle.normal.background = FlatColorTexture(currentTheme.currentLine);
		styles.currentLineInactiveStyle.normal.background = FlatColorTexture(currentTheme.currentLineInactive);
		styles.referenceHighlightStyle.normal.background = FlatColorTexture(currentTheme.referenceHighlight);
		styles.referenceModifyHighlightStyle.normal.background = FlatColorTexture(currentTheme.referenceModifyHighlight);
		styles.tooltipBgStyle.normal.background = FlatColorTexture(currentTheme.tooltipBackground);
		styles.tooltipFrameStyle.normal.background = FlatColorTexture(currentTheme.tooltipFrame);
		
		styles.listBgStyle.normal.background = FlatColorTexture(currentTheme.listPopupBackground);
		
		styles.normalStyle.normal.textColor = currentTheme.text;
		styles.keywordStyle.normal.textColor = currentTheme.keywords;
		styles.constantStyle.normal.textColor = currentTheme.constants;
		styles.stringStyle.normal.textColor = currentTheme.strings;
		styles.builtInLiteralsStyle.normal.textColor = currentTheme.builtInLiterals;
		styles.operatorStyle.normal.textColor = currentTheme.operators;
		styles.referenceTypeStyle.normal.textColor = currentTheme.referenceTypes;
		styles.valueTypeStyle.normal.textColor = currentTheme.valueTypes;
		styles.interfaceTypeStyle.normal.textColor = currentTheme.interfaceTypes;
		styles.enumTypeStyle.normal.textColor = currentTheme.enumTypes;
		styles.delegateTypeStyle.normal.textColor = currentTheme.delegateTypes;
		styles.builtInRefTypeStyle.normal.textColor = currentTheme.builtInTypes.a > 0f ? currentTheme.builtInTypes : currentTheme.referenceTypes;
		styles.builtInValueTypeStyle.normal.textColor = currentTheme.builtInTypes.a > 0f ? currentTheme.builtInTypes : currentTheme.valueTypes;
		styles.namespaceStyle.normal.textColor = currentTheme.namespaces;
		styles.methodStyle.normal.textColor = currentTheme.methods;
		styles.fieldStyle.normal.textColor = currentTheme.fields;
		styles.propertyStyle.normal.textColor = currentTheme.properties;
		styles.eventStyle.normal.textColor = currentTheme.events;
		styles.parameterStyle.normal.textColor = currentTheme.parameters;
		styles.variableStyle.normal.textColor = currentTheme.variables;
		styles.typeParameterStyle.normal.textColor = currentTheme.typeParameters;
		styles.enumMemberStyle.normal.textColor = currentTheme.enumMembers.a != 0f ? currentTheme.enumMembers : currentTheme.text;
		styles.preprocessorStyle.normal.textColor = currentTheme.preprocessor;
		styles.defineSymbols.normal.textColor = currentTheme.defineSymbols;
		styles.inactiveCodeStyle.normal.textColor = currentTheme.inactiveCode;
		styles.commentStyle.normal.textColor = currentTheme.comments;
		styles.xmlDocsStyle.normal.textColor = currentTheme.xmlDocs;
		styles.xmlDocsTagsStyle.normal.textColor = currentTheme.xmlDocsTags;
		styles.tooltipTextStyle.normal.textColor = SISettings.useStandardColorInPopups ? currentTheme.text : currentTheme.tooltipText;

		styles.hyperlinkStyle.normal.textColor = currentTheme.hyperlinks;
		styles.mailtoStyle.normal.textColor = currentTheme.hyperlinks;
		styles.hyperlinkStyle.normal.background = styles.mailtoStyle.normal.background =
			UnderlineTexture(currentTheme.hyperlinks, (int)styles.mailtoStyle.lineHeight);

		styles.lineNumbersBackground.normal.background = FlatColorTexture(currentTheme.lineNumbersBackground);
		styles.lineNumbersSeparator.normal.background = FlatColorTexture(currentTheme.fold);

		styles.lineNumbersStyle.normal.textColor = currentTheme.lineNumbers;
		styles.lineNumbersStyle.hover.textColor = currentTheme.lineNumbersHighlight;
		styles.lineNumbersStyle.hover.background = styles.lineNumbersBackground.normal.background;
		styles.lineNumbersStyle.alignment = TextAnchor.UpperRight;

		bool isDynamic = GetDynamicFontSize(styles.normalStyle.font) != 0;
		int boldFilter = currentFont == "Fonts/DejaVu Sans Mono.ttf" ? 3 : 2;
		styles.commentStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.commentsStyle & boldFilter) : 0;
		styles.stringStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.stringsStyle & boldFilter) : 0;
		styles.keywordStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.keywordsStyle & boldFilter) : 0;
		styles.constantStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.constantsStyle & boldFilter) : 0;
		styles.referenceTypeStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.typesStyle & boldFilter) : 0;
		styles.valueTypeStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.typesStyle & boldFilter) : 0;
		styles.interfaceTypeStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.typesStyle & boldFilter) : 0;
		styles.enumTypeStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.typesStyle & boldFilter) : 0;
		styles.delegateTypeStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.typesStyle & boldFilter) : 0;
		styles.builtInRefTypeStyle.fontStyle = isDynamic ? (FontStyle)((int)(currentTheme.builtInTypes == Color.clear ? currentTheme.typesStyle : currentTheme.keywordsStyle) & boldFilter) : 0;
		styles.builtInValueTypeStyle.fontStyle = isDynamic ? (FontStyle)((int)(currentTheme.builtInTypes == Color.clear ? currentTheme.typesStyle : currentTheme.keywordsStyle) & boldFilter) : 0;
		styles.namespaceStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.namespacesStyle & boldFilter) : 0;
		styles.methodStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.methodsStyle & boldFilter) : 0;
		styles.fieldStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.fieldsStyle & boldFilter) : 0;
		styles.propertyStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.propertiesStyle & boldFilter) : 0;
		styles.eventStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.eventsStyle & boldFilter) : 0;
		styles.hyperlinkStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.hyperlinksStyle & boldFilter) : 0;
		styles.mailtoStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.hyperlinksStyle & boldFilter) : 0;
		styles.preprocessorStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.preprocessorStyle & boldFilter) : 0;
		styles.defineSymbols.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.preprocessorStyle & boldFilter) : 0;
		styles.inactiveCodeStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.inactiveCodeStyle & boldFilter) : 0;
		styles.parameterStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.parametersStyle & boldFilter) : 0;
		styles.variableStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.variablesStyle & boldFilter) : 0;
		styles.typeParameterStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.typeParametersStyle & boldFilter) : 0;
		styles.enumMemberStyle.fontStyle = isDynamic ? (FontStyle)((int)currentTheme.enumMembersStyle & boldFilter) : 0;
	}

	private static Texture2D FlatColorTexture(Color color)
	{
		Texture2D flat = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		flat.SetPixels(new Color[] { color });
		flat.Apply();
		flat.hideFlags = HideFlags.HideAndDontSave;
		return flat;
	}

	private static Texture2D UnderlineTexture(Color color, int lineHeight)
	{
		return CreateUnderlineTexture(color, lineHeight, Color.clear);
	}

	private static Texture2D CreateUnderlineTexture(Color color, int lineHeight, Color bgColor)
	{
		Texture2D underlined = new Texture2D(1, lineHeight, TextureFormat.RGBA32, false);
		underlined.SetPixel(0, 0, color);
		for (int i = 1; i < lineHeight; ++i)
			underlined.SetPixel(0, i, new Color32(0, 0, 0, 0));
		underlined.Apply();
		underlined.hideFlags = HideFlags.HideAndDontSave;
		return underlined;
	}

	private EditorWindow currentInspector;
	
	public EditorWindow OwnerWindow { get { return parentWindow ?? currentInspector; } }
	
	private bool helpButtonClicked = false;

	public void OnWindowGUI(EditorWindow window, RectOffset margins)
	{
		if (window != currentInspector)
			parentWindow = window;
		if (EditorWindow.focusedWindow == window)
			FGTextBuffer.activeEditor = this;

		if (Event.current.type != EventType.layout)
		{
			scrollViewRect = GUILayoutUtility.GetRect(1f, Screen.width, 1f, Screen.height);
			if (!(window is FGCodeWindow))
			{
				scrollViewRect.xMin = 0;
			//	scrollViewRect.xMax = Screen.width - 1;
			}
			scrollViewRect = margins.Remove(scrollViewRect);
		}
		else
		{
			GUILayoutUtility.GetRect(1f, Screen.width, 112f, Screen.height);
		}
		
		if (Screen.width < 10f || Screen.height < 10f || scrollViewRect.width < 10f || scrollViewRect.height < 10f)
		{
			return;
		}

		bool enabled = GUI.enabled;
		GUI.enabled = CanEdit();

		if (textBuffer != null)
		{
			Rect rc = new Rect(scrollViewRect.xMax - 21f, scrollViewRect.yMin - 17f, 18f, 16f);
			if (GUI.Button(rc, GUIContent.none, EditorStyles.toolbarButton))
			{
				GenericMenu codeViewPopupMenu = new GenericMenu();
				
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1
				codeViewPopupMenu.AddItem(new GUIContent("Always open in Script Inspector"), SISettings.handleOpenAssets, ToggleHandleOpenAsset);
				if (SISettings.handleOpenAssets)
				{
					codeViewPopupMenu.AddDisabledItem(new GUIContent("Open on Double-Click/Scripts"));
					codeViewPopupMenu.AddDisabledItem(new GUIContent("Open on Double-Click/Shaders"));
					codeViewPopupMenu.AddDisabledItem(new GUIContent("Open on Double-Click/Text Assets"));
				}
				else
#endif
				{
					codeViewPopupMenu.AddItem(new GUIContent("Open on Double-Click/Scripts"), SISettings.handleOpeningScripts, ToggleHandleOpenFromProject);
					codeViewPopupMenu.AddItem(new GUIContent("Open on Double-Click/Shaders"), SISettings.handleOpeningShaders, ToggleHandleOpenShadersFromProject);
					codeViewPopupMenu.AddItem(new GUIContent("Open on Double-Click/Text Assets"), SISettings.handleOpeningText, ToggleHandleOpenTextsFromProject);
				}
				
				bool isInspector = parentWindow == null;
				codeViewPopupMenu.AddSeparator(string.Empty);
				if (textBuffer.isText)
				{
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Word Wrap (Text)"),
						isInspector ? SISettings.wordWrapTextInspector : SISettings.wordWrapText, ToggleWordWrapText);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Highlight Current Line"), SISettings.highlightCurrentLine, ToggleHighlightCurrentLine);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Frame Current Line"), SISettings.frameCurrentLine, ToggleFrameCurrentLine);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Line Numbers (Text)"),
						isInspector ? SISettings.showLineNumbersTextInspector : SISettings.showLineNumbersText, ToggleLineNumbersText);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Track Changes (Text)"),
						isInspector ? SISettings.trackChangesTextInspector : SISettings.trackChangesText, ToggleTrackChangesText);
				}
				else
				{
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Word Wrap (Code)"),
						isInspector ? SISettings.wordWrapCodeInspector : SISettings.wordWrapCode, ToggleWordWrapCode);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Highlight Current Line"), SISettings.highlightCurrentLine, ToggleHighlightCurrentLine);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Frame Current Line"), SISettings.frameCurrentLine, ToggleFrameCurrentLine);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Line Numbers (Code)"),
						isInspector ? SISettings.showLineNumbersCodeInspector : SISettings.showLineNumbersCode, ToggleLineNumbersCode);
					codeViewPopupMenu.AddItem(new GUIContent("View Options/Track Changes (Code)"),
						isInspector ? SISettings.trackChangesCodeInspector : SISettings.trackChangesCode, ToggleTrackChangesCode);
				}
	
				//codeViewPopupMenu.AddSeparator(string.Empty);
				for (int i = 0; i < availableFonts.Length; ++i)
					codeViewPopupMenu.AddItem(new GUIContent("Font/" + Path.GetFileNameWithoutExtension(availableFonts[i])), currentFont == availableFonts[i], x => SelectFont((int)x), i);
				codeViewPopupMenu.AddSeparator("Font//");
				codeViewPopupMenu.AddItem(new GUIContent("Font/Use Font Hinting"), SISettings.fontHinting, ToggleFontHinting);
				
				string[] sortedThemes = availableThemes.Clone() as string[];
				Array.Sort<string>(sortedThemes, StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < sortedThemes.Length; ++i)
				{
					int themeIndex = Array.IndexOf(availableThemes, sortedThemes[i]);
					if (textBuffer.isText)
						codeViewPopupMenu.AddItem(new GUIContent("Color Scheme (Text)/" + sortedThemes[i]), currentThemeText == themes[themeIndex], x => SelectTheme((int)x, textBuffer.isText), themeIndex);
					else
						codeViewPopupMenu.AddItem(new GUIContent("Color Scheme (Code)/" + sortedThemes[i]), currentThemeCode == themes[themeIndex], x => SelectTheme((int)x, textBuffer.isText), themeIndex);
				}
				
				codeViewPopupMenu.AddSeparator(string.Empty);
				codeViewPopupMenu.AddItem(new GUIContent("Preferences..."), false, SISettings.OpenSIPreferences);
				
				codeViewPopupMenu.AddSeparator(string.Empty);
				codeViewPopupMenu.AddItem(new GUIContent("About..."), false, About);
	
				codeViewPopupMenu.DropDown(rc);
				GUIUtility.ExitGUI();
			}
		}

		Color oldColor = GUI.color;
		if (!GUI.enabled && Event.current.type == EventType.Repaint)
		{
			GUI.color = new Color(0.85f, 0.85f, 0.85f);
			if (textBuffer != null)
				textBuffer.LoadFaster();
		}

		try
		{
			if (hasCodeViewFocus && GUI.enabled)
			{
				FGTextBuffer.activeEditor = this;
				
				textBuffer.BeginEdit("change");
				try
				{
					DoGUIWithAutocomplete(enabled);
				}
				finally
				{
					textBuffer.EndEdit();
				}
			}
			else
			{
				DoGUIWithAutocomplete(enabled);
			}
		}
		finally
		{
			GUI.color = oldColor;
			GUI.enabled = enabled;
		}
	}
	
	public void OnInspectorGUI(bool isScriptInspector, RectOffset margins, EditorWindow currentInspector)
	{
		this.currentInspector = currentInspector;
		
		if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp)
		{
			if (TabSwitcher.OnGUIGlobal())
				return;
		}
		
		if (!isScriptInspector)
		{
			OnWindowGUI(currentInspector, margins);
			return;
		}
		
		// Disabling the functionality of the default inspector's header help button
		// (located just below the cancel search button) by zeroing hotControl on
		// mouse down, which effectivly deactivates the button so it doesn't fire up
		// on mouse up. Detection is done by comparing hotControl with the next available
		// controlID - 2, which is super-hacky, but so far I haven't found any nicer way
		// of doing this.
		int nextControlID = GUIUtility.GetControlID(buttonHash, FocusType.Native, new Rect());
		if (GUIUtility.hotControl != 0)
		{
			//Debug.Log("hotControl: " + GUIUtility.hotControl + "  nextControlID: " + nextControlID + "  Event: " + Event.current);
			if (GUIUtility.hotControl == nextControlID - 2)
			{
				GUIUtility.hotControl = 0;
				helpButtonClicked = true;
			//	Repaint();
			}
		}

		if (Event.current.type != EventType.layout)
		{
			scrollViewRect = GUILayoutUtility.GetRect(1f, Screen.width, 1f, Screen.height);
			scrollViewRect.xMin = 0f;
			scrollViewRect.xMax = Screen.width - 1f;
			scrollViewRect.yMin -= 32f;
			scrollViewRect.yMax += 13f;
		}
		else
		{
			GUILayoutUtility.GetRect(1f, Screen.width, 1f, Screen.height);
		}

		bool enabled = GUI.enabled;
		//GUI.enabled = true;

		Color oldColor = GUI.color;
		if (!GUI.enabled && Event.current.type == EventType.Repaint)
		{
			GUI.color = new Color(0.85f, 0.85f, 0.85f);
			if (textBuffer != null)
				textBuffer.LoadFaster();
		}
		
		try
		{
			if (hasCodeViewFocus)
			{
				FGTextBuffer.activeEditor = this;
	
				textBuffer.BeginEdit("change");
				try
				{
					DoGUIWithAutocomplete(enabled);
				}
				finally
				{
					textBuffer.EndEdit();
				}
			}
			else
			{
				DoGUIWithAutocomplete(enabled);
			}
		}
		finally
		{
			GUI.color = oldColor;
			GUI.enabled = enabled;
		}
	}

	public Rect codeViewRect;
	private Rect fullCodeViewRect;
	private static int codeViewControlID = Mathf.Abs("SiCodeView".GetHashCode());
	
	private Event lastMouseEvent;
	private bool simulateLastMouseEvent;

	private bool tryAutocomplete;
	private bool tryAutoSuggestion;
	private bool tryArgumentsHint;
	
	private string lastTypedText;

	private void DoGUIWithAutocomplete(bool enableGUI)
	{
		if (caretMoveTime == 0f)
		{
			caretMoveTime = Time.realtimeSinceStartup;
			lastCaretMoveWasSearch = false;
//			if (EditorWindow.focusedWindow == parentWindow)
//				parentWindow.Focus();
		}
		
		tryEditState = TryEditState.Ask;

		if (tryAutocomplete)
		{
			Autocomplete(tryAutoSuggestion);
			tryAutoSuggestion = false;
			tryAutocomplete = false;
		}
		else
		{
			if (autocompleteWindow == null)
			{
				try
				{
					DoGUI(enableGUI);
				}
				catch (ExitGUIException) { }
			}
			else
			{
				FGTextBuffer.CaretPos caretPosBefore = caretPosition.Clone();
				try
				{
					DoGUI(enableGUI);
				}
				catch (ExitGUIException) { }

				if (autocompleteWindow != null)
				{
					if (!hasCodeViewFocus && EditorWindow.focusedWindow != autocompleteWindow)
					{
						//	Debug.Log("Closing. Focus lost...");
						CloseAutocomplete();
					}
					else if (caretPosition.line == caretPosBefore.line && caretPosition != caretPosBefore)
					{
						autocompleteWindow.UpdateTypedInPart();
					}
				}
			}
		}
		
		if (argumentsHint != null)
		{
			if (!hasCodeViewFocus && EditorWindow.focusedWindow != argumentsHint)
			{
				//	Debug.Log("Closing. Focus lost...");
				CloseArgumentsHint();
				Repaint();
			}
		}
		
		if (textBuffer == null)
			return;
			
		if (caretPosition != matchedBracesAtCaretPosition ||
			scrollToCaret || caretMoveTime == Time.realtimeSinceStartup)
		{
			if (showingArgumentsForMethod != null)
			{
				UpdateArgumentsHint(false);
			}
			
			if (autoClosingStack.Count > 0)
			{
				var caretPos = new TextPosition(caretPosition.line, caretPosition.characterIndex);
				for (var i = autoClosingStack.Count; i --> 0; )
				{
					var entry = autoClosingStack[i];
					if (caretPos <= entry.openingPos || caretPos > entry.closingPos)
						autoClosingStack.RemoveAt(i);
				}
				if (autoClosingStack.Count == 0)
				{
					textBuffer.onInsertedText -= OnInsertedText;
					textBuffer.onRemovedText -= OnRemovedText;
				}
			}
		}
		
		if (caretPosition != matchedBracesAtCaretPosition || textBuffer.undoPosition != matchedBracesAtUndoPosition)
		{
			if (selectionStartPosition == null && !IsLoading)
			{
				UpdateMatchingBraces();
				matchedBracesAtUndoPosition = textBuffer.undoPosition;
				matchedBracesAtCaretPosition = caretPosition.Clone();
			}
		}
	}
	
	private void UpdateMatchingBraces()
	{
		matchingBraceLeft = matchingBraceRight = new TextPosition();
		
		int lineRight, index;
		bool atTokenEnd;
		var tokenAtCursor = textBuffer.GetTokenAt(caretPosition, out lineRight, out index, out atTokenEnd);
		
		var formatedLines = textBuffer.formatedLines;
		if (lineRight >= formatedLines.Length)
			return;
		
		var lineLeft = lineRight;
		var tokens = formatedLines[lineLeft].tokens;
		if (tokens == null)
			return;
		
		if (tokenAtCursor != null && (atTokenEnd || caretPosition.characterIndex == 0) && tokenAtCursor.text.Length == 1 &&
			(tokenAtCursor.text[0] == '}' || tokenAtCursor.text[0] == ']' || tokenAtCursor.text[0] == ')'))
		{
			var nextToken = index + 1 < tokens.Count ? tokens[index + 1] : null;
			if (nextToken == null || nextToken.text.Length != 1 ||
				nextToken.text[0] != '}' && nextToken.text[0] != ']' && nextToken.text[0] != ')')
			{
				--index;
			}
		}
		else if (tokenAtCursor != null && atTokenEnd)
		{
			if (tokenAtCursor.text.Length != 1 ||
				tokenAtCursor.text[0] != '{' && tokenAtCursor.text[0] != '[' && tokenAtCursor.text[0] != '(')
			{
				var nextToken = index + 1 < tokens.Count ? tokens[index + 1] : null;
				if (nextToken != null && nextToken.text.Length == 1 &&
					(nextToken.text[0] == '{' || nextToken.text[0] == '[' || nextToken.text[0] == '('))
				{
					++index;
				}
			}
		}
		
		matchingBraceLeft = textBuffer.GetOpeningBraceLeftOf(lineLeft, index, -1);
		matchingBraceRight = textBuffer.GetClosingBraceRightOf(lineRight, index, -1);
	}
	
	[NonSerialized]
	private List<List<int>> _softLineBreaks;
	private static readonly List<int> NO_SOFT_LINE_BREAKS = new List<int>();
	
	private List<int> GetSoftLineBreaks(int line)
	{
		if (!wordWrapping /*|| IsLoading*/)
			return NO_SOFT_LINE_BREAKS;

		if (_softLineBreaks == null)
			_softLineBreaks = new List<List<int>>(textBuffer.lines.Count);
		if (line < _softLineBreaks.Count && _softLineBreaks[line] != null)
			return _softLineBreaks[line];

		if (line >= _softLineBreaks.Count)
		{
			if (_softLineBreaks.Capacity < textBuffer.lines.Count)
				_softLineBreaks.Capacity = textBuffer.lines.Count;
			for (int i = _softLineBreaks.Count; i < textBuffer.lines.Count; ++i)
				_softLineBreaks.Add(null);
		}

		if (charSize.x == 0 || charSize.x * 2f > codeViewRect.width)
		{
			return NO_SOFT_LINE_BREAKS; // _softLineBreaks[line];
		}
		
		FGTextBuffer.FormatedLine formatedLine = textBuffer.formatedLines[line];
		if (formatedLine.tokens == null)
			return NO_SOFT_LINE_BREAKS; // _softLineBreaks[line];

		var lineBreaks = _softLineBreaks[line] = NO_SOFT_LINE_BREAKS;
		int maxWidth = (int) (codeViewRect.width / charSize.x);

		int charIndex = 0;
		int column = 0;
		foreach (var token in formatedLine.tokens)
		{
			if (token == null)
				continue;

			//var tokenEndColumn = column;
			if (token.tokenKind > SyntaxToken.Kind.LastWSToken)
			{
				// Token with no whitespaces

				var tokenLength = token.text.Length;
				if (column + tokenLength < maxWidth)
				{
					charIndex += tokenLength;
					column += tokenLength;
					continue;
				}

				// Doesn't fit in current row

				if (lineBreaks == NO_SOFT_LINE_BREAKS)
					_softLineBreaks[line] = lineBreaks = new List<int>();

				if (column > 0)
				{
					lineBreaks.Add(charIndex);
					column = 0;
				}

				if (tokenLength > maxWidth)
				{
					// Doesn't fit in a single row

					for (var i = maxWidth; i < tokenLength; i += maxWidth)
						lineBreaks.Add(charIndex + i);
					column = tokenLength % maxWidth;
				}
				else
				{
					column = tokenLength;
				}

				charIndex += tokenLength;
			}
			else
			{
				// May contain whitespaces

				var lastLineBreak = column > 0 ? -1 : 0;
				var lastWhitespace = -1;
				for (var i = 0; i < token.text.Length; ++i)
				{
					column += token.text[i] != '\t' ? 1 : 4 - (column & 3);
					if (token.text[i] == ' ' || token.text[i] == '\t')
						lastWhitespace = i;
					if (column >= maxWidth)
					{
						if (lineBreaks == NO_SOFT_LINE_BREAKS)
							lineBreaks = _softLineBreaks[line] = new List<int>();

						if (lastWhitespace >= lastLineBreak)
						{
							lastLineBreak = lastWhitespace + 1;
							lineBreaks.Add(charIndex + lastLineBreak);
							lastWhitespace = -1;
						}
						else if (lastLineBreak >= 0)
						{
							lastLineBreak = i;
							lineBreaks.Add(charIndex + lastLineBreak);
						}
						else
						{
							lineBreaks.Add(charIndex);
						}
						column = i - lastLineBreak;
					}
				}

				charIndex += token.text.Length;
			}
		}

		//int rowLength = 0;
		
		//int lineLength = 0;
		//foreach (FGTextBuffer.TextBlock textBlock in formatedLine.textBlocks)
		//{
		//    int remaining = textBlock.text.Length;
		//    while (remaining > 0)
		//    {
		//        int exceed = rowLength + remaining - maxWidth;
		//        if (exceed <= 0)
		//        {
		//            rowLength += remaining;
		//            lineLength += remaining;
		//            break;
		//        }

		//        int startAt = textBlock.text.Length - remaining;
		//        int i = textBlock.text.Length - exceed;
		//        i = i > 0 ? textBlock.text.LastIndexOf(' ', i - 1, i - startAt) : -1;
		//        i -= startAt;
		//        ++i;
		//        if (i <= 0)
		//        {
		//            if (rowLength == 0)
		//                i = maxWidth;
		//            else
		//                i = 0;
		//        }
		//        lineLength += i;
		//        remaining -= i;
		//        if (rowLength + i > 0)
		//        {
		//            if (lineBreaks == NO_SOFT_LINE_BREAKS)
		//                lineBreaks = _softLineBreaks[line] = new List<int>();
		//            lineBreaks.Add(lineLength);
		//        }
		//        rowLength = 0;
		//    }
		//}

		if (yLineOffsets != null && yLineOffsets.Count > line)
		{
			float y = line > 0 ? yLineOffsets[line - 1] : 0f;
			float next = y + charSize.y * (lineBreaks.Count + 1);
			if (next != yLineOffsets[line])
			{
				y = next - yLineOffsets[line];
				for (int i = line; i < yLineOffsets.Count; ++i)
					yLineOffsets[i] += y;
			}
		}

		return lineBreaks;
	}
	
	//private int BufferToEditorLine(int bufferLine)
	//{
	//    if (!wordWrapping)
	//        return bufferLine;
	//    if (bufferToEditorLine != null && bufferToEditorLine.Length == numFormatedBufferLines && bufferLine < numFormatedBufferLines)
	//        return bufferToEditorLine[bufferLine];
	//    return bufferLine;
	//}
	
	//private FGTextBuffer.CaretPos EditorToBufferPos(int column, int line)
	//{
	//    var caretPos = new FGTextBuffer.CaretPos { column = column, virtualColumn = column, line = line };
	//    if (!wordWrapping)
	//    {
	//        caretPos.characterIndex = textBuffer.ColumnToCharIndex(ref caretPos.column, line);
	//    }
	//    return caretPos;
	//}

	private static EditorWindow lastFocusedWindow = null;

	public void DoGUI(bool enableGUI)
	{
		if (textBuffer == null)
			return;

		if (textBuffer.assetPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			return;
		
		if (textBuffer.assetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			return;
		
		textBuffer.styles = styles = textBuffer.isText ? stylesText : stylesCode;
		
		if (Event.current.type == EventType.Layout)
			Initialize();
		
		if (Event.current.rawType == EventType.MouseMove && mouseDownOnSelection && codeViewDragging)
		{
			mouseDownOnSelection = false;
			codeViewDragging = false;
			lineSelectMode = false;
		}

		var newFocusedWindow = EditorWindow.focusedWindow;
		if (newFocusedWindow != null)
		{
			if (newFocusedWindow == tokenTooltip || newFocusedWindow == argumentsHint || newFocusedWindow == autocompleteWindow)
				newFocusedWindow = lastFocusedWindow;
		}
		bool windowFocusChanged = lastFocusedWindow != EditorWindow.focusedWindow;
		lastFocusedWindow = EditorWindow.focusedWindow;
		if (windowFocusChanged)
		{
			if (OwnerWindow == lastFocusedWindow)
				focusCodeView = true;
		}

		var toolbarsHeight = DoToolbar();
		toolbarsHeight += DoCodeNavigationToolbar();

		bool showLineNumbers = parentWindow != null ?
			(isTextAsset ? SISettings.showLineNumbersText : SISettings.showLineNumbersCode) :
			(isTextAsset ? SISettings.showLineNumbersTextInspector : SISettings.showLineNumbersCodeInspector);
		
		if (hasCodeViewFocus)
		{
			if (tokenTooltip != null && tokenTooltip.text != null)
				tokenTooltip.OnOwnerGUI();
			
			if (autocompleteWindow != null)
			{
				var typedInLength = autocompleteWindow.TypedInPart.Length;

				var committedSymbol = autocompleteWindow.OnOwnerGUI();
				var committedWord = committedSymbol != null ? committedSymbol.name ?? "" : null;
				if (FGListPopup.shortAttributeNames && committedWord != null && committedWord.EndsWith("Attribute", StringComparison.Ordinal))
					committedWord = FGListPopup.NameOf(committedSymbol);
				if (committedWord == autocompleteWindow.TypedInPart && Event.current.character != '\n' && Event.current.keyCode != KeyCode.KeypadEnter)
				{
					//Debug.Log("Ignoring " + committedWord);
					committedWord = "";
				}
				if (committedWord != null)
				{
					//Debug.Log("Closing. Word commited: " + committedWord + " typedInPart: " + autocompleteWindow.TypedInPart);
					CloseAutocomplete();
					focusCodeView = true;
					var ownerWnd = OwnerWindow;
					if (ownerWnd != null && EditorWindow.focusedWindow != ownerWnd)
						ownerWnd.Focus();
				}
				if (!string.IsNullOrEmpty(committedWord))
				{
					if (!TryEdit())
					{
						committedWord = null;
						Event.current.Use();
					}
				}
				if (!string.IsNullOrEmpty(committedWord))
				{
					//var numLines = textBuffer.lines.Count;
					var originalLineText = textBuffer.lines[caretPosition.line];
					var insertAt = caretPosition.Clone();
					var codeSnippet = committedSymbol as SnippetCompletion;
					if (codeSnippet != null)
					{
						if (Event.current.character != '\n' && Event.current.character != '\t' && Event.current.keyCode != KeyCode.KeypadEnter)
							goto ignoreSnippet;
						if (committedWord.EndsWith("...", StringComparison.Ordinal))
							committedWord = committedWord.Substring(0, committedWord.Length - 3);
					}
					if (Event.current.isKey && codeSnippet == null)
					{
						try
						{
							lastTypedText = null;
							ProcessEditorKeyboard(Event.current, true);
							
							//Event.current.Use();
						}
						catch (ExitGUIException) {}
					}
					
					textBuffer.EndEdit();
					textBuffer.BeginEdit("Auto Completion '" + committedWord + "'");

					string insertedChar = null;
					if (codeSnippet == null && caretPosition > insertAt
						&& textBuffer.lines[insertAt.line].Length >= insertAt.characterIndex)
					{
						insertedChar = textBuffer.GetTextRange(insertAt, caretPosition);
						if (insertedChar[0] == '\n')
						{
							caretPosition = textBuffer.DeleteText(insertAt , caretPosition);
							var lineText = textBuffer.lines[caretPosition.line];
							var deltaLength = lineText.Length - originalLineText.Length;
							if (deltaLength < 0)
								textBuffer.InsertText(caretPosition, originalLineText.Substring(caretPosition.characterIndex, -deltaLength));
						}
						else if (insertedChar[0] != '\t')
						{
							committedWord += insertedChar;
							caretPosition = textBuffer.DeleteText(insertAt, caretPosition);
						}
						else
						{
							caretPosition = textBuffer.DeleteText(insertAt, caretPosition);
						}
					}
					if (typedInLength > 0)
					{
						var endTypedIn = insertAt.Clone();
						insertAt.characterIndex -= typedInLength;
						caretPosition = textBuffer.DeleteText(insertAt, endTypedIn);
					}
					caretPosition = textBuffer.InsertText(insertAt, committedWord);

					string autoTextAfter = null;
					if (insertedChar != null)
						autoTextAfter = CheckAutoClose(insertedChar[0]);
					var updateToLine = caretPosition.line;
					if (autoTextAfter != null)
					{
						FGTextBufferManager.insertingTextAfterCaret = true;
						updateToLine = textBuffer.InsertText(caretPosition, autoTextAfter).line;
						FGTextBufferManager.insertingTextAfterCaret = false;

						var newAutoClose = new AutoClosePair
						{
							openingPos = new TextPosition(caretPosition.line, caretPosition.characterIndex - 1),
							closingPos = new TextPosition(caretPosition.line, caretPosition.characterIndex)
						};
						autoClosingStack.Add(newAutoClose);
						if (autoClosingStack.Count == 1)
						{
							textBuffer.onRemovedText += OnRemovedText;
							textBuffer.onInsertedText += OnInsertedText;
						}
					}

					if (wordWrapping)
						caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
					textBuffer.UpdateHighlighting(insertAt.line, updateToLine);
					if (codeSnippet != null)
						ExpandSnippet(codeSnippet);
					else
						ReindentLines(insertAt.line, updateToLine);

					caretMoveTime = Time.realtimeSinceStartup;
					lastCaretMoveWasSearch = false;
					scrollToCaret = true;
					//Repaint();
					
					//if (committedWord == "new " || committedWord == "case ")
					if (lastTypedText != null && codeSnippet == null)
					{
						AfterCharecterTyped(lastTypedText, caretPosition.line, caretPosition.characterIndex);
						tryArgumentsHint = true;
					}
					
					GUIUtility.ExitGUI();
				}
				if (Event.current.type == EventType.Used)
					return;
			}
			
		ignoreSnippet:

			if (argumentsHint != null && argumentsHint.text != null)
				argumentsHint.OnOwnerGUI();
				
			if (textBuffer != null && ProcessCodeViewCommands())
			{
				return;
			}
			
			bool isOSX = Application.platform == RuntimePlatform.OSXEditor;
			bool contextClick = Event.current.type == EventType.ContextClick
				|| isOSX && Event.current.type == EventType.MouseUp && Event.current.button == 1;
			contextClick |= Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Menu;
			if (contextClick && scrollViewRect.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
				var codeViewPopupMenu = new GenericMenu();

				if (textBuffer.isCsFile)
				{
					int lineIndex, tokenIndex;
					bool atTokenEnd;
					var token = textBuffer.GetTokenAt(caretPosition, out lineIndex, out tokenIndex, out atTokenEnd);
					if (token != null)
					{
						if (atTokenEnd &&
							token.tokenKind != SyntaxToken.Kind.Identifier &&
							token.tokenKind != SyntaxToken.Kind.Keyword)
						{
							var tokens = textBuffer.formatedLines[lineIndex].tokens;
							if (tokenIndex < tokens.Count - 1)
								token = tokens[tokenIndex + 1];
						}

						string helpUrl = HelpURL();
						if (helpUrl != null)
						{
							if (token.tokenKind == SyntaxToken.Kind.Keyword)
							{
								codeViewPopupMenu.AddItem(new GUIContent(isOSX ? "MSDN C# Reference _%'" : "MSDN C# Reference _f1"), false,
								() => Help.BrowseURL(helpUrl));
							}
							else if (helpUrl.StartsWith("file:///unity/ScriptReference/", StringComparison.OrdinalIgnoreCase))
							{
								codeViewPopupMenu.AddItem(
									new GUIContent(isOSX ? "Unity Script Reference _%'" : "Unity Scripting Reference _f1"), false,
									() => Help.ShowHelpPage(helpUrl));
							}
							else if (helpUrl.StartsWith("http://docs.unity3d.com/", StringComparison.OrdinalIgnoreCase))
							{
								codeViewPopupMenu.AddItem(
									new GUIContent(isOSX ? "Unity Script Reference _%'" : "Unity Scripting Reference _f1"), false,
									() => Help.BrowseURL(helpUrl));
							}
							else
							{
								codeViewPopupMenu.AddItem(
									new GUIContent(isOSX ? "MSDN .Net Reference _%'" : "MSDN .Net Reference _f1"), false,
									() => Help.BrowseURL(helpUrl));
							}
						}
						
						if (token.parent != null && token.parent.resolvedSymbol != null)
						{
							var symbol = token.parent.resolvedSymbol;
							var assembly = symbol.Assembly;

							if (assembly == null && symbol.declarations != null)
								assembly = ((CompilationUnitScope) textBuffer.Parser.parseTree.root.scope).assembly;

							if (assembly != null)
							{
								var assemblyName = assembly.AssemblyName;
								if (assemblyName == "mscorlib" || assemblyName == "System" || assemblyName.StartsWith("System.", StringComparison.Ordinal))
								{
									if (symbol.kind != SymbolKind.Namespace)
									{
										var input = symbol.XmlDocsName;
										if (input != null)
										{
											const int digits = 16;
											var md5 = System.Security.Cryptography.MD5.Create();
											var bytes = Encoding.UTF8.GetBytes(input);
											var hashBytes = md5.ComputeHash(bytes);
		
											var c = new char[digits];
											byte b;
											for (var i = 0; i < digits / 2; ++i)
											{
												b = ((byte) (hashBytes[i] >> 4));
												c[i * 2] = (char) (b > 9 ? b + 87 : b + 0x30);
												b = ((byte) (hashBytes[i] & 0xF));
												c[i * 2 + 1] = (char) (b > 9 ? b + 87 : b + 0x30);
											}
		
											codeViewPopupMenu.AddItem(new GUIContent(isOSX ? "Go to Definition (.Net) %y" : "Go to Definition (.Net) _f12"), false,
												() => Help.BrowseURL("http://referencesource.microsoft.com/mscorlib/a.html#" + new string(c)));
										}
									}
								}
								else if (assembly.assemblyId == AssemblyDefinition.UnityAssembly.CSharpFirstPass
									|| assembly.assemblyId == AssemblyDefinition.UnityAssembly.CSharp
									|| assembly.assemblyId == AssemblyDefinition.UnityAssembly.CSharpEditorFirstPass
									|| assembly.assemblyId == AssemblyDefinition.UnityAssembly.CSharpEditor)
								{
									var declarations = GetSymbolDeclarations();
									if (declarations != null && declarations.Count == 1)
									{
										codeViewPopupMenu.AddItem(
											new GUIContent(isOSX ? "Go to Definition %y" : "Go to Definition _f12"),
											false,
											() => GoToSymbolDeclaration(declarations[0]));
									}
									else if (declarations != null && declarations.Count > 0)
									{
										foreach (var decl in declarations)
										{
											var co = decl.scope;
											while (co.parentScope != null)
												co = co.parentScope;
											var fileName = ((CompilationUnitScope) co).path;
											fileName = AssetDatabase.AssetPathToGUID(fileName);
											fileName = AssetDatabase.GUIDToAssetPath(fileName);
											fileName = Path.GetFileName(fileName);
											fileName = fileName.Replace('_', '\xFF3F');
											var nameNode = decl.NameNode();
											var firstLeaf = nameNode as ParseTree.Leaf ?? (nameNode as ParseTree.Node).GetFirstLeaf();
											codeViewPopupMenu.AddItem(
												new GUIContent("Go to Definition/" + fileName + " : " + (firstLeaf.token.Line + 1) + " _"),
												false,
												d => GoToSymbolDeclaration((SymbolDeclaration) d),
												decl);
										}
									}
								}
							}
							
							if (symbol.kind == SymbolKind.Method && symbol.GetParameters().Count == 0 && symbol.IsStatic)
							{
								if (codeViewPopupMenu.GetItemCount() > 0)
									codeViewPopupMenu.AddSeparator("");
								codeViewPopupMenu.AddItem(new GUIContent(isOSX ? "Execute %e" : "Execute %e"), false, ExecuteStaticMethod);
							}
						}
					}
				}
				
				if (codeViewPopupMenu.GetItemCount() > 0)
					codeViewPopupMenu.AddSeparator(string.Empty);

				if (selectionStartPosition != null)
				{
					codeViewPopupMenu.AddItem(new GUIContent("Copy %c"), false, () => EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Copy")));
					codeViewPopupMenu.AddItem(new GUIContent("Cut %x"), false, () => EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Cut")));
				}
				else if (SISettings.copyCutFullLine)
				{
					codeViewPopupMenu.AddItem(new GUIContent("Copy Line %c"), false, () => EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Copy")));
					codeViewPopupMenu.AddItem(new GUIContent("Cut Line %x"), false, () => EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Cut")));
				}
				else
				{
					codeViewPopupMenu.AddItem(new GUIContent("Copy %c"), false, null);
					codeViewPopupMenu.AddItem(new GUIContent("Cut %x"), false, null);
				}
				if (string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer))
					codeViewPopupMenu.AddItem(new GUIContent("Paste %v"), false, null);
				else
					codeViewPopupMenu.AddItem(new GUIContent("Paste %v"), false, () => EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Paste")));
				codeViewPopupMenu.AddSeparator(string.Empty);

				codeViewPopupMenu.AddItem(new GUIContent("Select All %a"), false, () => EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("SelectAll")));
				codeViewPopupMenu.AddSeparator(string.Empty);
				if (!textBuffer.isText)
					codeViewPopupMenu.AddItem(new GUIContent("Toggle Comment Selection %k"), false, ToggleCommentSelection);
				codeViewPopupMenu.AddItem(new GUIContent("Increase Line Indent %]"), false, IndentMore);
				codeViewPopupMenu.AddItem(new GUIContent("Decrease Line Indent %["), false, IndentLess);
				codeViewPopupMenu.AddItem(new GUIContent("Open at Line " + (caretPosition.line + 1) + (isOSX ? "... %\n" : "... %Enter")), false,
					() => EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("OpenAtCursor")));

				codeViewPopupMenu.ShowAsContext();
				return;
			}

			if (Event.current.isKey)
			{
				ProcessEditorKeyboard(Event.current, false);
				if (Event.current == null || Event.current.type == EventType.used)
				{
					//	tryArgumentsHint = true;
					GUIUtility.ExitGUI();
					return;
				}
				Event.current.Use();
			}
		}

		if (Event.current.type == EventType.scrollWheel)
		{
			mouseHoverToken = null;
			mouseHoverTime = 0f;
			if (tokenTooltip != null)
				tokenTooltip.Hide();
			
			if (argumentsHint != null)
				CloseArgumentsHint();
			
		//	needsRepaint = true;
		}

		var contentWidth = charSize.x * textBuffer.longestLine;
		contentRect.Set(-4, -4, contentWidth + 8f, 8f + charSize.y * textBuffer.formatedLines.Length);
		
		var lineNumbersWidth = 0f;
		var lineNumbersMaxLength = 0;

		if (Event.current.type != EventType.layout)
		{
			marginLeft = 0f;
			
			if (showLineNumbers)
			{
				lineNumbersMaxLength = textBuffer.formatedLines.Length.ToString().Length;
				lineNumbersWidth = charSize.x * lineNumbersMaxLength;
				marginLeft = lineNumbersWidth;
			}
			if (trackChanges)
			{
				marginLeft += showLineNumbers ? 7f : 3f;
			}
			if (showLineNumbers || trackChanges)
			{
				marginLeft += 9f;
			}
		}
		
	onScroll:
		
		scrollPosition.y = GetLineOffset(scrollPositionLine) + scrollPositionOffset;

		int fromLine = wordWrapping ? GetLineAt(scrollPosition.y) : Math.Max(0, ((int)(scrollPosition.y / charSize.y)) - 1);
		int toLine = wordWrapping ?
			1 + GetLineAt(scrollPosition.y + scrollViewRect.height) :
			scrollViewRect.height > 0f ?
				fromLine + 2 + (int)(scrollViewRect.height / charSize.y) :
				(int)(Screen.height / charSize.y);

		if (toLine > textBuffer.formatedLines.Length)
			toLine = textBuffer.formatedLines.Length;

		List<int> softLineBreaks = null;

		if (scrollToCaret && Event.current.type != EventType.layout)
		{
			scrollToCaret = false;
			FGTextBuffer.CaretPos caretPos = codeViewDragging && mouseDownOnSelection ? mouseDropPosition : caretPosition;

//			if (showLineNumbers || trackChanges)
//				contentRect.xMax += marginLeft;

			codeViewRect.x = scrollPosition.x + marginLeft;
			codeViewRect.y = scrollPosition.y;
			codeViewRect.width = scrollViewRect.width - marginLeft - 4f;
			codeViewRect.height = scrollViewRect.height - 4f;

			float contentHeight2 = wordWrapping ? GetLineOffset(textBuffer.lines.Count) + 8f : contentRect.height;
			
			bool hasHorizontalSB = !wordWrapping && contentRect.width - 4f - marginLeft - marginRight > codeViewRect.width;
			bool hasVerticalSB = (wordWrapping ? contentHeight2 : contentRect.height) - 4f > codeViewRect.height;
			if (hasHorizontalSB && hasVerticalSB)
			{
				codeViewRect.width -= 15f;
				codeViewRect.height -= 15f;
			}
			else if (hasHorizontalSB)
			{
				codeViewRect.height -= 15f;
				hasVerticalSB = contentRect.height - 4f > codeViewRect.height;
				if (hasVerticalSB)
					codeViewRect.width -= 15f;
			}
			else if (hasVerticalSB)
			{
				codeViewRect.width -= 15f;
				hasHorizontalSB = contentRect.width - 4f - marginLeft - marginRight > codeViewRect.width;
				if (hasHorizontalSB)
					codeViewRect.height -= 15f;
			}
			
			codeViewRect.xMin = Mathf.Ceil((codeViewRect.x - 1f - marginLeft) / charSize.x) * charSize.x + 0f + marginLeft;
			codeViewRect.width = Mathf.Floor(codeViewRect.width / charSize.x) * charSize.x;
			codeViewRect.yMin = Mathf.Ceil(codeViewRect.y / charSize.y) * charSize.y;
			codeViewRect.height = Mathf.Floor(codeViewRect.height / charSize.y) * charSize.y;
			
			float yOffset;
			if (wordWrapping /*&& !IsLoading*/)
			{
				int row, column;
				BufferToViewPosition(caretPosition, out row, out column);
				yOffset = charSize.y * row + GetLineOffset(caretPos.line);
			}
			else
			{
				yOffset = charSize.y * caretPos.line;
			}
				
			if (yOffset < scrollPosition.y)
			{
				scrollPosition.y = yOffset;
				needsRepaint = true;
				scrollToCaret = true;
			}
			if (yOffset + charSize.y > scrollPosition.y + scrollViewRect.height - (hasHorizontalSB ? 23f : 8f))
			{
				scrollPosition.y = Mathf.Max(0f, yOffset + charSize.y - scrollViewRect.height + (hasHorizontalSB ? 23f : 8f));
				needsRepaint = true;
				scrollToCaret = true;
			}

			if (!wordWrapping)
			{
				if (caretPos.column * charSize.x < scrollPosition.x)
				{
					var oldX = scrollPosition.x;
					scrollPosition.x = Mathf.Max(0, (caretPos.column - 20) * charSize.x);
					if (oldX != scrollPosition.x)
					{
						needsRepaint = true;
						scrollToCaret = true;
						if (autocompleteWindow != null)
						{
							var newPos = autocompleteWindow.position;
							newPos.x += oldX - scrollPosition.x;
							autocompleteWindow.position = newPos;
						}
					}
				}
				else if (((caretPos.column + 1) * charSize.x) > (scrollPosition.x + scrollViewRect.width - marginLeft - 22f))
				{
					var oldX = scrollPosition.x;
					scrollPosition.x = Mathf.Max(0f, (caretPos.column + 21) * charSize.x - scrollViewRect.width + marginLeft + 22f);
					if (oldX != scrollPosition.x)
					{
						needsRepaint = true;
						scrollToCaret = true;
						if (autocompleteWindow != null)
						{
							var newPos = autocompleteWindow.position;
							newPos.x += oldX - scrollPosition.x;
							autocompleteWindow.position = newPos;
						}
					}
				}
			}
			
			if (needsRepaint)
			{
				if (scrollToCaret && argumentsHint != null)
				{
					//Debug.Log("Closing [Scroll]");//
					CloseArgumentsHint();
				}
				if (CanEdit())
				{
					scrollPositionLine = GetLineAt(scrollPosition.y);
					scrollPositionOffset = scrollPosition.y - GetLineOffset(scrollPositionLine);
				}
				needsRepaint = false;
				goto onScroll;
			}
		}

		if (pingTimer >= 1f && scrollViewRect.height > 1f && Event.current.type == EventType.layout)
		{
			if (scrollToRect.yMin < scrollPosition.y + 30f ||
				scrollToRect.yMax > scrollPosition.y + scrollViewRect.height - 50f)
			{
				var y = Mathf.Floor(Mathf.Max(0f, scrollToRect.center.y - scrollViewRect.height * 0.5f));
				if (scrollPosition.y != y)
				{
					scrollPosition.y = y;
					needsRepaint = true;
				}
			}

			if (scrollToRect.xMin < scrollPosition.x + 30f ||
				scrollToRect.xMax > scrollPosition.x + scrollViewRect.width - 30f - marginLeft)
			{
				var x = Mathf.Floor(Mathf.Max(0f, scrollToRect.center.x - scrollViewRect.width * 0.5f));
				if (scrollPosition.x != x)
				{
					scrollPosition.x = x;
					needsRepaint = true;
				}
			}

			pingStartTime = Time.realtimeSinceStartup;
			
			if (needsRepaint)
			{
				if (CanEdit())
				{
					scrollPositionLine = GetLineAt(scrollPosition.y);
					scrollPositionOffset = scrollPosition.y - GetLineOffset(scrollPositionLine);
				}
				needsRepaint = false;
				goto onScroll;
			}
		}

		if (Event.current.type == EventType.repaint)
		{
			if (needsRepaint)
			{
				needsRepaint = false;
				if (CanEdit())
				{
					var line = scrollPositionLine;
					var offset = scrollPositionOffset;
					scrollPositionLine = GetLineAt(scrollPosition.y);
					scrollPositionOffset = scrollPosition.y - GetLineOffset(scrollPositionLine);
					if (line != scrollPositionLine || offset != scrollPositionOffset)
					{
						goto onScroll;
					}
				}
				else
				{
				//	Repaint();
				//	DoGUI(enableGUI);
					goto onScroll;
				//	return;
				}
			}
		}

		if (Event.current.type == EventType.layout)
		{
			if (CanEdit())
			{
				scrollPositionLine = GetLineAt(scrollPosition.y);
				scrollPositionOffset = scrollPosition.y - GetLineOffset(scrollPositionLine);
			}
			return;
		}

		if (showLineNumbers || trackChanges)
			contentRect.xMax += marginLeft + marginRight;

		// Filling the background
		GUI.Box(scrollViewRect, GUIContent.none, styles.scrollViewStyle);

		if (lastMouseEvent != null && autoScrollDelta != Vector2.zero)
		{
			lastMouseEvent.mousePosition -= scrollPosition;
			scrollPosition += autoScrollDelta;
			scrollPositionLine = GetLineAt(scrollPosition.y);
			scrollPositionOffset = scrollPosition.y - GetLineOffset(scrollPositionLine);
			
			fromLine = wordWrapping ? scrollPositionLine : Math.Max(0, ((int)(scrollPosition.y / charSize.y)) - 1);
			toLine = wordWrapping ?
				1 + GetLineAt(scrollPosition.y + scrollViewRect.height) :
				scrollViewRect.height > 0f ?
				fromLine + 2 + (int)(scrollViewRect.height / charSize.y) :
				(int)(Screen.height / charSize.y);
			
			if (toLine > textBuffer.formatedLines.Length)
				toLine = textBuffer.formatedLines.Length;
		}

		if (!scrollPositionInitialized)
		{
			scrollPositionInitialized = true;
			smoothScrollPosition = scrollPosition;
			currentScrollVelocity = Vector2.zero;
			lastSmoothScrollTime = 0f;
		}

		if (float.IsNaN(smoothScrollPosition.x) || float.IsNaN((smoothScrollPosition.y)))
		{
			smoothScrollPosition = scrollPosition;
			currentScrollVelocity = Vector2.zero;
			lastSmoothScrollTime = 0f;
			if (float.IsNaN(smoothScrollPosition.x) || float.IsNaN((smoothScrollPosition.y)))
				scrollPositionInitialized = false;
		}
		
		if (SISettings.smoothScrolling)
		{
			if (Event.current.type == EventType.repaint)
			{
				var currentTime = Time.realtimeSinceStartup;
				var deltaTime = Mathf.Clamp01(currentTime - lastSmoothScrollTime);
				lastSmoothScrollTime = currentTime;
				if (smoothScrollPosition != scrollPosition)
				{
					smoothScrollPosition.x = Mathf.SmoothDamp(smoothScrollPosition.x, scrollPosition.x, ref currentScrollVelocity.x, 0.05f, Mathf.Infinity, deltaTime);
					smoothScrollPosition.y = Mathf.SmoothDamp(smoothScrollPosition.y, scrollPosition.y, ref currentScrollVelocity.y, 0.05f, Mathf.Infinity, deltaTime);
				}
			}
		}
		else
		{
			smoothScrollPosition = scrollPosition;
		}
		var roundedSmoothScrollPosition = new Vector2(Mathf.Round(smoothScrollPosition.x), Mathf.Round(smoothScrollPosition.y));

		float contentHeight = wordWrapping ? GetLineOffset(textBuffer.lines.Count) + 8f : contentRect.height;

	beginScrollViewAgain:

		Vector2 newScrollPosition = GUI.BeginScrollView(scrollViewRect, roundedSmoothScrollPosition, wordWrapping ? new Rect(contentRect.x, contentRect.y, 1, contentHeight) : contentRect);
		if (CanEdit() && scrollPosition != newScrollPosition)
		{
			if (roundedSmoothScrollPosition != newScrollPosition)
			{
				scrollPosition = newScrollPosition;

				scrollPositionLine = GetLineAt(newScrollPosition.y);
				scrollPositionOffset = newScrollPosition.y - GetLineOffset(scrollPositionLine);

				newScrollPosition = roundedSmoothScrollPosition;

				GUI.EndScrollView();
				goto beginScrollViewAgain;
			}

			var smoothScrollPositionLine = GetLineAt(newScrollPosition.y);
			
			fromLine = wordWrapping ? smoothScrollPositionLine : Math.Max(0, ((int)(newScrollPosition.y / charSize.y)) - 1);
			toLine = wordWrapping ?
				1 + GetLineAt(newScrollPosition.y + scrollViewRect.height) :
				scrollViewRect.height > 0f ?
				fromLine + 2 + (int)(scrollViewRect.height / charSize.y) :
				(int)(Screen.height / charSize.y);
			
			if (toLine > textBuffer.formatedLines.Length)
				toLine = textBuffer.formatedLines.Length;
		}
		else
		{
			smoothScrollPosition = newScrollPosition;
		}
		if (textBuffer.lines.Count == 0)
		{
			GUI.EndScrollView();
			return;
		}

		// Hack: Workaround for not receiving mouseUp event if dragged outside of the scrollview's clipped content.
		// Note: mousePosition here will be incorrect, don't use it!
		if (Event.current.rawType == EventType.mouseUp && GUIUtility.hotControl == codeViewControlID)
		{
			//Event.current.mousePosition = Event.current.mousePosition - new Vector2(scrollViewRect.x, scrollViewRect.y);
			ProcessEditorMouse(marginLeft, Event.current);
			goto endScrollViewAndExit;
		}

		if (lastMouseEvent != null && autoScrollDelta != Vector2.zero)
		{
			lastMouseEvent.mousePosition += scrollPosition;
			autoScrollDelta = Vector2.zero;
		}

		if (focusCodeView)
		{
			focusCodeView = false;
			caretMoveTime = Time.realtimeSinceStartup;
			GUIUtility.keyboardControl = codeViewControlID;
			Repaint();
		}
		hasCodeViewFocus = GUIUtility.keyboardControl == codeViewControlID;
		if (hasCodeViewFocus)
			Input.imeCompositionMode = IMECompositionMode.On;
		if (hasCodeViewFocus && Event.current.rawType != EventType.mouseUp)
		{
			EditorWindow wnd = EditorWindow.focusedWindow;
			if (wnd == null)
			{
				hasCodeViewFocus = false;
			}
			else if (wnd != autocompleteWindow)
			{
				hasCodeViewFocus = wnd == OwnerWindow;
			}
		}

		Rect lastCodeViewRect = codeViewRect;
		if (Event.current.type != EventType.layout)
		{
			codeViewRect.x = scrollPosition.x + marginLeft;
			codeViewRect.y = scrollPosition.y;
			codeViewRect.width = scrollViewRect.width - marginLeft - 4f - marginRight;
			codeViewRect.height = scrollViewRect.height - 4f;

			bool hasHorizontalSB = !wordWrapping && contentRect.width - 4f - marginLeft - marginRight > codeViewRect.width;
			bool hasVerticalSB = (wordWrapping ? contentHeight : contentRect.height) - 4f > codeViewRect.height;
			if (hasHorizontalSB && hasVerticalSB)
			{
				codeViewRect.width -= 15f;
				codeViewRect.height -= 15f;
			}
			else if (hasHorizontalSB)
			{
				codeViewRect.height -= 15f;
				hasVerticalSB = contentRect.height - 4f > codeViewRect.height;
				if (hasVerticalSB)
					codeViewRect.width -= 15f;
			}
			else if (hasVerticalSB)
			{
				codeViewRect.width -= 15f;
				hasHorizontalSB = contentRect.width - 4f - marginLeft - marginRight > codeViewRect.width;
				if (hasHorizontalSB)
					codeViewRect.height -= 15f;
			}
		
			fullCodeViewRect = codeViewRect;
			fullCodeViewRect.xMin += -4f;
			fullCodeViewRect.yMin += -4f;
			fullCodeViewRect.yMax += 4f;
		
			codeViewRect.xMin = Mathf.Ceil((codeViewRect.x - 1f - marginLeft) / charSize.x) * charSize.x + 0f + marginLeft;
			codeViewRect.width = Mathf.Floor(codeViewRect.width / charSize.x) * charSize.x;
			codeViewRect.yMin = Mathf.Ceil(codeViewRect.y / charSize.y) * charSize.y;
			codeViewRect.height = Mathf.Floor(codeViewRect.height / charSize.y) * charSize.y;

			// Uncomment for debugging only
			//GUI.Box(codeViewRect, GUIContent.none);
		}

		//if (needsRepaint)
		//{
		//	GUI.EndScrollView();
		//	Repaint();
		//	return;
		//}

		if (Event.current.type == EventType.repaint)
		{
			if (wordWrapping && lastCodeViewRect.width != codeViewRect.width)
			{
				//Debug.Log(lastCodeViewRect + " != " + codeViewRect);
				if (_softLineBreaks != null)
					for (int i = 0; i < _softLineBreaks.Count; ++i)
						_softLineBreaks[i] = null;
				yLineOffsets = null;
				GetLineOffset(textBuffer.lines.Count);
				
				//GUI.EndScrollView();
				//Repaint();
				//return;
			}
		}

		if (Event.current.type == EventType.repaint /*&& !IsLoading*/)
		{
			Rect rc = new Rect();

			// Current line highlighting
			if ((SISettings.highlightCurrentLine || SISettings.frameCurrentLine)
				&& !hasSelection && Event.current.type == EventType.repaint
				&& caretPosition.line >= fromLine && caretPosition.line < toLine
				&& !IsLoading)
			{
				int row;
				int column;
				BufferToViewPosition(caretPosition, out row, out column);
				float yOffset = charSize.y * row + GetLineOffset(caretPosition.line);
				Rect currentLineRect = new Rect(marginLeft - 4f + smoothScrollPosition.x, yOffset, codeViewRect.width + charSize.x + 4f, 1f);
				if (SISettings.frameCurrentLine)
					GUI.Label(currentLineRect, GUIContent.none, hasCodeViewFocus ? styles.currentLineStyle : styles.currentLineInactiveStyle);

				currentLineRect.y += charSize.y - 1f;
				if (SISettings.frameCurrentLine)
					GUI.Label(currentLineRect, GUIContent.none, hasCodeViewFocus ? styles.currentLineStyle : styles.currentLineInactiveStyle);

				currentLineRect.y -= charSize.y - 2f;
				currentLineRect.height = charSize.y - 2f;
				Color oldColor = GUI.color;
				GUI.color = new Color(1f, 1f, 1f, SISettings.highlightCurrentLineAlpha);
				if (SISettings.highlightCurrentLine)
					GUI.Label(currentLineRect, GUIContent.none, hasCodeViewFocus ? styles.currentLineStyle : styles.currentLineInactiveStyle);
				GUI.color = oldColor;
			}

			// Highlighting search results
			if (textBuffer.undoPosition == searchResultAge && highlightSearchResults)
			{
				var oldCVD = codeViewDragging;
				codeViewDragging = true;
				
				int searchStringLength = searchString.Length;
				rc.height = charSize.y;
				var startFrom = searchResults.BinarySearch(new FGTextBuffer.CaretPos{ line = fromLine, characterIndex = -1 });
				if (startFrom < 0)
					startFrom = ~startFrom;
				for (var i = startFrom; i < searchResults.Count; ++i)
				{
					FGTextBuffer.CaretPos highlightRect = searchResults[i];
					if (highlightRect.line >= fromLine)
					{
						if (highlightRect.line > toLine)
							break;
						
						DrawSelectionRectCharIndex(highlightRect.line, highlightRect.characterIndex, searchStringLength, false, styles.searchResultStyle);
					}
				}
				
				codeViewDragging = oldCVD;
			}
			
			// Drawing selection
			if (!IsLoading && hasSelection)
			{
				if (selectionStartPosition.line == caretPosition.line)
				{
					// Single line selection
					if (wordWrapping)
						DrawSelectionRectCharIndex(caretPosition.line,
							Math.Min(caretPosition.characterIndex, selectionStartPosition.characterIndex),
							Math.Abs(caretPosition.characterIndex - selectionStartPosition.characterIndex), false, null);
					else
						DrawSelectionRect(caretPosition.line, Math.Min(caretPosition.column, selectionStartPosition.column),
							Math.Abs(caretPosition.column - selectionStartPosition.column));
				}
				else
				{
					var inverse = caretPosition < selectionStartPosition;
					var fromPos = inverse ? caretPosition : selectionStartPosition;
					var toPos = inverse ? selectionStartPosition : caretPosition;
					int firstLine = fromPos.line;
					int lastLine = toPos.line;

					if (wordWrapping)
					{
						DrawSelectionRectCharIndex(firstLine, fromPos.characterIndex,
							textBuffer.lines[firstLine].Length - fromPos.characterIndex, true, null);
						DrawSelectionRectCharIndex(lastLine, 0, toPos.characterIndex, false, null);
					}
					else
					{
						DrawSelectionRect(firstLine, fromPos.column,
							textBuffer.CharIndexToColumn(textBuffer.lines[firstLine].Length, firstLine) - fromPos.column + 1);
						DrawSelectionRect(lastLine, 0, toPos.column);
					}

					++firstLine;
					--lastLine;

					if (firstLine < fromLine)
						firstLine = fromLine;
					if (lastLine >= toLine)
						lastLine = toLine - 1;
					for (int line = firstLine; line <= lastLine; ++line)
						DrawSelectionRectCharIndex(line, 0, textBuffer.lines[line].Length, true, null);
				}
			}

			if (pingTimer > 0f && pingStartTime != 0f)
			{
				pingTimer = 1f - (Time.realtimeSinceStartup - pingStartTime) * 0.5f;
				if (pingTimer < 0f)
				{
					pingTimer = 0f;
					pingStartTime = 0f;
				}

				int row;
				int column;
				BufferToViewPosition(caretPosition, out row, out column);
				var rcPing = scrollToRect;
				rcPing.y = charSize.y * row + GetLineOffset(caretPosition.line);
				
				DrawPing(marginLeft, rcPing, true);
			}
		}

		if (Event.current.type == EventType.ScrollWheel && EditorGUI.actionKey)
		{
			Event.current.Use();
			GUI.EndScrollView();
			ModifyFontSize(-(int)Event.current.delta.y);
			return;
		}

		List<SyntaxToken> tempTokens = null;
		Rect rect = new Rect();
		rect.y = wordWrapping ? GetLineOffset(fromLine) - charSize.y : (fromLine - 1) * charSize.y;
		for (int i = fromLine; i < toLine; ++i)
		{
			rect.x = marginLeft;
			rect.y += charSize.y;
			rect.height = charSize.y;

			FGTextBuffer.FormatedLine line = textBuffer.formatedLines[i];

			var charIndex = 0;
			var startAtColumn = 0;
			var tokens = line.tokens;
			if (tokens == null)
			{
				if (tempTokens == null)
					tempTokens = new List<SyntaxToken> { new SyntaxToken(SyntaxToken.Kind.PreprocessorArguments, FGTextBuffer.ExpandTabs(textBuffer.lines[i], 0)) };
				else
					tempTokens[0].text = FGTextBuffer.ExpandTabs(textBuffer.lines[i], 0);
				tokens = tempTokens;
			}

			softLineBreaks = GetSoftLineBreaks(i);
			int numBreaks = wordWrapping /*&& !IsLoading*/ ? softLineBreaks.Count : 0;
			if (numBreaks == 0)
			{
				for (var j = 0; j < tokens.Count; ++j)
				{
					var token = tokens[j];
					
					if (token == null)
						continue;

					var tokenText = token.text;
					//var tabIndex = token.text.IndexOf('\t');

					charIndex += tokenText.Length;
					var endAtColumn = textBuffer.CharIndexToColumn(charIndex, i);
					rect.width = charSize.x * (endAtColumn - startAtColumn);

					tokenText = FGTextBuffer.ExpandTabs(tokenText, startAtColumn);
					startAtColumn = endAtColumn;

					if (token.tokenKind != SyntaxToken.Kind.Whitespace)
					{
						if (token.tokenKind == SyntaxToken.Kind.Missing)
						{
							if (Event.current.type == EventType.repaint)
							{
								var rcMissing = new Rect(rect.xMax, rect.yMin, charSize.x * 2f, charSize.y);
								DrawWavyUnderline(rcMissing, new Color(1f, 0f, 0f, .8f));
							}
						}
						else if (token.style == styles.hyperlinkStyle || token.style == styles.mailtoStyle)
						{
							if (GUI.Button(rect, tokenText, token.style))
							{
								if (token.style == styles.hyperlinkStyle)
									Application.OpenURL(token.text);
								else
									Application.OpenURL("mailto:" + token.text);
							}
							
							if (Event.current.type == EventType.repaint)
							{
								// show the "Link" cursor when the mouse is hovering over this rectangle.
								EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
							}
						}
						else
						{
							token.style = GetTokenStyle(token);
							
							// Highlighting references
							if (!hasSelection && pingTimer == 0f && SISettings.referenceHighlighting)
							{
								if (highlightedSymbol != null)
								{
									if (token.parent != null && token.parent.resolvedSymbol != null &&
									    token.parent.resolvedSymbol.GetGenericSymbol() == highlightedSymbol)
								    {
									    var referenceStyle = GetReferenceHighlightStyle(token);
								        GUI.Label(rect, GUIContent.none, referenceStyle);
								    }
								}
								else if (highlightedPPSymbol != null)
								{
									if (token.tokenKind == SyntaxToken.Kind.PreprocessorSymbol && token.text == highlightedPPSymbol)
									{
										var referenceStyle = GetReferenceHighlightStyle(token);
										GUI.Label(rect, GUIContent.none, referenceStyle);
									}
								}
							}
							
							if (Event.current.type == EventType.repaint)
							{
								var errorNode = token.parent;
								if (errorNode != null && token.tokenKind != SyntaxToken.Kind.Missing)
								{
									if (errorNode.syntaxError != null)
									{
										DrawWavyUnderline(rect, new Color(1f, 0f, 0f, .8f));
									}
									else if (errorNode.semanticError != null ||
										errorNode.resolvedSymbol != null && errorNode.resolvedSymbol.kind == SymbolKind.Error)
									{
										DrawWavyUnderline(rect, new Color(1f, 0f, 1f, .8f));
									}
								}
							}

							GUI.Label(rect, tokenText, token.style);
						}
					}

					rect.xMin = rect.xMax;
				}
			}
			else
			{
				int column = 0;
				int softRow = 0;
				for (var j = 0; j < tokens.Count; ++j)
				{
					var token = tokens[j];
					
					if (token == null)
						continue;

					if (token.tokenKind == SyntaxToken.Kind.Missing)
					{
						if (Event.current.type == EventType.repaint)
						{
							var rcMissing = new Rect(rect.xMax, rect.yMin, charSize.x * 2f, charSize.y);
							DrawWavyUnderline(rcMissing, new Color(1f, 0f, 0f, .8f));
						}
						continue;
					}
					
					int tokenTextStart = 0;
					int tokenTextLength = token.text.Length;
					while (tokenTextStart < tokenTextLength)
					{
						int rowStart = softRow > 0 ? softLineBreaks[softRow - 1] : 0;
						int rowLength = softRow < numBreaks ? softLineBreaks[softRow] - charIndex : int.MaxValue;
						int charsToDraw = Math.Min(tokenTextLength - tokenTextStart, rowLength);

						if (charsToDraw > 0)
						{
							var tokenText = token.text.Substring(tokenTextStart, charsToDraw);
							
							charIndex += charsToDraw;
							var endAtColumn = textBuffer.CharIndexToColumn(charIndex, i, rowStart);
							rect.width = charSize.x * (endAtColumn - column);
							
							tokenText = FGTextBuffer.ExpandTabs(tokenText, column);
							column = endAtColumn;
							
							if (token.style == styles.hyperlinkStyle || token.style == styles.mailtoStyle)
							{
								if (GUI.Button(rect, tokenText, token.style))
								{
									if (token.style == styles.hyperlinkStyle)
										Application.OpenURL(token.text);
									else
										Application.OpenURL("mailto:" + token.text);
								}
								
								if (Event.current.type == EventType.repaint)
								{
								// show the "Link" cursor when the mouse is hovering over this rectangle.
									EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
								}
							}
							else
							{
								token.style = GetTokenStyle(token);
								
								// Highlighting references
								if (!hasSelection && pingTimer == 0f && SISettings.referenceHighlighting)
								{
									if (highlightedSymbol != null)
									{
										if (token.parent != null && token.parent.resolvedSymbol != null &&
											token.parent.resolvedSymbol.GetGenericSymbol() == highlightedSymbol)
										{
											var referenceStyle = GetReferenceHighlightStyle(token);
											GUI.Label(rect, GUIContent.none, referenceStyle);
										}
									}
									else if (highlightedPPSymbol != null)
									{
										if (token.tokenKind == SyntaxToken.Kind.PreprocessorSymbol && token.text == highlightedPPSymbol)
										{
											var referenceStyle = GetReferenceHighlightStyle(token);
											GUI.Label(rect, GUIContent.none, referenceStyle);
										}
									}
								}
								
								if (Event.current.type == EventType.repaint)
								{
									var errorNode = token.parent;
									if (errorNode != null && token.tokenKind != SyntaxToken.Kind.Missing)
									{
										if (errorNode.syntaxError != null)
										{
											DrawWavyUnderline(rect, new Color(1f, 0f, 0f, .8f));
										}
										else if (errorNode.semanticError != null ||
											errorNode.resolvedSymbol != null && errorNode.resolvedSymbol.kind == SymbolKind.Error)
										{
											DrawWavyUnderline(rect, new Color(1f, 0f, 1f, .8f));
										}
									}
								}
								
								GUI.Label(rect, tokenText, token.style);
							}
						}

						rect.xMin = rect.xMax;

						tokenTextStart += charsToDraw;
						if (tokenTextStart < tokenTextLength)
						{
							rect.x = marginLeft;
							rect.width = 0f;
							rect.y += charSize.y;
							++softRow;
							column = 0;
						}
					}
	
					rect.xMin = rect.xMax;
				}

				//Debug.Log("Line " + i + " " + sb.ToString());
			}
		}
		
		if (Event.current.type == EventType.Repaint)
		{
			// Matching braces highlighting
			if (!IsLoading && !hasSelection && pingTimer == 0f &&
				matchingBraceLeft < matchingBraceRight &&
				matchingBraceLeft.line >= 0 && matchingBraceLeft.line < textBuffer.formatedLines.Length &&
				matchingBraceRight.line >= 0 && matchingBraceRight.line < textBuffer.formatedLines.Length &&
				matchingBraceLeft.index < textBuffer.formatedLines[matchingBraceLeft.line].tokens.Count &&
				matchingBraceRight.index < textBuffer.formatedLines[matchingBraceRight.line].tokens.Count)
			{
				var spanLeft = textBuffer.GetTokenSpan(matchingBraceLeft.line, matchingBraceLeft.index);
				var spanRight = textBuffer.GetTokenSpan(matchingBraceRight.line, matchingBraceRight.index);
				var rcLeft = GetTextRect(spanLeft);
				var rcRight = GetTextRect(spanRight);
				var charLeft = textBuffer.lines[spanLeft.line][spanLeft.index];
				var charRight = textBuffer.lines[spanRight.line][spanRight.index];
				var diff = charRight - charLeft;
				var matchingStyle = diff == 1 || diff == 2 ? styles.referenceHighlightStyle : styles.referenceModifyHighlightStyle;
				var rcLeftPlus = new Rect(rcLeft.x - 1f, rcLeft.yMax, rcLeft.width + 2f, 1f);
				var rcRightPlus = new Rect(rcRight.x - 1f, rcRight.yMax, rcRight.width + 2f, 1f);
				
				var oldColor = GUI.color;
				GUI.color = styles.normalStyle.normal.textColor;
				GUI.DrawTexture(rcLeftPlus, EditorGUIUtility.whiteTexture);
				GUI.DrawTexture(rcRightPlus, EditorGUIUtility.whiteTexture);
				GUI.color = oldColor;
				
				var tokenLeft = textBuffer.formatedLines[matchingBraceLeft.line].tokens[matchingBraceLeft.index];
				var tokenRight = textBuffer.formatedLines[matchingBraceRight.line].tokens[matchingBraceRight.index];
				
				GUI.Label(rcLeft, GUIContent.none, matchingStyle);
				GUI.Label(rcRight, GUIContent.none, matchingStyle);
				GUI.Label(rcLeft, new GUIContent(tokenLeft.text), styles.normalStyle);
				GUI.Label(rcRight, new GUIContent(tokenRight.text), styles.normalStyle);
				
				if (tokenLeft.parent != null && tokenLeft.parent.syntaxError != null)
					DrawWavyUnderline(rcLeft, new Color(1f, 0f, 0f, .8f));
				if (tokenRight.parent != null && tokenRight.parent.syntaxError != null)
					DrawWavyUnderline(rcRight, new Color(1f, 0f, 0f, .8f));
			}
		}

		bool isDragEvent = Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragUpdated;
		if (isDragEvent || Event.current.isMouse || Event.current.type == EventType.Repaint && simulateLastMouseEvent)
		{
			simulateLastMouseEvent = simulateLastMouseEvent && !Event.current.isMouse && !isDragEvent;
			Event current = simulateLastMouseEvent ? new Event(lastMouseEvent) : Event.current;
			if (!simulateLastMouseEvent)
				lastMouseEvent = new Event(Event.current);
			simulateLastMouseEvent = false;

			ProcessEditorMouse(marginLeft, current);

			if (codeViewDragging)
			{
				autoScrollLeft = !wordWrapping && lastMouseEvent.mousePosition.x < codeViewRect.x;
				autoScrollRight = !wordWrapping && lastMouseEvent.mousePosition.x >= codeViewRect.xMax;
				autoScrollUp = lastMouseEvent.mousePosition.y < codeViewRect.y;
				autoScrollDown = lastMouseEvent.mousePosition.y >= codeViewRect.yMax;
			}

			if (Event.current.type == EventType.used)
			{
				goto endScrollViewAndExit;
			}
		}

		if (hasCodeViewFocus && Event.current.type == EventType.repaint && CanEdit())
		{
			FGTextBuffer.CaretPos position = codeViewDragging && mouseDownOnSelection ? mouseDropPosition.Clone() : caretPosition.Clone();

			float caretTime = (Time.realtimeSinceStartup - caretMoveTime) % 1f; // / 1f;
			isCaretOn = caretTime < 0.5f;
			if ((isCaretOn || pingTimer > 0f && pingStartTime != 0f) && position.line >= fromLine && position.line < toLine)
			{
				int row;
				int column;
				BufferToViewPosition(position, out row, out column);
				Rect caretRect = new Rect(
					charSize.x * column + marginLeft,
					charSize.y * row + GetLineOffset(position.line),
					1,
					charSize.y);
				if (SISettings.showThickerCaret)
					caretRect.xMin -= 1f;
				GUI.Label(caretRect, GUIContent.none, isCaretOn ? styles.caretStyle : styles.scrollViewStyle);
			}
			
			if (Input.compositionString != "" && selectionStartPosition != null)
			{
				int row;
				int column;
				BufferToViewPosition(selectionStartPosition, out row, out column);
				Input.compositionCursorPos = new Vector2(charSize.x * column + marginLeft + 4f, charSize.y * row + GetLineOffset(position.line) + charSize.y + 36f) - scrollPosition;
			}
		}

		if (Event.current.type == EventType.repaint)
		{
			if (showLineNumbers || trackChanges)
			{
				rect.Set(-4f, -4f, marginLeft - 2f + smoothScrollPosition.x, contentHeight);
				EditorGUIUtility.AddCursorRect(rect, MouseCursor.Arrow);

				rect.Set(-4f, -4f, marginLeft - 1f + smoothScrollPosition.x, contentHeight);

				// if the source code is shorter than the view...
				if (rect.height < scrollViewRect.height)
					rect.height = scrollViewRect.height;
				GUI.Label(rect, GUIContent.none, styles.lineNumbersBackground);

				rect.xMin = marginLeft - 5f + smoothScrollPosition.x;
				rect.width = 1f;
				GUI.Label(rect, GUIContent.none, styles.lineNumbersSeparator);
			}

			if (showLineNumbers)
			{
				var cache = lineNumberCachedStrings ?? (lineNumberCachedStrings = new string[textBuffer.lines.Count]);
				if (cache.Length < textBuffer.lines.Count)
					Array.Resize(ref cache, textBuffer.lines.Count);

				if (wordWrapping /*&& !IsLoading*/)
				{
					for (int i = fromLine; i < toLine; ++i)
					{
						GUIContent lineNumberContent = new GUIContent(cache[i] ?? (cache[i] = (i + 1).ToString()));
						rect.Set(smoothScrollPosition.x, GetLineOffset(i), lineNumbersWidth, charSize.y);
						styles.lineNumbersStyle.Draw(rect, lineNumberContent, caretPosition.line == i, false, false, false);
					}
				}
				else
				{
					for (int i = fromLine; i < toLine; ++i)
					{
						GUIContent lineNumberContent = new GUIContent(cache[i] ?? (cache[i] = (i + 1).ToString()));
						rect.Set(smoothScrollPosition.x, charSize.y * i, lineNumbersWidth, charSize.y);
						styles.lineNumbersStyle.Draw(rect, lineNumberContent, caretPosition.line == i, false, false, false);
					}
				}

				lineNumberCachedStrings = cache;
			}

			if (trackChanges)
			{
				rect.xMin = marginLeft - 13f + smoothScrollPosition.x;
				//tempRC.yMin = GetLineOffset(fromLine);
				rect.width = 5f;
				//tempRC.height = GetLineOffset(fromLine + 1) - tempRC.yMin;

				for (int i = fromLine; i < toLine; ++i)
				{
					int savedVersion = textBuffer.formatedLines[i].savedVersion;
					int version = textBuffer.formatedLines[i].lastChange;
					if (savedVersion > 0 || version > 0)
					{
						rect.yMin = GetLineOffset(i);
						rect.yMax = GetLineOffset(i + 1);

						if (version == savedVersion)
							GUI.Label(rect, GUIContent.none, styles.trackChangesAfterSaveStyle);
						else if (version > savedVersion)
							GUI.Label(rect, GUIContent.none, styles.trackChangesBeforeSaveStyle);
						else
							GUI.Label(rect, GUIContent.none, styles.trackChangesRevertedStyle);
					}
				}
			}
		}

		if (Event.current.type == EventType.repaint)
			EditorGUIUtility.AddCursorRect(wordWrapping ? new Rect(contentRect.x, contentRect.y, contentRect.width, contentHeight) : contentRect, MouseCursor.Text);

		if (Event.current.type == EventType.repaint && pingTimer > 0f)
		{
			int row;
			int column;
			BufferToViewPosition(caretPosition, out row, out column);
			var rcPing = scrollToRect;
			rcPing.y = charSize.y * row + GetLineOffset(caretPosition.line);
			
			DrawPing(marginLeft, rcPing, false);
			if (pingTimer > 0f)
				Repaint();
		}

	endScrollViewAndExit:

		if (smoothScrollPosition != scrollPosition)
			Repaint();
		
		if (tryArgumentsHint)
			UpdateArgumentsHint(true);
		else if (argumentsHint != null && caretMoveTime == Time.realtimeSinceStartup)
			UpdateArgumentsHint(true);
		tryArgumentsHint = false;
		
		GUI.EndScrollView();
	}
	
	private GUIStyle GetTokenStyle(SyntaxToken token)
	{
		var tokenStyle = token.style ?? textBuffer.styles.normalStyle;
		if (token.tokenKind == SyntaxToken.Kind.ContextualKeyword)
		{
			tokenStyle = token.text == "value" ? textBuffer.styles.parameterStyle : textBuffer.styles.keywordStyle;
			if (token.text == "var" && token.parent != null && (token.parent.resolvedSymbol == null || token.parent.resolvedSymbol.kind == SymbolKind.Error))
				FGResolver.ResolveNode(token.parent.parent);
			return tokenStyle;
		}
		
		var leaf = token.parent;
		if (leaf != null && leaf.parent != null)
		{
			if (token.tokenKind == SyntaxToken.Kind.Keyword)
			{
				if ((token.text == "base" || token.text == "this") && (leaf.resolvedSymbol == null || leaf.syntaxError != null))
					FGResolver.ResolveNode(leaf.parent);
			}
			else if (token.tokenKind == SyntaxToken.Kind.Identifier)
			{
				if (leaf.resolvedSymbol == null || leaf.syntaxError != null)
					FGResolver.ResolveNode(leaf.parent);
				
				if (leaf.resolvedSymbol != null)
				{
					switch (leaf.resolvedSymbol.kind)
					{
					case SymbolKind.Null:
						tokenStyle = textBuffer.styles.builtInLiteralsStyle;
						break;
					case SymbolKind.Namespace:
						tokenStyle = textBuffer.styles.namespaceStyle;
						break;
					case SymbolKind.Class:
						tokenStyle = textBuffer.styles.referenceTypeStyle;
						break;
					case SymbolKind.Struct:
						tokenStyle = textBuffer.styles.valueTypeStyle;
						break;
					case SymbolKind.Interface:
						tokenStyle = textBuffer.styles.interfaceTypeStyle;
						break;
					case SymbolKind.Enum:
						tokenStyle = textBuffer.styles.enumTypeStyle;
						break;
					case SymbolKind.Delegate:
						tokenStyle = textBuffer.styles.delegateTypeStyle;
						break;
					case SymbolKind.Parameter:
					case SymbolKind.CatchParameter:
						tokenStyle = textBuffer.styles.parameterStyle;
						break;
					case SymbolKind.TypeParameter:
						tokenStyle = textBuffer.styles.typeParameterStyle;
						break;
					case SymbolKind.Label:
					case SymbolKind.EnumMember:
						tokenStyle = textBuffer.styles.enumMemberStyle;
						break;
					case SymbolKind.Method:
					case SymbolKind.MethodGroup:
					case SymbolKind.Accessor:
					case SymbolKind.Constructor:
					case SymbolKind.Destructor:
						tokenStyle = textBuffer.styles.methodStyle;
						break;
					case SymbolKind.ForEachVariable:
					case SymbolKind.FromClauseVariable:
					case SymbolKind.Variable:
					case SymbolKind.LocalConstant:
						tokenStyle = textBuffer.styles.variableStyle;
						break;
					case SymbolKind.Event:
						tokenStyle = textBuffer.styles.eventStyle;
						break;
					case SymbolKind.Property:
						tokenStyle = textBuffer.styles.propertyStyle;
						break;
					case SymbolKind.ConstantField:
					case SymbolKind.Field:
						if (leaf.resolvedSymbol.parentSymbol != null &&
							leaf.resolvedSymbol.parentSymbol.kind == SymbolKind.Enum)
						{
							tokenStyle = textBuffer.styles.enumMemberStyle;
						}
						else
						{
							var typeOfSymbol = leaf.resolvedSymbol.TypeOf();
							if (typeOfSymbol != null && typeOfSymbol.kind == SymbolKind.Delegate)
								tokenStyle = textBuffer.styles.eventStyle;
							else
								tokenStyle = textBuffer.styles.fieldStyle;
						}
						break;
					}
				}
			}
		}
		
		return tokenStyle;
	}

	private static void DrawWavyUnderline(Rect rect, Color color)
	{
		var oldColor = GUI.color;
		GUI.color = color;

		rect.yMin = rect.yMax - 2f;
		rect.yMax += 1f;
		GUI.DrawTextureWithTexCoords(rect, wavyUnderline, new Rect(rect.xMin / 6f, 0f, rect.width / 6f, 1f));

		GUI.color = oldColor;
	}

	private void DrawSelectionRect(int line, int startColumn, int numColumns)
	{
		if (!wordWrapping)
		{
			Rect selectionRect = new Rect(charSize.x * startColumn + marginLeft, charSize.y * line, charSize.x * numColumns, charSize.y);
			GUI.Label(selectionRect, GUIContent.none, hasCodeViewFocus ? styles.activeSelectionStyle : styles.passiveSelectionStyle);

			if (!codeViewDragging)
			{
				// show the "Arrow" cursor when the mouse is hovering over this rectangle.
				EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.MoveArrow);
			}
		}
		else
		{
			float yOffset = GetLineOffset(line);

			List<int> softLineBreaks = GetSoftLineBreaks(line);
			int row = FindFirstIndexGreaterThanOrEqualTo<int>(softLineBreaks, startColumn);
			if (row < softLineBreaks.Count && startColumn == softLineBreaks[row])
				++row;

			int rowStart = row > 0 ? softLineBreaks[row - 1] : 0;
			startColumn -= rowStart;
			while (numColumns > 0)
			{
				int rowLength = (row < softLineBreaks.Count ? softLineBreaks[row] - rowStart : startColumn + numColumns);
				int nCols = Math.Min(numColumns, rowLength - startColumn);

				Rect selectionRect = new Rect(charSize.x * startColumn + marginLeft, yOffset + charSize.y * row, charSize.x * nCols, charSize.y);
				GUI.Label(selectionRect, GUIContent.none, hasCodeViewFocus ? styles.activeSelectionStyle : styles.passiveSelectionStyle);
				if (!codeViewDragging)
					EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.MoveArrow);

				numColumns -= nCols;
				rowStart += rowLength;
				startColumn = 0;
				++row;
			}
		}
	}

	private void DrawSelectionRectCharIndex(int line, int startCharIndex, int numChars, bool newLine, GUIStyle style)
	{
		if (style == null)
			style = hasCodeViewFocus ? styles.activeSelectionStyle : styles.passiveSelectionStyle;

		if (!wordWrapping)
		{
			var fromColumn = textBuffer.CharIndexToColumn(startCharIndex, line);
			var toColumn = textBuffer.CharIndexToColumn(startCharIndex + numChars, line) + (newLine ? 1 : 0);
			var selectionRect = new Rect(charSize.x * fromColumn + marginLeft, charSize.y * line, charSize.x * (toColumn - fromColumn), charSize.y);
			GUI.Label(selectionRect, GUIContent.none, style);

			if (!codeViewDragging)
			{
				// show the "Arrow" cursor when the mouse is hovering over this rectangle.
				EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.MoveArrow);
			}
		}
		else
		{
			var yOffset = GetLineOffset(line);
			var softLineBreaks = GetSoftLineBreaks(line);

			var row = FindFirstIndexGreaterThanOrEqualTo<int>(softLineBreaks, startCharIndex);
			if (row < softLineBreaks.Count && startCharIndex == softLineBreaks[row])
				++row;
			var rowStart = row > 0 ? softLineBreaks[row - 1] : 0;

			if (newLine && numChars == 0 && startCharIndex == textBuffer.lines[line].Length)
			{
				var fromColumn = textBuffer.CharIndexToColumn(startCharIndex, line, rowStart);
				//var toColumn = fromColumn + 1;
				var selectionRect = new Rect(charSize.x * fromColumn + marginLeft, yOffset + row * charSize.y, charSize.x, charSize.y);
				GUI.Label(selectionRect, GUIContent.none, style);
				if (!codeViewDragging)
					EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.MoveArrow);

				return;
			}

			yOffset += charSize.y * row;
			startCharIndex -= rowStart;
			while (numChars > 0)
			{
				var rowLength = (row < softLineBreaks.Count ? softLineBreaks[row] - rowStart : startCharIndex + numChars);
				var nChars = Math.Min(numChars, rowLength - startCharIndex);

				var fromColumn = textBuffer.CharIndexToColumn(rowStart + startCharIndex, line, rowStart);
				var toColumn = textBuffer.CharIndexToColumn(rowStart + startCharIndex + nChars, line, rowStart)
					+ (numChars == nChars && newLine ? 1 : 0);
				var selectionRect = new Rect(charSize.x * fromColumn + marginLeft, yOffset, charSize.x * (toColumn - fromColumn), charSize.y);
				GUI.Label(selectionRect, GUIContent.none, style);
				if (!codeViewDragging)
					EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.MoveArrow);

				numChars -= nChars;
				rowStart += rowLength;
				startCharIndex = 0;
				++row;
				yOffset += charSize.y;
			}
		}
	}
	
	public GUIStyle GetReferenceHighlightStyle(SyntaxToken token)
	{
		var style = styles.referenceHighlightStyle;
		if (!SISettings.highlightWritesInRed)
			return style;
		
		if (highlightedSymbol is InstanceDefinition)
		{
			var parent = token.parent.parent;
			var parentRule = parent.RuleName;
			switch (highlightedSymbol.kind)
			{
			case SymbolKind.Field:
			case SymbolKind.Property:
			case SymbolKind.Parameter:
			case SymbolKind.ForEachVariable:
			case SymbolKind.FromClauseVariable:
			case SymbolKind.Variable:
			case SymbolKind.LocalConstant:
			case SymbolKind.ConstantField:
			case SymbolKind.Event:
			case SymbolKind.CatchParameter:
				if (parentRule == "localVariableDeclarator")
				{
					if (parent.numValidNodes == 1)
						break;
				}
				else if (parentRule == "variableDeclarator" || parentRule == "eventDeclarator")
				{
					// fields are always initialized
				}
				else if (parentRule == "foreachStatement")
				{
					// always initialized
				}
				else if (parentRule == "memberInitializer")
				{
					// always initialized
				}
				else if (parentRule == "fixedParameter" || parentRule == "parameterArray")
				{
					// parameters are always initialized
				}
				else if (parentRule == "constantDeclarator")
				{
					// always initialized
				}
				else if (parentRule == "eventDeclarator")
				{
					// always initialized
				}
				else if (parentRule == "catchExceptionIdentifier")
				{
					// always initialized
				}
				else if (parentRule == "qidStart")
				{
					if (parent.childIndex < parent.parent.numValidNodes - 1)
						break;
					if (parent.numValidNodes == 3 && token.parent.childIndex != 2)
						break;
					// only the last token in a qid sequence is always initialized
				}
				else if (parentRule == "accessIdentifier" && parent.parent.RuleName == "qidPart")
				{
					if (parent.parent.childIndex < parent.parent.parent.numValidNodes - 1)
						break;
					// only the last token in a qid sequence is always initialized
				}
				else if (parentRule == "primaryExpressionStart" && parent.parent.numValidNodes == 1 ||
					parentRule == "accessIdentifier" && parent.parent.RuleName == "primaryExpressionPart" && parent.parent.childIndex == parent.parent.parent.numValidNodes - 1)
				{
					var primaryExpressionNode = parentRule == "accessIdentifier" ? parent.parent.parent : parent.parent;
					var incrementExpressionNode = primaryExpressionNode.parent.parent;
					parentRule = incrementExpressionNode.RuleName;
					if (parentRule != "preIncrementExpression" && parentRule != "preDecrementExpression")
					{
						var nextLeaf = primaryExpressionNode.parent.LeafAt(1);
						if (nextLeaf == null || !nextLeaf.IsLit("++") && !nextLeaf.IsLit("--"))
						{
							if (parentRule != "assignment" || primaryExpressionNode.parent.childIndex != 0)
							{
								while (incrementExpressionNode != null && incrementExpressionNode.RuleName != "expression")
									incrementExpressionNode = incrementExpressionNode.parent;
								if (incrementExpressionNode == null || incrementExpressionNode.parent.RuleName != "variableReference")
									break;
							}
						}
					}
				}
				else
				{
					var prevLeaf = token.parent.FindPreviousLeaf();
					if (prevLeaf == null || !prevLeaf.IsLit("ref") && !prevLeaf.IsLit("out"))
					{
						var nextLeaf = token.parent.FindNextLeaf();
						if (nextLeaf == null || nextLeaf.parent.RuleName != "assignmentOperator")
						{
							break;
						}
					}
				}

				style = styles.referenceModifyHighlightStyle;
				break;
            }
        }
		return style;
	}

	public void ValidateCarets()
	{
		if (CanEdit())
		{
			ValidateCaret(ref caretPosition);
			if (hasSelection)
				ValidateCaret(ref _selectionStartPosition);
			Repaint();
		}
	}

	private bool ValidateCaret(ref FGTextBuffer.CaretPos caret)
	{
		if (caret != null && textBuffer.lines.Count > 0)
		{
			if (caret.line < 0)
			{
				return false;
			}
			else if (caret.line >= textBuffer.lines.Count)
			{
				caret = new FGTextBuffer.CaretPos() { line = textBuffer.lines.Count - 1, characterIndex = 0, column = 0, virtualColumn = 0 };
				return false;
			}
			else if (caret.characterIndex > textBuffer.lines[caret.line].Length)
			{
				caret = new FGTextBuffer.CaretPos() { line = caret.line, characterIndex = 0, column = 0, virtualColumn = 0 };
				return false;
			}
			else
			{
				caret.column = caret.virtualColumn = CharIndexToColumn(caret.characterIndex, caret.line);
			}
		}
		return true;
	}
	
	private Rect GetTokenRect(SyntaxToken token)
	{
		Rect result = new Rect();
		
		if (textBuffer == null || token.parent == null)
			return result;
		
		var tokenSpan = textBuffer.GetTokenSpan(token.parent.line, token.parent.tokenIndex);		
		result = GetTextRect(tokenSpan);
		
		Vector2 screenPoint = GUIUtility.GUIToScreenPoint(new Vector2(result.xMin, result.yMin));
		result.x = screenPoint.x;
		result.y = screenPoint.y;
		
		return result;
	}
	
	private Rect GetTextRect(TextSpan span)
	{
		Rect result;
		int row = span.line, column = span.index;
		if (wordWrapping)
		{
			BufferToViewPosition(
				new FGTextBuffer.CaretPos { characterIndex = span.StartPosition.index, line = span.line },
				out row, out column);
			result = new Rect(charSize.x * column + marginLeft, charSize.y * row + GetLineOffset(span.line), charSize.x * span.indexOffset, charSize.y);
		}
		else
		{
			column = textBuffer.CharIndexToColumn(column, row);
			result = new Rect(charSize.x * column + marginLeft, charSize.y * row, charSize.x * span.indexOffset, charSize.y);
		}
		
		return result;
	}
	
	[NonSerialized]
	private bool wordSelectMode = false;
	[NonSerialized]
	private FGTextBuffer.CaretPos anchorWordStart;
	[NonSerialized]
	private FGTextBuffer.CaretPos anchorWordEnd;
	[NonSerialized]
	private bool lineSelectMode = false;
	[NonSerialized]
	private bool mouseIsDown = false;
	[NonSerialized]
	public static float lastDoubleClickTime = 0;

	[NonSerialized]
	public SyntaxToken mouseHoverToken;
	[NonSerialized]
	private Rect mouseHoverTokenRect;
	[NonSerialized]
	public float mouseHoverTime;
	[NonSerialized]
	public FGTooltip tokenTooltip;
	
	[NonSerialized]
	public FGTooltip argumentsHint;
	
	private void ProcessEditorMouse(float margin, Event current)
	{		
		if (current.type == EventType.mouseDrag || current.type == EventType.mouseDown)
			pingTimer = 0f;
		
		if (!CanEdit())
			return;

		if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != codeViewControlID && DragAndDrop.GetGenericData("ScriptInspector.Text") == null)
			return;

		if (current.type == EventType.MouseDown && current.button == 0)
		{
			mouseIsDown = true;
		}
		if (!mouseIsDown && current.button == 0 && current.rawType != EventType.MouseUp && current.type != EventType.MouseMove)
			return;
		if (current.rawType == EventType.MouseUp && current.button == 0)
		{
			mouseIsDown = false;
			codeViewDragging = false;
			lineSelectMode = false;
		}

		EventType eventType = current.type;
		EventModifiers modifiers = current.modifiers & ~(EventModifiers.CapsLock | EventModifiers.Numeric);
		bool isDrag = eventType == EventType.mouseDrag && current.button == 0;

		int nextCaretColumn = caretPosition.virtualColumn;
		int nextCharacterIndex = caretPosition.characterIndex;
		int nextCaretLine = caretPosition.line;
		FGTextBuffer.CaretPos nextCaretPos = caretPosition.Clone();

		float x = current.mousePosition.x;
		float y = current.mousePosition.y;
		if (isDrag)
		{
			x = Mathf.Clamp(x, codeViewRect.x, codeViewRect.xMax);
			y = Mathf.Clamp(y, codeViewRect.y, codeViewRect.yMax);
		}
		x -= margin;

		var clickedColumn = Mathf.RoundToInt(x / charSize.x);
		var mouseOverColumn = (int)(x / charSize.x);
		int clickedLine;
		int clickedCharIndex;
		int mouseOverCharIndex;
		FGTextBuffer.CaretPos clickedPos;
		FGTextBuffer.CaretPos mouseOverPos;
		if (!wordWrapping)
		{
			clickedLine = Math.Min((int)(y / charSize.y), textBuffer.lines.Count - 1);
			if (mouseOverColumn == clickedColumn)
			{
				clickedCharIndex = textBuffer.ColumnToCharIndex(ref clickedColumn, clickedLine);			
				clickedPos = new FGTextBuffer.CaretPos
				{
					line = clickedLine,
					virtualColumn = clickedColumn,
					column = clickedColumn,
					characterIndex = clickedCharIndex
				};
				
				mouseOverPos = clickedPos;
				mouseOverCharIndex = clickedCharIndex;
			}
			else
			{
				clickedCharIndex = textBuffer.ColumnToCharIndex(ref clickedColumn, clickedLine);
				clickedPos = new FGTextBuffer.CaretPos
				{
					line = clickedLine,
					virtualColumn = clickedColumn,
					column = clickedColumn,
					characterIndex = clickedCharIndex
				};
				
				mouseOverCharIndex = textBuffer.ColumnToCharIndex(ref mouseOverColumn, clickedLine);
				mouseOverPos = new FGTextBuffer.CaretPos
				{
					line = clickedLine,
					virtualColumn = mouseOverColumn,
					column = mouseOverColumn,
					characterIndex = mouseOverCharIndex
				};
			}
		}
		else
		{
			clickedLine = Mathf.Min(GetLineAt(y), textBuffer.lines.Count - 1);
			int row = (int) ((y - GetLineOffset(clickedLine)) / charSize.y);
			clickedPos = ViewToBufferPosition(clickedLine, row, clickedColumn);
			clickedCharIndex = clickedPos.characterIndex;
			
			if (mouseOverColumn == clickedColumn)
			{
				mouseOverPos = clickedPos;
				mouseOverCharIndex = clickedCharIndex;
			}
			else
			{
				mouseOverPos = ViewToBufferPosition(clickedLine, row, mouseOverColumn);
				mouseOverCharIndex = mouseOverPos.characterIndex;
			}
			
			clickedColumn = clickedPos.column;
		}

		if (textBuffer.isCsFile
			&& current.type == EventType.MouseMove
			&& modifiers == 0
			&& fullCodeViewRect.Contains(current.mousePosition)
			&& mouseOverCharIndex < textBuffer.lines[clickedLine].Length)
		{
			int tokenLine, tokenIndex;
			bool atTokenEnd;
			var textPos = new TextPosition(clickedLine, mouseOverCharIndex + 1);
			var hoverToken = textBuffer.GetTokenAt(textPos, out tokenLine, out tokenIndex, out atTokenEnd);
			if (hoverToken != mouseHoverToken)
			{
				mouseHoverTime = 0f;
				mouseHoverToken = null;
				if (tokenTooltip != null)
					tokenTooltip.Hide();

				if (hoverToken != null && hoverToken.parent != null
					&& (hoverToken.style == styles.referenceTypeStyle
					|| hoverToken.parent.syntaxError != null
					|| hoverToken.parent.semanticError != null
					|| hoverToken.tokenKind >= SyntaxToken.Kind.BuiltInLiteral
						&& (hoverToken.tokenKind != SyntaxToken.Kind.Keyword
				    		|| hoverToken.text == "this" || hoverToken.text == "base"
							|| textBuffer.Parser.IsBuiltInType(hoverToken.text)
							|| textBuffer.Parser.IsBuiltInLiteral(hoverToken.text))
						&& hoverToken.tokenKind != SyntaxToken.Kind.Punctuator))
				{
					mouseHoverTokenRect = GetTokenRect(hoverToken);
					Vector2 mouseScreenPoint = GUIUtility.GUIToScreenPoint(current.mousePosition);
					if (mouseHoverTokenRect.Contains(mouseScreenPoint))
					{
						mouseHoverToken = hoverToken;
						mouseHoverTime = hoverToken != null ? Time.realtimeSinceStartup : 0;
					}
				}
			}
		}
		else
		{
			mouseHoverTime = 0f;
			mouseHoverToken = null;
		}

		if (eventType == EventType.DragPerform || eventType == EventType.DragUpdated)
		{
			int mousePosToCaretPos = clickedPos.CompareTo(caretPosition);
			int mousePosToSelStartPos = selectionStartPosition != null ? clickedPos.CompareTo(selectionStartPosition) : mousePosToCaretPos;
			bool mouseOnSelection = !((mousePosToCaretPos < 0 && mousePosToSelStartPos < 0) || (mousePosToCaretPos > 0 && mousePosToSelStartPos > 0));
			if (EditorGUI.actionKey && (mousePosToCaretPos == 0 || mousePosToSelStartPos == 0))
				mouseOnSelection = false;

			DragAndDrop.visualMode = mouseOnSelection ? DragAndDropVisualMode.Rejected : EditorGUI.actionKey ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Move;

			if (eventType == EventType.DragPerform)
			{
				object data = DragAndDrop.GetGenericData("ScriptInspector.Text");
				if (!string.IsNullOrEmpty(data as string) && TryEdit())
				{
					textBuffer.BeginEdit("Drag selection");

					if (!EditorGUI.actionKey)
					{
						FGTextBuffer.CaretPos temp = textBuffer.DeleteText(selectionStartPosition, caretPosition);
						textBuffer.UpdateHighlighting(temp.line, temp.line);

						if (mouseDropPosition > caretPosition)
						{
							int linesDeleted = Math.Abs(caretPosition.line - selectionStartPosition.line);
							if (linesDeleted == 0)
							{
								if (caretPosition.line == mouseDropPosition.line)
									mouseDropPosition.characterIndex -= Math.Abs(caretPosition.characterIndex - selectionStartPosition.characterIndex);
							}
							else
							{
								if (Math.Max(caretPosition.line, selectionStartPosition.line) == mouseDropPosition.line)
								{
									mouseDropPosition.line = temp.line;
									mouseDropPosition.characterIndex -= (caretPosition > selectionStartPosition ?
										caretPosition.characterIndex : selectionStartPosition.characterIndex) - temp.characterIndex;
								}
								else
								{
									mouseDropPosition.line -= linesDeleted;
								}
							}
							mouseDropPosition.column = mouseDropPosition.virtualColumn = CharIndexToColumn(mouseDropPosition.characterIndex, mouseDropPosition.line);
						}
					}

					caretPosition = textBuffer.InsertText(mouseDropPosition, data as string);
					if (wordWrapping)
						caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
					textBuffer.UpdateHighlighting(mouseDropPosition.line, caretPosition.line);
					selectionStartPosition = mouseDropPosition.Clone();

					textBuffer.EndEdit();
				}
			}

			if (eventType != EventType.DragUpdated)
			{
				DragAndDrop.AcceptDrag();
				DragAndDrop.SetGenericData("ScriptInspector.Text", null);

				GUIUtility.hotControl = 0;
				lineSelectMode = false;
				codeViewDragging = false;
				mouseDownOnSelection = false;
				autoScrollLeft = false;
				autoScrollRight = false;
				autoScrollUp = false;
				autoScrollDown = false;
				current.Use();
				return;
			}

			if (mouseOnSelection)
				clickedPos = caretPosition.Clone();

			if (clickedPos != mouseDropPosition)
			{
				mouseDropPosition = clickedPos.Clone();
				caretMoveTime = Time.realtimeSinceStartup;
				lastCaretMoveWasSearch = false;
				//scrollToCaret = true;
				needsRepaint = true;
			}

			//GUIUtility.hotControl = codeViewControlID;
			focusCodeView = true;
			current.Use();
			return;
		}

		if (!codeViewDragging && current.mousePosition.y >= 0 &&
			eventType == EventType.MouseDown &&
			(isDrag && current.mousePosition.x >= codeViewRect.x || eventType == EventType.MouseDown && current.mousePosition.x >= 0))
		{
			if (hasSelection)
			{
				int clickedPosToCaretPos = mouseOverPos.CompareTo(caretPosition);
				int clickedPosToSelStartPos = mouseOverPos.CompareTo(selectionStartPosition);
				mouseDownOnSelection = !((clickedPosToCaretPos < 0 && clickedPosToSelStartPos < 0) || (clickedPosToCaretPos >= 0 && clickedPosToSelStartPos >= 0));
			}
			else
			{
				mouseDownOnSelection = false;
			}
		}

		if (isDrag && current.button == 0)
		{
			if (!codeViewDragging)
			{
//				if (!codeViewRect.Contains(current.mousePosition))
//					return;

				lastAutoScrollTime = Time.realtimeSinceStartup;

				if (mouseDownOnSelection && !current.shift)
				{
					DragAndDrop.PrepareStartDrag();
					DragAndDrop.objectReferences = new UnityEngine.Object[] { textBuffer };
					DragAndDrop.StartDrag("Dragging selected text");
					DragAndDrop.SetGenericData("ScriptInspector.Text", textBuffer.GetTextRange(selectionStartPosition, caretPosition));

					GUIUtility.hotControl = 0;
					current.Use();
					codeViewDragging = true;
					mouseDropPosition = caretPosition.Clone();
					return;
				}
				else
				{
					mouseDownOnSelection = false;
				}
			}
			codeViewDragging = true;
			//needsRepaint = true;
		}
		else if (current.button == 1)
		{
			if (eventType == EventType.mouseDown && current.mousePosition.x >= 0 && current.mousePosition.y >= 0)
			{
				current.Use();
				if (!mouseDownOnSelection)
				{
					nextCharacterIndex = clickedPos.characterIndex;
					nextCaretColumn = clickedColumn;
					nextCaretLine = clickedLine;
					nextCaretPos = clickedPos.Clone();
					scrollToCaret = true;
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}
		}

		if (isDrag || eventType == EventType.mouseDown)
		{
			GUIUtility.hotControl = codeViewControlID;
			focusCodeView = true;
			if (current.mousePosition.x >= 0 && current.mousePosition.y >= 0 || isDrag)
			{
				if (!isDrag && current.button == 0)
				{
					if (current.mousePosition.x < margin + scrollPosition.x)
					{
						lineSelectMode = true;
						wordSelectMode = false;
						anchorWordStart = anchorWordEnd = null;
					}
					else if (Time.realtimeSinceStartup > lastDoubleClickTime &&
						Time.realtimeSinceStartup <= lastDoubleClickTime + 0.5f)
					{
						lineSelectMode = true;
						wordSelectMode = false;
						anchorWordStart = anchorWordEnd = null;
						current.clickCount = 3;
						mouseDownOnSelection = false;
					}
				}

				scrollToCaret = !isDrag && !mouseDownOnSelection;

				nextCharacterIndex = clickedCharIndex;
				nextCaretColumn = clickedColumn;
				nextCaretLine = clickedLine;
				nextCaretPos = clickedPos.Clone();
					
				if (current.button == 0)
				{
					if (current.clickCount == 1 && mouseDownOnSelection)
					{
						needsRepaint = true;
						return;
					}

					if (!lineSelectMode && (current.clickCount == 2 || EditorGUI.actionKey || wordSelectMode))
					{
						if (current.clickCount == 2)
							lastDoubleClickTime = Time.realtimeSinceStartup;
						
						int wordStart;
						int wordEnd;
						if (textBuffer.GetWordExtents(nextCharacterIndex, clickedLine, out wordStart, out wordEnd))
						{
							// select word
							if (anchorWordStart != null && ((modifiers & EventModifiers.Shift) != 0 || wordSelectMode))
							{
								var forward = clickedLine > anchorWordStart.line ||
									clickedLine == anchorWordStart.line && wordStart >= anchorWordStart.characterIndex;
								if (forward)
								{
									selectionStartPosition = anchorWordStart.Clone();
									nextCharacterIndex = wordEnd;
									nextCaretColumn = CharIndexToColumn(wordEnd, clickedLine);
									nextCaretPos = clickedPos.Clone();
									nextCaretPos.column = nextCaretColumn;
									nextCaretPos.characterIndex = nextCharacterIndex;
								}
								else
								{
									selectionStartPosition = anchorWordEnd.Clone();
									nextCharacterIndex = wordStart;
									nextCaretColumn = CharIndexToColumn(wordStart, clickedLine);
									nextCaretPos = clickedPos.Clone();
									nextCaretPos.column = nextCaretColumn;
									nextCaretPos.characterIndex = nextCharacterIndex;
								}
							}
							else
							{
								if (selectionStartPosition != null)
									caretPosition = selectionStartPosition;
								selectionStartPosition = null;
								
								if ((modifiers & EventModifiers.Shift) == 0)
								{
									caretPosition.line = clickedLine;
									caretPosition.characterIndex = wordStart;
									caretPosition.virtualColumn = caretPosition.column = CharIndexToColumn(wordStart, clickedLine);
									nextCharacterIndex = wordEnd;
									nextCaretColumn = CharIndexToColumn(wordEnd, clickedLine);
									nextCaretPos = clickedPos.Clone();
									nextCaretPos.column = nextCaretColumn;
									nextCaretPos.characterIndex = nextCharacterIndex;
									
									anchorWordStart = caretPosition.Clone();
									anchorWordEnd = nextCaretPos.Clone();
									anchorWordEnd.virtualColumn = anchorWordEnd.column;
								}
								else
								{
									bool forward = clickedLine > caretPosition.line ||
										clickedLine == caretPosition.line && clickedCharIndex > caretPosition.characterIndex;
									
									int fromWordStart;
									int fromWordEnd;
									if (textBuffer.GetWordExtents(caretPosition.characterIndex, caretPosition.line, out fromWordStart, out fromWordEnd))
									{
										caretPosition.characterIndex = forward ? fromWordStart : fromWordEnd;
										caretPosition.virtualColumn = caretPosition.column = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);

										anchorWordStart = caretPosition.Clone();
										anchorWordStart.characterIndex = fromWordStart;
										anchorWordStart.virtualColumn = anchorWordStart.column = CharIndexToColumn(fromWordStart, anchorWordStart.line);
										anchorWordEnd = caretPosition.Clone();
										anchorWordEnd.characterIndex = fromWordEnd;
										anchorWordEnd.virtualColumn = anchorWordEnd.column = CharIndexToColumn(fromWordEnd, anchorWordEnd.line);
									}
									else
									{
										anchorWordStart = caretPosition.Clone();
										anchorWordEnd = caretPosition.Clone();
										Debug.Log(anchorWordEnd.characterIndex >= anchorWordStart.characterIndex ? "OK" : "WRONG");
									}
									
									nextCharacterIndex = forward ? wordEnd : wordStart;
									nextCaretColumn = CharIndexToColumn(nextCharacterIndex, clickedLine);
									nextCaretPos = clickedPos.Clone();
									nextCaretPos.column = nextCaretColumn;
									nextCaretPos.characterIndex = nextCharacterIndex;
								}
							}
							modifiers |= EventModifiers.Shift;
							
							lineSelectMode = false;
							wordSelectMode = true;
						}
					}
					else
					{
						anchorWordStart = null;
						anchorWordEnd = null;
					}
				}
			}
			current.Use();
		}

		int lineSelectOffset = 0;
		if (lineSelectMode && selectionStartPosition != null && selectionStartPosition < caretPosition)
			lineSelectOffset = -1;

		if (current.rawType == EventType.mouseUp && current.button == 0 && GUIUtility.hotControl != 0)
		{
			if (mouseDownOnSelection)
			{
				if (!codeViewDragging)
				{
					nextCharacterIndex = clickedCharIndex;
					nextCaretColumn = clickedColumn;
					nextCaretLine = clickedLine;
					nextCaretPos = clickedPos.Clone();
					--caretPosition.virtualColumn;
				}
			}

			GUIUtility.hotControl = 0;
			//autoScrolling = Vector2.zero;
			lineSelectMode = false;
			wordSelectMode = false;
			codeViewDragging = false;
			mouseDownOnSelection = false;
			autoScrollLeft = false;
			autoScrollRight = false;
			autoScrollUp = false;
			autoScrollDown = false;
			current.Use();

			needsRepaint = true;
			lineSelectOffset = 0;
		}

		//if (nextCaretColumn != caretPosition.virtualColumn ||
		//    nextCaretLine != (caretPosition.line + lineSelectOffset) ||
		//    eventType == EventType.mouseDown && current.button == 0)
		if (!nextCaretPos.IsSameAs(caretPosition) ||
			nextCaretPos.line != (caretPosition.line + lineSelectOffset) ||
			eventType == EventType.mouseDown && current.button == 0)
		{
			caretMoveTime = Time.realtimeSinceStartup;
			lastCaretMoveWasSearch = false;

			//if (nextCaretLine < 0)
			//    nextCaretLine = 0;
			if (nextCaretPos.line < 0)
				nextCaretPos.Set(0, 0, 0, 0);
			//if (nextCaretLine >= textBuffer.numParsedLines)
			//    nextCaretLine = textBuffer.numParsedLines - 1;
			if (nextCaretPos.line >= textBuffer.numParsedLines)
				nextCaretPos.Set(textBuffer.numParsedLines - 1, 0, 0, 0);
			nextCaretLine = nextCaretPos.line;

			if (selectionStartPosition == null && (isDrag || (modifiers & EventModifiers.Shift) != 0))
				selectionStartPosition = caretPosition.Clone();

			if (lineSelectMode)
			{
				if (selectionStartPosition == null || !isDrag && (modifiers & EventModifiers.Shift) == 0)
					selectionStartPosition = new FGTextBuffer.CaretPos { column = 0, virtualColumn = 0, characterIndex = 0, line = nextCaretLine };

				nextCharacterIndex = 0;
				nextCaretColumn = 0;
				if (nextCaretLine >= selectionStartPosition.line)
				{
					++nextCaretLine;
					nextCaretPos.Set(nextCaretPos.line + 1, 0, 0, 0);
					if (nextCaretLine >= textBuffer.numParsedLines)
					{
						nextCaretLine = textBuffer.numParsedLines - 1;
						nextCharacterIndex = textBuffer.lines[nextCaretLine].Length;
						nextCaretColumn = textBuffer.CharIndexToColumn(nextCharacterIndex, nextCaretLine);
						nextCaretPos.Set(textBuffer.numParsedLines - 1, textBuffer.lines[nextCaretLine].Length, nextCaretColumn);
					}
					selectionStartPosition = new FGTextBuffer.CaretPos { characterIndex = 0, column = 0, virtualColumn = 0, line = selectionStartPosition.line };
				}
				else
				{
					int charIndex = textBuffer.lines[selectionStartPosition.line].Length;
					int column = textBuffer.CharIndexToColumn(charIndex, selectionStartPosition.line);
					nextCaretPos.Set(nextCaretPos.line, 0, 0, 0);
					selectionStartPosition = new FGTextBuffer.CaretPos { characterIndex = charIndex, column = column, virtualColumn = column, line = selectionStartPosition.line };
				}
			}

			caretPosition = nextCaretPos;
			//caretPosition.line = nextCaretLine;
			if (nextCaretLine >= 0)
			{
				//caretPosition.characterIndex = nextCharacterIndex;
				//caretPosition.column = caretPosition.virtualColumn = Math.Min(nextCaretColumn, CharIndexToColumn(textBuffer.lines[nextCaretLine].Length, nextCaretLine));
				//if (wordWrapping)
				//{
				//	List<int> softLineBreaks = GetSoftLineBreaks(caretPosition.line);
				//	int row = FindFirstIndexGreaterThanOrEqualTo<int>(softLineBreaks, caretPosition.column);
				//	if (row < softLineBreaks.Count && caretPosition.column == softLineBreaks[row] && clickedPos.virtualColumn == 0)
				//		++row;
				//	caretPosition.virtualColumn -= row > 0 ? softLineBreaks[row - 1] : 0;
				//}
				caretPosition.virtualColumn = caretPosition.column;
			}

			if (!isDrag && !lineSelectMode && (modifiers & EventModifiers.Shift) == 0)
				selectionStartPosition = null;

			if (!codeViewDragging)
			{
				AddRecentLocation(11, true);
				scrollToCaret = true;
			}
			//Repaint();
			needsRepaint = true;
		}
	}

	private bool ProcessCodeViewCommands()
	{
		if (Event.current.type == EventType.ValidateCommand)
		{
			if (Event.current.commandName == "SelectAll")
			{
				Event.current.Use();
				return true;
			}
			else if (Event.current.commandName == "Copy" || Event.current.commandName == "Cut")
			{
				if (selectionStartPosition != null || SISettings.copyCutFullLine)
				{
					Event.current.Use();
					return true;
				}
			}
			else if (Event.current.commandName == "Paste")
			{
				if (!string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer))
				{
					Event.current.Use();
					return true;
				}
			}
			else if (Event.current.commandName == "Delete")
			{
				Event.current.Use();
				return true;
			}
			else if (Event.current.commandName == "Duplicate")
			{
				Event.current.Use();
				return true;
			}
			else if (Event.current.commandName == "OpenAtCursor")
			{
				Event.current.Use();
				return true;
			}
			else if (Event.current.commandName == "ScriptInspector.Autocomplete")
			{
				Event.current.Use();
				return true;
			}
		}
		else if (!CanEdit())
		{
			return false;
		}
		else if (Event.current.type == EventType.ExecuteCommand)
		{
			pingTimer = 0f;
			
			if (Event.current.commandName == "SelectAll")
			{
				selectionStartPosition = new FGTextBuffer.CaretPos { column = 0, virtualColumn = 0, characterIndex = 0, line = 0 };

				caretPosition.line = textBuffer.numParsedLines - 1;
				caretPosition.characterIndex = textBuffer.numParsedLines > 0 ? textBuffer.lines[textBuffer.numParsedLines - 1].Length : 0;
				caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, textBuffer.numParsedLines - 1);

				Event.current.Use();
				caretMoveTime = Time.realtimeSinceStartup;
				lastCaretMoveWasSearch = false;
				scrollToCaret = true;
				Repaint();
				return true;
			}
			else if (Event.current.commandName == "Paste")
			{
				if (!string.IsNullOrEmpty(EditorGUIUtility.systemCopyBuffer) && TryEdit())
				{
					var pasteFullLine = fullLineCopied == EditorGUIUtility.systemCopyBuffer;
					if (selectionStartPosition != null)
					{
						caretPosition = textBuffer.DeleteText(selectionStartPosition, caretPosition);
						selectionStartPosition = null;
						pasteFullLine = false;
					}
					
					string copyBuffer = EditorGUIUtility.systemCopyBuffer;
					copyBuffer = copyBuffer.Replace("\r\n", "\n");
					copyBuffer = copyBuffer.Replace('\r', '\n');

					textBuffer.BeginEdit("Paste");
					int insertedAtLine = caretPosition.line;
					bool emptyLine = textBuffer.FirstNonWhitespace(insertedAtLine) == textBuffer.lines[insertedAtLine].Length;
					if (pasteFullLine)
					{
						textBuffer.InsertText(new FGTextBuffer.CaretPos { line = insertedAtLine }, copyBuffer);
						++caretPosition.line;
					}
					else
					{
						caretPosition = textBuffer.InsertText(caretPosition, copyBuffer);
						if (wordWrapping)
							caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
					}
					textBuffer.UpdateHighlighting(insertedAtLine, caretPosition.line);
					if (insertedAtLine < caretPosition.line || emptyLine)
						ReindentLines(insertedAtLine, caretPosition.line);
					textBuffer.EndEdit();
					
					AddRecentLocation(0, true);
					tryArgumentsHint = true;
					
					Event.current.Use();
					caretMoveTime = Time.realtimeSinceStartup;
					lastCaretMoveWasSearch = false;
					scrollToCaret = true;
					Repaint();
					return true;
				}
			}
			else if (Event.current.commandName == "Copy" || Event.current.commandName == "Cut" && TryEdit())
			{
				if (selectionStartPosition != null)
				{
					fullLineCopied = null;
					EditorGUIUtility.systemCopyBuffer = textBuffer.GetTextRange(caretPosition, selectionStartPosition);

					if (Event.current.commandName == "Cut")
					{
						textBuffer.BeginEdit("Cut Selection");
						caretPosition = textBuffer.DeleteText(selectionStartPosition, caretPosition);
						if (wordWrapping)
							caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
						textBuffer.UpdateHighlighting(caretPosition.line, caretPosition.line);
						textBuffer.EndEdit();
						selectionStartPosition = null;
						
						AddRecentLocation(0, true);
						tryArgumentsHint = true;
						
						caretMoveTime = Time.realtimeSinceStartup;
						lastCaretMoveWasSearch = false;
						scrollToCaret = true;
						Repaint();
					}
				}
				else if (SISettings.copyCutFullLine)
				{
					var fullLineToCopy = textBuffer.lines[caretPosition.line];
					if (fullLineToCopy != "")
					{
						fullLineCopied = fullLineToCopy + '\n';
						EditorGUIUtility.systemCopyBuffer = fullLineCopied;
						
						if (Event.current.commandName == "Cut")
						{
							textBuffer.BeginEdit("Cut Line");
							var cutTo = new FGTextBuffer.CaretPos { line = caretPosition.line + 1 };
							if (textBuffer.lines.Count == cutTo.line)
							{
								cutTo.characterIndex = textBuffer.lines[textBuffer.lines.Count - 1].Length;
								--cutTo.line;
								cutTo.column = cutTo.virtualColumn = CharIndexToColumn(cutTo.characterIndex, cutTo.line);
							}
							caretPosition = textBuffer.DeleteText(new FGTextBuffer.CaretPos{ line = caretPosition.line }, cutTo);
							if (wordWrapping)
								caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
							textBuffer.UpdateHighlighting(caretPosition.line, caretPosition.line);
							textBuffer.EndEdit();
							
							AddRecentLocation(0, true);
							
							caretMoveTime = Time.realtimeSinceStartup;
							lastCaretMoveWasSearch = false;
							scrollToCaret = true;
							Repaint();
						}
					}
				}
				
				Event.current.Use();
				return true;
			}
			else if (Event.current.commandName == "Delete" && TryEdit())
			{
				Event simKey = new Event();
				simKey.type = EventType.keyDown;
				simKey.keyCode = KeyCode.Delete;
				simKey.modifiers = EventModifiers.Shift;
				ProcessEditorKeyboard(simKey, true);

				Event.current.Use();
				return true;
			}
			else if (Event.current.commandName == "Duplicate" && TryEdit())
			{
				var numLines = selectionStartPosition == null ? 1 : Mathf.Abs(caretPosition.line - selectionStartPosition.line) + 1;
				if (selectionStartPosition != null)
				{
					if (selectionStartPosition < caretPosition)
					{
						if (caretPosition.characterIndex == 0)
							--numLines;
					}
					else if (selectionStartPosition.characterIndex == 0)
					{
						--numLines;
					}
				}
				
				var insertAt = selectionStartPosition != null ?
					(caretPosition < selectionStartPosition ? caretPosition.Clone() : selectionStartPosition.Clone()) :
					caretPosition.Clone();
				insertAt.column = 0;
				insertAt.characterIndex = 0;
				
				var insertText = new StringBuilder();
				for (int i = 0; i < numLines; ++i)
					insertText.Append(textBuffer.lines[i + insertAt.line]).Append('\n');
				
				textBuffer.InsertText(insertAt, insertText.ToString());
				if (selectionStartPosition != null)
					selectionStartPosition.line += numLines;
				caretPosition.line += numLines;
				
				textBuffer.UpdateHighlighting(insertAt.line, insertAt.line + numLines - 1);
				
				tryArgumentsHint = true;
				Event.current.Use();
				return true;
			}
			else if (Event.current.commandName == "OpenAtCursor")
			{
				Event.current.Use();
				OpenAtCursor();
				return true;
			}
			else if (Event.current.commandName == "ScriptInspector.Autocomplete" && TryEdit())
			{
				Event.current.Use();
				tryAutocomplete = true;
				tryArgumentsHint = true;
				Repaint();
				return true;
			}
		}
		return false;
	}
	
	private void OpenAtCursor()
	{
		if (IsModified)
		{
			if (EditorUtility.DisplayDialog(
				"Script Inspector",
				AssetDatabase.GUIDToAssetPath(textBuffer.guid)
					+ "\n\nThis asset has been modified inside the Script Inspector.\nDo you want to save the changes before opening it in the external IDE?",
				"Save",
				"Cancel"))
			{
				SaveBuffer();
			}
			else
			{
				return;
			}
		}
		FGCodeWindow.openInExternalIDE = true;
		AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(textBuffer.guid), typeof(UnityEngine.Object)), caretPosition.line + 1);
	}

	private void UseSelectionForSearch()
	{
		var text = GetSearchTextFromSelection();
		if (text != "")
		{
			searchString = text;
			SetSearchText(text);
		}
	}
	
	public string GetSearchTextFromSelection()
	{
		var text = "";
		if (selectionStartPosition != null && selectionStartPosition.line == caretPosition.line)
		{
			if (caretPosition > selectionStartPosition)
				text = textBuffer.lines[caretPosition.line].Substring(
					selectionStartPosition.characterIndex, caretPosition.characterIndex - selectionStartPosition.characterIndex);
			else
				text = textBuffer.lines[caretPosition.line].Substring(
					caretPosition.characterIndex, selectionStartPosition.characterIndex - caretPosition.characterIndex);
			return text;
		}

		int wordStart;
		int wordEnd;
		if (!textBuffer.GetWordExtents(caretPosition.characterIndex, caretPosition.line, out wordStart, out wordEnd))
			return "";
		
		var line = textBuffer.lines[caretPosition.line];
		text = line.Substring(wordStart, wordEnd - wordStart);
		if (text.Trim() == "")
			return "";
		
		if (wordStart > 0 && caretPosition.characterIndex == wordStart &&
			!(line[wordStart] == '_' || char.IsLetterOrDigit(line, wordStart)) &&
			(line[wordStart] == '_' || char.IsLetterOrDigit(line, wordStart - 1)))
		{
			// Taking the previous word
			if (textBuffer.GetWordExtents(caretPosition.characterIndex - 1, caretPosition.line, out wordStart, out wordEnd))
				return line.Substring(wordStart, wordEnd - wordStart);
		}
		
		return text;
	}
	
	private bool ExpandSnippet(SnippetCompletion completion)
	{
		if (!textBuffer.isCsFile)
			return false;
		
		int lineIndex, tokenIndex;
		bool atTokenEnd;
		var tokenLeft = textBuffer.GetTokenAt(caretPosition, out lineIndex, out tokenIndex, out atTokenEnd);
		if (tokenLeft != null && (tokenLeft.tokenKind >= SyntaxToken.Kind.Keyword || tokenLeft.tokenKind == SyntaxToken.Kind.Preprocessor || tokenLeft.tokenKind == SyntaxToken.Kind.Comment))
		{
			var span = textBuffer.GetTokenSpan(lineIndex, tokenIndex);
			var text = textBuffer.lines[lineIndex].Substring(span.index, caretPosition.characterIndex - span.index);
			
			string snippet = completion != null ? completion.Expand() : null;
			snippet = snippet ?? CodeSnippets.Get(text, codePathSymbol, null);
			if (snippet != null)
			{
				textBuffer.BeginEdit("Expand Snippet");
				
				var indent = "\n" + textBuffer.lines[lineIndex].Substring(0, textBuffer.FirstNonWhitespace(lineIndex));
				//Debug.Log(lineIndex + "'"+textBuffer.lines[lineIndex]+"'"+indent.Length);
				var lines = snippet.Split('\n');
				var indented = string.Join(indent, lines);
				
				var replaceMacro = caretPosition.Clone();
				replaceMacro.characterIndex = span.index;
				replaceMacro.column -= text.Length;
				replaceMacro.virtualColumn -= text.Length;
				caretPosition = textBuffer.DeleteText(replaceMacro, caretPosition);
				
				var end = indented.IndexOf("$end$");
				if (end < 0)
					end = indented.Length;
				
				var part = indented.Substring(0, end);
				CodeSnippets.Substitute(ref part, codePathSymbol);
				caretPosition = textBuffer.InsertText(caretPosition, part);
				if (wordWrapping)
					caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
				
				if (end < indented.Length)
				{
					part = indented.Substring(end + 5);
					CodeSnippets.Substitute(ref part, codePathSymbol);
					textBuffer.InsertText(caretPosition, part);
				}
					
				textBuffer.UpdateHighlighting(lineIndex, lineIndex + snippet.Count(x => x == '\n'));
				textBuffer.EndEdit();
				return true;
			}
		}
		
		return false;
	}

	private void IndentMoreOrInsertTab(bool expandSnippets)
	{
		if (!TryEdit())
			return;
		
		if (selectionStartPosition != null)
		{
			IndentMore();
		}
		else
		{
			if (expandSnippets && ExpandSnippet(null))
				return;

			textBuffer.BeginEdit("Insert Tab");

			int spaces = 0;
			while (((caretPosition.column - spaces) & 3) > 0)
			{
				int prev = caretPosition.characterIndex - spaces - 1;
				if (prev >= 0 && textBuffer.lines[caretPosition.line][prev] == ' ')
					++spaces;
				else
					break;
			}
			if (spaces > 0)
			{
				FGTextBuffer.CaretPos replaceSpaces = caretPosition.Clone();
				replaceSpaces.characterIndex -= spaces;
				replaceSpaces.column -= spaces;
				replaceSpaces.virtualColumn -= spaces;
				caretPosition = textBuffer.DeleteText(replaceSpaces, caretPosition);
			}
			caretPosition = textBuffer.InsertText(caretPosition, "\t");
			if (wordWrapping)
				caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
			textBuffer.UpdateHighlighting(caretPosition.line, caretPosition.line);

			textBuffer.EndEdit();
		}
	}

	private void IndentMore()
	{
		if (!TryEdit())
			return;
		
		textBuffer.BeginEdit("Increase Indent");

		bool hasSelection = selectionStartPosition != null;
		if (!hasSelection)
			selectionStartPosition = caretPosition.Clone();

		FGTextBuffer.CaretPos from = caretPosition.Clone();
		FGTextBuffer.CaretPos to = caretPosition.Clone();
		int fromLine = caretPosition.line;
		int toLine = caretPosition.line;
		if (caretPosition < selectionStartPosition)
		{
			to = selectionStartPosition.Clone();
			toLine = to.line;
		}
		else
		{
			from = selectionStartPosition.Clone();
			fromLine = from.line;
		}
		if (to.characterIndex == 0 && fromLine < toLine)
			--toLine;

		bool moveFromPos = textBuffer.FirstNonWhitespace(fromLine) < from.characterIndex;
		bool moveToPos = to.line == toLine && textBuffer.FirstNonWhitespace(toLine) <= to.characterIndex;
		for (FGTextBuffer.CaretPos i = new FGTextBuffer.CaretPos { characterIndex = 0, column = 0, line = fromLine, virtualColumn = 0 }; i.line <= toLine; ++i.line)
			textBuffer.InsertText(i, "\t");
		textBuffer.UpdateHighlighting(fromLine, toLine);

		if (moveFromPos)
			++from.characterIndex;
		if (moveToPos)
			++to.characterIndex;
		from.column = from.virtualColumn = textBuffer.CharIndexToColumn(from.characterIndex, from.line);
		to.column = to.virtualColumn = textBuffer.CharIndexToColumn(to.characterIndex, to.line);

		if (caretPosition < selectionStartPosition)
		{
			caretPosition = from.Clone();
			selectionStartPosition = to.Clone();
		}
		else
		{
			selectionStartPosition = from.Clone();
			caretPosition = to.Clone();
		}

		if (!hasSelection)
			selectionStartPosition = null;

		if (wordWrapping)
		{
			caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
			if (selectionStartPosition != null)
				selectionStartPosition.column = selectionStartPosition.virtualColumn = CharIndexToColumn(selectionStartPosition.characterIndex, selectionStartPosition.line);
		}

		textBuffer.EndEdit();
	}

	private void IndentLess()
	{
		if (!TryEdit())
			return;
		
		textBuffer.BeginEdit("Decrease Indent");

		bool hasSelection = selectionStartPosition != null;
		if (!hasSelection)
			selectionStartPosition = caretPosition.Clone();

		FGTextBuffer.CaretPos from = caretPosition.Clone();
		FGTextBuffer.CaretPos to = caretPosition.Clone();
		int fromLine = caretPosition.line;
		int toLine = caretPosition.line;
		if (caretPosition < selectionStartPosition)
		{
			to = selectionStartPosition.Clone();
			toLine = to.line;
		}
		else
		{
			from = selectionStartPosition.Clone();
			fromLine = from.line;
		}
		if (to.characterIndex == 0 && fromLine < toLine)
			--toLine;

		bool moveFromPos = from.characterIndex > 0 && (caretPosition.line == selectionStartPosition.line || textBuffer.FirstNonWhitespace(fromLine) <= from.characterIndex);
		bool moveToPos = to.characterIndex > 0 && textBuffer.FirstNonWhitespace(toLine) <= to.characterIndex;
		for (FGTextBuffer.CaretPos i = new FGTextBuffer.CaretPos { characterIndex = 0, column = 0, line = fromLine, virtualColumn = 0 }; i.line <= toLine; ++i.line)
		{
			FGTextBuffer.CaretPos j = i.Clone();
			while (j.characterIndex < textBuffer.lines[i.line].Length && FGTextBuffer.GetCharClass(textBuffer.lines[i.line][j.characterIndex]) == 0)
			{
				j.column = j.virtualColumn = textBuffer.CharIndexToColumn(++j.characterIndex, j.line);
				if (j.column == 4)
					break;
			}
			if (i != j)
			{
				textBuffer.DeleteText(i, j);
			}
			else
			{
				if (i.line == fromLine)
					moveFromPos = false;
				if (i.line == toLine)
					moveToPos = false;
			}
		}
		textBuffer.UpdateHighlighting(fromLine, toLine);

		if (moveFromPos)
			--from.characterIndex;
		if (moveToPos)
			--to.characterIndex;
		from.column = from.virtualColumn = textBuffer.CharIndexToColumn(from.characterIndex, from.line);
		to.column = to.virtualColumn = textBuffer.CharIndexToColumn(to.characterIndex, to.line);

		if (caretPosition < selectionStartPosition)
		{
			caretPosition = from.Clone();
			selectionStartPosition = to.Clone();
		}
		else
		{
			selectionStartPosition = from.Clone();
			caretPosition = to.Clone();
		}

		if (!hasSelection)
			selectionStartPosition = null;

		if (wordWrapping)
		{
			caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, caretPosition.line);
			if (selectionStartPosition != null)
				selectionStartPosition.column = selectionStartPosition.virtualColumn = CharIndexToColumn(selectionStartPosition.characterIndex, selectionStartPosition.line);
		}

		textBuffer.EndEdit();
	}

	private FGListPopup autocompleteWindow;
	private string autoCompleteWord;

	private class KeywordAsSD : SymbolDefinition
	{
		public KeywordAsSD(string keyword)
		{
			name = keyword;
			kind = SymbolKind._Keyword;
		}
	}
	
	[NonSerialized]
	private SymbolDefinition showingArgumentsForMethod;
	[NonSerialized]
	private TextPosition showingArgumentsForToken;
	[NonSerialized]
	private Rect methodTokenRect;
	
	private void ShowArgumentsHint(ParseTree.Leaf methodLeaf)
	{
		showingArgumentsForMethod = null;
		showingArgumentsForToken = new TextPosition(-1, -1);

		if (methodLeaf.resolvedSymbol == null)
			FGResolver.ResolveNode(methodLeaf.parent);
		var methodSymbol = methodLeaf.resolvedSymbol;
		if (methodSymbol == null)
		{
			if (argumentsHint != null)
				argumentsHint.Hide();
		}
		else if(methodSymbol.kind == SymbolKind.Method || methodSymbol.kind == SymbolKind.MethodGroup)
		{
			var wasFlipped = false;
			if (argumentsHint != null)
			{
				wasFlipped = argumentsHint.Flipped;
				argumentsHint.Hide();
			}
			
			argumentsHint = FGTooltip.CreateTokenWidget(this, methodTokenRect, methodLeaf, false);
			if (argumentsHint)
			{
				argumentsHint.CurrentParameterIndex = currentArgumentIndex;
				
				showingArgumentsForMethod = methodLeaf.resolvedSymbol;
				showingArgumentsForToken = new TextPosition(methodLeaf.line, methodLeaf.tokenIndex);
				
				if (GetSoftLineBreaks(caretPosition.line) != NO_SOFT_LINE_BREAKS)
					argumentsHint.Flipped = true;
				else if (wasFlipped && autocompleteWindow == null || methodLeaf.line < caretPosition.line)
					argumentsHint.Flipped = true;
				else if (autocompleteWindow != null)
					autocompleteWindow.Flipped = !argumentsHint.Flipped;
			}
		}
	}

	private void Autocomplete(bool suggestionsOnly)
	{
		if (hasSelection || !textBuffer.isCsFile)
			return;
		
		if (autocompleteWindow == null)
		{
			Rect caretRect;

			if (wordWrapping)
			{
				int row, column;
				BufferToViewPosition(caretPosition, out row, out column);
				caretRect = new Rect(charSize.x * column + marginLeft, charSize.y * row + GetLineOffset(caretPosition.line), 1, charSize.y);
			}
			else
			{
				caretRect = new Rect(charSize.x * caretPosition.column + marginLeft, charSize.y * caretPosition.line, 1, charSize.y);
			}
			caretRect.x += 4f + scrollViewRect.x - scrollPosition.x;
			caretRect.y += 4f + scrollViewRect.y - scrollPosition.y;

			autocompleteWindow = FGListPopup.Create(this, caretRect, argumentsHint != null && !argumentsHint.Flipped);
			if (autocompleteWindow == null)
				return;
			
			autocompleteWindow.UpdateTypedInPart();
			
			if (argumentsHint != null)
				argumentsHint.Flipped = !autocompleteWindow.Flipped;
			
			var data = new HashSet<SymbolDefinition>();
			FGListPopup.shortAttributeNames = false;

			var typeArgumentList = false;
			
			var tokenLeft = FGListPopup.tokenLeft;
			if (tokenLeft != null)
			{
				var completionLeaf = tokenLeft.parent;
				if (completionLeaf != null && completionLeaf.parent == null)
				{
					var lineIndex = completionLeaf.line;
					var tokenIndex = completionLeaf.tokenIndex;
					while (completionLeaf != null && completionLeaf.parent == null)
					{
						var token = textBuffer.GetTokenLeftOf(ref lineIndex, ref tokenIndex);
						if (token == null)
							break;
						completionLeaf = token.parent;
					}
					if (completionLeaf != null)
					{
						if (tokenLeft.text == "," &&
							completionLeaf.resolvedSymbol != null &&
							completionLeaf.resolvedSymbol is TypeDefinitionBase)
						{
							typeArgumentList = true;
						}
						tokenLeft = completionLeaf.token;
					}
				}
			}
			if (tokenLeft != null && (tokenLeft.parent == null || tokenLeft.parent.grammarNode == null))
			{
			//	Debug.Log(tokenLeft + "\nkind: " + tokenLeft.tokenKind);
			}
			else //if (tokenLeft != null)
			{
				//Debug.Log("tokenLeft: " + tokenLeft);
				//Debug.Log("tokenLeft: " + tokenLeft.parent.grammarNode);

				var scanner = TextBuffer.Parser.MoveAfterLeaf(tokenLeft != null ? tokenLeft.parent : null);
			//	Debug.Log("scanner.CurrentParseTreeNode before: " + ParentsLog(scanner.CurrentParseTreeNode));

				var grammar = CsGrammar.Instance;

				var tokenSet = new FGGrammar.TokenSet();
				if (scanner != null)
				{
					scanner.CollectCompletions(tokenSet);
					if (tryAutocomplete && tokenSet.Matches(grammar.tokenName))
					{
						autocompleteWindow.Close();
						autocompleteWindow = null;
						return;
					}
				}

				var expectedTypeTokenId = grammar.tokenExpectedType;
				if (suggestionsOnly && !tokenSet.Matches(expectedTypeTokenId))
				{
					autocompleteWindow.Close();
					autocompleteWindow = null;
					return;
				}
				
				tokenSet.Remove(grammar.tokenEOF);
				if (tokenSet.Remove(grammar.tokenLiteral))
					if (!autocompleteWindow.IdentifiersOnly)
						data.Add(new KeywordAsSD("null"));
				
				var currentScannerNode = scanner != null ? scanner.CurrentParseTreeNode : null;
				var enclosingScopeNode = CsGrammar.EnclosingScopeNode(currentScannerNode);
				var enclosingScope = enclosingScopeNode != null ? enclosingScopeNode.scope : null;
				
				var parser = grammar.GetParser;
				var identifierTokenId = grammar.tokenIdentifier;
				var memberInitializerTokenId = grammar.tokenMemberInitializer;

				BitArray bitArray;
				var tokenId = tokenSet.GetDataSet(out bitArray);

				var addSymbols = typeArgumentList || autocompleteWindow.IdentifiersOnly;
				var addSnippets = false;
				var addMemberInitializerNames = false;
				if (tokenId != -1)
				{
					if (tokenId == identifierTokenId)
					{
						addSymbols = true;
					}
					else if (tokenId == memberInitializerTokenId)
					{
						addMemberInitializerNames = true;
					}
					else if (!autocompleteWindow.IdentifiersOnly)
					{
						var token = parser.GetToken(tokenId);
						if (token[0] == '_' || char.IsLetterOrDigit(token[0]))
						{
							data.Add(new KeywordAsSD(token));
							addSnippets = true;
						}
					}
				}
				else if (bitArray != null)
				{
					for (var i = 0; i < bitArray.Length; ++i)
					{
						if (bitArray[i])
						{
							if (i == expectedTypeTokenId)
							{
								var suggestionFound = false;
								
								var argumentIndex = -1;
								ParseTree.Node parentNode = null;
								ParseTree.BaseNode typeDefiningNode = null;
								TypeDefinitionBase expectedType = null;
								
								if (currentScannerNode != null && currentScannerNode.RuleName == "primaryExpression" &&
									tokenLeft != null && (tokenLeft.text == "new" || tokenLeft.text == "case"))
								{
									currentScannerNode = currentScannerNode.parent;
									while (currentScannerNode != null && currentScannerNode.childIndex == 0)
										currentScannerNode = currentScannerNode.parent;
									if (currentScannerNode!= null && currentScannerNode.RuleName == "expression")
										currentScannerNode = currentScannerNode.parent;
								}
								
								while (currentScannerNode != null && currentScannerNode.RuleName == "unaryExpression")
									currentScannerNode = currentScannerNode.parent;
								
								if (currentScannerNode != null)
								{
									switch (currentScannerNode.RuleName)
									{
									case "switchLabel":
										typeDefiningNode = currentScannerNode.parent.parent.parent.NodeAt(2);
										break;
									case "assignment":
									case "localVariableDeclarator":
									case "variableDeclarator":
									case "eventDeclarator":
									case "relationalExpression":
									case "inclusiveOrExpression":
									case "exclusiveOrExpression":
									case "andExpression":
									case "equalityExpression":
									case "multiplicativeExpression":
										typeDefiningNode = currentScannerNode.ChildAt(0);
										break;
									case "variableInitializerList":
										parentNode = currentScannerNode.parent.parent;
										if (parentNode.RuleName == "arrayCreationExpression")
										{
											typeDefiningNode = parentNode.parent.NodeAt(1);
										}
										else
										{
											typeDefiningNode = null;
										}
										break;
									case "localVariableInitializer":
										typeDefiningNode = currentScannerNode.parent.parent.parent.NodeAt(0);
										break;
									case "variableInitializer":
										typeDefiningNode = currentScannerNode.parent.LeafAt(0);
										break;
									case "arguments":
										typeDefiningNode = currentScannerNode;
										argumentIndex = 0;
										break;
									case "argumentList":
										typeDefiningNode = currentScannerNode.parent;
										argumentIndex = currentArgumentIndex;
										break;
									case "argument":
										typeDefiningNode = currentScannerNode.parent.parent;
										argumentIndex = currentArgumentIndex;
										break;
									case "fixedParameter":
										typeDefiningNode = currentScannerNode.FindChildByName("type") as ParseTree.Node;
										break;
									case "throwStatement":
										expectedType = SymbolDefinition.builtInTypes_Exception;
										break;
									case "constantDeclarator":
										typeDefiningNode = currentScannerNode.parent.parent.NodeAt(1);
										break;
									case "arrayInitializer":
										typeDefiningNode = currentScannerNode.parent.FindPreviousNode();
										break;
									case "defaultArgument":
										typeDefiningNode = currentScannerNode.parent.ChildAt(0);
										break;
									case "memberInitializer":
										typeDefiningNode = currentScannerNode.ChildAt(0);
										break;
									case "statementList":
									case "statement":
										break;
									default:
#if SI3_WARNINGS
										Debug.LogWarning(tokenLeft);
										Debug.LogWarning("CurrentParseTreeNode: " + currentScannerNode);
#endif
										break;
									}
								}
								
								if (typeDefiningNode != null && enclosingScope != null)
								{
									var resolved = FGResolver.GetResolvedSymbol(typeDefiningNode);
									if (resolved != null && resolved.kind != SymbolKind.Error)
									{
										if (argumentIndex < 0)
										{
											expectedType = resolved.TypeOf() as TypeDefinitionBase;
										}
										else
										{
											if (resolved.kind == SymbolKind.MethodGroup)
											{
												var methodGroup = resolved as MethodGroupDefinition;
												if (methodGroup != null)
												{
													foreach (var method in methodGroup.methods)
													{
														var parameters = method.GetParameters();
														if (parameters != null && argumentIndex < parameters.Count)
														{
															resolved = method;
															break;
														}
													}
												}
											}
											if (resolved.kind == SymbolKind.Method)
											{
												var parameters = resolved.GetParameters();
												if (argumentIndex < parameters.Count)
													expectedType = parameters[argumentIndex].TypeOf() as TypeDefinitionBase;
											}
										}
									}
								}
								
								if (expectedType != null)
								{
									SymbolDefinition topSuggestion = null;

									if (expectedType.kind == SymbolKind.Enum)
									{
										if (!data.Contains(expectedType))
										{
											var typeName = expectedType.RelativeName(enclosingScope);

											var suggestion = new KeywordAsSD(typeName);
											suggestion.kind = SymbolKind.Enum;
											data.Add(suggestion);
											
											suggestionFound = true;
											topSuggestion = topSuggestion ?? suggestion;
										}
									}
									else if (tokenLeft != null && tokenLeft.text == "new")
									{
										foreach (TypeDefinitionBase type in enclosingScope.GetAssembly().EnumAssignableTypesFor(expectedType))
										{
											if (type.kind == SymbolKind.Enum || type.kind == SymbolKind.Delegate || type.kind == SymbolKind.Interface ||
												type.IsStatic || type.IsAbstract || data.Contains(type))
											{
												continue;
											}
										
											var typeName = expectedType.RelativeName(enclosingScope);

											var suggestion = new KeywordAsSD(typeName);
											suggestion.kind = SymbolKind.Enum;
											data.Add(suggestion);

											suggestionFound = true;
											topSuggestion = topSuggestion ?? suggestion;
										}
									}

									if (topSuggestion != null)
										FGListPopup.SetTopSuggestion(topSuggestion);
								}
								
								if (suggestionsOnly && !suggestionFound)
								{
									autocompleteWindow.Close();
									autocompleteWindow = null;
									return;
								}
							}
							else if (i == identifierTokenId)
							{
								addSymbols = true;
							}
							else if (i == memberInitializerTokenId)
							{
								addMemberInitializerNames = true;
							}
							else if (!autocompleteWindow.IdentifiersOnly)
							{
								var token = parser.GetToken(i);
								if (token[0] == '_' || char.IsLetterOrDigit(token[0]))
								{
									data.Add(new KeywordAsSD(token));
									addSnippets = true;
								}
							}
						}
					}
				}
				else if (!autocompleteWindow.IdentifiersOnly)
				{
					//Debug.LogWarning("Empty BitArray!!! Adding all keywords...");
					foreach (var i in textBuffer.Parser.Keywords)
						data.Add(new KeywordAsSD(i));
					foreach (var i in textBuffer.Parser.BuiltInLiterals)
						data.Add(new KeywordAsSD(i));

					addSymbols = true;
					addSnippets = true;
				}

			//	Debug.Log("CurrentParseTreeNode.declaration: " + scanner.CurrentParseTreeNode.declaration);
				if (addSymbols && tokenLeft != null)
				{
					ParseTree.BaseNode completionNode = tokenLeft.parent;
					if (completionNode.IsLit("}"))
					{
						completionNode = completionNode.FindNextLeaf();
					}
					else if (completionNode.IsLit("=>"))
					{
						completionNode = completionNode.parent.NodeAt(completionNode.childIndex + 1) ?? completionNode;
					}
					else if (completionNode.IsLit("]") && completionNode.parent.RuleName == "attributes")
					{
						completionNode = completionNode.parent.parent.NodeAt(completionNode.parent.childIndex + 1);
					}

					var completionTypes = grammar.GetCompletionTypes(completionNode);
				//	Debug.Log(completionTypes);
					if (addMemberInitializerNames)
					{
						if (!addSnippets)
							completionTypes = IdentifierCompletionsType.MemberName;
						else
							completionTypes |= IdentifierCompletionsType.MemberName;
					}
					FGResolver.GetCompletions(completionTypes, completionNode, data, textBuffer.assetPath);

					data.RemoveWhere(x => !x.IsValid() || x.name == "" || x.name[0] == '<' || x.name[0] == '.');
					
					FGListPopup.shortAttributeNames = (completionTypes & IdentifierCompletionsType.AttributeClassType) != 0;
					if (FGListPopup.shortAttributeNames)
						FilterCompletions(data);
				}
				else if (addMemberInitializerNames && tokenLeft != null)
				{
					FGResolver.GetCompletions(IdentifierCompletionsType.MemberName, tokenLeft.parent, data, textBuffer.assetPath);
				}
				
				if (addSnippets)
				{
					try
					{
						foreach (var snippet in CodeSnippets.EnumSnippets(codePathSymbol, tokenSet, tokenLeft, enclosingScope))
							data.Add(snippet);
					}
					catch (System.Exception e)
					{
						Debug.LogException(e);
					}
				}
			}

			if (data.Count > 0)
			{
				autocompleteWindow.SetCompletionData(data);
				//if (argumentsHint != null)
				//	argumentsHint.Flipped = !autocompleteWindow.Flipped;
			}
			else
			{
				autocompleteWindow.Close();
				autocompleteWindow = null;
			}
		}
	}

	private static void FilterCompletions(HashSet<SymbolDefinition> data)
	{
		data.RemoveWhere(x => !DerivesFromOrContains(x, attributeType));
	}

	private static TypeDefinitionBase _attributeType;
	private static TypeDefinitionBase attributeType {
		get
		{
			if (_attributeType == null)
				_attributeType = ReflectedTypeReference.ForType(typeof(System.Attribute)).definition as TypeDefinitionBase;
			return _attributeType;
		}
	}

	private static bool DerivesFromOrContains(SymbolDefinition symbol, TypeDefinitionBase type)
	{
		if (symbol.kind != SymbolKind.Namespace && symbol.kind != SymbolKind.Class)
			return false;
		if (symbol.kind == SymbolKind.Class)
		{
			var asType = symbol as TypeDefinitionBase;
			if (asType.DerivesFrom(type))
				return true;
		}
		foreach (var child in symbol.members)
		{
			if (DerivesFromOrContains(child, type))
				return true;
		}
		return false;
	}

	//private string ParentsLog(ParseTree.BaseNode node)
	//{
	//	if (node == null)
	//		return "null";

	//	var s = string.Empty;
	//	for (int i = node.Depth; i >= 0; --i)
	//	{
	//		s = '\n' + new string(' ', i * 2) + (node is ParseTree.Node ? ((ParseTree.Node) node).RuleName : ((ParseTree.Leaf) node).grammarNode) + " " + node.childIndex + s;
	//		node = node.parent;
	//	}
	//	return s;
	//}
	
	public void CloseAllPopups()
	{
		if (tokenTooltip != null)
			tokenTooltip.Hide();
		if (argumentsHint != null)
			CloseArgumentsHint();
		if (autocompleteWindow != null)
			CloseAutocomplete();
	}

	public void CloseAutocomplete()
	{
		if (autocompleteWindow != null)
		{
			autocompleteWindow.Close();
			autocompleteWindow = null;
		}
	}

	public void CloseArgumentsHint()
	{
		if (argumentsHint != null)
		{
			argumentsHint.Hide();
			argumentsHint = null;
		}
		showingArgumentsForMethod = null;
		showingArgumentsForToken = new TextPosition(-1, -1);
	}

	public void BufferToViewPosition(FGTextBuffer.CaretPos position, out int row, out int column)
	{
		var softLineBreaks = GetSoftLineBreaks(position.line);
		row = FindFirstIndexGreaterThanOrEqualTo<int>(softLineBreaks, position.characterIndex);
		if (row < softLineBreaks.Count && position.characterIndex == softLineBreaks[row] && position.virtualColumn == 0)
			++row;

		var rowStart = row > 0 ? softLineBreaks[row - 1] : 0;
		//int rowLength = (row < softLineBreaks.Count ? softLineBreaks[row] : FGTextBuffer.ExpandTabs(textBuffer.lines[position.line]).Length) - rowStart;
		column = textBuffer.CharIndexToColumn(position.characterIndex, position.line, rowStart);
//		column = position.column - rowStart;
//		if (column > rowLength)
//			column = rowLength;
	}

	public FGTextBuffer.CaretPos ViewToBufferPosition(int line, int row, int column)
	{
		if (line >= textBuffer.formatedLines.Length)
			line = textBuffer.formatedLines.Length - 1;
		if (line < 0)
			line = 0;
		if (row < 0)
			row = 0;

		var position = new FGTextBuffer.CaretPos { column = column, line = line, virtualColumn = column };

		var softLineBreaks = GetSoftLineBreaks(line);
		if (row > softLineBreaks.Count)
			row = softLineBreaks.Count;
		var rowStart = row > 0 ? softLineBreaks[row - 1] : 0;

		position.characterIndex = textBuffer.ColumnToCharIndex(ref position.column, line, rowStart);

		var rowEnd = row < softLineBreaks.Count ? softLineBreaks[row] : textBuffer.lines[line].Length;
		if (position.characterIndex > rowEnd)
			position.characterIndex = rowEnd;

	//	position.column += rowStart;
	//	if (row < softLineBreaks.Count && position.column > softLineBreaks[row])
	//		position.column = softLineBreaks[row];
	//	position.characterIndex = textBuffer.ColumnToCharIndex(ref position.column, line);
		return position;
	}

    public FGTextBuffer.CaretPos GetLinesOffset(FGTextBuffer.CaretPos position, int linesDown)
    {
	    var pos = position.Clone();

		if (!wordWrapping)
		{
			pos.line += linesDown;
			if (pos.line < 0)
			{
				pos.line = 0;
				pos.characterIndex = 0;
				pos.column = 0;
				pos.virtualColumn = 0;
			}
			else if (pos.line >= textBuffer.lines.Count)
			{
				pos.line = textBuffer.lines.Count - 1;
				pos.characterIndex = textBuffer.lines[pos.line].Length;
				pos.column = textBuffer.CharIndexToColumn(pos.characterIndex, pos.line);
				pos.virtualColumn = pos.column;
			}
			else
			{
	            if (pos.line != position.line)
    	            pos.column = pos.virtualColumn;
				pos.characterIndex = textBuffer.ColumnToCharIndex(ref pos.column, pos.line);
			}
        }
        else
        {
            List<int> softLineBreaks = GetSoftLineBreaks(pos.line);
            int numSoftRows = softLineBreaks.Count + 1;
	        int softRow = FindFirstIndexGreaterThanOrEqualTo<int>(softLineBreaks, pos.characterIndex);
            if (softRow < softLineBreaks.Count && pos.characterIndex == softLineBreaks[softRow] && pos.virtualColumn == 0)
                ++softRow;

            while (linesDown > 0)
            {
                --linesDown;
                if (softRow < numSoftRows - 1)
                {
                    ++softRow;
                }
                else
                {
	                if (pos.line == textBuffer.lines.Count - 1)
	                {
		                int lastRowStart = softRow > 0 ? softLineBreaks[softRow - 1] : 0;
		                pos.virtualColumn = softRow < softLineBreaks.Count ? softLineBreaks[softRow] : textBuffer.CharIndexToColumn(textBuffer.lines[pos.line].Length, pos.line, lastRowStart);
		                break;
	                }
                    ++pos.line;
                    softRow = 0;
                    softLineBreaks = GetSoftLineBreaks(pos.line);
                    numSoftRows = softLineBreaks.Count + 1;
                }
            }
            while (linesDown < 0)
            {
                ++linesDown;
                if (softRow > 0)
                {
                    --softRow;
                }
                else
                {
	                if (pos.line == 0)
	                {
		                pos.virtualColumn = 0;
		                break;
	                }
                    --pos.line;
                    softLineBreaks = GetSoftLineBreaks(pos.line);
                    numSoftRows = softLineBreaks.Count + 1;
                    softRow = numSoftRows - 1;
                }
            }

            int rowStart = softRow > 0 ? softLineBreaks[softRow - 1] : 0;
        	int rowEnd = softRow < softLineBreaks.Count ? softLineBreaks[softRow] : textBuffer.lines[pos.line].Length;

			pos.column = pos.virtualColumn;
			pos.characterIndex = textBuffer.ColumnToCharIndex(ref pos.column, pos.line, rowStart);
			if (pos.characterIndex > rowEnd)
			{
				pos.characterIndex = rowEnd;
				pos.column = textBuffer.CharIndexToColumn(pos.characterIndex, pos.line, rowStart);
			}
		}

//		pos.characterIndex = textBuffer.ColumnToCharIndex(ref pos.column, pos.line);
        return pos;
    }

	public SyntaxToken GetTokenAtCursor()
	{
		int lineIndex, tokenIndex;
		return GetTokenAtCursor(out lineIndex, out tokenIndex);
	}
	
	public SyntaxToken GetTokenAtCursor(out int lineIndex, out int tokenIndex)
	{
		if (textBuffer == null)
		{
			lineIndex = -1;
			tokenIndex = -1;
			return null;
		}
		
		bool atTokenEnd;
		var token = textBuffer.GetTokenAt(caretPosition, out lineIndex, out tokenIndex, out atTokenEnd);
		if (token != null)
		{
			if (atTokenEnd && (
				token.tokenKind == SyntaxToken.Kind.Whitespace ||
				token.tokenKind != SyntaxToken.Kind.Identifier &&
				token.tokenKind != SyntaxToken.Kind.BuiltInLiteral &&
				token.tokenKind != SyntaxToken.Kind.ContextualKeyword &&
				token.tokenKind != SyntaxToken.Kind.Keyword &&
				token.tokenKind != SyntaxToken.Kind.Preprocessor &&
				token.tokenKind != SyntaxToken.Kind.PreprocessorSymbol))
			{
				var tokens = textBuffer.formatedLines[lineIndex].tokens;
				if (tokenIndex < tokens.Count - 1)
					token = tokens[tokenIndex + 1];
			}
		}
		return token;
	}
	
	private SyntaxToken GetTokenAtPosition(int line, int characterIndex)
	{
		int lineIndex, tokenIndex;
		bool atTokenEnd;
		var token = textBuffer.GetTokenAt(new TextPosition(line, characterIndex),
			out lineIndex, out tokenIndex, out atTokenEnd);
		return token;
	}
	
	private string HelpURL()
	{
		var token = GetTokenAtCursor();

		if ((token.tokenKind == SyntaxToken.Kind.Identifier ||
			token.tokenKind == SyntaxToken.Kind.Keyword && textBuffer.Parser.IsBuiltInType(token.text)) &&
			token.parent != null && token.parent.resolvedSymbol == null)
				FGResolver.ResolveNode(token.parent.parent);

		string id = null;
		if (token.tokenKind == SyntaxToken.Kind.Keyword)
		{
			switch (token.text)
			{
				case "abstract": id = "sf985hc5"; break;
				case "as": id = "cscsdfbt"; break;
				case "base": id = "hfw7t1ce"; break;

				case "break": id = "adbctzc4"; break;
				case "switch": case "case": id = "06tc147t"; break;
				case "try": case "catch": id = "0yd65esw"; break;

				case "class": id = "0b0thckt"; break;
				case "const": id = "e6w8fe1b"; break;

				case "continue": id = "923ahwt1"; break;
				case "default": id = token.parent.parent.RuleName == "defaultValueExpression" ? "xwth0h0d" : "06tc147t"; break;
				case "delegate": id = "900fyy8e"; break;

				case "do": id = "370s1zax"; break;
				case "if": case "else": id = "5011f09h"; break;
				case "enum": id = "sbbt4032"; break;

				case "event": id = "8627sbea"; break;
				case "explicit": id = "xhbhezf4"; break;
				case "extern": id = "e59b22c5"; break;

				case "finally": id = "zwc8s4fz"; break;
				case "for": id = "ch45axte"; break;

				case "foreach": case "in": id = "ttw7t8t6"; break;
				case "goto": id = "13940fs2"; break;
				case "implicit": id = "z5z9kes2"; break;

				case "interface": id = "87d83y5b"; break;

				case "internal": id = "7c5ka91b"; break;
				case "is": id = "scekt9xw"; break;
				case "lock": id = "c5kehkcz"; break;

				case "namespace": id = "z2kcy19k"; break;
				case "new": id = "51y09td4"; break; // TODO: Make this more awesome!

				case "operator": id = "s53ehcz3"; break;
				case "out": id = "ee332485"; break;
				case "override": id = "ebca9ah3"; break;

				case "params": id = "w5zay9db"; break;
				case "private": id = "st6sy9xe"; break;
				case "protected": id = "bcd5672a"; break;
				case "public": id = "yzh058ae"; break;

				case "readonly": id = "acdd6hb7"; break;
				case "ref": id = "14akc2c7"; break;
				case "return": id = "1h3swy84"; break;

				case "sealed": id = "88c54tsw"; break;
				case "sizeof": id = "eahchzkf"; break;
				case "stackalloc": id = "cx9s2sy4"; break;

				case "static": id = "98f28cdx"; break;
				case "struct": id = "ah19swz4"; break;

				case "this": id = "dk1507sz"; break;
				case "throw": id = "1ah5wsex"; break;

				case "typeof": id = "58918ffs"; break;
				case "using": id = token.parent.parent.RuleName == "usingStatement" ? "yh598w02" : "sf0df423"; break;
				case "virtual": id = "9fkccyh4"; break;

				case "volatile": id = "x13ttww7"; break;
				case "while": id = "2aeyhxcd"; break;
			}
		}

		if (id != null)
		{
			return "http://msdn.microsoft.com/library/" + id;
		}
		else if (token.parent != null && token.parent.resolvedSymbol != null)
		{
			var symbol = token.parent.resolvedSymbol;
			var assembly = symbol.Assembly;

			if (assembly == null && symbol.declarations != null)
				assembly = ((CompilationUnitScope) textBuffer.Parser.parseTree.root.scope).assembly;

			if (assembly != null)
			{
				var assemblyName = assembly.AssemblyName;
				if (assemblyName == "UnityEngine" || assemblyName == "UnityEditor" ||
					assemblyName.StartsWith("UnityEngine.", StringComparison.Ordinal) ||
					assemblyName.StartsWith("UnityEditor.", StringComparison.Ordinal))
				{
					if (SISettings.useLocalUnityDocumentation)
						return "file:///unity/ScriptReference/" + symbol.UnityHelpName;
					else
						return "http://docs.unity3d.com/ScriptReference/" + symbol.UnityHelpName + ".html";
				}
				else if (assemblyName == "mscorlib" || assemblyName == "System" || assemblyName.StartsWith("System.", StringComparison.Ordinal))
				{
					var nonEnumMember = symbol;
					if (nonEnumMember.kind == SymbolKind.EnumMember)
						nonEnumMember = nonEnumMember.parentSymbol;
					if (nonEnumMember.FullName == nonEnumMember.FullReflectionName)
					{
						return "http://msdn.microsoft.com/library/" + nonEnumMember.FullName + "(v=vs.90)";
					}
					else
					{
						return
							"http://msdn.microsoft.com/query/dev12.query?appId=Dev12IDEF1&l=EN-US&k=k(" +
							nonEnumMember.GetGenericSymbol().FullReflectionName +
							");k(TargetFrameworkMoniker-.NETFramework,Version%3Dv3.5);k(DevLang-csharp)";
					}
				}
			}
		}
		
		return null;
	}
	
	private AssemblyDefinition GetSymbolAssembly(SyntaxToken token)
	{
		if (token.tokenKind == SyntaxToken.Kind.Identifier && token.parent != null && token.parent.resolvedSymbol == null)
			FGResolver.ResolveNode(token.parent.parent);
		
		if (token.parent == null || token.parent.resolvedSymbol == null)
			return null;
		
		var symbol = token.parent.resolvedSymbol;
		var assembly = symbol.Assembly;
		
		if (assembly == null && symbol.declarations != null)
			assembly = ((CompilationUnitScope) textBuffer.Parser.parseTree.root.scope).assembly;
		
		return assembly;
	}
	
	private void ExecuteStaticMethod()
	{
		var token = GetTokenAtCursor();
		if (token == null)
			return;
				
		var symbol = token.parent.resolvedSymbol;
		if (!symbol.IsStatic || symbol.kind != SymbolKind.Method || symbol.GetParameters().Count != 0)
			return;
		
		var runtimeType = symbol.GetRuntimeType();
		if (runtimeType == null)
			return;
		
		var methodInfo = runtimeType.GetMethod(
			symbol.name,
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
			null,
			Type.EmptyTypes,
			null);
		
		if (methodInfo == null)
			return;
		
		if (methodInfo.GetParameters().Length != 0)
			return;
		
		var constructedSymbol = symbol as ConstructedMethodDefinition;
		if (constructedSymbol != null)
		{
			var typeArguments = constructedSymbol.typeArguments;
			if (typeArguments != null && typeArguments.Length > 0)
			{
				var runtimeTypeArguments = new System.Type[typeArguments.Length];
				for (int i = 0; i < typeArguments.Length; ++i)
				{
					var typeArgument = typeArguments[i].definition;
					if (typeArgument == null || typeArgument.kind == SymbolKind.Error)
						return;
					runtimeTypeArguments[i] = typeArgument.GetRuntimeType();
				}
				methodInfo = methodInfo.MakeGenericMethod(runtimeTypeArguments);
			}
		}
		
		if (!methodInfo.ContainsGenericParameters)
		{
			methodInfo.Invoke(null, null);
		}
	}
	
	private void GoToDefinition()
	{
		var declarations = GetSymbolDeclarations();
		if (declarations == null || declarations.Count == 0)
			return;
		
		if (declarations.Count == 1)
		{
			GoToSymbolDeclaration(declarations[0]);
		}
		else
		{
			var menu = new GenericMenu();
			foreach (var decl in declarations)
			{
				var co = decl.scope;
				while (co.parentScope != null)
					co = co.parentScope;
				var fileName = ((CompilationUnitScope) co).path;
				fileName = AssetDatabase.AssetPathToGUID(fileName);
				fileName = AssetDatabase.GUIDToAssetPath(fileName);
				fileName = Path.GetFileName(fileName);
				fileName = fileName.Replace('_', '\xFF3F');
				var nameNode = decl.NameNode();
				var firstLeaf = nameNode as ParseTree.Leaf ?? (nameNode as ParseTree.Node).GetFirstLeaf();
				menu.AddItem(new GUIContent(fileName + " : " + (firstLeaf.token.Line + 1)),
					false, d => GoToSymbolDeclaration((SymbolDeclaration) d), decl);
			}
			
			var token = GetTokenAtCursor();
			var rcToken = GetTokenRect(token);
			rcToken.x += scrollViewRect.x - scrollPosition.x;
			rcToken.y += 4f + scrollViewRect.y - scrollPosition.y;
			var ssTopLeft = GUIUtility.ScreenToGUIPoint(new Vector2(rcToken.x, rcToken.y));
			rcToken.x += ssTopLeft.x - rcToken.x;
			rcToken.y += ssTopLeft.y - rcToken.y;
			menu.DropDown(rcToken);
		}
	}
	
	private List<SymbolDeclaration> GetSymbolDeclarations()
	{
		var token = GetTokenAtCursor();
		if (token == null)
			return null;
		
		var assembly = GetSymbolAssembly(token);
		if (assembly == null)
			return null;
		
		var symbol = token.parent.resolvedSymbol;

		var assemblyName = assembly.AssemblyName;
		if (assemblyName == "mscorlib" || assemblyName == "System" || assemblyName.StartsWith("System.", StringComparison.Ordinal))
		{
			var input = symbol.XmlDocsName;
			if(input != null)
			{
				const int digits = 16;
				var md5 = System.Security.Cryptography.MD5.Create();
				var bytes = Encoding.UTF8.GetBytes(input);
				var hashBytes = md5.ComputeHash(bytes);
	
				var c = new char[digits];
				byte b;
				for (var i = 0; i < digits / 2; ++i)
				{
					b = ((byte) (hashBytes[i] >> 4));
					c[i * 2] = (char) (b > 9 ? b + 87 : b + 0x30);
					b = ((byte) (hashBytes[i] & 0xF));
					c[i * 2 + 1] = (char) (b > 9 ? b + 87 : b + 0x30);
				}
	
				Help.BrowseURL("http://referencesource.microsoft.com/mscorlib/a.html#" + new string(c));
				
				return null;
			}
		}
		
		if (assembly.assemblyId != AssemblyDefinition.UnityAssembly.CSharpFirstPass
			&& assembly.assemblyId != AssemblyDefinition.UnityAssembly.CSharp
			&& assembly.assemblyId != AssemblyDefinition.UnityAssembly.CSharpEditorFirstPass
			&& assembly.assemblyId != AssemblyDefinition.UnityAssembly.CSharpEditor)
		{
			return null;
		}
		
		var declarations = symbol.declarations;
		if (declarations == null || declarations.Count == 0)
		{
			declarations = FGFindInFiles.FindDeclarations(symbol);
			if (declarations == null || declarations.Count == 0)
			{
				token.parent.resolvedSymbol = null;
				FGResolver.ResolveNode(token.parent.parent);
				symbol = token.parent.resolvedSymbol;
				if (symbol != null)
				{
					declarations = symbol.declarations;
				}
			}
		}
		if (declarations != null && declarations.Count > 0)
		{
			var validDeclarations = new List<SymbolDeclaration>(declarations.Count);
			foreach (var decl in declarations)
				if (IsValidSymbolDeclaration(decl))
					validDeclarations.Add(decl);
			
			return validDeclarations;
		}
		
		return null;
	}
	
	private void ProcessEditorKeyboard(Event current, bool acceptingAutoComplete)
	{
		if (!CanEdit())
			return;
		
		if (current.type == EventType.keyDown &&
			current.keyCode != KeyCode.LeftControl && current.keyCode != KeyCode.RightControl &&
			current.keyCode != KeyCode.LeftCommand && current.keyCode != KeyCode.RightCommand &&
			current.keyCode != KeyCode.LeftShift && current.keyCode != KeyCode.RightShift)
		{
			pingTimer = 0f;
		}
		
		bool addRecentLocationIfUsed = true;
		
		bool isOSX = Application.platform == RuntimePlatform.OSXEditor;
		EventModifiers modifiers = current.modifiers & ~(EventModifiers.CapsLock | EventModifiers.Numeric | EventModifiers.FunctionKey);
		bool isActionKey = 0 != (current.modifiers & (isOSX ? EventModifiers.Command : EventModifiers.Control));

		int reindentLinesFrom = -1;
		int reindentLinesTo = -1;

		if (isOSX && current.type == EventType.keyDown)
		{
			if (current.keyCode == KeyCode.Z)
			{
				if (modifiers == EventModifiers.Control ||
					modifiers == (EventModifiers.Command | EventModifiers.Alt))
				{
					Undo();
					current.Use();
					tryArgumentsHint = true;
					return;
				}
				else if (modifiers == (EventModifiers.Control | EventModifiers.Shift) ||
					modifiers == (EventModifiers.Command | EventModifiers.Alt | EventModifiers.Shift))
				{
					Redo();
					current.Use();
					tryArgumentsHint = true;
					return;
				}
			}
			else if (current.keyCode == KeyCode.A && modifiers == EventModifiers.Control
				|| current.keyCode == KeyCode.LeftArrow && isActionKey)
			{
				current.Use();
				var simKey = Event.KeyboardEvent("home");
				simKey.modifiers = current.modifiers & (EventModifiers.Shift);
				ProcessEditorKeyboard(simKey, false);
				return;
			}
			else if (current.keyCode == KeyCode.E && modifiers == EventModifiers.Control
				|| current.keyCode == KeyCode.RightArrow && isActionKey)
			{
				current.Use();
				var simKey = Event.KeyboardEvent("end");
				simKey.modifiers = current.modifiers & (EventModifiers.Shift);
				ProcessEditorKeyboard(simKey, false);
				return;
			}
			else if (current.keyCode == KeyCode.S && modifiers == EventModifiers.Control)
			{
				current.Use();
				SaveBuffer();
				return;
			}
			else if (current.keyCode == KeyCode.G && modifiers == EventModifiers.Control)
			{
				current.Use();
				EditorApplication.delayCall += () => GoToLineWindow.Create(this);
				return;
			}
		}
		
		if (current.type == EventType.keyDown && !current.shift && !current.alt
			&& (isOSX ? !current.control : !current.command))
		{
			if (isActionKey && current.keyCode == KeyCode.Quote ||
				!isOSX && !current.control && current.keyCode == KeyCode.F1)
			{
				var helpUrl = HelpURL();
				if (helpUrl != null)
				{
					current.Use();
					
					if (helpUrl.StartsWith("file:///unity/ScriptReference/", StringComparison.OrdinalIgnoreCase))
						Help.ShowHelpPage(helpUrl);
					else
						Help.BrowseURL(helpUrl);
				}
			}
			else if (isOSX && current.command && current.keyCode == KeyCode.Y
				|| !current.control && !current.command && current.keyCode == KeyCode.F12)
			{
				current.Use();
				addRecentLocationIfUsed = false;
				
				GoToDefinition();
			}
			else if (isOSX
				? current.command && current.keyCode == KeyCode.L
				: current.control && current.keyCode == KeyCode.G)
			{
				current.Use();
				EditorApplication.delayCall += () => GoToLineWindow.Create(this);
				return;
			}
			else if (isOSX
				? current.command && current.keyCode == KeyCode.E
				: current.control && current.keyCode == KeyCode.E)
			{
				current.Use();
				ExecuteStaticMethod();
				return;
			}
		}

		if (isActionKey && current.type == EventType.keyDown)
		{
			EventModifiers mods = modifiers & ~(EventModifiers.Control | EventModifiers.Command);

			if (current.keyCode == KeyCode.Space && mods == 0)
			{
				current.Use();
				Autocomplete(false);
				return;
			}
			else if (current.keyCode == KeyCode.T && mods == 0)
			{
				current.Use();
				EditorApplication.delayCall += OpenInNewTab;
				return;
			}
			else if ((current.keyCode == KeyCode.K || current.keyCode == KeyCode.Slash) && mods == 0)
			{
				current.Use();
				ToggleCommentSelection();
				return;
			}
			else if (!isOSX && current.keyCode == KeyCode.Z && mods == EventModifiers.Shift)
			{
				current.Use();
				Undo();
				tryArgumentsHint = true;
				return;
			}
			else if (!isOSX && current.keyCode == KeyCode.Y && mods == EventModifiers.Shift)
			{
				current.Use();
				Redo();
				tryArgumentsHint = true;
				return;
			}
			else if (current.keyCode == KeyCode.S && mods == EventModifiers.Alt)
			{
				current.Use();
				SaveBuffer();
				return;
			}
			else if (current.keyCode == KeyCode.R && mods == (isOSX ? EventModifiers.Alt : EventModifiers.Shift))
			{
				current.Use();
				MenuReloadAssemblies();
				Repaint();
				return;
			}
			else if (!current.alt)
			{
				if (current.keyCode == KeyCode.Minus || current.keyCode == KeyCode.KeypadMinus)
				{
					current.Use();
					ModifyFontSize(-1);
					return;
				}
				else if (current.keyCode == KeyCode.Plus || current.keyCode == KeyCode.Equals || current.keyCode == KeyCode.KeypadPlus)
				{
					current.Use();
					ModifyFontSize(1);
					return;
				}
			}
		}
		
		if (isOSX && current.type == EventType.keyDown && current.keyCode == KeyCode.S
			&& modifiers == (EventModifiers.Control | EventModifiers.Alt))
		{
			current.Use();
			SaveBuffer();
			return;
		}

		bool moveByWordMode = isOSX
			? (modifiers & (EventModifiers.Control | EventModifiers.Alt)) != 0
			: isActionKey;

		int nextCharacterIndex = caretPosition.characterIndex;
		int nextCaretColumn = caretPosition.virtualColumn;
		int nextCaretLine = caretPosition.line;
		bool isHorizontal = false;
		bool clearSelection = false;

		if (current.type == EventType.keyDown)
		{
			switch (current.keyCode)
			{
				case KeyCode.Escape:
					addRecentLocationIfUsed = false;
				
					if (autocompleteWindow != null)
					{
						CloseAutocomplete();
						current.Use();
					}
					else if (argumentsHint != null)
					{
						argumentsHint.Hide();
						argumentsHint = null;
						//showingArgumentsForMethod = null;
						current.Use();
					}
					else if (!string.IsNullOrEmpty(searchString) && searchResultAge == textBuffer.undoPosition)
					{
						searchResultAge = -1;
						atLastSearchResult = false;
						current.Use();
					}
					else if (selectionStartPosition != null)
					{
						clearSelection = true;
						current.Use();
					}
					else if (SISettings.openAutoCompleteOnEscape)
					{
						current.Use();
						Autocomplete(false);
						return;
					}
					break;

				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					if (isActionKey && EditorWindow.focusedWindow != null)
					{
						//current.Use();
						//OpenAtCursor();
						//EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("OpenAtCursor"));
						//GUIUtility.ExitGUI();
						return;
					}
					break;

				case KeyCode.PageUp:
					if (isActionKey)
					{
						isHorizontal = true;
						
						nextCaretLine = 0;
						nextCharacterIndex = 0;
					}
					else
					{
						FGTextBuffer.CaretPos nextCaretPos = GetLinesOffset(caretPosition, -(int) (codeViewRect.height / charSize.y));
						nextCaretLine = nextCaretPos.line;
						nextCaretColumn = nextCaretPos.virtualColumn;
						nextCharacterIndex = nextCaretPos.characterIndex;
						
						addRecentLocationIfUsed = false;
					}
					current.Use();
					break;

				case KeyCode.PageDown:
					if (isActionKey)
					{
						isHorizontal = true;
						
						nextCaretLine = textBuffer.lines.Count - 1;
						nextCharacterIndex = textBuffer.lines[nextCaretLine].Length;
					}
					else
					{
						FGTextBuffer.CaretPos nextCaretPos = GetLinesOffset(caretPosition, (int)(codeViewRect.height / charSize.y));
						nextCaretLine = nextCaretPos.line;
						nextCaretColumn = nextCaretPos.virtualColumn;
						nextCharacterIndex = nextCaretPos.characterIndex;
						
						addRecentLocationIfUsed = false;
					}
					current.Use();
					break;

				case KeyCode.Home:
					if (isActionKey)
					{
						isHorizontal = true;
						
						nextCaretLine = 0;
						nextCharacterIndex = 0;
					}
					else
					{
						isHorizontal = true;

						int firstNonWhitespace = textBuffer.FirstNonWhitespace(nextCaretLine);
						if (firstNonWhitespace == textBuffer.lines[nextCaretLine].Length)
							firstNonWhitespace = 0;
						if (nextCharacterIndex == firstNonWhitespace)
							nextCharacterIndex = 0;
						else
							nextCharacterIndex = firstNonWhitespace;
						
						addRecentLocationIfUsed = false;
					}
					current.Use();
					break;

				case KeyCode.End:
					if (isActionKey)
					{
						isHorizontal = true;
						
						nextCaretLine = textBuffer.lines.Count - 1;
						nextCharacterIndex = textBuffer.lines[nextCaretLine].Length;
					}
					else
					{
						isHorizontal = true;

						if (!current.shift && selectionStartPosition != null)
						{
							if (selectionStartPosition.line > nextCaretLine)
								nextCaretLine = selectionStartPosition.line;
						}
						nextCharacterIndex = textBuffer.lines[nextCaretLine].Length;
						
						addRecentLocationIfUsed = false;
					}
					current.Use();
					break;

				case KeyCode.UpArrow:
					if (isOSX && modifiers == EventModifiers.Command)
					{
						isHorizontal = true;
						
						nextCaretLine = 0;
						nextCharacterIndex = 0;
					}
					else if (!current.control && !isActionKey)
					{
						if (!current.shift && selectionStartPosition != null && selectionStartPosition.line < nextCaretLine)
							//nextCaretLine = selectionStartPosition.line;
							caretPosition = selectionStartPosition.Clone();
						
						//--nextCaretLine;
						FGTextBuffer.CaretPos nextCaretPos = GetLinesOffset(caretPosition, -1);
						nextCaretLine = nextCaretPos.line;
						nextCaretColumn = nextCaretPos.virtualColumn;
						nextCharacterIndex = nextCaretPos.characterIndex;
					}
					else if (!current.shift)
					{
						scrollPosition.y -= charSize.y;
						if (scrollPosition.y < 0f)
							scrollPosition.y = 0f;
						scrollPositionLine = GetLineAt(scrollPosition.y);
						scrollPositionOffset = scrollPosition.y - GetLineOffset(scrollPositionLine);
						//Repaint();
						needsRepaint = true;
						current.Use();
						return;
					}
					else
					{
						UseSelectionForSearch();
						SearchPrevious();
						current.Use();
						focusCodeView = true;
						return;
					}
					current.Use();
					addRecentLocationIfUsed = false;
					break;

				case KeyCode.DownArrow:
					if (isOSX && modifiers == EventModifiers.Command)
					{
						isHorizontal = true;
						
						nextCaretLine = textBuffer.lines.Count - 1;
						nextCharacterIndex = textBuffer.lines[nextCaretLine].Length;
					}
					else if (!current.control && !isActionKey)
					{
						if (!current.shift && selectionStartPosition != null && selectionStartPosition.line > nextCaretLine)
							//nextCaretLine = selectionStartPosition.line;
							caretPosition = selectionStartPosition.Clone();
						
						//++nextCaretLine;
						FGTextBuffer.CaretPos nextCaretPos = GetLinesOffset(caretPosition, 1);
						nextCaretLine = nextCaretPos.line;
						nextCaretColumn = nextCaretPos.virtualColumn;
						nextCharacterIndex = nextCaretPos.characterIndex;
					}
					else if (!current.shift)
					{
						scrollPosition.y += charSize.y;
						scrollPositionLine = GetLineAt(scrollPosition.y);
						scrollPositionOffset = scrollPosition.y - GetLineOffset(scrollPositionLine);
						//Repaint();
						needsRepaint = true;
						current.Use();
						return;
					}
					else
					{
						UseSelectionForSearch();
						SearchNext();
						current.Use();
						focusCodeView = true;
						return;
					}
					current.Use();
					addRecentLocationIfUsed = false;
					break;

				case KeyCode.LeftArrow:
					if (modifiers == EventModifiers.Alt)
					{
						current.Use();
						if (CanGoBack())
							GoToRecentLocation(false);
						return;
					}
					
					isHorizontal = true;
					addRecentLocationIfUsed = false;

					if (!current.shift && !moveByWordMode && selectionStartPosition != null)
					{
						if (selectionStartPosition < caretPosition)
						{
							nextCaretLine = selectionStartPosition.line;
							nextCaretColumn = selectionStartPosition.column;
							nextCharacterIndex = selectionStartPosition.characterIndex;
						}
						else
						{
							clearSelection = true;
						}
						current.Use();
						break;
					}

					if (moveByWordMode)
					{
						FGTextBuffer.CaretPos nextPos = textBuffer.WordStopLeft(caretPosition);
						nextCaretLine = nextPos.line;
						nextCaretColumn = nextPos.column;
						nextCharacterIndex = nextPos.characterIndex;
						current.Use();
						break;
					}

					--nextCharacterIndex;
					if (nextCharacterIndex < 0)
					{
						if (--nextCaretLine >= 0)
						{
							nextCharacterIndex = textBuffer.lines[nextCaretLine].Length;
						}
						else
						{
							nextCaretLine = 0;
							nextCharacterIndex = 0;
						}
					}
					current.Use();
					break;

				case KeyCode.RightArrow:
					if (modifiers == EventModifiers.Alt)
					{
						current.Use();
						if (CanGoForward())
							GoToRecentLocation(true);
						return;
					}
					
					isHorizontal = true;
					addRecentLocationIfUsed = false;

					if (!current.shift && !moveByWordMode && selectionStartPosition != null)
					{
						if (selectionStartPosition > caretPosition)
						{
							nextCaretLine = selectionStartPosition.line;
							nextCaretColumn = selectionStartPosition.column;
							nextCharacterIndex = selectionStartPosition.characterIndex;
						}
						else
						{
							clearSelection = true;
						}
						current.Use();
						break;
					}

					if (moveByWordMode)
					{
						FGTextBuffer.CaretPos nextPos = textBuffer.WordStopRight(caretPosition);
						nextCaretLine = nextPos.line;
						nextCaretColumn = nextPos.column;
						nextCharacterIndex = nextPos.characterIndex;
						current.Use();
						break;
					}

					if (nextCaretLine >= 0)
					{
						++nextCharacterIndex;
						if (nextCharacterIndex > textBuffer.lines[nextCaretLine].Length)
						{
							if (++nextCaretLine < textBuffer.numParsedLines)
							{
								nextCharacterIndex = 0;
							}
							else
							{
								--nextCaretLine;
								--nextCharacterIndex;
							}
						}
					}
					current.Use();
					break;
				
				case KeyCode.Mouse3:
					current.Use();
					if (CanGoBack())
						GoToRecentLocation(false);
					break;
				
				case KeyCode.Mouse4:
					current.Use();
					if (CanGoForward())
						GoToRecentLocation(true);
					break;
			}
		}

		if (current.shift && current.keyCode == KeyCode.Space)
		{
			current.Use();
			addRecentLocationIfUsed = false;
		}
		
		char typedChar = '\0';
		
		if (current.type == EventType.keyDown)
		{
			if (modifiers == EventModifiers.Control && current.character == ' ')
			{
				current.Use();
				return;
			}
			else if (isActionKey && current.character == '\n' && !current.shift && !current.alt)
			{
				AddRecentLocation(1, true);
				
				if (EditorWindow.focusedWindow != null)
					OpenAtCursor();
				current.Use();
				return;
			}
			else if ((current.keyCode == KeyCode.Tab || current.character == '\t' || current.character == 25) && (modifiers & ~EventModifiers.Shift) == 0)
			{
				if (current.keyCode != KeyCode.Tab)
				{
					if (modifiers == EventModifiers.Shift)
						IndentLess();
					else
						IndentMoreOrInsertTab(!acceptingAutoComplete);
					
					focusCodeView = true;
					caretMoveTime = Time.realtimeSinceStartup;
					lastCaretMoveWasSearch = false;
					needsRepaint = true;
				}
				current.Use();
				return;
			}
			else if (isActionKey && (current.keyCode == KeyCode.LeftBracket || current.keyCode == KeyCode.RightBracket))
			{
				if (current.keyCode == KeyCode.LeftBracket)
					IndentLess();
				else
					IndentMore();
				
				focusCodeView = true;
				caretMoveTime = Time.realtimeSinceStartup;
				lastCaretMoveWasSearch = false;
				needsRepaint = true;
				current.Use();
				return;
			}
			else if ((current.character >= ' ' || current.character == '\n' || current.character == 0 && Input.compositionString != "")
				&& (!isActionKey || (modifiers & EventModifiers.Command) == 0 && current.keyCode == KeyCode.None)
				&& TryEdit())
			{
				typedChar = current.character;
				string text = typedChar != 0 ? typedChar.ToString() : Input.compositionString;
				string autoTextAfter = null;
				
				if (autoClosingStack.Count > 0 && "}])\">".IndexOf(typedChar) != -1)
				{
					if (selectionStartPosition == null)
					{
						var nextCharPos = textBuffer.FirstNonWhitespacePos(caretPosition.line, caretPosition.characterIndex);
						if (nextCharPos == autoClosingStack.Last().closingPos && textBuffer.lines[nextCharPos.line][nextCharPos.index] == typedChar)
						{
							selectionStartPosition = new FGTextBuffer.CaretPos {
								line = nextCharPos.line,
								characterIndex = nextCharPos.index + 1
							};
							selectionStartPosition.column = selectionStartPosition.virtualColumn =
								CharIndexToColumn(selectionStartPosition.characterIndex, selectionStartPosition.line);
							
							autoClosingStack.RemoveAt(autoClosingStack.Count - 1);
							if (autoClosingStack.Count == 0)
							{
								textBuffer.onInsertedText -= OnInsertedText;
								textBuffer.onRemovedText -= OnRemovedText;
							}
						}
					}
				}
				if (!acceptingAutoComplete && selectionStartPosition == null && "{[(\"<".IndexOf(typedChar) != -1)
				{
					autoTextAfter = CheckAutoClose(typedChar);
				}
				else if (typedChar == '\n')
				{
					// Keep the same indent level for newly inserted lines
					int firstNonWhitespace = textBuffer.FirstNonWhitespace(caretPosition.line);
					if (firstNonWhitespace > caretPosition.characterIndex)
						firstNonWhitespace = caretPosition.characterIndex;
					var indent = textBuffer.lines[caretPosition.line].Substring(0, firstNonWhitespace);
					text += indent;
					
					if (caretPosition.characterIndex > 0)
					{
						var tokenLeft = textBuffer.GetNonTriviaTokenLeftOf(caretPosition.line, caretPosition.characterIndex);
						if (tokenLeft != null &&
							tokenLeft.tokenKind == SyntaxToken.Kind.Punctuator &&
							tokenLeft.text == "{" &&
							tokenLeft.parent != null &&
							tokenLeft.parent.line == caretPosition.line)
						{
							var nextLeaf = tokenLeft.parent.FindNextLeaf();
							if (nextLeaf != null && nextLeaf.line == caretPosition.line && nextLeaf.IsLit("}"))
							{
								autoTextAfter = text;
							}
							
							text += "\t";
						}
						else if (tokenLeft != null && tokenLeft.tokenKind == SyntaxToken.Kind.StringLiteral)
						{
							var charLeftOfCaret = textBuffer.lines[caretPosition.line][caretPosition.characterIndex - 1];
							if (charLeftOfCaret != '"' && charLeftOfCaret != '\\')
							{
								text = "\" +" + text + '"';
							}
						}
					}
		
					if (argumentsHint != null)
						argumentsHint.Flipped = true;
				}

				FGTextBuffer.CaretPos newCaretPosition = caretPosition.Clone();
				if (selectionStartPosition != null)
				{
					newCaretPosition = textBuffer.DeleteText(selectionStartPosition, caretPosition);
					clearSelection = true;
				}
				
				int insertedAtLine = newCaretPosition.line;

				if (current.character == '\n' && !acceptingAutoComplete)
				{
					string breakingLine = textBuffer.lines[insertedAtLine];
					int lineLength = breakingLine.Length;
					int deleteSpaces = 0;
					while (deleteSpaces + newCaretPosition.characterIndex < lineLength)
						if (char.IsWhiteSpace(breakingLine[deleteSpaces + newCaretPosition.characterIndex]))
							++deleteSpaces;
						else
							break;
					if (deleteSpaces > 0)
					{
						var deleteSpacesTo = newCaretPosition.Clone();
						deleteSpacesTo.characterIndex += deleteSpaces;
						textBuffer.DeleteText(newCaretPosition, deleteSpacesTo);
					}
				}
				
				if (typedChar == 0)
				{
					selectionStartPosition = newCaretPosition.Clone();
					clearSelection = false;
				}
				
				newCaretPosition = textBuffer.InsertText(newCaretPosition, text);
				var updateToLine = newCaretPosition.line;
				if (!acceptingAutoComplete && autoTextAfter != null)
				{
					FGTextBufferManager.insertingTextAfterCaret = true;
					updateToLine = textBuffer.InsertText(newCaretPosition, autoTextAfter).line;
					FGTextBufferManager.insertingTextAfterCaret = false;

					if (typedChar != '\n')
					{
						var newAutoClose = new AutoClosePair
						{
							openingPos = new TextPosition(caretPosition.line, caretPosition.characterIndex),
							closingPos = new TextPosition(caretPosition.line, caretPosition.characterIndex + 1)
						};
						autoClosingStack.Add(newAutoClose);
						if (autoClosingStack.Count == 1)
						{
							textBuffer.onRemovedText += OnRemovedText;
							textBuffer.onInsertedText += OnInsertedText;
						}
					}
				}
				nextCharacterIndex = newCaretPosition.characterIndex;
				nextCaretColumn = newCaretPosition.column;
				nextCaretLine = newCaretPosition.line;
				isHorizontal = true;
				
				if (!acceptingAutoComplete)
					textBuffer.UpdateHighlighting(insertedAtLine, updateToLine);
				//if (typedChar == '\n' || typedChar == '{' || typedChar == '}' || typedChar == ':')
				if (typedChar != 0 && typedChar != ' ' && !acceptingAutoComplete)
				{
					reindentLinesFrom = insertedAtLine;
					reindentLinesTo = updateToLine;
				}

				modifiers &= ~EventModifiers.Shift;
				current.Use();
				
				if (!acceptingAutoComplete)
				{
					AfterCharecterTyped(text, nextCaretLine, nextCharacterIndex);
					tryArgumentsHint = true;
				}
				
				lastTypedText = text;
			}
			else if (current.keyCode == KeyCode.Delete && modifiers == EventModifiers.Shift && selectionStartPosition == null
				&& TryEdit())
			{
				modifiers = 0;

				if (caretPosition.line == textBuffer.numParsedLines - 1)
				{
					textBuffer.lines[caretPosition.line] = string.Empty;
					nextCharacterIndex = 0;
					nextCaretColumn = 0;
					//isHorizontal = true;
				}
				else
				{
					textBuffer.DeleteText(new FGTextBuffer.CaretPos { characterIndex = 0, column = 0, virtualColumn = 0, line = caretPosition.line },
						new FGTextBuffer.CaretPos { characterIndex = 0, column = 0, virtualColumn = 0, line = caretPosition.line + 1 });
					nextCaretColumn = caretPosition.column;
					nextCharacterIndex = textBuffer.ColumnToCharIndex(ref nextCaretColumn, caretPosition.line);
				}
				
				clearSelection = true;
				textBuffer.UpdateHighlighting(caretPosition.line, caretPosition.line);
				current.Use();
			}
			else if ((current.keyCode == KeyCode.Backspace || current.keyCode == KeyCode.Delete)
				&& TryEdit())
			{
				modifiers &= ~EventModifiers.Shift;
				
				FGTextBuffer.CaretPos newCaretPosition = caretPosition.Clone();
				if (selectionStartPosition == null)
				{
					Event simKey = new Event(current);
					bool checkAutoClosed = false;
					
					if (current.keyCode == KeyCode.Delete)
					{
						simKey.keyCode = KeyCode.RightArrow;
						if (caretPosition.characterIndex == textBuffer.lines[caretPosition.line].Length
							&& caretPosition.line + 1 < textBuffer.lines.Count)
						{
							var nextLineText = textBuffer.lines[caretPosition.line + 1];
							if (nextLineText != "" && (nextLineText[0] == ' ' || nextLineText[0] == '\t'))
								simKey.modifiers |= EventModifiers.FunctionKey | EventModifiers.Control;
						}
					}
					else
					{
						simKey.keyCode = KeyCode.LeftArrow;
						checkAutoClosed = true;
					}
					simKey.modifiers |= EventModifiers.Shift;
					ProcessEditorKeyboard(simKey, true);
					
					if (checkAutoClosed && autoClosingStack.Count > 0)
					{
						AutoClosePair entry = autoClosingStack.Last();
						{
							var textPos = entry.openingPos;
							if (textPos.line == caretPosition.line && textPos.index == caretPosition.characterIndex)
							{
								textPos.Move(textBuffer, 1);
								while (textPos < entry.closingPos)
								{
									var textLine = textBuffer.lines[textPos.line];
									var toIndex = textPos.line == entry.closingPos.line ? entry.closingPos.index : textLine.Length;
									for (; textPos.index < toIndex; ++textPos.index)
										if (!char.IsWhiteSpace(textLine, textPos.index))
											break;
									if (textPos.index < toIndex)
										break;
									textPos.index = 0;
									++textPos.line;
								}
								if (textPos >= entry.closingPos)
								{
									selectionStartPosition = new FGTextBuffer.CaretPos {
										line = entry.closingPos.line,
										characterIndex = entry.closingPos.index + 1
										};
									selectionStartPosition.column = selectionStartPosition.virtualColumn =
										CharIndexToColumn(selectionStartPosition.characterIndex, selectionStartPosition.line);
									
									autoClosingStack.RemoveAt(autoClosingStack.Count - 1);
									if (autoClosingStack.Count == 0)
									{
										textBuffer.onInsertedText -= OnInsertedText;
										textBuffer.onRemovedText -= OnRemovedText;
									}
								}
							}
						}
					}
				}

				modifiers &= ~EventModifiers.Shift;

				if (selectionStartPosition != null)
				{
					newCaretPosition = textBuffer.DeleteText(selectionStartPosition, caretPosition);

					nextCharacterIndex = newCaretPosition.characterIndex;
					nextCaretColumn = newCaretPosition.column;
					nextCaretLine = newCaretPosition.line;
					isHorizontal = true;
					clearSelection = true;

					textBuffer.UpdateHighlighting(newCaretPosition.line, newCaretPosition.line);
					current.Use();
				}
				
				tryArgumentsHint = true;
			}
		}

		//if (clearSelection ||
		//    isHorizontal && nextCharacterIndex != caretPosition.characterIndex ||
		//    nextCaretColumn != caretPosition.virtualColumn ||
		//    nextCaretLine != caretPosition.line)
		if (current.type == EventType.Used)
		{
			caretMoveTime = Time.realtimeSinceStartup;
			lastCaretMoveWasSearch = false;

			if (selectionStartPosition == null && current.shift)
			{
				selectionStartPosition = new FGTextBuffer.CaretPos
				{
					column = caretPosition.column,
					line = caretPosition.line,
					virtualColumn = caretPosition.column,
					characterIndex = caretPosition.characterIndex
				};
				if (!isHorizontal && nextCaretLine != caretPosition.line)
					nextCaretColumn = caretPosition.column;
			}

			if (nextCaretLine < 0)
				nextCaretLine = 0;
			if (nextCaretLine >= textBuffer.numParsedLines)
				nextCaretLine = textBuffer.numParsedLines - 1;

			//caretPosition.virtualColumn = nextCaretColumn;
			if (isHorizontal)
			{
				if (wordWrapping)
				{
					List<int> softLineBreaks = GetSoftLineBreaks(nextCaretLine);
					int softRow = FindFirstIndexGreaterThanOrEqualTo<int>(softLineBreaks, nextCharacterIndex);
					if (softRow < softLineBreaks.Count && nextCharacterIndex == softLineBreaks[softRow])
						++softRow;
					caretPosition.virtualColumn = nextCaretColumn =
						textBuffer.CharIndexToColumn(nextCharacterIndex, nextCaretLine, softRow > 0 ? softLineBreaks[softRow - 1] : 0);
					//	caretPosition.virtualColumn -= softRow > 0 ? softLineBreaks[softRow - 1] : 0;
				}
				else
				{
					caretPosition.virtualColumn = nextCaretColumn = textBuffer.CharIndexToColumn(nextCharacterIndex, nextCaretLine);
				}
			}
			else
			{
//				nextCharacterIndex = textBuffer.ColumnToCharIndex(ref nextCaretColumn, nextCaretLine);
			}
			caretPosition.column = nextCaretColumn;
			caretPosition.characterIndex = nextCharacterIndex;
			caretPosition.line = nextCaretLine;

			if (!isHorizontal && nextCaretLine >= 0)
			{
			    //caretPosition.characterIndex = Math.Min(nextCharacterIndex, textBuffer.lines[nextCaretLine].Length);
				caretPosition.virtualColumn = nextCaretColumn;
			    caretPosition.column = textBuffer.CharIndexToColumn(caretPosition.characterIndex, nextCaretLine);
			}
			
			if (current.character != 0 || Input.compositionString == "")
			{
				if (clearSelection || selectionStartPosition != null && ((modifiers & EventModifiers.Shift) == 0 || selectionStartPosition == caretPosition))
					selectionStartPosition = null;
			}
			
			scrollToCaret = true;
			//Repaint();
			needsRepaint = true;
			
			if (reindentLinesFrom >= 0 && reindentLinesFrom <= reindentLinesTo && textBuffer.CanEdit())
			{
				ReindentLines(reindentLinesFrom, reindentLinesTo);
			}
			
			if (addRecentLocationIfUsed)
			{
				AddRecentLocation(0, true);
			}
		}
	}

	private string CheckAutoClose(char typedChar)
	{
		string autoTextAfter = null;
		var tokenLeft = GetTokenAtPosition(caretPosition.line, caretPosition.characterIndex);

		if (tokenLeft == null ||
			tokenLeft.tokenKind != SyntaxToken.Kind.Comment &&
			tokenLeft.tokenKind != SyntaxToken.Kind.CharLiteral &&
			tokenLeft.tokenKind != SyntaxToken.Kind.StringLiteral &&
			tokenLeft.tokenKind != SyntaxToken.Kind.VerbatimStringBegin &&
			tokenLeft.tokenKind != SyntaxToken.Kind.VerbatimStringLiteral)
		{
			if (typedChar == '{')
			{
				autoTextAfter = TryAutoClose("}");
			}
			else if (typedChar == '[')
			{
				autoTextAfter = TryAutoClose("]");
			}
			else if (typedChar == '(')
			{
				autoTextAfter = TryAutoClose(")");
			}
			else if (typedChar == '"')
			{
				autoTextAfter = TryAutoClose("\"");
			}
			else if (typedChar == '<')
			{
				if (tokenLeft != null && tokenLeft.parent != null)
				{
					var symbolLeft = tokenLeft.parent.resolvedSymbol;
					if (symbolLeft != null)
					{
						if (symbolLeft is TypeDefinitionBase ||
							symbolLeft.kind == SymbolKind.Method ||
							symbolLeft.kind == SymbolKind.MethodGroup)
						{
							autoTextAfter = TryAutoClose(">");
						}
					}
				}
			}
		}
		return autoTextAfter;
	}

	private void AfterCharecterTyped(string text, int nextCaretLine, int nextCharacterIndex)
	{
		var autoPopupCompletions = false;
		if (text.Length == 1 && autocompleteWindow == null)
		{
			var tokenLeft = GetTokenAtPosition(nextCaretLine, nextCharacterIndex);
			if (tokenLeft != null && tokenLeft.tokenKind == SyntaxToken.Kind.Whitespace && tokenLeft.TokenIndex > 0)
			{
				tokenLeft = textBuffer.formatedLines[tokenLeft.Line].tokens[tokenLeft.TokenIndex - 1];
			
				while (tokenLeft != null && tokenLeft.TokenIndex > 0
					&& (tokenLeft.tokenKind == SyntaxToken.Kind.Missing || tokenLeft.tokenKind == SyntaxToken.Kind.Whitespace))
				{
					tokenLeft = textBuffer.formatedLines[tokenLeft.Line].tokens[tokenLeft.TokenIndex - 1];
				}
	
				if (tokenLeft != null && tokenLeft.parent != null && tokenLeft.parent.syntaxError == null)
				{
					var tokenLeftText = tokenLeft.text;
					var lastChar = tokenLeftText[tokenLeftText.Length - 1];
					if (lastChar == '=' || tokenLeftText == ">" || tokenLeftText == "<" || tokenLeftText == "~" ||
						tokenLeftText == "|" || tokenLeftText == "&" || tokenLeftText == "(" || tokenLeftText == "," || 
						tokenLeftText == "case" || tokenLeftText == "new")
					{
						autoPopupCompletions = true;
						tryAutoSuggestion = true;
					}
				}
			}
			else if (tokenLeft != null && tokenLeft.tokenKind == SyntaxToken.Kind.Punctuator && tokenLeft.text == "~")
			{
				autoPopupCompletions = true;
				tryAutoSuggestion = true;
			}
			else if (tokenLeft != null && tokenLeft.parent != null && tokenLeft.parent.syntaxError == null)
			{
				if (tokenLeft.tokenKind == SyntaxToken.Kind.Punctuator)
				{
					if (text == ".")
					{
						autoPopupCompletions = true;
						var nextLineText = textBuffer.lines[nextCaretLine];
						if (nextCharacterIndex > 1 && char.IsDigit(nextLineText, nextCharacterIndex - 2))
						{
							autoPopupCompletions = false;
							for (var checkDigit = nextCharacterIndex - 3; checkDigit >= 0; --checkDigit)
							{
								if (!char.IsDigit(nextLineText, checkDigit))
								{
									var c = nextLineText[checkDigit];
									autoPopupCompletions = c == '.' || c == '_' || char.IsLetter(c);
									break;
								}
							}
						}
					}
					else if (text == ":")
					{
						if (nextCharacterIndex >= 1 && textBuffer.lines[nextCaretLine][nextCharacterIndex - 2] == ':')
						{
							autoPopupCompletions = true;
						}
					}
				}
			}
			
			if (tokenLeft == null || tokenLeft.tokenKind != SyntaxToken.Kind.Comment)
			{
				var uc = text[0];//char.ToUpper(text[0]);
				
				//if (uc == ' ')
				//{
				//	var line = textBuffer.formatedLines[insertedAtLine];
				//	var tokenLeft = GetTokenAtCursor();
				//	if (tokenLeft.tokenKind == SyntaxToken.Kind.Whitespace)
				//		Debug.Log(tokenLeft);
				//}
				
				if (char.IsLetterOrDigit(uc) || uc == '_')
				{
					autoPopupCompletions = true;
					if (nextCharacterIndex >= 2)
					{
						var prevChar = textBuffer.lines[nextCaretLine][nextCharacterIndex - 2];
						if (char.IsLetterOrDigit(prevChar) || prevChar == '_')
							autoPopupCompletions = false;
					}
				}
			}
		}
		
		if (autoPopupCompletions)
			tryAutocomplete = true;
	}
	
	private struct AutoClosePair
	{
		public TextPosition openingPos;
		public TextPosition closingPos;
	}
	[NonSerialized]
	private List<AutoClosePair> autoClosingStack = new List<AutoClosePair>();
	
	private void OnRemovedText(FGTextBuffer.CaretPos fromPos, FGTextBuffer.CaretPos toPos)
	{
		var from = new TextPosition(fromPos.line, fromPos.characterIndex);
		var to = new TextPosition(toPos.line, toPos.characterIndex);
		
		for (var i = autoClosingStack.Count; i --> 0; )
		{
			var entry = autoClosingStack[i];
			if (from <= entry.openingPos)
			{
				if (to > entry.openingPos)
				{
					autoClosingStack.RemoveAt(i);
					continue;
				}
				
				if (to.line == entry.openingPos.line)
					entry.openingPos.index -= to.index - from.index;
				if (to.line == entry.closingPos.line)
					entry.closingPos.index -= to.index - from.index;
				entry.openingPos.line -= to.line - from.line;
				entry.closingPos.line -= to.line - from.line;

				autoClosingStack[i] = entry;
			}
			else if (from <= entry.closingPos)
			{
				if (to > entry.closingPos)
				{
					autoClosingStack.RemoveAt(i);
					continue;
				}
				
				if (to.line == entry.closingPos.line)
					entry.closingPos.index -= to.index - from.index;
				entry.closingPos.line -= to.line - from.line;

				autoClosingStack[i] = entry;
			}
		}

		if (autoClosingStack.Count == 0)
		{
			textBuffer.onInsertedText -= OnInsertedText;
			textBuffer.onRemovedText -= OnRemovedText;
		}
	}
	
	private void OnInsertedText(FGTextBuffer.CaretPos fromPos, FGTextBuffer.CaretPos toPos)
	{
		var from = new TextPosition(fromPos.line, fromPos.characterIndex);
		var to = new TextPosition(toPos.line, toPos.characterIndex);
		
		for (var i = autoClosingStack.Count; i --> 0; )
		{
			var entry = autoClosingStack[i];
			if (from <= entry.openingPos)
			{
				if (from.line == entry.openingPos.line)
					entry.openingPos.index += to.index - from.index;
				if (from.line == entry.closingPos.line)
					entry.closingPos.index += to.index - from.index;
				entry.openingPos.line += to.line - from.line;
				entry.closingPos.line += to.line - from.line;
				
				autoClosingStack[i] = entry;
			}
			else if (from <= entry.closingPos)
			{
				if (from.line == entry.closingPos.line)
					entry.closingPos.index += to.index - from.index;
				entry.closingPos.line += to.line - from.line;
				
				autoClosingStack[i] = entry;
			}
		}
	}
	
	private string TryAutoClose(string closeWith)
	{
		var textLine = textBuffer.lines[caretPosition.line];
		var scanPos = caretPosition.characterIndex;
		var toPos = textLine.Length;
		if (autoClosingStack.Count > 0)
		{
			var lastAutoClose = autoClosingStack.Last().closingPos;
			if (lastAutoClose.line == caretPosition.line)
				toPos = Math.Min(lastAutoClose.index, textLine.Length);
		}

		for (; scanPos < toPos; ++scanPos)
			if (!char.IsWhiteSpace(textLine, scanPos))
				return null;
		
		return closeWith;
	}
	
	private void ReindentLines(int from, int to)
	{
		if (autocompleteWindow != null)
			return;
		
		textBuffer.BeginEdit("Auto-indent");
		
		var updatedFrom = int.MaxValue;
		var updatedTo = -1;
		
		for (var line = from; line <= to; ++line)
		{
			var newIndent = textBuffer.CalcAutoIndent(line);
			if (newIndent == null)
				continue;
			
			var firstChar = textBuffer.FirstNonWhitespace(line);
			if (firstChar != newIndent.Length)
			{
				updatedFrom = Mathf.Min(updatedFrom, line);
				updatedTo = Mathf.Max(updatedTo, line);
				
				var tcp = textBuffer.DeleteText(
					new FGTextBuffer.CaretPos { line = line, characterIndex = 0 },
					new FGTextBuffer.CaretPos { line = line, characterIndex = firstChar });
				tcp = textBuffer.InsertText(tcp, newIndent);
				
				if (caretPosition.line == line && firstChar <= caretPosition.characterIndex)
				{
					caretPosition.characterIndex += newIndent.Length - firstChar;
					caretPosition.column = caretPosition.virtualColumn = CharIndexToColumn(caretPosition.characterIndex, line);
				}
			}
		}
		
		textBuffer.EndEdit();
		if (updatedTo > -1)
			textBuffer.UpdateHighlighting(updatedFrom, updatedTo);
	}
	
	[NonSerialized]
	private ParseTree.Leaf showArgumentsHintForLeaf;
	[NonSerialized]
	private int currentArgumentIndex = -1;
		
	private void UpdateArgumentsHint(bool canShow)
	{
		showArgumentsHintForLeaf = null;
		currentArgumentIndex = -1;
		
		int line, characterIndex;
		var tokenLeft = textBuffer.GetNonTriviaTokenLeftOf(caretPosition, out line, out characterIndex);
		if (tokenLeft != null && tokenLeft.parent != null)
		{
			int tokenIndex = tokenLeft.parent.tokenIndex;
			while (tokenLeft != null && (tokenLeft.parent == null ||tokenLeft.parent.syntaxError != null))
				tokenLeft = textBuffer.GetTokenLeftOf(ref line, ref tokenIndex);
		}
		if (tokenLeft != null && tokenLeft.parent != null && tokenLeft.parent.syntaxError == null)
		{
			var argumentsNode = tokenLeft.parent.parent;
			if (tokenLeft.text == ")" && argumentsNode != null && argumentsNode.RuleName == "arguments")
			{
				argumentsNode = argumentsNode.parent;
			}
			
			if (argumentsNode != null)
			{
				if (argumentsNode.RuleName == "argumentList")
					currentArgumentIndex = (tokenLeft.parent.childIndex + 1) / 2;
				else if (argumentsNode.RuleName == "arguments")
					currentArgumentIndex = 0;
				
				while (argumentsNode != null && argumentsNode.RuleName != "arguments")
				{
					if (argumentsNode.RuleName == "argument" ||
						argumentsNode.parent != null && argumentsNode.parent.RuleName == "argumentList")
					{
						currentArgumentIndex = (argumentsNode.childIndex + 1) / 2;
					}
					if (argumentsNode.RuleName == "lambdaExpressionBody" ||
						argumentsNode.RuleName == "objectOrCollectionInitializer")
					{
						argumentsNode = null;
					}
					else
					{
						argumentsNode = argumentsNode.parent;
					}
				}
			}
			
			if (argumentsNode != null)
			{
				// Are we typing in the method arguments?
				
				var leafBeforeArgs = argumentsNode.FindPreviousLeaf();
				if (leafBeforeArgs != null && leafBeforeArgs.parent != null)
				{
					var nodeLeft = leafBeforeArgs.parent;
					if (nodeLeft != null)
					{
						ParseTree.Leaf methodLeaf = null;
						if (nodeLeft.RuleName == "typeArgumentList")
							nodeLeft = nodeLeft.parent;
						if (nodeLeft.RuleName == "primaryExpressionStart")
							methodLeaf = nodeLeft.LeafAt(0);
						else if (nodeLeft.RuleName == "accessIdentifier")
							methodLeaf = nodeLeft.LeafAt(1);
						if (methodLeaf != null)
						{
							if (methodLeaf.parent != null)
							{
								showArgumentsHintForLeaf = methodLeaf;
							}
						}
					}
				}
			}
		}
		
		if (showArgumentsHintForLeaf == null)
		{
			showingArgumentsForMethod = null;
			if (argumentsHint != null)
			{
				//Debug.Log("Closing [null]");
				CloseArgumentsHint();
			}
		}
		else if (!canShow)
		{
			if (showingArgumentsForToken.line >= 0 &&
				(showingArgumentsForToken.line != showArgumentsHintForLeaf.line ||
				showingArgumentsForToken.index != showArgumentsHintForLeaf.tokenIndex))
			{
					//Debug.Log("Closing [showingArgumentsForToken != showArgumentsHintForLeaf]");
				CloseArgumentsHint();
			}
			else if (argumentsHint)
			{
				argumentsHint.CurrentParameterIndex = currentArgumentIndex;
				
				if (GetSoftLineBreaks(caretPosition.line) != NO_SOFT_LINE_BREAKS)
					argumentsHint.Flipped = true;
				else if (showingArgumentsForToken.line < caretPosition.line)
					argumentsHint.Flipped = true;
				if (autocompleteWindow != null)
					autocompleteWindow.Flipped = !argumentsHint.Flipped;
			}
			showArgumentsHintForLeaf = null;
			return;
		}
		else
		{
			if (showArgumentsHintForLeaf.resolvedSymbol == null)
				FGResolver.ResolveNode(showArgumentsHintForLeaf.parent.parent);
			
			if (showingArgumentsForMethod == showArgumentsHintForLeaf.resolvedSymbol)
			{
				if (showingArgumentsForToken.line != showArgumentsHintForLeaf.line ||
					showingArgumentsForToken.index != showArgumentsHintForLeaf.tokenIndex)
				{
					//Debug.Log("Closing [showingArgumentsForToken != showArgumentsHintForLeaf]");
					CloseArgumentsHint();
				}
				
				if (argumentsHint)
					argumentsHint.CurrentParameterIndex = currentArgumentIndex;
				
				showArgumentsHintForLeaf = null;
				return;
			}

			if (argumentsHint)
				argumentsHint.CurrentParameterIndex = currentArgumentIndex;
			
			if (argumentsHint && showingArgumentsForMethod != null)
			{
				var toHide = showingArgumentsForMethod;
				while (toHide != null && toHide.kind == SymbolKind.Method)
					toHide = toHide.parentSymbol;
				var toShow = showArgumentsHintForLeaf.resolvedSymbol;
				while (toShow != null && toShow.kind == SymbolKind.Method)
					toShow = toShow.parentSymbol;
				if (toHide == toShow)
				{
					showArgumentsHintForLeaf = null;
					return;
				}
			}
			
			methodTokenRect = GetTokenRect(showArgumentsHintForLeaf.token);
			methodTokenRect.y -= 2f;
			methodTokenRect.height += 4f;
		}
	}
	
	private void DrawPing(float indent, Rect rcPing, bool bgOnly)
	{
		if (styles.ping == null)
			return;

		float t = (1f - pingTimer) * 64f;
		if (t > 0f && t < 64f)
		{
			rcPing.x += indent;

			GUIStyle ping = styles.ping;
			int left = ping.padding.left;
			ping.padding.left = 0;

			Color oldColor = GUI.color;
			Color oldBgColor = GUI.backgroundColor;
			if (t > 4f)
			{
				if (!bgOnly)
					GUI.backgroundColor = new Color(pingColor.r, pingColor.g, pingColor.b, pingColor.a * (8f - t) * 0.125f);
				else
					GUI.backgroundColor = pingColor;
				//else
				//	GUI.backgroundColor = new Color(pingColor.r, pingColor.g, pingColor.b, (64f - t) * 0.125f);
				if (t > 56f)
					GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, pingColor.a * (64f - t) * 0.125f);
			}
			else
			{
				GUI.backgroundColor = pingColor;				
			}

			Matrix4x4 matrix = GUI.matrix;
			if (t < 4f)
			{
				float scale = 2f - Mathf.Abs(1f - t * 0.5f);
				Vector2 pos = rcPing.center;
				GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), pos);
			}
			ping.Draw(rcPing, bgOnly ? GUIContent.none : pingContent, false, false, false, false);
			GUI.matrix = matrix;

			ping.padding.left = left;
			GUI.color = oldColor;
			GUI.backgroundColor = oldBgColor;
		}
	}

	// "Links" drop-down menu items handler
	private void FollowHyperlink(object hyperlink)
	{
		Application.OpenURL((string) hyperlink);
	}

	private bool CanUndo()
	{
		return CanEdit() && textBuffer.CanUndo();
	}

	private bool CanRedo()
	{
		return CanEdit() && textBuffer.CanRedo();
	}

	private void Undo()
	{
		if (!TryEdit())
			return;
		
		textBuffer.Undo();
		AddRecentLocation(0, true);
	}

	private void Redo()
	{
		if (!TryEdit())
			return;
		
		textBuffer.Redo();
		AddRecentLocation(0, true);
	}

	private void ToggleCommentSelection()
	{
		if (!TryEdit())
			return;
		
		textBuffer.BeginEdit("Toggle Comment");

		FGTextBuffer.CaretPos from = caretPosition.Clone();
		FGTextBuffer.CaretPos to = caretPosition.Clone();
		int fromLine = caretPosition.line;
		int toLine = caretPosition.line;

		if (selectionStartPosition != null)
		{
			if (caretPosition < selectionStartPosition)
			{
				to = selectionStartPosition.Clone();
				toLine = to.line;
			}
			else
			{
				from = selectionStartPosition.Clone();
				fromLine = from.line;
		    }
		    if (to.characterIndex == 0)
		        --toLine;
		}
		
		int leftmostNWS = int.MaxValue;
		int[] fnws = new int[toLine - fromLine + 1];
		for (int i = 0; i < fnws.Length; ++i)
		{
			fnws[i] = textBuffer.FirstNonWhitespace(fromLine + i);
			if (fnws[i] < textBuffer.lines[fromLine + i].Length)
			{
				int column = CharIndexToColumn(fnws[i], fromLine + i);
				if (column < leftmostNWS)
					leftmostNWS = column;
			}
		}
		if (leftmostNWS == int.MaxValue)
		{
			textBuffer.EndEdit();
			return; // No code is selected, nothing to comment out
		}

		bool allCommentedOut = true;
		for (int i = 0; i < fnws.Length; ++i)
		{
			int lineLength = textBuffer.lines[fromLine + i].Length;
			if (fnws[i] < lineLength)
			{
				int index = textBuffer.ColumnToCharIndex(ref leftmostNWS, fromLine + i);
				if (textBuffer.isBooFile ? textBuffer.lines[fromLine + i][index] != '#' : textBuffer.lines[fromLine + i][index] != '/'
					|| index + 1 >= lineLength || textBuffer.lines[fromLine + i][index + 1] != '/')
				{
					allCommentedOut = false;
					break;
				}
			}
		}

		bool moveFromPos = from.line == fromLine && (allCommentedOut ? fnws[0] < from.characterIndex : fnws[0] <= from.characterIndex);
		bool moveToPos = to.line == toLine && (allCommentedOut ? fnws[fnws.Length - 1] < to.characterIndex : fnws[fnws.Length - 1] <= to.characterIndex);
		if (allCommentedOut)
		{
			for (FGTextBuffer.CaretPos i = new FGTextBuffer.CaretPos { characterIndex = 0, column = leftmostNWS, line = fromLine, virtualColumn = leftmostNWS }; i.line <= toLine; ++i.line)
			{
				if (fnws[i.line - fromLine] < textBuffer.lines[i.line].Length)
				{
					i.characterIndex = textBuffer.ColumnToCharIndex(ref leftmostNWS, i.line);
					FGTextBuffer.CaretPos j = i.Clone();
					j.column = j.virtualColumn += textBuffer.isBooFile ? 1 : 2;
					j.characterIndex += textBuffer.isBooFile ? 1 : 2;
					textBuffer.DeleteText(i, j);
				}
			}
		}
		else
		{
			for (FGTextBuffer.CaretPos i = new FGTextBuffer.CaretPos { characterIndex = 0, column = leftmostNWS, line = fromLine, virtualColumn = leftmostNWS }; i.line <= toLine; ++i.line)
			{
				if (fnws[i.line - fromLine] < textBuffer.lines[i.line].Length)
				{
					i.characterIndex = textBuffer.ColumnToCharIndex(ref leftmostNWS, i.line);
					textBuffer.InsertText(i, textBuffer.isBooFile ? "#" : "//");
				}
			}
		}
		textBuffer.UpdateHighlighting(fromLine, toLine);

		if (moveFromPos)
			from.characterIndex += textBuffer.isBooFile ? (allCommentedOut ? -1 : 1) : (allCommentedOut ? -2 : 2);
		if (moveToPos)
			to.characterIndex += textBuffer.isBooFile ? (allCommentedOut ? -1 : 1) : (allCommentedOut ? -2 : 2);
		from.column = from.virtualColumn = CharIndexToColumn(from.characterIndex, from.line);
		to.column = to.virtualColumn = CharIndexToColumn(to.characterIndex, to.line);

		if (selectionStartPosition != null)
		{
			if (caretPosition < selectionStartPosition)
			{
				caretPosition = from;
				selectionStartPosition = to;
			}
			else
			{
				selectionStartPosition = from;
				caretPosition = to;
			}
		}
		else
		{
			caretPosition = from;
		}
		caretMoveTime = Time.realtimeSinceStartup;
		lastCaretMoveWasSearch = false;
		scrollToCaret = true;

		textBuffer.EndEdit();
		
		AddRecentLocation(1, true);
	}

	public void PingLine(int line)
	{
		CloseAllPopups();
		
		if (line > textBuffer.lines.Count)
			line = textBuffer.lines.Count;
		
		int fnws = textBuffer.FirstNonWhitespace(line - 1);
		int fromColumn = CharIndexToColumn(fnws, line - 1);
		string expanded = FGTextBuffer.ExpandTabs(textBuffer.lines[line - 1], 0);
		
		pingContent.text = expanded.Trim();
		if (!string.IsNullOrEmpty(pingContent.text))
		{
			int toColumn = expanded.Length;
			caretPosition = new FGTextBuffer.CaretPos { line = line - 1, column = fromColumn, virtualColumn = fromColumn, characterIndex = fnws };
			selectionStartPosition = new FGTextBuffer.CaretPos { line = line - 1, column = toColumn, virtualColumn = toColumn, characterIndex = textBuffer.lines[line - 1].Length };
		}
		else
		{
			caretPosition = new FGTextBuffer.CaretPos { line = line - 1, column = 0, virtualColumn = 0, characterIndex = 0 };
			if (line < textBuffer.lines.Count)
				selectionStartPosition = new FGTextBuffer.CaretPos { line = line, column = 0, virtualColumn = 0, characterIndex = 0 };
			else
				selectionStartPosition = null;
		}
		//ValidateCarets();

		Initialize();

		pingTimer = 1f;
		scrollToRect.x = charSize.x * fromColumn;
		scrollToRect.y = GetLineOffset(caretPosition.line);
		scrollToRect.width = charSize.x * pingContent.text.Length;
		scrollToRect.height = charSize.y;

		caretMoveTime = Time.realtimeSinceStartup;
		Repaint();
	}
	
	private bool CanGoBack()
	{
		var recentLocations = FGTextBufferManager.instance.recentLocations;
		if (recentLocations.Count == 0)
			return false;
		
		var offset = FGTextBufferManager.instance.recentLocationsOffset;
		if (offset < recentLocations.Count - 1)
			return true;
		
		var firstLocation = recentLocations[0];
		if (firstLocation.assetGuid != targetGuid || caretPosition.line != firstLocation.line)
			return true;
		
		return false;
	}
	
	private bool CanGoForward()
	{
		return FGTextBufferManager.instance.recentLocationsOffset > 0;
	}
	
	public bool AddRecentLocation(int minLinesDistance, bool insert)
	{
		if (targetGuid == "")
			return false;

		var recentLocations = FGTextBufferManager.instance.recentLocations;
		var offset = FGTextBufferManager.instance.recentLocationsOffset;
		
		if (recentLocations.Count <= offset)
		{
			FGTextBufferManager.AddRecentLocation(targetGuid, caretPosition, false);
			return true;
		}
		else
		{
			var prevLocation = recentLocations[recentLocations.Count - 1 - offset];
			var prevLinesDistance = Mathf.Abs(caretPosition.line - prevLocation.line);
			var nextLocation = offset > 0 ? recentLocations[recentLocations.Count - offset] : null;
			var nextLinesDistance = nextLocation != null ? Mathf.Abs(caretPosition.line - nextLocation.line) : -1;
			if ((prevLocation.assetGuid != targetGuid ||
				prevLinesDistance >= minLinesDistance &&
				(prevLinesDistance > 0 || caretPosition.characterIndex != prevLocation.index)
			) && (
				nextLocation == null || nextLocation.assetGuid != targetGuid ||
				nextLinesDistance >= minLinesDistance &&
				(nextLinesDistance > 0 || caretPosition.characterIndex != nextLocation.index))
			)
			{
				FGTextBufferManager.AddRecentLocation(targetGuid, caretPosition, insert);
				return true;
			}
		}
		
		return false;
	}
	
	private void GoToRecentLocation(bool forward)
	{
		bool added = AddRecentLocation(1, true);
		
		var recentLocations = FGTextBufferManager.instance.recentLocations;
		var offset = FGTextBufferManager.instance.recentLocationsOffset;
		
		bool atCurrentLocation = added;
		if (!atCurrentLocation)
		{
			var currentLocation = recentLocations[recentLocations.Count - 1 - offset];
			atCurrentLocation = currentLocation.assetGuid == targetGuid && caretPosition.line == currentLocation.line;
		}
		
		if (forward)
			--offset;
		else if (atCurrentLocation)
			++offset;
		FGTextBufferManager.instance.recentLocationsOffset = offset;
		
		var location = recentLocations[recentLocations.Count - 1 - offset];
		if (location.assetGuid == targetGuid)
			SetCursorPosition(location.line, location.index);
		else
		{
			addRecentLocationOnFocusLost = false;
			FGCodeWindow.OpenAssetInTab(location.assetGuid, location.line, location.index);
		}
	}
	
	public void SetCursorPosition(int line, int characterIndex)
	{
		selectionStartPosition = null;
		var column = CharIndexToColumn(characterIndex, line);
		caretPosition = new FGTextBuffer.CaretPos {
			line = line,
			characterIndex = characterIndex,
			column = column,
			virtualColumn = column
		};
		ValidateCaret(ref caretPosition);
		scrollToCaret = true;
		caretMoveTime = Time.realtimeSinceStartup;
		
		pingContent = new GUIContent();
		pingColor = new Color(1f, 1f, 1f, 0.4f);
		pingTimer = 1f;
		scrollToRect.x = charSize.x * caretPosition.column - 3f;
		scrollToRect.y = GetLineOffset(caretPosition.line);
		scrollToRect.width = 6f;
		scrollToRect.height = charSize.y;
		
		Repaint();
	}
	
	struct CodePath
	{
		public SymbolDefinition symbol;
		public GUIContent content;
		public bool down;
	}

	private SymbolDefinition codePathSymbol;
	private List<CodePath> codePaths = new List<CodePath>();

	private static GUIStyle breadCrumbLeft, breadCrumbLeftOn;
	private static GUIStyle breadCrumbMid, breadCrumbMidOn;

	private float DoCodeNavigationToolbar()
	{
		if (!textBuffer.isCsFile)
			return 0f;

		scrollViewRect.yMin += 18f;
		//	codeViewRect.yMin += 18f;

		var wasGuiEnabled = GUI.enabled;
		GUI.enabled = true;
		
		var toolbarRect = new Rect(scrollViewRect.xMin, scrollViewRect.yMin - 18f, scrollViewRect.width, 17f);
		GUI.Label(toolbarRect, GUIContent.none, EditorStyles.toolbar);
		toolbarRect.width -= 60f;

		var isOSX = Application.platform == RuntimePlatform.OSXEditor;
		var rc = new Rect(5f, toolbarRect.yMin, 25f, 17f);
		GUI.enabled = CanGoBack() && CanEdit();
		if (GUI.Button(rc, new GUIContent("\u25c4", isOSX ? "Go Back ⌥←" : "Go Back\n(Alt+Left)"), EditorStyles.toolbarButton))
		{
			GoToRecentLocation(false);
		}
		rc.x += 25f;
		GUI.enabled = CanGoForward() && CanEdit();
		if (GUI.Button(rc, new GUIContent("\u25ba", isOSX ? "Go Forward ⌥→" : "Go Forward\n(Alt+Right)"), EditorStyles.toolbarButton))
		{
			GoToRecentLocation(true);
		}
		rc.x += 37f;

		GUI.enabled = CanEdit();
		
		if (caretPosition.line >= textBuffer.formatedLines.Length)
		{
			GUI.enabled = wasGuiEnabled;
			return 18f;
		}
		
		FGTextBuffer.FormatedLine ppLine = null;
		SyntaxToken token = null;
		var i = caretPosition.characterIndex;
		for (var atLine = caretPosition.line; token == null && atLine < textBuffer.lines.Count; ++atLine)
		{
			var line = textBuffer.formatedLines[atLine];
			if (line == null || line.tokens == null)
			{
				GUI.enabled = wasGuiEnabled;
				return 18f;
			}

			var tokens = line.tokens;
			var tokenIndex = 0;
			while (i > 0 && tokenIndex < tokens.Count - 1)
			{
				i -= tokens[tokenIndex].text.Length;
				if (i > 0)
					++tokenIndex;
			}

			for ( ; tokenIndex < tokens.Count; ++tokenIndex)
			{
				var t = tokens[tokenIndex];
				if (t.tokenKind > SyntaxToken.Kind.LastWSToken)
				{
					if (t.parent != null && t.parent.parent != null)
					{
						token = t;
						break;
					}
				}
				else if (ppLine == null && t.tokenKind >= SyntaxToken.Kind.Preprocessor
					&& t.tokenKind != SyntaxToken.Kind.VerbatimStringLiteral)
				{
					ppLine = line;
				}
			}
		}

		if (token == null)
		{
			for (var atLine = caretPosition.line - 1; token == null && atLine >= 0; --atLine)
			{
				var line = textBuffer.formatedLines[atLine];
				if (line == null || line.tokens == null)
				{
					GUI.enabled = wasGuiEnabled;
					return 18f;
				}

				var tokens = line.tokens;
				for (var tokenIndex = tokens.Count; tokenIndex --> 0; )
				{
					var t = tokens[tokenIndex];
					if (t.tokenKind > SyntaxToken.Kind.LastWSToken)
					{
						if (t.parent != null && t.parent.parent != null)
						{
							token = tokens[tokenIndex];
							break;
						}
					}
					else if (ppLine == null && t.tokenKind >= SyntaxToken.Kind.Preprocessor
						&& t.tokenKind != SyntaxToken.Kind.VerbatimStringLiteral)
					{
						ppLine = line;
					}
				}
			}
		}
		
		if (token != null)
		{
			SymbolDefinition symbol = null;
			if (token.parent != null)
			{
				var node = token.parent.parent;
				while (node != null)
				{
					var ruleName = node.RuleName;
					if (ruleName == "namespaceDeclaration" ||
						ruleName == "namespaceMemberDeclaration" ||
						ruleName == "classMemberDeclaration" ||
						ruleName == "structMemberDeclaration" ||
						ruleName == "interfaceMemberDeclaration" ||
						ruleName == "enumMemberDeclaration" ||
						ruleName == "delegateDeclaration" ||
						ruleName == "variableDeclarator" ||
						ruleName == "getAccessorDeclaration" ||
						ruleName == "setAccessorDeclaration" ||
						ruleName == "addAccessorDeclaration" ||
						ruleName == "removeAccessorDeclaration")
					{
						if (ruleName == "namespaceMemberDeclaration" ||
							ruleName == "classMemberDeclaration" ||
							ruleName == "structMemberDeclaration" ||
							ruleName == "interfaceMemberDeclaration")
						{
							node = node.NodeAt(node.numValidNodes - 1);
							ruleName = node != null ? node.RuleName : null;
						}

						if (ruleName == "constructorDeclaration" || ruleName == "destructorDeclaration")
						{
							if (node.declaration == null)
							{
								node = node.NodeAt(0);
								ruleName = node != null ? node.RuleName : null;
							}
						}
						else if (ruleName == "fieldDeclaration")
						{
							node = node.FindChildByName("variableDeclarators", "variableDeclarator") as ParseTree.Node;
						}
						else if (ruleName == "eventDeclaration")
						{
							node = node.FindChildByName("eventWithAccessorsDeclaration") as ParseTree.Node
								?? node.FindChildByName("eventDeclarators", "eventDeclarator") as ParseTree.Node;
						}

						if (node != null && node.declaration != null && node.declaration.definition != null)
						{
							symbol = node.declaration.definition;
						}
						break;
					}
					node = node.parent;
				}
			}
			
			if (breadCrumbLeft == null)
			{
				breadCrumbLeft = "GUIEditor.BreadcrumbLeft";
				breadCrumbMid = "GUIEditor.BreadcrumbMid";
	
				if (breadCrumbLeft == null)
				{
					breadCrumbLeft = new GUIStyle(EditorStyles.toolbarButton);
					breadCrumbLeftOn = new GUIStyle(EditorStyles.toolbarButton);
					breadCrumbLeftOn.normal = breadCrumbLeftOn.onNormal;
					breadCrumbLeftOn.active = breadCrumbLeftOn.onActive;
					breadCrumbLeftOn.hover = breadCrumbLeftOn.onHover;
					breadCrumbLeftOn.focused = breadCrumbLeftOn.onFocused;

					breadCrumbMid = breadCrumbLeft;
					breadCrumbMidOn = breadCrumbLeftOn;
				}
				else
				{
					breadCrumbLeft = new GUIStyle(breadCrumbLeft);
					breadCrumbLeft.padding.top -= 2;
					breadCrumbLeft.alignment = TextAnchor.UpperRight;
					breadCrumbLeftOn = new GUIStyle(breadCrumbLeft);
					breadCrumbLeftOn.normal = breadCrumbLeftOn.onNormal;
					breadCrumbLeftOn.active = breadCrumbLeftOn.onActive;
					breadCrumbLeftOn.hover = breadCrumbLeftOn.onHover;
					breadCrumbLeftOn.focused = breadCrumbLeftOn.onFocused;
					breadCrumbLeft.onNormal = breadCrumbLeft.normal;
					breadCrumbLeft.onActive = breadCrumbLeft.active;
					breadCrumbLeft.onHover = breadCrumbLeft.hover;
					breadCrumbLeft.onFocused = breadCrumbLeft.focused;

					breadCrumbMid = new GUIStyle(breadCrumbMid);
					breadCrumbMid.padding.top -= 2;
					breadCrumbMid.alignment = TextAnchor.UpperRight;
					breadCrumbMidOn = new GUIStyle(breadCrumbMid);
					breadCrumbMidOn.normal = breadCrumbMidOn.onNormal;
					breadCrumbMidOn.active = breadCrumbMidOn.onActive;
					breadCrumbMidOn.hover = breadCrumbMidOn.onHover;
					breadCrumbMidOn.focused = breadCrumbMidOn.onFocused;
					breadCrumbMid.onNormal = breadCrumbMid.normal;
					breadCrumbMid.onActive = breadCrumbMid.active;
					breadCrumbMid.onHover = breadCrumbMid.hover;
					breadCrumbMid.onFocused = breadCrumbMid.focused;
				}
			}

			if (symbol != null && !symbol.IsValid())
				symbol = null;

			if (symbol != codePathSymbol || codePaths.Count == 0)
			{
				codePathSymbol = symbol;
				codePaths.Clear();

				if (symbol == null)
				{
					codePaths.Add(new CodePath { content = new GUIContent("...") });
				}
				else
				{
					while (symbol != null && symbol.name != "")
					{
						if (symbol.kind != SymbolKind.MethodGroup)
						{
							var name = symbol.GetName();
							if (name == ".ctor")
								name = symbol.parentSymbol.GetName();
							
							var methodDef = symbol as MethodDefinition;
							if (methodDef != null)
							{
								name += " (" + methodDef.PrintParameters(methodDef.GetParameters(), true) + ")";
							}
							
							var path = new CodePath
							{
								symbol = symbol,
								content = new GUIContent(" " + name, FGListPopup.GetSymbolIcon(symbol)),
								down = codePaths.Count == 0,
							};
							codePaths.Insert(0, path);
						}
						symbol = symbol.parentSymbol;
					}

					if (codePathSymbol.kind == SymbolKind.Class || codePathSymbol.kind == SymbolKind.Enum ||
						codePathSymbol.kind == SymbolKind.Interface || codePathSymbol.kind == SymbolKind.Namespace ||
						codePathSymbol.kind == SymbolKind.Struct)
					{
						codePaths.Add(new CodePath { content = new GUIContent("...") });
					}
				}
			}
			
			//var codeFocused = hasCodeViewFocus;

			var buttonStyle = breadCrumbLeft;
			var buttonStyleOn = breadCrumbLeftOn;
			for (var j = 0; j < codePaths.Count; ++j)
			{
				if (rc.x >= toolbarRect.xMax)
					break;
				
				var path = codePaths[j];
				var content = path.content;
				var size = buttonStyle.CalcSize(content);
				rc.width = size.x;
				if (rc.xMax > toolbarRect.xMax)
				{
					rc.xMax = toolbarRect.xMax;
					size.x = rc.width;
				}
				if (GUI.Button(rc, content, path.down ? buttonStyleOn : buttonStyle))
				{
					var selectedSymbol = path.symbol;

					if (!path.down && path.symbol != null)
					{
						GoToSymbol(selectedSymbol);
					}
					else
					{
						var menu = new GenericMenu();

						var compilationUnitRoot = textBuffer.Parser.parseTree.root;
						if (j == 0)
						{
							var declarations = new List<SymbolDeclaration>();
							EnumScopeDeclarations(compilationUnitRoot, declarations.Add);
							declarations.Sort((x, y) => x.Name.CompareTo(y.Name));
							for (var d = 0; d < declarations.Count; ++d)
							{
								var decl = declarations[d];
								var name = decl.Name;
								menu.AddItem(new GUIContent(name), false, () => GoToSymbolDeclaration(decl));
							}
						}
						else
						{
							var members = codePaths[j - 1].symbol.members;
							var symbols = new List<SymbolDefinition>(members.Count);
							foreach (var m in members)
							{
								if (m.kind == SymbolKind.MethodGroup)
								{
									var methodGroup = (MethodGroupDefinition) m;
									foreach (var method in methodGroup.methods)
									{
										var declarations = method.declarations;
										if (declarations == null || declarations.Count == 0)
											continue;
										for (var d = 0; d < declarations.Count; ++d)
										{
											var decl = declarations[d];
											if (!decl.IsValid() || !compilationUnitRoot.IsAncestorOf(decl.parseTreeNode))
												continue;
											
											symbols.Add(method);
										}
									}
								}
								else
								{
									var declarations = m.declarations;
									if (declarations == null || declarations.Count == 0)
										continue;
									for (var d = 0; d < declarations.Count; ++d)
									{
										var decl = declarations[d];
										if (!decl.IsValid() || !compilationUnitRoot.IsAncestorOf(decl.parseTreeNode))
											continue;
										
										symbols.Add(m);
									}
								}
							}

							symbols.Sort((x, y) => {
								var xName = x.GetName();
								var yName = y.GetName();
								if (xName == ".ctor")
									xName = x.parentSymbol.kind == SymbolKind.MethodGroup ? x.parentSymbol.parentSymbol.GetName() : x.parentSymbol.GetName();
								if (yName == ".ctor")
									yName = y.parentSymbol.kind == SymbolKind.MethodGroup ? y.parentSymbol.parentSymbol.GetName() : y.parentSymbol.GetName();
								return xName.CompareTo(yName);
							});
							for (var s = 0; s < symbols.Count; s++)
							{
								var target = symbols[s];
								var name = target.GetName();
								if (name == ".ctor")
									name = target.parentSymbol.kind == SymbolKind.MethodGroup ? target.parentSymbol.parentSymbol.GetName() : target.parentSymbol.GetName();
								
								var methodDef = target as MethodDefinition;
								if (methodDef != null)
								{
									name += " (" + methodDef.PrintParameters(methodDef.GetParameters(), true) + ")";
								}
								
								menu.AddItem(new GUIContent(name), false, () => GoToSymbol(target));
							}
						}

						rc.x -= buttonStyle.overflow.left;
						menu.DropDown(rc);
					}
				}
				rc.x += size.x;

				buttonStyle = breadCrumbMid;
				buttonStyleOn = breadCrumbMidOn;
			}
		}
		
		var caretLine = textBuffer.formatedLines[caretPosition.line];
		var region = caretLine.regionTree;
		if (ppLine == caretLine && caretPosition.line > 0)
		{
			for (var j = ppLine.tokens.Count; j --> 0; )
			{
				if (ppLine.tokens[j].tokenKind == SyntaxToken.Kind.Preprocessor && ppLine.tokens[j].text == "endregion")
				{
					region = textBuffer.formatedLines[caretPosition.line - 1].regionTree;
					break;
				}
			}
		}
		while (region.parent != null &&
			region.kind != FGTextBuffer.RegionTree.Kind.Region &&
			region.kind != FGTextBuffer.RegionTree.Kind.InactiveRegion)
		{
			region = region.parent;
		}
		if (region.parent != null)
			ppLine = region.line;
		else
			ppLine = null;
		var regionContent = ppLine != null ? new GUIContent("#region ") : new GUIContent("No Region");
		if (ppLine != null)
		{
			for (var j = ppLine.tokens.Count; j --> 0; )
			{
				if (ppLine.tokens[j].tokenKind == SyntaxToken.Kind.PreprocessorArguments)
				{
					regionContent.text += ppLine.tokens[j].text;
					break;
				}
			}
		}
		
		toolbarRect.xMax += 60f;
		
		rc.x += 3f;
		rc.xMax = toolbarRect.xMax - 6f;
		var regionButtonSize = EditorStyles.toolbarDropDown.CalcSize(regionContent);
		if (regionButtonSize.x < rc.width)
			rc.xMin = rc.xMax - regionButtonSize.x;
		if (GUI.Button(rc, regionContent, EditorStyles.toolbarDropDown))
		{
			var allRegions = new List<FGTextBuffer.RegionTree>();
			ListAllRegions(textBuffer.rootRegion, allRegions);
			if (!SISettings.sortRegionsByName)
				allRegions.Sort((a, b) => a.line.index.CompareTo(b.line.index));
			
			var names = new string[allRegions.Count];
			for (var j = allRegions.Count; j --> 0; )
			{
				names[j] = "";
				
				var r = allRegions[j];
				var tokens = r.line.tokens;
				for (var k = 0; k < tokens.Count; ++k)
				{
					if (tokens[k].tokenKind == SyntaxToken.Kind.PreprocessorArguments)
					{
						names[j] = tokens[k].text;
						break;
					}
				}
			}
			var regions = allRegions.ToArray();
			if (SISettings.sortRegionsByName)
				Array.Sort(names, regions);
			var menu = new GenericMenu();
			for (var j = 0; j < names.Length; ++j)
			{
				menu.AddItem(new GUIContent(names[j]),
					ppLine != null && regions[j] == ppLine.regionTree,
					x => GoToRegion((FGTextBuffer.RegionTree)x), regions[j]);
			}
			if (names.Length > 0)
			{
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Sort by name"), SISettings.sortRegionsByName,
					() => {SISettings.sortRegionsByName.Toggle();});
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("No regions"));
			}
			menu.DropDown(rc);
		}
			
		GUI.enabled = wasGuiEnabled;
		return 18f;
	}
	
	private void GoToRegion(FGTextBuffer.RegionTree region)
	{
		var line = region.line.index;
		if (line >= 0 && line < textBuffer.lines.Count && textBuffer.formatedLines[line] == region.line)
		{
			var tokens = region.line.tokens;
			for (var i = 0; i < tokens.Count; ++i)
			{
				if (tokens[i].tokenKind == SyntaxToken.Kind.Preprocessor && tokens[i].text == "region")
				{
					AddRecentLocation(0, true);
					
					var span = textBuffer.GetTokenSpan(line, i);
					PingText(new FGTextBuffer.CaretPos{ line = line, characterIndex = span.index }, "region".Length, Color.white);
					break;
				}
			}
		}
	}
	
	private void ListAllRegions(FGTextBuffer.RegionTree root, List<FGTextBuffer.RegionTree> regions)
	{
		if (root.children == null)
			return;
		var numLines = textBuffer.lines.Count;
		for (var i = root.children.Count; i --> 0; )
		{
			var r = root.children[i];
			var line = r.line;
			var lineIndex = line.index;
			if (lineIndex < 0 || lineIndex >= numLines || textBuffer.formatedLines[lineIndex] != line)
			{
				root.children.RemoveAt(i);
				continue;
			}
			
			var tokens = r.line.tokens;
			for (var j = 0; j < tokens.Count; ++j)
			{
				var t = tokens[j];
				if (t.tokenKind == SyntaxToken.Kind.Preprocessor && t.text != "#")
				{
					if (t.text == "region")
						regions.Add(r);
					ListAllRegions(r, regions);
					break;
				}
				if (t.tokenKind > SyntaxToken.Kind.Preprocessor)
					break;
			}
		}
		if (root.children.Count == 0)
			root.children = null;
	}

	private static void EnumScopeDeclarations(ParseTree.Node root, Action<SymbolDeclaration> action)
	{
		for (var i = 0; i < root.numValidNodes; ++i)
		{
			var node = root.NodeAt(i);
			if (node == null)
				continue;
			var flags = node.semantics;
			var decl = flags & SemanticFlags.SymbolDeclarationsMask;
			if (node.declaration != null &&
				(decl == SemanticFlags.ExternAlias || decl == SemanticFlags.NamespaceDeclaration ||
				decl == SemanticFlags.ClassDeclaration || decl == SemanticFlags.StructDeclaration ||
				decl == SemanticFlags.InterfaceDeclaration || decl == SemanticFlags.EnumDeclaration ||
				decl == SemanticFlags.DelegateDeclaration))
			{
				action.Invoke(node.declaration);
			}
			if ((flags & SemanticFlags.ScopesMask) == 0)
				EnumScopeDeclarations(node, action);
		}
	}
	
	public void GoToSymbol(SymbolDefinition symbol)
	{
		var declarations = symbol.declarations;
		if (declarations == null || declarations.Count == 0)
			declarations = FGFindInFiles.FindDeclarations(symbol);
		if (declarations == null)
			return;
		
		foreach (var declaration in symbol.declarations)
		{
			if (declaration.IsValid())
			{
				GoToSymbolDeclaration(declaration);
				break;
			}
		}
	}
	
	public bool IsValidSymbolDeclaration(SymbolDeclaration declaration)
	{
		var node = declaration.NameNode();
		if (node == null)
			return false;
		
		string cuPath = null;
		for (var scope = declaration.scope; scope != null; scope = scope.parentScope)
		{
			var cuScope = scope as CompilationUnitScope;
			if (cuScope != null)
			{
				cuPath = cuScope.path;
				break;
			}
		}
		if (cuPath == null)
			return false;
		
		var cuObject = AssetDatabase.LoadAssetAtPath(cuPath, typeof(MonoScript));
		if (cuObject == null)
			return false;
		
		var buffer = FGTextBufferManager.GetBuffer(cuObject);
		if (buffer == null)
			return false;
		
		return true;
	}
	
	public bool GoToSymbolDeclaration(SymbolDeclaration declaration)
	{
		var node = declaration.NameNode();
		if (node == null)
			return false;

		string cuPath = null;
		for (var scope = declaration.scope; scope != null; scope = scope.parentScope)
		{
			var cuScope = scope as CompilationUnitScope;
			if (cuScope != null)
			{
				cuPath = cuScope.path;
				break;
			}
		}
		if (cuPath == null)
		{
			Debug.Log("Source code for '" + node.Print() + "' is not available.");
			return false;
		}

		string cuGuid = AssetDatabase.AssetPathToGUID(cuPath);
		var buffer = FGTextBufferManager.GetBuffer(cuGuid);
		if (buffer == null)
		{
			Debug.Log("Error: Failed to load " + cuPath);
			return false;
		}

		if (buffer.lines.Count == 0)
		{
			buffer.LoadImmediately();
		}
		
		AddRecentLocation(1, true);

		var span = buffer.GetParseTreeNodeSpan(declaration.NameNode());
		if (buffer == textBuffer)
		{
			var startAtColumn = CharIndexToColumn(span.index, span.line);
			var declarationPosition = new FGTextBuffer.CaretPos
			{
				characterIndex = span.index,
				column = startAtColumn,
				virtualColumn = startAtColumn,
				line = span.line
			};
			PingText(declarationPosition, span.lineOffset == 0 ? span.indexOffset : buffer.lines[span.line].Length - span.index, Color.white);
		}
		else
		{
			EditorApplication.delayCall += () => FGCodeWindow.OpenAssetInTab(AssetDatabase.AssetPathToGUID(cuPath), span.line + 1);
		}
		
		return true;
	}
	
	private class P4MenuItem
	{
		public string menuItem;
		public int index;
		public int priority;
		public MethodInfo validateMethod;
		public MethodInfo executeMethod;
	}
	private static Type p4MenusType = Type.GetType("P4Connect.Menus,P4Connect");
	private static MethodInfo[] p4MenusMethods;
	private static PropertyInfo p4ValidConfigurationProperty;
	
	private GUIContent popOutButtonContent = new GUIContent();
	private GUIContent saveButtonContent = new GUIContent();
	private GUIContent buildButtonContent = new GUIContent();
	private GUIContent undoButtonContent = new GUIContent();
	private GUIContent redoButtonContent = new GUIContent();
	private GUIContent toggleCommentButtonContent = new GUIContent();

	private float DoToolbar()
	{
		Rect toolbarRect = new Rect(scrollViewRect.xMin, scrollViewRect.yMin - 18f /*- 18f*/, scrollViewRect.width, 17f);
		var wasGuiEnabled = GUI.enabled;
		GUI.enabled = true;
		GUI.Label(toolbarRect, GUIContent.none, EditorStyles.toolbar);
		GUI.enabled = wasGuiEnabled;

		bool isOSX = Application.platform == RuntimePlatform.OSXEditor;

		if (!saveButtonContent.image)
		{
			string targetName = System.IO.Path.GetFileName(targetPath);

			if (isOSX)
			{
				popOutButtonContent = new GUIContent(popOutIcon, "New Tab ⌘T");
				saveButtonContent = new GUIContent(saveIcon, "Save " + targetName + " ⌥⌘S");
				buildButtonContent = new GUIContent(buildIcon, "Build Assemblies ⌥⌘S");
				undoButtonContent = new GUIContent(undoIcon, "Undo ⌃Z");
				redoButtonContent = new GUIContent(redoIcon, "Redo ⇧⌃Z");
				toggleCommentButtonContent = new GUIContent(textBuffer.isBooFile ? "#..." : "//...", "Toggle Comment Selection\n⌘K or ⌘/");
			}
			else
			{
				popOutButtonContent = new GUIContent(popOutIcon, "New Tab\n(Ctrl+T)");
				saveButtonContent = new GUIContent(saveIcon, "Save " + targetName + "\n(Ctrl+S)");
				buildButtonContent = new GUIContent(buildIcon, "Build Assemblies\n(Ctrl+S)");
				undoButtonContent = new GUIContent(undoIcon, "Undo\n(Ctrl+Z)");
				redoButtonContent = new GUIContent(redoIcon, "Redo\n(Ctrl+Shift+Z)");
				toggleCommentButtonContent = new GUIContent(textBuffer.isBooFile ? "#..." : "//...", "Toggle Comment Selection\n(Ctrl+K or Ctrl+/)");
			}
		}
		
		Color oldColor = GUI.color;
		GUI.contentColor = EditorStyles.toolbarButton.normal.textColor;

		Vector2 contentSize = EditorStyles.toolbarButton.CalcSize(popOutButtonContent);
		Rect rc = new Rect(toolbarRect.xMin + 6f, toolbarRect.yMin, contentSize.x, contentSize.y);
		if (GUI.Button(rc, popOutButtonContent, EditorStyles.toolbarButton))
		{
			EditorApplication.delayCall += OpenInNewTab;
			return 18f;
		}
		
		var enableBuildIcon = !IsModified && FGTextBufferManager.HasPendingAssetImports;
		
		GUI.enabled = CanEdit() && (IsModified || enableBuildIcon);
		contentSize = EditorStyles.toolbarButton.CalcSize(saveButtonContent);
		rc.Set(rc.xMax + 6f, toolbarRect.yMin, contentSize.x, contentSize.y);
		//if (Event.current.type != EventType.Repaint)
		if (GUI.Button(rc, enableBuildIcon ? buildButtonContent : saveButtonContent, EditorStyles.toolbarButton))
			SaveBuffer();
		/* WIP Code
		GUIStyle splitButton = new GUIStyle(EditorStyles.toolbarDropDown);
		splitButton.padding.right -= 4;
		splitButton.padding.left -= 2;
		splitButton.contentOffset = new Vector2(4f, 0f);
		contentSize = splitButton.CalcSize(GUIContent.none);
		contentSize.x -= 5f;
		Rect rc2 = new Rect(rc.xMax + 1f, toolbarRect.yMin, contentSize.x, contentSize.y);
		GUI.enabled = CanEdit();

		if (GUI.Button(rc2, GUIContent.none, splitButton))
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Save " + targetName + " %&s"), false, SaveScript);
			menu.AddItem(new GUIContent("Save as"), false, SaveScript);
			menu.AddSeparator(string.Empty);
			menu.AddItem(new GUIContent("Save All"), false, () => FGTextBufferManager.SaveAllModified(false));
			menu.DropDown(rc);
		}
		if (Event.current.type == EventType.Repaint)
		{
			GUI.enabled = CanEdit();
			if (GUI.Button(rc, new GUIContent(saveIcon, "Save " + targetName + (isOSX ? " ⌥⌘S" : "\n(Ctrl+Alt+S)")), EditorStyles.toolbarButton))
				SaveScript();
		}
		*/

		GUI.enabled = CanUndo();
		contentSize = EditorStyles.toolbarButton.CalcSize(undoButtonContent);
		rc = new Rect(rc.xMax + 6f, toolbarRect.yMin, contentSize.x, contentSize.y);
		if (GUI.Button(rc, undoButtonContent, EditorStyles.toolbarButton))
		{
			focusCodeView = true;
			Undo();
		}

		GUI.enabled = CanRedo();
		contentSize = EditorStyles.toolbarButton.CalcSize(new GUIContent(redoIcon));
		rc = new Rect(rc.xMax, toolbarRect.yMin, contentSize.x, contentSize.y);
		if (GUI.Button(rc, redoButtonContent, EditorStyles.toolbarButton))
		{
			focusCodeView = true;
			Redo();
		}

		if (textBuffer != null && !textBuffer.isText)
		{
			GUI.enabled = CanEdit();
			contentSize = EditorStyles.toolbarButton.CalcSize(toggleCommentButtonContent);
			rc = new Rect(rc.xMax, toolbarRect.yMin, contentSize.x, contentSize.y);
			if (GUI.Button(rc, toggleCommentButtonContent, EditorStyles.toolbarButton))
			{
				focusCodeView = true;
				ToggleCommentSelection();
			}
		}

		//GUI.enabled = textBuffer.hyperlinks.Count > 0;
		//GUIContent links = new GUIContent(textBuffer.hyperlinks.Count.ToString(), hyperlinksIcon);

		//contentSize = EditorStyles.toolbarDropDown.CalcSize(links);
		//rc.Set(rc.xMax + 6f, toolbarRect.yMin, contentSize.x, EditorStyles.toolbar.fixedHeight);
		//if (GUI.Button(rc, links, EditorStyles.toolbarDropDown))
		//{
		//	GenericMenu menu = new GenericMenu();
		//	foreach (string hyperlink in textBuffer.hyperlinks)
		//	{
		//		if (hyperlink.StartsWith("mailto:"))
		//		{
		//			menu.AddItem(new GUIContent(hyperlink.Substring(7)), false, FollowHyperlink, hyperlink);
		//		}
		//		else
		//		{
		//			// Shortening the URL since we cannot display slashes in the menu item,
		//			// so in most cases we'll end up with something like www.flipbookgames.com...
		//			// The first two or three slashes and the last one will be trimmed too.
		//			string escaped = hyperlink.Substring(hyperlink.IndexOf(':') + 1);

		//			// Unity cannot display a slash in menu items. Replacing any remaining slashes with
		//			// the best alternative - backslash
		//			escaped = escaped.Replace('/', '\\');

		//			// On Windows the '&' has special meaning, unless it's escaped with another '&'
		//			if (Application.platform == RuntimePlatform.WindowsEditor)
		//			{
		//				int index = 0;
		//				while (index < escaped.Length && (index = escaped.IndexOf('&', index + 1)) >= 0)
		//					escaped = escaped.Insert(index++, "&");
		//			}

		//			menu.AddItem(new GUIContent(escaped.Trim('\\')), false, FollowHyperlink, hyperlink);
		//		}
		//	}
		//	menu.DropDown(rc);
		//}
		
		
		if (p4MenusType != null)
		{
			GUI.enabled = true;
			GUIContent p4Content = new GUIContent("P4");
			
			contentSize = EditorStyles.toolbarDropDown.CalcSize(p4Content);
			rc.Set(rc.xMax + 6f, toolbarRect.yMin, contentSize.x, EditorStyles.toolbar.fixedHeight);
			if (GUI.Button(rc, p4Content, EditorStyles.toolbarDropDown))
			{
				if (p4ValidConfigurationProperty == null)
				{
					var p4ConfigType = Type.GetType("P4Connect.Config,P4Connect");
					p4ValidConfigurationProperty = p4ConfigType != null ? p4ConfigType.GetProperty("ValidConfiguration") : null;
				}
				if(p4ValidConfigurationProperty != null && !(bool) p4ValidConfigurationProperty.GetValue(null, null))
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Perforce Settings"), false, () => EditorApplication.ExecuteMenuItem("Edit/Perforce Settings"));
					menu.DropDown(rc);
				}
				else
				{
					var menuItems = new Dictionary<string, P4MenuItem>();
					
					if (p4MenusMethods == null)
						p4MenusMethods = p4MenusType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					for (int index = 0; index < p4MenusMethods.Length; ++index)
					{
						var methodInfo = p4MenusMethods[index];
						foreach (MenuItem menuItem in methodInfo.GetCustomAttributes(typeof(MenuItem), false))
						{
							P4MenuItem p4MenuItem;
							if (!menuItem.menuItem.StartsWith("Assets/Perforce/", StringComparison.OrdinalIgnoreCase))
								continue;
							
							if (!menuItems.TryGetValue(menuItem.menuItem, out p4MenuItem))
								menuItems[menuItem.menuItem] = p4MenuItem = new P4MenuItem();
							p4MenuItem.menuItem = menuItem.menuItem;
							if (menuItem.validate)
							{
								p4MenuItem.validateMethod = methodInfo;
							}
							else
							{
								p4MenuItem.executeMethod = methodInfo;
								p4MenuItem.index = index;
								p4MenuItem.priority = menuItem.priority;
							}
						}
					}
					
					if (menuItems.Count > 0)
					{
						var p4MenuItemsArray = System.Linq.Enumerable.ToArray(menuItems.Values);
						Array.Sort(p4MenuItemsArray, (a, b) =>
							a.priority != b.priority ?
							a.priority.CompareTo(b.priority) :
							a.index.CompareTo(b.index));
						
						var savedSelection = Selection.objects;
						var targetAsset = AssetDatabase.LoadAssetAtPath(targetPath, typeof(TextAsset)) as TextAsset;
						Selection.objects = new[] { targetAsset };
						
						var menu = new GenericMenu();
						foreach (var item in p4MenuItemsArray)
						{
							var enabled = item.validateMethod == null || (bool) item.validateMethod.Invoke(null, null);
							var menuItemContent = new GUIContent(item.menuItem.Substring("Assets/Perforce/".Length));
							if (enabled)
							{
								menu.AddItem(menuItemContent, false, executeMethod => ((MethodInfo) executeMethod).Invoke(null, null), item.executeMethod);
							}
							else
							{
								menu.AddDisabledItem(menuItemContent);
							}
						}
						
						menu.DropDown(rc);
						
						EditorApplication.delayCall += () => { Selection.objects = savedSelection; };
					}
				}
			}
		}
		

		//GUI.enabled = true;
		//GUIContent dump = new GUIContent("Dump");

		//contentSize = EditorStyles.toolbarButton.CalcSize(dump);
		//rc.Set(rc.xMax + 6f, toolbarRect.yMin, contentSize.x, EditorStyles.toolbar.fixedHeight);
		//if (GUI.Button(rc, dump, EditorStyles.toolbarDropDown))
		//{
		//	var debug = textBuffer.Parser.parseTree.root.ToString();
		//	Debug.Log(debug);
		//	EditorGUIUtility.systemCopyBuffer = debug;
		//}


		
		
		if (EditorApplication.isCompiling && holdingAssemblies > 0 && compilers.Count == 0)
		{
			GUI.enabled = true;
			GUI.backgroundColor = Color.green;
			GUIContent reload = new GUIContent("RELOAD");
				
			contentSize = EditorStyles.toolbarButton.CalcSize(reload);
			rc.Set(rc.xMax + 6f, toolbarRect.yMin, contentSize.x, EditorStyles.toolbar.fixedHeight);
			if (GUI.Button(rc, reload, EditorStyles.toolbarButton))
			{
				MenuReloadAssemblies();
				Repaint();
			}
			GUI.backgroundColor = Color.white;
		}

		GUI.contentColor = oldColor;
		
		rc.xMin = rc.xMax + 8f;
		rc.xMax = toolbarRect.xMax - 25f;
		string infoText =
			textBuffer.IsLoading ? "Loading..." :
			!EditorApplication.isCompiling && FGTextBufferManager.HasPendingAssetImports && !textBuffer.IsModified ? "Saved... Save again to compile!" :
			!EditorApplication.isCompiling ? null :
			compilers.Count > 0 ? "Compiling in background..." :
			holdingAssemblies == 0 ? "Compiling and reloading assemblies..." :
			IsModified && isOSX ? "Cmd-Alt-R to reload assemblies..." :
			IsModified ? "Ctrl+R to reload assemblies..." :
			isOSX ? "Cmd-Alt-R or Save again to reload assemblies..." :
			"Ctrl+R or Save again to reload assemblies...";
		if (infoText == null)
		{
			GUI.enabled = false;
			GUI.Label(rc, textBuffer.fileEncoding.EncodingName);
		}
		else
		{
			GUI.enabled = true;
			if (orangeBoldLabel == null)
			{
				orangeBoldLabel = new GUIStyle(EditorStyles.boldLabel);
				orangeBoldLabel.normal.textColor =
					EditorGUIUtility.isProSkin ? new Color(1f, 0.5f, 0f) : new Color(0.9f, 0.4f, 0f);
			}
			GUI.Label(rc, infoText, orangeBoldLabel);
		}
		
		GUI.enabled = CanEdit();

		rc.y += 2f;
		rc.height = 16f;
		if (rc.width > 331f)
			rc.xMin = rc.xMax - 331f;
		DoSearchBox(rc);
		
		// Only redrawing the default wrench icon after being covered with our toolbar.
		// The default icon still handles the functionality in the Inspector tab.
		if (wrenchIcon != null)
			GUI.Label(new Rect(toolbarRect.xMax - 20f, toolbarRect.yMin + 2f, 15f, 14f), wrenchIcon, GUIStyle.none);

		if (Event.current.type == EventType.ContextClick && toolbarRect.Contains(Event.current.mousePosition))
			Event.current.Use();

		GUI.enabled = true;
		return 18f;
	}

	private void OpenInNewTab()
	{
		if (EditorWindow.focusedWindow.GetType() != typeof(FGCodeWindow))
			FGCodeWindow.OpenNewWindow(null, null, false);
		else
			EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("ScriptInspector.AddTab"));
	}

	private void DoSearchBox(Rect position)
	{
		if (Event.current.type == EventType.ValidateCommand)
		{
			if (Event.current.commandName == "Find")
			{
				Event.current.Use();
				return;
			}
		}
		else if (Event.current.type == EventType.ExecuteCommand)
		{
			if (Event.current.commandName == "Find")
			{
				if (hasCodeViewFocus)
					UseSelectionForSearch();
				focusSearchBox = true;
				Event.current.Use();
			}
		}

		if (textBuffer.undoPosition != searchResultAge)
		{
			currentSearchResult = -1;
			atLastSearchResult = false;
		}

		if ((Event.current.type == EventType.MouseDown) && position.Contains(Event.current.mousePosition))
		{
			focusSearchBox = true;
		}

		if (Event.current.type == EventType.KeyDown && !Event.current.alt)
		{
			var isOSX = Application.platform == RuntimePlatform.OSXEditor;
			if (Event.current.keyCode == KeyCode.F3 && !EditorGUI.actionKey ||
				isOSX && Event.current.keyCode == KeyCode.G && EditorGUI.actionKey)
			{
				if (Event.current.shift)
					SearchPrevious();
				else
					SearchNext();
				
				Event.current.Use();
			}
		}

		if (focusCodeViewOnEscapeUp && Event.current.rawType == EventType.KeyUp &&
			(Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Tab))
		{
			focusCodeViewOnEscapeUp = false;
			focusCodeView = true;
			Event.current.Use();
		}

		if (hasSearchBoxFocus && Event.current.type == EventType.KeyDown)
		{
			if (Event.current.keyCode == KeyCode.Escape)
			{
				searchString = string.Empty;
				SetSearchText(searchString);
				focusCodeViewOnEscapeUp = true;
				Event.current.Use();
			}
			else if (Event.current.keyCode == KeyCode.UpArrow)
			{
				SearchPrevious();
				Event.current.Use();
			}
			else if (Event.current.keyCode == KeyCode.DownArrow)
			{
				SearchNext();
				Event.current.Use();
			}
			else if (Event.current.character == '\n')
			{
				//currentSearchResult = currentSearchResult < 0 ? 0 :
				//	currentSearchResult < searchResults.Count ? currentSearchResult : searchResults.Count - 1;
				//ShowSearchResult(currentSearchResult);
				SearchNext();
				focusCodeView = true;
				Event.current.Use();
			}
			else if (Event.current.keyCode == KeyCode.Tab)
			{
				focusCodeViewOnEscapeUp = true;
				Event.current.Use();
			}
		}

		string text = ToolbarSearchField(position, searchString);
		
		if (searchString != text)
		{
			searchString = text;
			SetSearchText(searchString);
			hasSearchBoxFocus = true;
		}
	}

	private void SetSearchText(string text)
	{
		searchResultAge = textBuffer.undoPosition;
		//atLastSearchResult = false;
		defaultSearchString = text;

		searchResults.Clear();
		currentSearchResult = -1;
		int textLength = text.Length;

		if (textLength == 0)
		{
			Repaint();
			return;
		}

		int i = 0;
		foreach (string line in textBuffer.lines)
		{
			for (int pos = 0; (pos = line.IndexOf(text, pos, StringComparison.InvariantCultureIgnoreCase)) != -1; pos += textLength )
			{
				int columnFrom = textBuffer.CharIndexToColumn(pos, i);
				FGTextBuffer.CaretPos caretPos = new FGTextBuffer.CaretPos { line = i, characterIndex = pos, column = columnFrom, virtualColumn = columnFrom };
				searchResults.Add(caretPos);
			}
			++i;
		}
		
		highlightSearchResults = text != ""; //text.Trim() != "";
		Repaint();
	}

	public static bool OverrideButton(Rect position, GUIContent content, GUIStyle style, bool forceHot)
	{
		int controlID = GUIUtility.GetControlID(buttonHash, FocusType.Passive, position);
		if (forceHot)
			GUIUtility.hotControl = controlID;

		switch (Event.current.GetTypeForControl(controlID))
		{
			case EventType.MouseDown:
				if (position.Contains(Event.current.mousePosition))
				{
					GUIUtility.hotControl = controlID;
					Event.current.Use();
				}
				return false;

			case EventType.MouseUp:
				if (GUIUtility.hotControl != controlID)
					return false;

				GUIUtility.hotControl = 0;
				Event.current.Use();
				return position.Contains(Event.current.mousePosition);

			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
					Event.current.Use();
				break;

			case EventType.Repaint:
				style.Draw(position, content, controlID);
				break;
		}

		return false;
	}

	private string ToolbarSearchField(Rect position, string text)
    {
		if (styles.toolbarSearchField == null)
		{
			styles.toolbarSearchField = "ToolbarSeachTextField";
			styles.toolbarSearchFieldCancelButton = "ToolbarSeachCancelButton";
			styles.toolbarSearchFieldCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
		}

		Rect rc = position;
		rc.width -= 14f;
		if (Event.current.type == EventType.repaint)
		{
			styles.toolbarSearchField.Draw(rc, GUIContent.none, false, false, false, hasSearchBoxFocus);

			if (searchResults.Count > 0)
			{
				bool enabled = GUI.enabled;
				GUI.enabled = false;
				Color color = GUI.backgroundColor;
				GUI.backgroundColor = Color.clear;
				styles.toolbarSearchField.alignment = TextAnchor.UpperRight;
				rc.width -= 20f;
				styles.toolbarSearchField.Draw(rc, (currentSearchResult >= 0 ? (currentSearchResult + 1).ToString() + " of " + searchResults.Count :
					searchResults.Count.ToString() + " results") + '\xa0', false, false, false, hasSearchBoxFocus);
				rc.width += 20f;
				styles.toolbarSearchField.alignment = TextAnchor.UpperLeft;
				GUI.enabled = enabled;
				GUI.backgroundColor = color;
			}
		}
		rc.width -= 20f;
	    
	    GUI.SetNextControlName("SearchBox");
		if (focusSearchBox)
		{
			GUI.FocusControl("SearchBox");
#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
			EditorGUI.FocusTextInControl("SearchBox");
#endif

			if (Event.current.type == EventType.Repaint)
			{
				focusSearchBox = false;
			}
		}

		Color bgColor = GUI.backgroundColor;
		GUI.backgroundColor = Color.clear;
		text = EditorGUI.TextField(rc, text, styles.toolbarSearchField);
		GUI.backgroundColor = bgColor;
		
		hasSearchBoxFocus = GUI.GetNameOfFocusedControl() == "SearchBox" || focusSearchBox;

		bool isEmpty = text == string.Empty;

	    if (!hasSearchBoxFocus && !isEmpty && text.Trim() == "")
			GUI.Label(rc, text.Replace("\t", "<tab>").Replace(" ", "<space>"), styles.toolbarSearchField);

		rc = position;
		rc.x += position.width - 14f;
		rc.width = 14f;
		if (!isEmpty)
		{
			if (OverrideButton(rc, GUIContent.none, styles.toolbarSearchFieldCancelButton, helpButtonClicked))
			{
				text = string.Empty;
				focusCodeView = true;
				//GUIUtility.keyboardControl = 0;
			}
		}
		else
		{
			GUI.Label(rc, GUIContent.none, styles.toolbarSearchFieldCancelButtonEmpty);
			if (helpButtonClicked)
				focusSearchBox = true;
		}
		helpButtonClicked = false;

		rc.x -= 10f;
		rc.y += 1f;
        rc.width = 10f;
		rc.height = 13f;
		if (!isEmpty && searchResults.Count != 0 && GUI.Button(rc, GUIContent.none, styles.downArrowStyle))
			SearchNext();

		rc.x -= 10f;
		if (!isEmpty && searchResults.Count != 0 && GUI.Button(rc, GUIContent.none, styles.upArrowStyle))
			SearchPrevious();

		return text;
	}

	private static int FindFirstIndexGreaterThanOrEqualTo<T>(IList<T> sortedCollection, T key)
	{
		return FindFirstIndexGreaterThanOrEqualTo<T>(sortedCollection, key, Comparer<T>.Default);
	}

	private static int FindFirstIndexGreaterThanOrEqualTo<T>(IList<T> sortedCollection, T key, IComparer<T> comparer)
	{
		int begin = 0;
		int end = sortedCollection.Count;
		while (end > begin)
		{
			int index = (begin + end) / 2;
			T el = sortedCollection[index];
			if (comparer.Compare(el, key) >= 0)
				end = index;
			else
				begin = index + 1;
		}
		return end;
	}
	
	[NonSerialized]
	private bool lastCaretMoveWasSearch;

	private void SearchPrevious()
	{
		if (searchResultAge != textBuffer.undoPosition)
			SetSearchText(searchString);

		FGTextBuffer.CaretPos beginning = caretPosition;
		if (selectionStartPosition != null && selectionStartPosition < caretPosition)
			beginning = selectionStartPosition;
		currentSearchResult = FindFirstIndexGreaterThanOrEqualTo(searchResults, beginning) - 1;
		currentSearchResult = Math.Min(searchResults.Count - 1, currentSearchResult);
		if (currentSearchResult == -1)
		{
			if (atLastSearchResult && SISettings.loopSearchResults)
			{
				currentSearchResult = searchResults.Count - 1;
				atLastSearchResult = searchResults.Count == 1;
			}
			else
			{
				currentSearchResult = 0;
				atLastSearchResult = true;
			}
		}
		else
		{
			atLastSearchResult = searchResults.Count == 1;
		}
		ShowSearchResult(currentSearchResult);
	}

	private void SearchNext()
	{
		if (searchResultAge != textBuffer.undoPosition)
			SetSearchText(searchString);
		
		var end = caretPosition;
		if (selectionStartPosition != null && selectionStartPosition > caretPosition)
			end = selectionStartPosition;
		currentSearchResult = FindFirstIndexGreaterThanOrEqualTo(searchResults, end);
		currentSearchResult = Math.Max(0, currentSearchResult);
		if (currentSearchResult >= searchResults.Count)
		{
			if (atLastSearchResult && SISettings.loopSearchResults)
			{
				currentSearchResult = 0;
				atLastSearchResult = searchResults.Count == 1;
			}
			else
			{
				currentSearchResult = searchResults.Count - 1;
				atLastSearchResult = true;
			}
		}
		else
		{
			atLastSearchResult = searchResults.Count == 1;
		}
		ShowSearchResult(currentSearchResult);
	}

	private void ShowSearchResult(int index)
	{
		if (!lastCaretMoveWasSearch)
		{
			lastCaretMoveWasSearch = true;
			AddRecentLocation(0, true);
		}
		
		if (searchResultAge != textBuffer.undoPosition)
			SetSearchText(searchString);

		if (index >= 0 && index < searchResults.Count)
		{
			PingText(searchResults[index], searchString.Length, atLastSearchResult ? new Color(1f, .5f, 1f) : Color.white);
		}
	}

	private int CharIndexToColumn(int charIndex, int line)
	{
		if (wordWrapping)
		{
			List<int> softLineBreaks = GetSoftLineBreaks(line);
			int softRow = FindFirstIndexGreaterThanOrEqualTo<int>(softLineBreaks, charIndex);
			return textBuffer.CharIndexToColumn(charIndex, line, softRow > 0 ? softLineBreaks[softRow - 1] : 0);
		}
		else
		{
			return textBuffer.CharIndexToColumn(charIndex, line);
		}
	}

	public void PingText(FGTextBuffer.CaretPos startPosition, int numChars, Color color)
	{
		CloseAllPopups();
		
		startPosition = startPosition.Clone();
		
		if (startPosition.line >= textBuffer.lines.Count)
			startPosition.line = textBuffer.lines.Count - 1;
		var lineText = textBuffer.lines[startPosition.line];
		startPosition.characterIndex = Mathf.Min(startPosition.characterIndex, lineText.Length);
		
		selectionStartPosition = startPosition.Clone();
		
		selectionStartPosition.column = selectionStartPosition.virtualColumn = CharIndexToColumn(startPosition.characterIndex, startPosition.line);

		int toCharIndex = Mathf.Min(selectionStartPosition.characterIndex + numChars, lineText.Length);
		int columnTo = CharIndexToColumn(toCharIndex, selectionStartPosition.line);
		caretPosition = new FGTextBuffer.CaretPos {
			characterIndex = startPosition.characterIndex + numChars,
			column = columnTo,
			virtualColumn = columnTo,
			line = selectionStartPosition.line
		};
		caretMoveTime = Time.realtimeSinceStartup;

		int fromRow, fromColumn, toRow, toColumn;
		int fromCharIndex = selectionStartPosition.characterIndex;
		BufferToViewPosition(selectionStartPosition, out fromRow, out fromColumn);
		BufferToViewPosition(caretPosition, out toRow, out toColumn);
		if (fromRow != toRow)
		{
			fromColumn = 0;
			var newFromCharIndex = GetSoftLineBreaks(caretPosition.line)[toRow - 1];
			numChars -= newFromCharIndex - fromCharIndex;
			fromCharIndex = newFromCharIndex;
		}

		scrollToRect.x = charSize.x * fromColumn;
		scrollToRect.y = GetLineOffset(caretPosition.line) + charSize.y * toRow;
		scrollToRect.xMax = charSize.x * toColumn;
		scrollToRect.height = charSize.y;

		pingTimer = 1f;
		numChars = Mathf.Min(numChars, lineText.Length - fromCharIndex);
		pingContent.text = lineText.Substring(fromCharIndex, numChars);
		//pingStartTime = Time.realtimeSinceStartup;
		pingColor = color;

		Repaint();
	}

	public static void RepaintAllInstances()
	{
		var allInspectors = (ScriptInspector[]) Resources.FindObjectsOfTypeAll(typeof(ScriptInspector));
		if (allInspectors != null)
			foreach (var wnd in allInspectors)
				wnd.Repaint();
		FGCodeWindow.RepaintAllWindows();
	}

	[MenuItem("CONTEXT/MonoScript/Word Wrap (Code)", false, 141)]
	private static void ToggleWordWrapCode()
	{
		if (EditorWindow.focusedWindow is FGCodeWindow)
			SISettings.wordWrapCode.Toggle();
		else
			SISettings.wordWrapCodeInspector.Toggle();
	}

	[MenuItem("CONTEXT/MonoScript/Highlight Current Line", false, 142)]
	private static void ToggleHighlightCurrentLine()
	{
		SISettings.highlightCurrentLine.Toggle();
	}

	[MenuItem("CONTEXT/MonoScript/Frame Current Line", false, 143)]
	private static void ToggleFrameCurrentLine()
	{
		SISettings.frameCurrentLine.Toggle();
	}

	[MenuItem("CONTEXT/MonoScript/Line Numbers (Code)", false, 144)]
	private static void ToggleLineNumbersCode()
	{
		if (EditorWindow.focusedWindow is FGCodeWindow)
			SISettings.showLineNumbersCode.Toggle();
		else
			SISettings.showLineNumbersCodeInspector.Toggle();
	}

	private static void ToggleLineNumbersText()
	{
		if (EditorWindow.focusedWindow is FGCodeWindow)
			SISettings.showLineNumbersText.Toggle();
		else
			SISettings.showLineNumbersTextInspector.Toggle();
	}
	
	[MenuItem("CONTEXT/MonoScript/Track Changes (Code)", false, 145)]
	private static void ToggleTrackChangesCode()
	{
		if (EditorWindow.focusedWindow is FGCodeWindow)
			SISettings.trackChangesCode.Toggle();
		else
			SISettings.trackChangesCodeInspector.Toggle();
	}

	private static void ToggleTrackChangesText()
	{
		if (EditorWindow.focusedWindow is FGCodeWindow)
			SISettings.trackChangesText.Toggle();
		else
			SISettings.trackChangesTextInspector.Toggle();
	}

	private static void ToggleWordWrapText()
	{
		if (EditorWindow.focusedWindow is FGCodeWindow)
			SISettings.wordWrapText.Toggle();
		else
			SISettings.wordWrapTextInspector.Toggle();
	}

	[MenuItem("CONTEXT/MonoScript/Darcula Theme", false, 160)]
	private static void SetCodeStyleDarcula() { SelectTheme(8, false); }

	[MenuItem("CONTEXT/MonoScript/MD Brown Theme", false, 160)]
	private static void SetCodeStyleMDBrown() { SelectTheme(4, false); }

	[MenuItem("CONTEXT/MonoScript/MD Brown - Dark Theme", false, 161)]
	private static void SetCodeStyleMDBrownDark() { SelectTheme(5, false); }

	[MenuItem("CONTEXT/MonoScript/Monokai Theme", false, 162)]
	private static void SetCodeStyleMonokai() { SelectTheme(6, false); }

	[MenuItem("CONTEXT/MonoScript/Solarized Dark Theme", false, 163)]
	private static void SetCodeStyleSolarideDark() { SelectTheme(12, false); }

	[MenuItem("CONTEXT/MonoScript/Solarized Light Theme", false, 163)]
	private static void SetCodeStyleSolarizedLight() { SelectTheme(13, false); }

	[MenuItem("CONTEXT/MonoScript/Son of Obsidian Theme", false, 163)]
	private static void SetCodeStyleSonOfObsidian() { SelectTheme(7, false); }

	[MenuItem("CONTEXT/MonoScript/Tango Dark (Oblivion) Theme", false, 164)]
	private static void SetCodeStyleTangoDark() { SelectTheme(2, false); }

	[MenuItem("CONTEXT/MonoScript/Tango Light Theme", false, 165)]
	private static void SetCodeStyleTangoLight() { SelectTheme(3, false); }

	[MenuItem("CONTEXT/MonoScript/Visual Studio Theme", false, 166)]
	private static void SetCodeStyleVisualStudio() { SelectTheme(0, false); }

	[MenuItem("CONTEXT/MonoScript/Visual Studio Dark Theme", false, 166)]
	private static void SetCodeStyleVisualStudioDark() { SelectTheme(9, false); }

	[MenuItem("CONTEXT/MonoScript/VS Dark with ReSharper Theme", false, 166)]
	private static void SetCodeStyleVSDarkWithReSharper() { SelectTheme(14, false); }

	[MenuItem("CONTEXT/MonoScript/VS Dark with VA X Theme", false, 166)]
	private static void SetCodeStyleVSDarkWithVAX() { SelectTheme(11, false); }

	[MenuItem("CONTEXT/MonoScript/VS Light with VA X Theme", false, 166)]
	private static void SetCodeStyleVSLightWithVAX() { SelectTheme(10, false); }

	[MenuItem("CONTEXT/MonoScript/Xcode Theme", false, 167)]
	private static void SetCodeStyleXcode() { SelectTheme(1, false); }

	private static void SelectTheme(int themeIndex, bool forText)
	{
		if (forText)
			currentThemeText = themes[themeIndex];
		else
			currentThemeCode = themes[themeIndex];
		ApplyTheme(forText ? stylesText : stylesCode, themes[themeIndex]);
		if (forText)
			SISettings.themeNameText.Value = availableThemes[themeIndex];
		else
			SISettings.themeNameCode.Value = availableThemes[themeIndex];
		RepaintAllInstances();
	}

	private static void ResetFontSize()
	{
		SISettings.fontSizeDelta.Value = 0;
		SISettings.fontSizeDeltaInspector.Value = 0;
		resetCodeFont = true;
		resetTextFont = true;
		if (stylesCode.normalStyle != null)
		{
			stylesCode.normalStyle.fontSize = 0;
			stylesCode.hyperlinkStyle.fontSize = 0;
			stylesCode.mailtoStyle.fontSize = 0;
			stylesCode.keywordStyle.fontSize = 0;
			stylesCode.constantStyle.fontSize = 0;
			stylesCode.referenceTypeStyle.fontSize = 0;
			stylesCode.commentStyle.fontSize = 0;
			stylesCode.stringStyle.fontSize = 0;
			stylesCode.lineNumbersStyle.fontSize = 0;
			stylesCode.preprocessorStyle.fontSize = 0;
			stylesCode.parameterStyle.fontSize = 0;
			stylesCode.typeParameterStyle.fontSize = 0;
			stylesCode.enumMemberStyle.fontSize = 0;
			stylesCode.tooltipTextStyle.fontSize = 0;
			stylesCode.ping.fontSize = 0;

			stylesCode.normalStyle.fontStyle = 0;
			stylesCode.hyperlinkStyle.fontStyle = 0;
			stylesCode.mailtoStyle.fontStyle = 0;
			stylesCode.keywordStyle.fontStyle = 0;
			stylesCode.constantStyle.fontStyle = 0;
			stylesCode.referenceTypeStyle.fontStyle = 0;
			stylesCode.commentStyle.fontStyle = 0;
			stylesCode.stringStyle.fontStyle = 0;
			stylesCode.lineNumbersStyle.fontStyle = 0;
			stylesCode.preprocessorStyle.fontStyle = 0;
			stylesCode.parameterStyle.fontStyle = 0;
			stylesCode.typeParameterStyle.fontStyle = 0;
			stylesCode.enumMemberStyle.fontStyle = 0;
			stylesCode.ping.fontStyle = 0;
		}
		if (stylesText.normalStyle != null)
		{
			stylesText.normalStyle.fontSize = 0;
			stylesText.hyperlinkStyle.fontSize = 0;
			stylesText.mailtoStyle.fontSize = 0;
			stylesText.keywordStyle.fontSize = 0;
			stylesText.constantStyle.fontSize = 0;
			stylesText.referenceTypeStyle.fontSize = 0;
			stylesText.commentStyle.fontSize = 0;
			stylesText.stringStyle.fontSize = 0;
			stylesText.lineNumbersStyle.fontSize = 0;
			stylesText.preprocessorStyle.fontSize = 0;
			stylesText.parameterStyle.fontSize = 0;
			stylesText.typeParameterStyle.fontSize = 0;
			stylesText.enumMemberStyle.fontSize = 0;
			stylesText.tooltipTextStyle.fontSize = 0;
			stylesText.ping.fontSize = 0;

			stylesText.normalStyle.fontStyle = 0;
			stylesText.hyperlinkStyle.fontStyle = 0;
			stylesText.mailtoStyle.fontStyle = 0;
			stylesText.keywordStyle.fontStyle = 0;
			stylesText.constantStyle.fontStyle = 0;
			stylesText.referenceTypeStyle.fontStyle = 0;
			stylesText.commentStyle.fontStyle = 0;
			stylesText.stringStyle.fontStyle = 0;
			stylesText.lineNumbersStyle.fontStyle = 0;
			stylesText.preprocessorStyle.fontStyle = 0;
			stylesText.parameterStyle.fontStyle = 0;
			stylesText.typeParameterStyle.fontStyle = 0;
			stylesText.enumMemberStyle.fontStyle = 0;
			stylesText.ping.fontStyle = 0;
		}
	}

	private static void SelectFont(int fontIndex)
	{
		resetCodeFont = true;
		resetTextFont = true;
		SISettings.editorFont.Value = currentFont = availableFonts[fontIndex];
		//ApplyTheme();
		//RepaintAllInstances();
	}

	private static void ToggleFontHinting()
	{
		resetCodeFont = true;
		resetTextFont = true;
		SISettings.fontHinting.Toggle();
	}

	private void ModifyFontSize(int delta)
	{
		resetCodeFont = true;
		resetTextFont = true;
		//var isInspector = parentWindow == null;
		//if (isInspector)
		//	SISettings.fontSizeDeltaInspector.Value = Math.Max(-10, Math.Min(10, SISettings.fontSizeDeltaInspector + Math.Sign(delta)));
		//else
			SISettings.fontSizeDelta.Value = Math.Max(-10, Math.Min(10, SISettings.fontSizeDelta + Math.Sign(delta)));
	}

	private static void ToggleHandleOpenAsset()
	{
		SISettings.handleOpenAssets.Toggle();
	}

	private static void ToggleHandleOpenFromProject()
	{
		SISettings.handleOpeningScripts.Toggle();
	}

	private static void ToggleHandleOpenTextsFromProject()
	{
		SISettings.handleOpeningText.Toggle();
	}

	private static void ToggleHandleOpenShadersFromProject()
	{
		SISettings.handleOpeningShaders.Toggle();
	}

	[MenuItem("Window/Script Inspector 3/About...", false, 2000)]
	[MenuItem("CONTEXT/MonoScript/About...", false, 210)]
	private static void About()
	{
		var wnd = EditorWindow.GetWindow<AboutScriptInspector>(true);
		wnd.ShowAuxWindow();
	}
}

}
