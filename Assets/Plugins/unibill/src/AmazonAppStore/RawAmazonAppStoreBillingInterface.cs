#if UNITY_ANDROID
//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEngine;
using System.IO;

namespace Unibill.Impl {
    public class RawAmazonAppStoreBillingInterface : IRawAmazonAppStoreBillingInterface {

        private AndroidJavaObject amazon;

        public RawAmazonAppStoreBillingInterface (UnibillConfiguration config) {
            if (config.CurrentPlatform == BillingPlatform.AmazonAppstore && config.AmazonSandboxEnabled) {
                string json = ((TextAsset)Resources.Load ("amazon.sdktester.json")).text;
                File.WriteAllText ("/sdcard/amazon.sdktester.json", json);
            }

            using (var pluginClass = new AndroidJavaClass("com.outlinegames.unibillAmazon.Unibill" )) {
                amazon = pluginClass.CallStatic<AndroidJavaObject> ("instance");
            }
        }

        public void initialise (AmazonAppStoreBillingService amazon) {
            new GameObject().AddComponent<AmazonAppStoreCallbackMonoBehaviour>().initialise(amazon);
        }

        public void initiateItemDataRequest (string[] productIds) {
            var initMethod = AndroidJNI.GetMethodID(amazon.GetRawClass(), "initiateItemDataRequest", "([Ljava/lang/String;)V" );
            AndroidJNI.CallVoidMethod(amazon.GetRawObject(), initMethod, AndroidJNIHelper.CreateJNIArgArray( new object[] { productIds }));
        }

        public void initiatePurchaseRequest (string productId) {
            amazon.Call("initiatePurchaseRequest", productId);
        }

        public void restoreTransactions() {
            amazon.Call("restoreTransactions");
        }

        public void finishTransaction (string productId)
        {
            amazon.Call ("finishTransaction", productId);
        }
    }
}
#endif
