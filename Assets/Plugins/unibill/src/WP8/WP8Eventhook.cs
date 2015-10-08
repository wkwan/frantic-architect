#if UNITY_WINRT
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unibill.Impl;

public class WP8Eventhook : MonoBehaviour {

    public WP8BillingService callback = null;
    
    public void Start() {
        GameObject.DontDestroyOnLoad(gameObject);
    }

    public void OnApplicationPause(bool paused) {
        if (!paused && callback != null) {
            callback.enumerateLicenses();
        }
    }
}
#endif
