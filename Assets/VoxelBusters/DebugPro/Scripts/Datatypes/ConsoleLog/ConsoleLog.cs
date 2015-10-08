using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Reflection;

namespace VoxelBusters.DebugPRO.Internal
{
	[System.Serializable]
	public struct ConsoleLog
	{
		#region Property

		[SerializeField]
		private int						m_ID;
		public int						ID
		{
			get
			{
				return m_ID;
			}
			
			private set
			{
				m_ID	= value;
			}
		}

		[SerializeField]
		private int						m_tagID;
		public int					TagID
		{
			get
			{
				return m_tagID;
			}
			
			private set
			{
				m_tagID	= value;
			}
		}

		[SerializeField]
		private string					m_message;
		public string					Message
		{
			get
			{
				return m_message;
			}
			
			private set
			{
				m_message	= value;
			}
		}

		[SerializeField]
		private eConsoleLogType			m_type;
		public eConsoleLogType		Type
		{
			get
			{
				return m_type;
			}
			
			private set
			{
				m_type	= value;
			}
		}
		
		public UnityEngine.Object		Context
		{
			get;
			private set;
		}

		[SerializeField]
		private string					m_stackTrace;
		public string					StackTrace
		{
			get
			{
				return m_stackTrace;
			}
			
			private set
			{
				m_stackTrace	= value;
			}
		}

		[SerializeField]
		private string					m_description;
		public string					Description
		{
			get
			{
				return m_description;
			}
			
			private set
			{
				m_description	= value;
			}
		}

#pragma warning disable
		[SerializeField]
		private string					m_callerFileName;

		[SerializeField]
		private int						m_callerFileLineNumber;
#pragma warning restore

		#endregion

		#region Constructor

		public ConsoleLog (int _logID, int _tagID, string _message, eConsoleLogType _type, UnityEngine.Object _context, int _skipFrames) : this ()
		{
			// Set console log details
			ID						= _logID;
			TagID					= _tagID;
			Message					= _message;
			Type					= _type;
			Context					= _context;
#if !NETFX_CORE
			// Collect information using stacktrace
			// Using skipframes to reach out to the exact callers
			StackTrace _stackTrace	= new StackTrace(++_skipFrames, true);
			StackFrame _firstFrame	= _stackTrace.GetFrame(0);
			m_callerFileName		= _firstFrame.GetFileName();
			m_callerFileLineNumber	= _firstFrame.GetFileLineNumber();

			// Collect description
			CollectStackTraceInfo(_stackTrace);
#else
			StackTrace				= "NOT_IMPLEMENTED_FOR_NETFX_CORE";//System.Environment.StackTrace;
#endif
			// Description
			Description				= Message + "\n" + StackTrace;
		}

		#endregion

		#region Methods
#if !NETFX_CORE
		private void CollectStackTraceInfo (System.Diagnostics.StackTrace _stackTrace)
		{
			// Gathering information related to stackoverflow
			StringBuilder _desciptionBuilder	= new StringBuilder();
			int _totalFrames					= _stackTrace.FrameCount;
			int _totalFramesMinus1				= _totalFrames - 1;		

			// Append stacktrace info
			for (int _iter = 0; _iter < _totalFrames; _iter++)
			{
				StackFrame _stackFrame			= _stackTrace.GetFrame(_iter);

				// Method info
				MethodBase _method				= _stackFrame.GetMethod();
				string _methodName 				= _method.ToString();
				string _className 				= _method.DeclaringType.FullName;

				_desciptionBuilder.AppendFormat("{0}:{1}", _className, _methodName);

				// File info
				string _fileAbsolutePath		= _stackFrame.GetFileName();

				if (!string.IsNullOrEmpty(_fileAbsolutePath))
				{
					string _fileRelativePath		= GetRelativePath(_fileAbsolutePath);

					// Following unity standard stacktrace output "class-name:method-definition() (at relative-path:10)"
					_desciptionBuilder.AppendFormat("(at {0}:{1})", _fileRelativePath, _stackFrame.GetFileLineNumber());
				}

				if (_iter < _totalFramesMinus1)
					_desciptionBuilder.AppendLine();
			}

			// Set value
			StackTrace	= _desciptionBuilder.ToString();
		}
#endif

		private string GetRelativePath (string _absolutePath)
		{
			if (_absolutePath != null && _absolutePath.StartsWith(Application.dataPath))
			{
				return "Assets" + _absolutePath.Substring(Application.dataPath.Length);
			}

			return _absolutePath;
		}

		public bool IsValid ()
		{
			return (this.ID > 0);
		}

		public bool Equals (ConsoleLog _log)
		{
			return (this.ID == _log.ID);
		}

		public void OnSelect ()
		{
#if UNITY_EDITOR
			if (Context != null)
			{
				UnityEditor.Selection.activeObject	= Context;
			}
#endif
		}

		public void OnPress ()
		{
#if UNITY_EDITOR
			// Open file
			UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(m_callerFileName, m_callerFileLineNumber);
#endif
		}
		
		#endregion
	}
}
