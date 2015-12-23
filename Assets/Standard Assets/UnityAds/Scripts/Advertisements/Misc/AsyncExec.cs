#if UNITY_ANDROID || UNITY_IOS

namespace UnityEngine.Advertisements {
  using UnityEngine;
  using System.Collections;

  internal class AsyncExec {
    private static GameObject asyncExecGameObject;
    private static MonoBehaviour coroutineHost;
    private static AsyncExec asyncImpl;
    private static bool init = false;

    private static MonoBehaviour getImpl() {
      if(!init) {
        asyncImpl = new AsyncExec();
        asyncExecGameObject = new GameObject("Unity Ads Coroutine Host") { hideFlags = HideFlags.HideAndDontSave };
        coroutineHost = asyncExecGameObject.AddComponent<MonoBehaviour>();
        Object.DontDestroyOnLoad(asyncExecGameObject);
        init = true;
      }

      return coroutineHost;
    }

    private static AsyncExec getAsyncImpl() {
      if(!init) {
        getImpl();
      }

      return asyncImpl;
    }

    public static void runWithCallback<K,T>(System.Func<K,System.Action<T>,IEnumerator> asyncMethod, K arg0, System.Action<T> callback) {
      getImpl().StartCoroutine(asyncMethod(arg0, callback));
    }
  }
}

#endif