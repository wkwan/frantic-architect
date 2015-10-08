using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Uniject;
using Uniject.Impl;

namespace Unibill.Impl {
    class RawBillingPlatformProvider : IRawBillingPlatformProvider {

        #if UNITY_ANDROID
        private UnibillConfiguration config;
        #endif
        private GameObject gameObject;
        public RawBillingPlatformProvider(UnibillConfiguration config, GameObject o) {
            #if UNITY_ANDROID
            this.config = config;
            #endif
            this.gameObject = o;
        }

        public IRawGooglePlayInterface getGooglePlay() {
            #if UNITY_ANDROID
            return new RawGooglePlayInterface();
            #else
            throw new NotImplementedException();
            #endif
        }

        public IRawAmazonAppStoreBillingInterface getAmazon() {
            #if UNITY_ANDROID
            return new RawAmazonAppStoreBillingInterface(config);
            #else
            throw new NotImplementedException();
            #endif
        }

        public IStoreKitPlugin getStorekit() {
            #if UNITY_IOS || UNITY_STANDALONE_OSX
            return gameObject.AddComponent<StoreKitPluginImpl> ();
            #else
            throw new NotImplementedException();
            #endif
        }

		public IRawSamsungAppsBillingService getSamsung() {
			return new RawSamsungAppsBillingInterface ();
		}

        private ILevelLoadListener listener;
        public Uniject.ILevelLoadListener getLevelLoadListener ()
        {
            if (null == listener) {
                listener = gameObject.AddComponent<UnityLevelLoadListener> ();
            }
            return listener;
        }

        private IHTTPClient client;
        public IHTTPClient getHTTPClient (IUtil util)
        {
            if (null == client) {
                client = new HTTPClient (util);
            }
            return client;
        }
    }
}
