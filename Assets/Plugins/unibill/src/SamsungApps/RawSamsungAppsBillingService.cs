//-----------------------------------------------------------------
//  Copyright 2014 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using UnityEngine;
using System.IO;

namespace Unibill.Impl {
	public class RawSamsungAppsBillingInterface : IRawSamsungAppsBillingService {

		#if UNITY_ANDROID
		private AndroidJavaObject samsung;
		#endif

		public RawSamsungAppsBillingInterface () {
			#if UNITY_ANDROID
			using (var pluginClass = new AndroidJavaClass("com.outlinegames.unibill.samsung.Unibill" )) {
				samsung = pluginClass.CallStatic<AndroidJavaObject> ("instance");
			}
			#endif
		}

		public void initialise (SamsungAppsBillingService samsung) {
			#if UNITY_ANDROID
			new GameObject().AddComponent<SamsungAppsCallbackMonoBehaviour>().initialise(samsung);
			#endif
		}

		public void getProductList (string json) {
			#if UNITY_ANDROID
			samsung.Call("initialise", json);
			#endif
		}

		public void initiatePurchaseRequest (string productId) {
			#if UNITY_ANDROID
			samsung.Call("initiatePurchaseRequest", productId);
			#endif
		}

		public void restoreTransactions() {
			#if UNITY_ANDROID
			samsung.Call("restoreTransactions");
			#endif
		}
	}
}
