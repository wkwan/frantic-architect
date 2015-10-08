using System;
using System.Collections.Generic;
using System.Reflection;
using unibill.Dummy;

namespace Unibill.Impl {

    /// <summary>
    /// Wraps Unity Analytics, forwarding calls to either the Plugin
    /// based Unity Analytics or the engine integrated Analytics,
    /// or both.
    /// </summary>
    public class UnityAnalytics : IUnityAnalytics {

        private MethodInfo[] analyticsMethods;
        private readonly string[] UnityAnalyticsTypes = new string[] {
            // The original, Plugin based Unity Analytics.
            "UnityEngine.Cloud.Analytics.UnityAnalytics",
            // The engine integrated analytics, released in Unity 5.1.
            "UnityEngine.Analytics.Analytics, UnityEngine.Analytics"
        };

        public UnityAnalytics() {
            analyticsMethods = GetUnityAnalyticsMethods (UnityAnalyticsTypes);

            #if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
            // Keep a hard reference to Transaction to stop it being stripped.
	        //William: comment out this
            //Func<string, decimal, string, UnityEngine.Analytics.AnalyticsResult> x = UnityEngine.Analytics.Analytics.Transaction;
            //if (null != x) {
            //}
            #endif
        }

        public void Transaction (string productId, decimal price, string currency, string receipt, string signature) {
            var args = new object[] {
                productId,
                price,
                currency,
                receipt,
                signature
            };

            foreach (var analyticsMethod in analyticsMethods) {
                analyticsMethod.Invoke (null, args);
            }
        }

        private static MethodInfo[] GetUnityAnalyticsMethods(string[] typeNamesToSearch) {
            var result = new List<MethodInfo> ();
            foreach (var typeName in typeNamesToSearch) {
                var t = Type.GetType (typeName);
                if (null != t) {
                    Type[] methodSignature = {
                        typeof(string),
                        typeof(decimal),
                        typeof(string),
                        typeof(string),
                        typeof(string)
                    };
                    var method = WinRTUtils.GetMethod(t, "Transaction", methodSignature);
                    if (null != method) {
                        result.Add (method);
                    }
                }
            }

            return result.ToArray ();
        }
    }
}
