#if UNITY_ANDROID || UNITY_IOS

namespace UnityEngine.Advertisements {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.InteropServices;

  internal abstract class UnityAdsPlatform {
    public abstract void init(string gameId, bool testModeEnabled, string gameObjectName, string unityVersion);
    public abstract bool show(string zoneId, string rewardItemKey, string options);
    public abstract void hide();
    public abstract bool isSupported();
    public abstract string getSDKVersion();
    public abstract bool canShowZone(string zone);
    public abstract bool hasMultipleRewardItems();
    public abstract string getRewardItemKeys();
    public abstract string getDefaultRewardItemKey();
    public abstract string getCurrentRewardItemKey();
    public abstract bool setRewardItemKey(string rewardItemKey);
    public abstract void setDefaultRewardItemAsRewardItem();
    public abstract string getRewardItemDetailsWithKey(string rewardItemKey);
    public abstract string getRewardItemDetailsKeys();
    public abstract void setLogLevel(Advertisement.DebugLevel logLevel);
  }
}

#endif
