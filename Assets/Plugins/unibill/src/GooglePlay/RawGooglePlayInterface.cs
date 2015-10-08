#if UNITY_ANDROID
//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEngine;

namespace Unibill.Impl {
    public class RawGooglePlayInterface : IRawGooglePlayInterface {

        private AndroidJavaObject plugin;

        public void initialise(GooglePlayBillingService callback, string publicKey, string[] productIds) {
            // Setup our GameObject to listen to events from the Java plugin.
            new GameObject().AddComponent<GooglePlayCallbackMonoBehaviour>().Initialise(callback);
            using (var pluginClass = new AndroidJavaClass("com.outlinegames.unibill.UniBill")) {
                plugin = pluginClass.CallStatic<AndroidJavaObject> ("instance");
            }
            plugin.Call("initialise", publicKey);
        }

        public void restoreTransactions() {
            plugin.Call("restoreTransactions");
        }

        public void purchase(string json) {
            plugin.Call("purchaseProduct", json);
        }
		
		public void pollForConsumables () {
			plugin.Call ("pollForConsumables");
		}

        public void finishTransaction (string transactionId)
        {
            plugin.Call ("finishTransaction", transactionId);
        }
    }
}
#endif
