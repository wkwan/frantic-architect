//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;

namespace Uniject.Impl {
    public class UnityLogger : ILogger{
        #region ILogger implementation
        public void LogWarning (string message, params object[] formatArgs) {
            UnityEngine.Debug.LogWarning(string.Format(message, formatArgs));
        }
        #endregion

        public string prefix { get; set; }

    	#region ILogger implementation
    	public void Log(string message) {
            UnityEngine.Debug.Log(formatMessageWithPrefix(message)); // Removed to avoid filling the Unity log. Uncomment to get complete tracing information.
    	}

        public void Log(string message, object[] args) {
            Log(safeFormat(message, args));
        }

        public void LogError(string message, params object[] formatArgs) {
            UnityEngine.Debug.LogError(formatMessageWithPrefix(safeFormat(message, formatArgs)));
        }

        private string safeFormat(string message, params object[] formatArgs) {
            try {
                return string.Format(message, formatArgs);
            } catch (FormatException f) {
                Log (f.Data.ToString ());
            }
            return message;
        }

        private string formatMessageWithPrefix (string message) {
            if (prefix == null) {
                return message;
            }
            return safeFormat("{0}: {1}", prefix, message);
        }
    	#endregion
    }
}
