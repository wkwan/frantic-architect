#if UNITY_IPHONE

namespace UnityEngine.Advertisements {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;

  internal class UnityAdsIos : UnityAdsPlatform {
	public override void init (string gameId, bool testModeEnabled, string gameObjectName, string unityVersion) {
		if(Advertisement.UnityDeveloperInternalTestMode) {
			UnityAdsIosBridge.UnityAdsEnableUnityDeveloperInternalTestMode();
		}

		UnityAdsIosBridge.UnityAdsInit(gameId, testModeEnabled, (Advertisement.debugLevel & Advertisement.DebugLevel.Debug) != Advertisement.DebugLevel.None ? true : false, gameObjectName, unityVersion);
	}
		
	public override bool show (string zoneId, string rewardItemKey, string options) {
		return UnityAdsIosBridge.UnityAdsShow(zoneId, rewardItemKey, options);
	}
		
	public override void hide () {
		UnityAdsIosBridge.UnityAdsHide();
	}
		
	public override bool isSupported () {
		return UnityAdsIosBridge.UnityAdsIsSupported();
	}
		
	public override string getSDKVersion () {
		return UnityAdsIosBridge.UnityAdsGetSDKVersion();
	}
		
	public override bool canShowZone (string zone) {
		if(!string.IsNullOrEmpty(zone)) {
			return UnityAdsIosBridge.UnityAdsCanShowZone(zone);
		} else {
			return UnityAdsIosBridge.UnityAdsCanShow();
		}
	}
		
	public override bool hasMultipleRewardItems () {
		return UnityAdsIosBridge.UnityAdsHasMultipleRewardItems();
	}
		
	public override string getRewardItemKeys () {
		return UnityAdsIosBridge.UnityAdsGetRewardItemKeys();
	}
		
	public override string getDefaultRewardItemKey () {
		return UnityAdsIosBridge.UnityAdsGetDefaultRewardItemKey();
	}
		
	public override string getCurrentRewardItemKey () {
		return UnityAdsIosBridge.UnityAdsGetCurrentRewardItemKey();
	}
		
	public override bool setRewardItemKey (string rewardItemKey) {
		return UnityAdsIosBridge.UnityAdsSetRewardItemKey(rewardItemKey);
	}
		
	public override void setDefaultRewardItemAsRewardItem () {
		UnityAdsIosBridge.UnityAdsSetDefaultRewardItemAsRewardItem();
	}
		
	public override string getRewardItemDetailsWithKey (string rewardItemKey) {
		return UnityAdsIosBridge.UnityAdsGetRewardItemDetailsWithKey(rewardItemKey);
	}
		
	public override string getRewardItemDetailsKeys () {
		return UnityAdsIosBridge.UnityAdsGetRewardItemDetailsKeys();
	}

	public override void setLogLevel(Advertisement.DebugLevel logLevel) {
		UnityAdsIosBridge.UnityAdsSetDebugMode((Advertisement.debugLevel & Advertisement.DebugLevel.Debug) != Advertisement.DebugLevel.None ? true : false);
	}
  }
}

#endif
