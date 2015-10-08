using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace VoxelBusters.DebugPRO
{
	using Internal;

	public partial class Console : EditorWindow
	{
		#region Properties						

		// Related to filters
		[SerializeField]
		private float			m_filtersWindowWidth				= kFiltersWindowMinWidth;
		private bool			m_resizingFiltersWindow;
		private Vector2			m_filtersScrollPosition				= Vector2.zero;

		// Related to displayable console logs
		private Vector2			m_displayableLogsScrollPosition		= Vector2.zero;

		// Related to log description
		[SerializeField]
		private float			m_descriptionWindowHeight			= kDescriptionWindowMinHeight;
		private Vector2			m_descriptionScrollPosition			= Vector2.zero;

		// Related to mouse events
		private float			m_lastMouseClickRegisteredAtTime;
		private float			m_lastKeyStrokeRegisteredAtTime;
		private int				m_clickCount;

		// Related to textures
		private Texture2D		m_consoleLogOnNormalBackgroundTexture;
		private Texture2D		m_consoleLogOffNormalBackgroundTexture;
		private Texture2D		m_logEntryActiveBackgroundTexture;

		// Related to GUI styles
		private GUIStyle		m_consoleLogOffGUIStyle;
		private GUIStyle		m_consoleLogOnGUIStyle;
		private GUIStyle		m_infoGUIStyle;
		private GUIStyle		m_warningGUIStyle;
		private GUIStyle		m_errorGUIStyle;
		private GUIStyle		m_filtersGUIStyle;

		// Related to Color
		private Color 			m_textColorNormal					= Color.black;
		private Color 			m_textColorActive					= new Color(1f, 1f, 1f, 1f);
		
		// Related to system icons
		private Texture2D		InfoIcon
		{
			get 
			{
				return m_infoGUIStyle.normal.background;
			}
		}

		private Texture2D		WarningIcon
		{
			get 
			{
				return m_warningGUIStyle.normal.background;
			}
		}
		
		private Texture2D		ErrorIcon
		{
			get 
			{
				return m_errorGUIStyle.normal.background;
			}
		}

		#endregion

		#region Constants

		// Related to menuitem
		private const int		kDebugConsoleItemPriority			= 200;

		// Related to GUI style
		private const string	kToolBarStyle						= "Toolbar";
		private const string	kToolBarButtonStyle					= "toolbarbutton";
		private const string	kInfoStyle							= "CN EntryInfo";
		private const string	kWarningStyle						= "CN EntryWarn";
		private const string	kErrorStyle							= "CN EntryError";
		private const string	kBoxStyle							= "CN Box";
		private const string	kWordWrappedStyle					= "WordWrappedLabel";
#if UNITY_PRO_LICENSE
		private const string	kFiltersTitleStyle					= "toolbarbutton";
		private const string	kFiltersStyle						= "toolbarbutton";
#else
		private const string	kFiltersStyle						= "PreToolbar";
		private const string	kFiltersTitleStyle					= "MeTransitionHead";
#endif

		// Related to window
		private const float		kFiltersWindowMinWidth				= 120f;
		private const float		kDescriptionWindowMinHeight			= 120f;

		// Related to user events
		private const float		kMaxDelayBetweenConsecutiveClicks	= 0.2f;
		private const float		kMinDelayBetweenConsecutiveKeyStroke= 0.25f;

		// Related to window
		private const string	kCanCreateInstance					= "debug-pro-can-create-instance";

		#endregion

		#region Static Methods

		[MenuItem("Window/Voxel Busters/Debug Console", false, kDebugConsoleItemPriority)]
		private static void ShowConsole ()
		{
			// Mark a flag to show console window
			EditorPrefs.SetBool(kCanCreateInstance, true);

			// Show
			Instance.Show();

			// Focus it if its open
			Instance.Focus();
		}

		#endregion

		#region Unity Methods

		private void OnDestroy ()
		{
			EditorPrefs.SetBool(kCanCreateInstance, false);
		}

		private void OnInspectorUpdate () 
		{
			// Call Repaint on OnInspectorUpdate as it repaints the windows
			// less times as if it was OnGUI/Update
			Repaint();
		}

		private void OnGUI () 
		{
			// Create GUI style
			CreateGUIStyle();

			// Monitoring user events
			CheckForUserEvents();

			GUILayout.BeginHorizontal();
			{
				// Draw filters window
				DrawFilters();
				
				GUILayout.BeginVertical();
				{
					// Draw toolbar
					DrawToolBar();
					
					// Draw console logs
					DrawConsoleLogs();
					
					// Draw description
					DrawDescription();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region GUI Drawing Methods 
		
		private void DrawToolBar ()
		{
			GUILayout.BeginHorizontal(kToolBarStyle);
			{
				// Clear all the logs
				if (GUILayout.Button("Clear", kToolBarButtonStyle))
				{
					Clear();
				}
				
				// Clear on play, Error pause
				m_clearOnPlay				= GUILayout.Toggle(m_clearOnPlay, "Clear on Play", kToolBarButtonStyle);
				m_errorPause				= GUILayout.Toggle(m_errorPause, "Error Pause", kToolBarButtonStyle);
				
				// Flexi space
				GUILayout.FlexibleSpace();
				EditorGUILayout.Separator();

				// Log-type info bar buttons
				bool _showInfoLogsNewValue		= GUILayout.Toggle(ShowInfoLogs, new GUIContent(m_infoLogsCounterStr, InfoIcon), kToolBarButtonStyle);
				bool _showWarningLogsNewValue	= GUILayout.Toggle(ShowWarningLogs, new GUIContent(m_warningLogsCounterStr, WarningIcon), kToolBarButtonStyle);
				bool _showErrorLogsNewValue		= GUILayout.Toggle(ShowErrorLogs, new GUIContent(m_errorLogsCounterStr, ErrorIcon), kToolBarButtonStyle);

				// Update instance value, if value has changed
				if (_showInfoLogsNewValue != m_showInfoLogs)
					ShowInfoLogs		= _showInfoLogsNewValue;

				if (_showWarningLogsNewValue != m_showWarningLogs)
					ShowWarningLogs		= _showWarningLogsNewValue;

				if (_showErrorLogsNewValue != ShowErrorLogs)
					ShowErrorLogs		= _showErrorLogsNewValue;
			}
			GUILayout.EndHorizontal();
		}

		private void DrawFilters ()
		{
			GUILayout.BeginVertical(kBoxStyle, GUILayout.MinWidth(kFiltersWindowMinWidth), GUILayout.Width(m_filtersWindowWidth));
			{
				// Filters tab
				GUILayout.Label("Filters", kFiltersTitleStyle, GUILayout.Width(m_filtersWindowWidth));

				m_filtersScrollPosition	= GUILayout.BeginScrollView(m_filtersScrollPosition);
				{
					int _consoleTagsCount	= 0;
					
					if (m_consoleTags != null)
						_consoleTagsCount	= m_consoleTags.Count;

					for (int _iter = 0; _iter < _consoleTagsCount; _iter++)
					{
						ConsoleTag _consoleTag	= m_consoleTags[_iter];

						// Check for change
						EditorGUI.BeginChangeCheck();

						_consoleTag.IsActive	= GUILayout.Toggle(_consoleTag.IsActive, _consoleTag.Name, m_filtersGUIStyle);

						// If state of console tag is modified, then we need to rebuild display list
						if (EditorGUI.EndChangeCheck())
						{
							RebuildDisplayableLogs();
						}
					}
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		private void DrawConsoleLogs ()
		{
			m_displayableLogsScrollPosition	= GUILayout.BeginScrollView(m_displayableLogsScrollPosition, kBoxStyle);
			{
				// Get all console logs that needs to be drawed in these frame
				int _displayableLogsCount	= 0;
				
				if (m_displayableConsoleLogsList != null)
					_displayableLogsCount	= m_displayableConsoleLogsList.Count;

				bool _selectionChanged	= false;

				for (int _iter = 0; _iter < _displayableLogsCount; _iter++)
				{
					// Display log entry
					_selectionChanged = DisplayConsoleLogEntry(m_displayableConsoleLogsList[_iter], (_iter % 2) == 0);

					if (_selectionChanged)
						break;
				}

				if (_selectionChanged)
				{
					EditorUtility.SetDirty(this);
				}
			}
			GUILayout.EndScrollView();
		}

		private bool DisplayConsoleLogEntry (ConsoleLog _consoleLog, bool _evenNumberedEntry)
		{
			GUIStyle _useStyle 		= null;
			Texture2D _iconTexture	= null;
			bool _isSelected		= _consoleLog.Equals(m_selectedConsoleLog);

			// Select icon based on log-type icon
			if (_consoleLog.Type == eConsoleLogType.INFO)
			{
				_iconTexture	= InfoIcon;
			}
			else if (_consoleLog.Type == eConsoleLogType.WARNING)
			{
				_iconTexture	= WarningIcon;
			}
			else
			{
				_iconTexture	= ErrorIcon;
			}

			// Select GUI style for this entry
			if (_evenNumberedEntry)
				_useStyle	= m_consoleLogOnGUIStyle;
			else
				_useStyle	= m_consoleLogOffGUIStyle;

			// When user clicks this element, perform action based on click-count
			bool _newSelection	= GUILayout.Toggle(_isSelected, new GUIContent(_consoleLog.Message, _iconTexture), _useStyle);
			
			if (_newSelection != _isSelected)
			{
				// Cache current log, which is used to show description
				m_selectedConsoleLog	= _consoleLog;
				
				// Perform on-select action
				_consoleLog.OnSelect();

				// Click count is 2, then perform on-press action
				if (m_clickCount == 2)
				{
					_consoleLog.OnPress();
				}

				return true;
			}

			return false;
		}

		private void DrawDescription ()
		{
			m_descriptionScrollPosition	= GUILayout.BeginScrollView(m_descriptionScrollPosition, kBoxStyle, GUILayout.MinHeight(kDescriptionWindowMinHeight), GUILayout.Height(m_descriptionWindowHeight));
			{
				if (m_selectedConsoleLog.IsValid())
				{
					EditorGUILayout.SelectableLabel(m_selectedConsoleLog.Description, kWordWrappedStyle, GUILayout.Height(m_descriptionWindowHeight));
				}
			}
			GUILayout.EndScrollView();
		}

		#endregion

		#region User Event Handling Methods

		private void CheckForUserEvents ()
		{
			// We are interested only when user has selected this particular window
			if (EditorWindow.focusedWindow != this)
				return;

			// Keyboard events
			CheckForKeyBoardEvents();

			// Mouse events
			CheckForMouseEvents();
		}

		private void CheckForKeyBoardEvents ()
		{
			Event _curEvent	= Event.current;

			if (!_curEvent.isKey)
				return;

			float _timeNow					= (float)EditorApplication.timeSinceStartup;
			float _timeSinceLastKeyPress	= _timeNow - m_lastKeyStrokeRegisteredAtTime;

			// Ignore this key stroke
			if (_timeSinceLastKeyPress < kMinDelayBetweenConsecutiveKeyStroke)
			{
				_curEvent.Use();
				return;
			}
		
			// Move to the next element, if "UpArrow" is pressed
			if (_curEvent.keyCode == KeyCode.UpArrow)
			{
				SelectAdjacentLogEntry(true);

				// Update key stroke time
				m_lastKeyStrokeRegisteredAtTime	= _timeNow;
			}

			// Move to the next element, if "DownArrow" is pressed
			if (_curEvent.keyCode == KeyCode.DownArrow)
			{
				SelectAdjacentLogEntry(false);

				// Update key stroke time
				m_lastKeyStrokeRegisteredAtTime	= _timeNow;
			}
		}

		private void CheckForMouseEvents ()
		{
			Event _curEvent	= Event.current;

			if (!_curEvent.isMouse)
				return;

			if (_curEvent.type == EventType.MouseDown)
			{
				float _timeNow				= (float)EditorApplication.timeSinceStartup;
				float _timeSinceLastClick	= _timeNow - m_lastMouseClickRegisteredAtTime;

				if (_timeSinceLastClick < kMaxDelayBetweenConsecutiveClicks)
				{
					m_clickCount++;
				}
				else
				{
					m_clickCount	= 1;
				}

				// Cache click time
				m_lastMouseClickRegisteredAtTime	= _timeNow; 
			}
		}

		#endregion

		#region Methods

		private static bool CanCreateInstance ()
		{
			return EditorPrefs.GetBool(kCanCreateInstance, false);
		}

		private void SelectAdjacentLogEntry (bool _selectPrevious)
		{
			// Doesnt have valid value
			if (!m_selectedConsoleLog.IsValid())
				return;

			int _indexOfCurSelectedLog	= m_displayableConsoleLogsList.IndexOf(m_selectedConsoleLog);
			int _displayableLogsCount	= m_displayableConsoleLogsList.Count;
			int _indexOfMoveToLog		= _indexOfCurSelectedLog;

			if (_selectPrevious)
			{
				if (_indexOfCurSelectedLog > 0)
				{
					_indexOfMoveToLog	= _indexOfCurSelectedLog - 1;
				}
			}
			else
			{
				if (_indexOfCurSelectedLog < (_displayableLogsCount - 1))
				{
					_indexOfMoveToLog	= _indexOfCurSelectedLog + 1;
				}
			}

			// Update selection
			if (_indexOfMoveToLog != _indexOfCurSelectedLog)
			{
				m_selectedConsoleLog	= m_displayableConsoleLogsList[_indexOfMoveToLog];
				
				// Use current event
				Event.current.Use();
			}
		}

		private Texture2D CreateSquareTexture (Color32 _color, int _width)
		{
			Color32[] _texColors	= new Color32[_width * _width];
			int _arrayLength		= _texColors.Length;

			for (int _iter = 0; _iter < _arrayLength; _iter++)
				_texColors[_iter]	= _color;

			Texture2D _newTexture	= new Texture2D(_width, _width);
			_newTexture.SetPixels32(_texColors);
			_newTexture.Apply();
			_newTexture.hideFlags	= HideFlags.DontSave;

			return _newTexture;
		}

		private void CreateTextures ()
		{
			if (m_consoleLogOnNormalBackgroundTexture != null)
				return;

#if UNITY_PRO_LICENSE
			m_consoleLogOnNormalBackgroundTexture	= CreateSquareTexture(new Color32(000, 000,	000, 000), 32);
			m_consoleLogOffNormalBackgroundTexture	= CreateSquareTexture(new Color32(255, 255, 255, 007), 32);
			m_logEntryActiveBackgroundTexture		= CreateSquareTexture(new Color32(060, 096,	150, 255), 32);

#else
			m_consoleLogOnNormalBackgroundTexture	= CreateSquareTexture(new Color32(214, 214, 214, 255), 32);
			m_consoleLogOffNormalBackgroundTexture	= CreateSquareTexture(new Color32(207, 207,	207, 255), 32);
			m_logEntryActiveBackgroundTexture		= CreateSquareTexture(new Color32(057, 101,	230, 255), 32);
#endif
		}

		private void SetTextColor ()
		{
#if UNITY_PRO_LICENSE
			m_textColorNormal	= Color.white;
			m_textColorActive	= Color.white;
#else
			m_textColorNormal	= Color.black;
			m_textColorActive	= Color.white;
#endif
		}

		private void CreateGUIStyle ()
		{
			CreateTextures();

			// Create named GUI styles
			m_infoGUIStyle					= new GUIStyle(kInfoStyle);
			m_warningGUIStyle				= new GUIStyle(kWarningStyle);
			m_errorGUIStyle					= new GUIStyle(kErrorStyle);

			// Create custom GUI styles
			m_filtersGUIStyle				= CreateCustomGUIStyle(_baseStyle:kFiltersStyle, _fixedHeight:30);
			m_consoleLogOnGUIStyle			= CreateCustomGUIStyle(kWordWrappedStyle, m_consoleLogOnNormalBackgroundTexture, m_textColorNormal, m_logEntryActiveBackgroundTexture, m_textColorActive);
			m_consoleLogOffGUIStyle			= CreateCustomGUIStyle(kWordWrappedStyle, m_consoleLogOffNormalBackgroundTexture, m_textColorNormal, m_logEntryActiveBackgroundTexture, m_textColorActive);
		}

		private GUIStyle CreateCustomGUIStyle (string _baseStyle, Texture2D _normalBackgroundTexture = null, Color? _normalTextColor = null, Texture2D _activeBackgroundTexture = null, Color? _activeTextColor = null, int _fixedHeight = 36, bool _richText = true)
		{
			GUIStyle _newStyle					= new GUIStyle(_baseStyle);

			// Set active color
			if (_activeTextColor.HasValue)
			{
				_newStyle.active.textColor		= _activeTextColor.Value;
				_newStyle.onActive.textColor	= _activeTextColor.Value;
			}
			
			// Set active background texture
			if (_activeBackgroundTexture)
			{
				_newStyle.onActive.background 	= _activeBackgroundTexture;
				_newStyle.active.background 	= _activeBackgroundTexture;
			}

			// Set normal color
			if (_normalTextColor.HasValue)
			{
				_newStyle.normal.textColor 		= _normalTextColor.Value;
				_newStyle.onNormal.textColor	= _activeTextColor.Value;
			}
			
			// Set normal background texture
			if (_normalBackgroundTexture)
			{
				_newStyle.normal.background 	= _normalBackgroundTexture;
				_newStyle.onNormal.background 	= _activeBackgroundTexture;
			}

			// Set margin
			_newStyle.margin					= new RectOffset(1, 0, 0, 0);

			// Set fixed height
			_newStyle.fixedHeight				= _fixedHeight;

			// Set rich text
			_newStyle.richText					= _richText;

			return _newStyle;
		}

		#endregion
	}
}
#endif