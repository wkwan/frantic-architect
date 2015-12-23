#if UNITY_ANDROID || UNITY_IOS

namespace UnityEngine.Advertisements {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using ShowOptionsExtended = Optional.ShowOptionsExtended;

  internal class UnityAds : MonoBehaviour {
    public static bool isShowing = false;
    public static bool isInitialized = false;
    public static bool allowPrecache = true;

	private static bool initCalled = false;

    private static UnityAds sharedInstance;

    private static string _rewardItemNameKey = "";
    private static string _rewardItemPictureKey = "";

    private static bool _resultDelivered = false;

	private static System.Action<ShowResult> resultCallback = null;

		private static string _versionString = Application.unityVersion + "_" + Advertisement.version;

    public static UnityAds SharedInstance {
      get {
        if(!sharedInstance) {
          sharedInstance = (UnityAds)FindObjectOfType(typeof(UnityAds));
        }

        if(!sharedInstance) {
          GameObject singleton = new GameObject() { hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector };
          sharedInstance = singleton.AddComponent<UnityAds>();
          singleton.name = "UnityAdsPluginBridgeObject";
          DontDestroyOnLoad(singleton);
        }

        return sharedInstance;
      }
    }

    public void Init(string gameId, bool testModeEnabled) {
		// Prevent double inits in all situations
		if (initCalled) return;
		initCalled = true;

		try {
			if(Application.internetReachability == NetworkReachability.NotReachable) {
				Utils.LogError("Internet not reachable, can't initialize ads");
				return;
			}
#if UNITY_ANDROID
			System.Net.IPHostEntry videoAdServer = System.Net.Dns.GetHostEntry("impact.applifier.com");
			if(videoAdServer.AddressList.Length == 1) {
				// 0x7F000001 equals to 127.0.0.1
				if(videoAdServer.AddressList[0].Equals(new System.Net.IPAddress(new byte[] {0x7F, 0x00, 0x00, 0x01}))) {
					Utils.LogError("Video ad server resolves to localhost (due to ad blocker?), can't initialize ads");
					return;
				}
			}
#endif
		} catch(System.Exception e) {
			Utils.LogDebug("Exception during connectivity check: " + e.Message);
		}

		UnityAdsExternal.init(gameId, testModeEnabled, SharedInstance.gameObject.name, _versionString);
    }

    public void Awake () {
      if(gameObject == SharedInstance.gameObject) {
        DontDestroyOnLoad(gameObject);
      }
      else {
        Destroy (gameObject);
      }
    }

    /* Static Methods */

    public static bool isSupported () {
      return UnityAdsExternal.isSupported();
    }

    public static string getSDKVersion () {
      return UnityAdsExternal.getSDKVersion();
    }

    public static void setLogLevel(Advertisement.DebugLevel logLevel) {
      UnityAdsExternal.setLogLevel(logLevel);
    }

    public static bool canShowZone (string zone) {
      if(!isInitialized || isShowing) return false;

      return UnityAdsExternal.canShowZone(zone);
    }

    public static bool hasMultipleRewardItems () {
      return UnityAdsExternal.hasMultipleRewardItems();
    }

    public static List<string> getRewardItemKeys () {
      List<string> retList = new List<string>();

      string keys = UnityAdsExternal.getRewardItemKeys();
      retList = new List<string>(keys.Split(';'));

      return retList;
    }

    public static string getDefaultRewardItemKey () {
      return UnityAdsExternal.getDefaultRewardItemKey();
    }

    public static string getCurrentRewardItemKey () {
      return UnityAdsExternal.getCurrentRewardItemKey();
    }

    public static bool setRewardItemKey (string rewardItemKey) {
      return UnityAdsExternal.setRewardItemKey(rewardItemKey);
    }

    public static void setDefaultRewardItemAsRewardItem () {
      UnityAdsExternal.setDefaultRewardItemAsRewardItem();
    }

    public static string getRewardItemNameKey () {
      if (_rewardItemNameKey == null || _rewardItemNameKey.Length == 0) {
        fillRewardItemKeyData();
      }

      return _rewardItemNameKey;
    }

    public static string getRewardItemPictureKey () {
      if (_rewardItemPictureKey == null || _rewardItemPictureKey.Length == 0) {
        fillRewardItemKeyData();
      }

      return _rewardItemPictureKey;
    }

    public static Dictionary<string, string> getRewardItemDetailsWithKey (string rewardItemKey) {
      Dictionary<string, string> retDict = new Dictionary<string, string>();
      string rewardItemDataString = "";

      rewardItemDataString = UnityAdsExternal.getRewardItemDetailsWithKey(rewardItemKey);

      if (rewardItemDataString != null) {
        List<string> splittedData = new List<string>(rewardItemDataString.Split(';'));
        Utils.LogDebug("UnityAndroid: getRewardItemDetailsWithKey() rewardItemDataString=" + rewardItemDataString);

        if (splittedData.Count == 2) {
          retDict.Add(getRewardItemNameKey(), splittedData.ToArray().GetValue(0).ToString());
          retDict.Add(getRewardItemPictureKey(), splittedData.ToArray().GetValue(1).ToString());
        }
      }

      return retDict;
    }

