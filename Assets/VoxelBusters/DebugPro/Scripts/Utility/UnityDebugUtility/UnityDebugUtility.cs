using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VoxelBusters.DebugPRO
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class UnityDebugUtility : ScriptableObject
	{
		#region Properties

		/// <summary>
		/// The log callback.
		/// </summary>
		public static event Application.LogCallback LogCallback;

		private static UnityDebugUtility			instance;

		#endregion

		#region Constructor

		static UnityDebugUtility ()
		{
#if UNITY_EDITOR
			// Unity callbacks
			EditorApplication.update	-= EditorUpdate;
			EditorApplication.update	+= EditorUpdate;
#endif
		}

		#endregion

		#region Unity Callbacks

		private static void EditorUpdate ()
		{
#if UNITY_EDITOR
			// Unregister from unity callbacks
			EditorApplication.update	-= EditorUpdate;
#endif

#pragma warning disable
			// Create a new instance
			if (instance == null)
				instance	= CreateInstance<UnityDebugUtility>();

			instance.hideFlags	= HideFlags.HideAndDontSave;
#pragma warning restore

#if  UNITY_5 || UNITY_6 || UNITY_7
			// Register for callbacks
			Application.logMessageReceived -= instance.HandleLog;
			Application.logMessageReceived += instance.HandleLog;
#else
			// Register for callbacks
			Application.RegisterLogCallback(instance.HandleLog);
#endif
		}

		private void HandleLog (string _message, string _stackTrace, LogType _logType)
		{
			if (LogCallback != null)
			{
				LogCallback(_message, _stackTrace, _logType);
			}
		}

		#endregion
	}
}