using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VoxelBusters.DebugPRO
{
	using Internal;

#if UNITY_EDITOR
	[InitializeOnLoad]
	public partial class Console : EditorWindow
#else
	public partial class Console : ScriptableObject
#endif
	{
		#region Properties

		[SerializeField]
		private List<ConsoleLog>					m_consoleLogsList			= new List<ConsoleLog>();
		private List<ConsoleLog>					m_displayableConsoleLogsList= new List<ConsoleLog>();

		[SerializeField]
		private List<ConsoleTag>					m_consoleTags				= new List<ConsoleTag>();

		/// <summary>
		/// In the Build Settings dialog there is a check box called "Development Build". If it is checked isDebugBuild will be true.
		/// </summary>
		/// <value><c>true</c> if "Development Build" checkbox is checked; otherwise, <c>false</c>.</value>
		public static bool							IsDebugMode
		{
			get
			{
				return 	UnityEngine.Debug.isDebugBuild;
			}
		}

#pragma warning disable
		[SerializeField]
		private bool								m_clearOnPlay				= true;
#pragma warning restore

#pragma warning disable
		[SerializeField]
		private bool								m_errorPause				= false;
#pragma warning restore

		[SerializeField]
		private eConsoleLogType						m_allowedLogTypes			= (eConsoleLogType)0xFF;

		[SerializeField]
		private bool								m_showInfoLogs				= true;
		private bool								ShowInfoLogs
		{
			get
			{
				return m_showInfoLogs;
			}

			set
			{
				if (m_showInfoLogs != value)
				{
					m_showInfoLogs	= value;

					if (value)
					{
						m_allowedLogTypes |= eConsoleLogType.INFO;
					}
					else
					{
						m_allowedLogTypes &= ~eConsoleLogType.INFO;
					}

					// Rebuild displayable logs list
					RebuildDisplayableLogs();
				}
			}
		}

		[SerializeField]
		private bool								m_showWarningLogs			= true;
		private bool								ShowWarningLogs
		{
			get
			{
				return m_showWarningLogs;
			}
			
			set
			{
				if (m_showWarningLogs != value)
				{
					m_showWarningLogs	= value;
					
					if (value)
					{
						m_allowedLogTypes |= eConsoleLogType.WARNING;
					}
					else
					{
						m_allowedLogTypes &= ~eConsoleLogType.WARNING;
					}

					// Rebuild displayable logs list
					RebuildDisplayableLogs();
				}
			}
		}

		[SerializeField]
		private bool								m_showErrorLogs				= true;
		private bool								ShowErrorLogs
		{
			get
			{
				return m_showErrorLogs;
			}
			
			set
			{
				if (m_showErrorLogs != value)
				{
					m_showErrorLogs	= value;
					
					if (value)
					{
						m_allowedLogTypes |= (eConsoleLogType.ERROR | eConsoleLogType.EXCEPTION | eConsoleLogType.ASSERT);
					}
					else
					{
						m_allowedLogTypes &= ~(eConsoleLogType.ERROR | eConsoleLogType.EXCEPTION | eConsoleLogType.ASSERT);
					}

					// Rebuild displayable logs list
					RebuildDisplayableLogs();
				}
			}
		}

#pragma warning disable
		private string								m_infoLogsCounterStr;
#pragma warning restore
		private int 								m_infoLogsCounter			= 0;
		public int 									InfoLogsCounter
		{
			get 
			{
				return m_infoLogsCounter;
			}

			set
			{
				if (value != m_infoLogsCounter)
				{
					if (value > 999)
						m_infoLogsCounterStr	= "999+";
					else
						m_infoLogsCounterStr	= value.ToString();
				}

				m_infoLogsCounter	= value;
			}
		}
	
#pragma warning disable
		private string								m_warningLogsCounterStr;
#pragma warning restore
		private int 								m_warningLogsCounter		= 0;
		public int 									WarningLogsCounter
		{
			get 
			{
				return m_warningLogsCounter;
			}
			
			set
			{
				if (value != m_warningLogsCounter)
				{
					if (value > 999)
						m_warningLogsCounterStr	= "999+";
					else
						m_warningLogsCounterStr	= value.ToString();
				}
				
				m_warningLogsCounter	= value;
			}
		}

#pragma warning disable
		private string								m_errorLogsCounterStr;
#pragma warning restore
		private int 								m_errorLogsCounter			= 0;
		public int 									ErrorLogsCounter
		{
			get 
			{
				return m_errorLogsCounter;
			}
			
			set
			{
				if (value != m_errorLogsCounter)
				{
					if (value > 999)
						m_errorLogsCounterStr	= "999+";
					else
						m_errorLogsCounterStr	= value.ToString();
				}
				
				m_errorLogsCounter	= value;
			}
		}
#pragma warning disable
		private ConsoleLog							m_selectedConsoleLog;
#pragma warning restore

		#endregion

		#region Static Properties

		private static Console						instance					= null;
		public static Console						Instance
		{
			get 
			{
				if (instance == null)
				{
#if UNITY_EDITOR
					if (CanCreateInstance())
					{
						int _saveInstanceID			= EditorPrefs.GetInt(kInstanceID, 0);
						Console _serializedInstance	= null;

						if (_saveInstanceID != 0)
						    _serializedInstance	= EditorUtility.InstanceIDToObject(_saveInstanceID) as Console;

						// Incase if getter gets called while loading assembly, Unity GetWindow returns null even if window exists, so using instance ID to get object
						if (_serializedInstance == null)
							instance	= GetWindow<Console>();
						else
							instance	= _serializedInstance;
					}
#else
					instance		= CreateInstance<Console>();
#endif
				}

				return instance;
			}
		}

		#endregion

		#region Constants

		private const string						kDefaultTag					= "untagged";
		private const string						kInstanceID					= "vb-debug-pro-console-id";
	
		#endregion
	
		#region Static Constructor

		static Console ()
		{
			// Log callbacks
			UnityDebugUtility.LogCallback			-= HandleUnityLog;
			UnityDebugUtility.LogCallback			+= HandleUnityLog;
			
#if UNITY_EDITOR
			// Unity callbacks
			EditorApplication.playmodeStateChanged	-= PlaymodeStateChanged;
			EditorApplication.playmodeStateChanged	+= PlaymodeStateChanged;
#endif
		}

		private Console ()
		{
#if UNITY_EDITOR
#if !(UNITY_5_0) && (UNITY_5 || UNITY_6 || UNITY_7)
			titleContent		= new GUIContent("Console");		
#else
			title				= "Console";
#endif
#endif
			// Set default values
			m_infoLogsCounterStr	= "0";
			m_warningLogsCounterStr	= "0";
			m_errorLogsCounterStr	= "0";
		}

		#endregion

		#region Unity Methods

		private void OnEnable ()
		{
			// Cache instance
			if (instance == null)
				instance	= this;

#if UNITY_EDITOR
			// Set text color used for showing logs
			SetTextColor();

			// Create textures used as backgrounds for logs
			CreateTextures();
#endif

			// Rebuild displayable logs list
			RebuildDisplayableLogs();
		}

		private void OnDisable ()
		{
#if UNITY_EDITOR
			EditorPrefs.SetInt(kInstanceID, GetInstanceID());
			DestroyImmediate(m_logEntryActiveBackgroundTexture);
			DestroyImmediate(m_consoleLogOnNormalBackgroundTexture);
			DestroyImmediate(m_consoleLogOffNormalBackgroundTexture);
#endif
		}

		#endregion
		
		#region Callbacks

		private static void HandleUnityLog (string _message, string _stackTrace, LogType _logType)
		{
			eConsoleLogType _consoleLogType;
			
			if (_logType == LogType.Log)
			{
				_consoleLogType	= eConsoleLogType.INFO;
			}
			else if (_logType == LogType.Warning)
			{
				_consoleLogType	= eConsoleLogType.WARNING;
			}
			else if (_logType == LogType.Error)
			{
				_consoleLogType	= eConsoleLogType.ERROR;
			}
			else if (_logType == LogType.Exception)
			{
				_consoleLogType	= eConsoleLogType.EXCEPTION;
			}
			else
			{
				_consoleLogType	= eConsoleLogType.ASSERT;
			}
			
			// As its unity callback redirected by Utility component, we need to skip 5 frames to get exact caller
			if (Instance != null)
				Instance.Log(kDefaultTag, _message, _consoleLogType, null, 5);
		}

#if UNITY_EDITOR

		private static void PlaymodeStateChanged ()
		{
			if (EditorApplication.isPlaying)
			{
				// Unregister from callback
				EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;

				// TODO: Callback is delayed until Start is called on gameobjects. Need to find better way.
				if (Instance != null && Instance.m_clearOnPlay)
				{
//					Instance.Clear();
				}
			}
		}
#endif

		#endregion

		#region Methods

		private void Clear ()
		{
			// Clear all the existing logs and tags
			m_consoleLogsList.Clear();
			m_consoleTags.Clear();

			// Rebuild display list
			RebuildDisplayableLogs();
		}
	
		private void RebuildDisplayableLogs ()
		{
			// Clear existing displayable logs
			m_displayableConsoleLogsList.Clear();
			
			// Reset selected logs
			m_selectedConsoleLog	= default(ConsoleLog);

			// Reset counters
			InfoLogsCounter			= 0;
			WarningLogsCounter		= 0;
			ErrorLogsCounter		= 0;

			// Iterate through all logs and create a new list of logs based on active configuration
			int _totalLogs = m_consoleLogsList.Count;

			for (int _iter = 0; _iter < _totalLogs; _iter++)
			{
				ConsoleLog _consoleLog	= m_consoleLogsList[_iter];

				// Adds to display list, if it satisfies console configuration
				AddToDisplayableLogList(_consoleLog);
			}
		}

		private bool AddToDisplayableLogList (ConsoleLog _consoleLog)
		{
			// First check if tag for this is active or not
			int _tagID				= _consoleLog.TagID;
			ConsoleTag _logTag		= m_consoleTags[_tagID];
			
			if (!_logTag.IsActive)
				return false;

			// Update log message counters
			if (_consoleLog.Type == eConsoleLogType.INFO)
				InfoLogsCounter++;
			else if (_consoleLog.Type == eConsoleLogType.WARNING)
				WarningLogsCounter++;
			else
				ErrorLogsCounter++;
			
			// Next we need to check log-type
			if (((int)_consoleLog.Type & (int)m_allowedLogTypes) == 0)
				return false;
			
			// Now that it passed our configurations, add it to the list
			m_displayableConsoleLogsList.Add(_consoleLog);

			return true;
		}

		private ConsoleTag GetConsoleTag (string _tagName)
		{
			if (m_consoleTags.Count	== 0)
				return null;

			return m_consoleTags.FirstOrDefault(_consoleTag => _consoleTag.Name.Equals(_tagName));
		}

		private int GetIndexOfConsoleTag (string _tagName)
		{
			if (m_consoleTags.Count	== 0)
				return -1;

			return m_consoleTags.FindIndex(_consoleTag => _consoleTag.Name.Equals(_tagName));
		}

		private bool IgnoreConsoleLog (string _tagName)
		{
			// Not debug mode, ignore all log messages
			if (!IsDebugMode)
				return true;
			
			// All untagged logs are grouped under default-tag-name
			if (string.IsNullOrEmpty(_tagName))
				_tagName	= kDefaultTag;
			
			// Check if this tag is marked as "Ignore"
			ConsoleTag _matchedConsoleTag	= GetConsoleTag(_tagName);

			if (_matchedConsoleTag != null && _matchedConsoleTag.Ignore)
				return true;
			
			return false;
		}
		
		private void Log (string _tagName, object _message, eConsoleLogType _logType, UnityEngine.Object _context, int _skipStackFrameCount = 1)
		{
			// Do I need to ignore this call
			if (IgnoreConsoleLog(_tagName))
				return;

			// Add additional skip frame
			_skipStackFrameCount++;

			int _tagID	= GetIndexOfConsoleTag(_tagName);
			
			// When we cant find our tag, add a new entry
			if (_tagID == -1)
			{
				m_consoleTags.Add(new ConsoleTag(_tagName));
				
				// And update tag-ID
				_tagID	= m_consoleTags.Count - 1;
			}
			
			ConsoleTag _consoleLogTag	= m_consoleTags[_tagID];
			
			// Marked to ignore this log
			if (_consoleLogTag.Ignore)
				return;
			
			int _logID					= m_consoleLogsList.Count + 1;
			ConsoleLog _newConsoleLog	= new ConsoleLog(_logID, _tagID, _message.ToString(), _logType, _context, _skipStackFrameCount);

#if UNITY_EDITOR
			// Add this new console to list of logs
			m_consoleLogsList.Add(_newConsoleLog);

			// Add it to display list
			bool _addSuccess = AddToDisplayableLogList(_newConsoleLog);

			if (_addSuccess)
			{
				// Pause unity player on error
				if (_logType != eConsoleLogType.INFO && _logType != eConsoleLogType.WARNING)
				{
					if (m_errorPause)
					{
						EditorApplication.isPaused	= true;
					}
				}
			}

			// Set as modified
			EditorUtility.SetDirty(this);
#else
			NativeBinding.Log(_newConsoleLog);
#endif
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Clears logs from console.
		/// </summary>
		public static void ClearConsole ()
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			// Clear existing logs
			Instance.Clear();
		}

		/// <summary>
		/// Adds the ignore tag.
		/// </summary>
		/// <param name="tag">Adds this tag to ignore list.</param>
		public static void AddIgnoreTag (string _tag)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			ConsoleTag _matchedConsoleTag	= Instance.GetConsoleTag(_tag);

			// We dont have a match, it means that we dont have a tag re
			if (_matchedConsoleTag == null)
			{
				ConsoleTag _newConsoleTag	= new ConsoleTag(_tag);

				// Add this to tags list
				Instance.m_consoleTags.Add(_newConsoleTag);

				// Use this new tag as matched
				_matchedConsoleTag			= _newConsoleTag;
			}

			// Update ignore flag to "ignore"
			_matchedConsoleTag.Ignore		= true;
		}

		/// <summary>
		/// Removes the ignore tag.
		/// </summary>
		/// <param name="tag">Removes this tag from ignore list.</param>
		public static void RemoveIgnoreTag (string _tag)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			// Get console tag
			ConsoleTag _matchedConsoleTag	= Instance.GetConsoleTag(_tag);

			if (_matchedConsoleTag != null)
			{
				// Update ignore flag to "dont ignore"
				_matchedConsoleTag.Ignore	= false;
			}
		}

		/// <summary>
		/// Draws a line from start to end in world coordinates..
		/// </summary>
		/// <param name="tag">Is a string indicating the component from which the message originates.</param>
		/// <param name="start">Point in world space where the line should start.</param>
		/// <param name="end">Point in world space where the line should end.</param>
		/// <param name="color">Color of the line.</param>
		/// <param name="duration">How long the line should be visible.</param>
		/// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
		public static void DrawLine (string _tag, Vector3 _start, Vector3 _end, Color _color = default(Color), float _duration = 0.0f, bool _depthTest = true)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			// Do I need to ignore this call
			if (Instance.IgnoreConsoleLog(_tag))
				return;

			Debug.DrawLine(_start, _end, _color, _duration, _depthTest);
		}

		/// <summary>
		/// Draws a line from start to start + dir in world coordinates.
		/// </summary>
		/// <param name="tag">Is a string indicating the component from which the message originates.</param>
		/// <param name="start">Point in world space where the ray should start.</param>
		/// <param name="direction">Direction and length of the ray.</param>
		/// <param name="color">Color of the line.</param>
		/// <param name="duration">How long the line should be visible.</param>
		/// <param name="depthTest">Should the line be obscured by objects closer to the camera?</param>
		public static void DrawRay (string _tag, Vector3 _start, Vector3 _direction, Color _color = default(Color), float _duration = 0.0f, bool _depthTest = true)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			// Do I need to ignore this call, please check it
			if (Instance.IgnoreConsoleLog(_tag))
				return;

			Debug.DrawRay(_start, _direction, _color, _duration, _depthTest);
		}

		/// <summary>
		/// Logs message to the console.
		/// </summary>
		/// <param name="tag">Is a string indicating the component from which the message originates.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void Log (string _tag, object _message, UnityEngine.Object _context = null)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			Instance.Log(_tag, _message, eConsoleLogType.INFO, _context);
		}

		/// <summary>
		/// Logs a warning message to the console.
		/// </summary>
		/// <param name="tag">Is a string indicating the component from which the message originates.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogWarning (string _tag, object _message, UnityEngine.Object _context = null)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			Instance.Log(_tag, _message, eConsoleLogType.WARNING, _context);
		}

		/// <summary>
		/// Logs a error message to the console.
		/// </summary>
		/// <param name="tag">Is a string indicating the component from which the message originates.</param>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogError (string _tag, object _message, UnityEngine.Object _context = null)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			Instance.Log(_tag, _message, eConsoleLogType.ERROR, _context);
		}

		/// <summary>
		/// Logs a exception message to the console.
		/// </summary>
		/// <param name="tag">Is a string indicating the component from which the message originates.</param>
		/// <param name="exception">Exception for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogException(string _tag, Exception _exception, UnityEngine.Object _context = null)
		{
			// Debug console is currently inactive, so ignore this call
			if (Instance == null)
				return;

			Instance.Log(_tag, _exception, eConsoleLogType.EXCEPTION, _context);
		}

		#endregion
	}
}