	public void Show(string zoneId = null, ShowOptions options = null) {
		string gamerSid = null;
		_resultDelivered = false;

		if (options != null) {
			if (options.resultCallback != null) {
				resultCallback = options.resultCallback;
			}

			// Disable obsolete method warnings for this piece of code because here we need to access legacy ShowOptionsExtended to maintain compability
#pragma warning disable 612, 618
			ShowOptionsExtended extendedOptions = options as ShowOptionsExtended;
			if(extendedOptions != null && extendedOptions.gamerSid != null && extendedOptions.gamerSid.Length > 0) {
				gamerSid = extendedOptions.gamerSid;
			} else {
				gamerSid = options.gamerSid;
			}
#pragma warning restore 612, 618
		}

		if (!isInitialized || isShowing) {
			deliverCallback (ShowResult.Failed);
			return;
		}
		
		if (gamerSid != null) {
			if (!show (zoneId, "", new Dictionary<string,string> {{"sid", gamerSid}})) {
				deliverCallback (ShowResult.Failed);
			}
		} else {
			if (!show (zoneId)) {
				deliverCallback (ShowResult.Failed);
			}
		}
	}

    public static bool show (string zoneId = null) {
      return show (zoneId, "", null);
    }

    public static bool show (string zoneId, string rewardItemKey) {
      return show (zoneId, rewardItemKey, null);
    }

    public static bool show (string zoneId, string rewardItemKey, Dictionary<string, string> options) {
      if (!isShowing) {
		isShowing = true;
		if (SharedInstance) {
		  string optionsString = parseOptionsDictionary (options);

		  if (UnityAdsExternal.show (zoneId, rewardItemKey, optionsString)) {
			return true;
		  }
		}
	  }

      return false;
    }

	private static void deliverCallback(ShowResult result) {
		isShowing = false;

		if (resultCallback != null && !_resultDelivered) {
		  _resultDelivered = true;
			resultCallback(result);
			resultCallback = null;
		}
	}

    public static void hide () {
      if (isShowing) {
        UnityAdsExternal.hide();
      }
    }

    private static void fillRewardItemKeyData () {
      string keyData = UnityAdsExternal.getRewardItemDetailsKeys();

      if (keyData != null && keyData.Length > 2) {
        List<string> splittedKeyData = new List<string>(keyData.Split(';'));
        _rewardItemNameKey = splittedKeyData.ToArray().GetValue(0).ToString();
        _rewardItemPictureKey = splittedKeyData.ToArray().GetValue(1).ToString();
      }
    }

    private static string parseOptionsDictionary(Dictionary<string, string> options) {
      string optionsString = "";
      if(options != null) {
        bool added = false;
        if(options.ContainsKey("noOfferScreen")) {
          optionsString += (added ? "," : "") + "noOfferScreen:" + options["noOfferScreen"];
          added = true;
        }
        if(options.ContainsKey("openAnimated")) {
          optionsString += (added ? "," : "") + "openAnimated:" + options["openAnimated"];
          added = true;
        }
        if(options.ContainsKey("sid")) {
          optionsString += (added ? "," : "") + "sid:" + options["sid"];
          added = true;
        }
        if(options.ContainsKey("muteVideoSounds")) {
          optionsString += (added ? "," : "") + "muteVideoSounds:" + options["muteVideoSounds"];
          added = true;
        }
        if(options.ContainsKey("useDeviceOrientationForVideo")) {
          optionsString += (added ? "," : "") + "useDeviceOrientationForVideo:" + options["useDeviceOrientationForVideo"];
          added = true;
        }
      }
      return optionsString;
    }

    /* Events */

    public void onHide () {
      isShowing = false;
      deliverCallback(ShowResult.Skipped);
      Utils.LogDebug("onHide");
    }

    public void onShow () {
      Utils.LogDebug("onShow");
    }

    public void onVideoStarted () {
      Utils.LogDebug("onVideoStarted");
    }

    public void onVideoCompleted (string parameters) {
      if (parameters != null) {
        List<string> splittedParameters = new List<string>(parameters.Split(';'));
        string rewardItemKey = splittedParameters.ToArray().GetValue(0).ToString();
        bool skipped = splittedParameters.ToArray().GetValue(1).ToString() == "true" ? true : false;

        Utils.LogDebug("onVideoCompleted: " + rewardItemKey + " - " + skipped);

		if(skipped) {
			deliverCallback (ShowResult.Skipped);
		} else {
			deliverCallback (ShowResult.Finished);
		}
      }
    }

    public void onFetchCompleted () {
	  isInitialized = true;
      Utils.LogDebug("onFetchCompleted");
    }

    public void onFetchFailed () {
      Utils.LogDebug("onFetchFailed");
    }
  }
}

#endif
