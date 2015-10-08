#if UNITY_ANDROID || UNITY_IOS

namespace UnityEngine.Advertisements {
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using DebugLevel = Advertisement.DebugLevel;

  internal static class Utils {
    private static void Log(DebugLevel debugLevel, string message) {
      if((Advertisement.debugLevel & debugLevel) != DebugLevel.None) {
        Debug.Log(message);
      }
    }

    public static void LogDebug(string message) {
      Log (DebugLevel.Debug,"Debug: " + message);
    }
    
    public static void LogInfo(string message) {
      Log (DebugLevel.Info, "Info:" + message);
    }
    
    public static void LogWarning(string message) {
      Log (DebugLevel.Warning,"Warning:" + message);
    }
    
    public static void LogError(string message) {
      Log (DebugLevel.Error, "Error: " + message);
    }
  }
}

#endif
