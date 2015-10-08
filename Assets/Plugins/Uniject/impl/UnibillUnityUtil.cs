//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using UnityEngine;
using Unibill.Impl;

public class UnibillUnityUtil : MonoBehaviour, Uniject.IUtil {

    private static List<Action> callbacks = new List<Action>();
    private static volatile bool callbacksPending;

    public T[] getAnyComponentsOfType<T>() where T : class {
        GameObject[] objects = (GameObject[]) GameObject.FindObjectsOfType(typeof(GameObject));
        List<T> result = new List<T>();
        foreach (GameObject o in objects) {
            foreach (MonoBehaviour mono in o.GetComponents<MonoBehaviour>()) {
                if (mono is T) {
                    result.Add(mono as T);
                }
            }
        }

        return result.ToArray();
    }

    void Start() {
		DontDestroyOnLoad(this.gameObject);
    }

    public DateTime currentTime { get { return DateTime.Now; } }

    public string persistentDataPath {
        get { return Application.persistentDataPath; }
    }

    public string loadedLevelName() {
        return Application.loadedLevelName;
    }

    public RuntimePlatform Platform {
        get { return Application.platform; }
    }

    public bool IsEditor {
        get { return Application.isEditor; }
    }

    public string DeviceModel {
        get { return SystemInfo.deviceModel; }
    }

    public string DeviceName {
        get { return SystemInfo.deviceName; }
    }

    public DeviceType DeviceType {
        get { return SystemInfo.deviceType; }
    }

    public string OperatingSystem {
        get { return SystemInfo.operatingSystem; }
    }

    private static List<RuntimePlatform> PCControlledPlatforms = new List<RuntimePlatform>() {
	    RuntimePlatform.LinuxPlayer,
        RuntimePlatform.OSXDashboardPlayer,
        RuntimePlatform.OSXEditor,
        RuntimePlatform.OSXPlayer,
        RuntimePlatform.OSXWebPlayer,
        RuntimePlatform.WindowsEditor,
        RuntimePlatform.WindowsPlayer,
        RuntimePlatform.WindowsWebPlayer,
	};

    public static T findInstanceOfType<T>() where T : MonoBehaviour {
        return (T) GameObject.FindObjectOfType(typeof(T));
    }

    public static T loadResourceInstanceOfType<T>() where T : MonoBehaviour {
        return ((GameObject) GameObject.Instantiate(Resources.Load(typeof(T).ToString()))).GetComponent<T>();
    }

    public static bool pcPlatform() {
        return PCControlledPlatforms.Contains(Application.platform);
    }

    public static void DebugLog(string message, params System.Object[] args) {
        try {
            UnityEngine.Debug.Log(string.Format(
                "com.ballatergames.debug - {0}",
                string.Format(message, args))
            );
        } catch (ArgumentNullException a) {
            UnityEngine.Debug.Log(a);
        } catch (FormatException f) {
            UnityEngine.Debug.Log(f);
        }
    }

    object Uniject.IUtil.InitiateCoroutine (System.Collections.IEnumerator start)
    {
        return StartCoroutine (start);
    }

    void Uniject.IUtil.InitiateCoroutine(System.Collections.IEnumerator start, int delay) {
        delayedCoroutine(start, delay);
    }

    private IEnumerator delayedCoroutine(IEnumerator coroutine, int delay) {
        yield return new WaitForSeconds(delay);
        StartCoroutine(coroutine);
    }

    public void RunOnThreadPool(Action runnable) {
        #if !(UNITY_WP8 || UNITY_METRO)
        ThreadPool.QueueUserWorkItem (x => runnable());
        #endif
    }

    void Update() {
        if (!callbacksPending) {
            return;
        }
        // We copy our actions to another array to avoid
        // locking the queue whilst we process them.
        Action[] copy;
        lock (callbacks) {
            if (callbacks.Count == 0) {
                return;
            }

            copy = new Action[callbacks.Count];
            callbacks.CopyTo(copy);
            callbacks.Clear();
            callbacksPending = false;
        }

        foreach (var action in copy) {
            action();
        }
    }


    public void RunOnMainThread(Action runnable) {
        lock (callbacks) {
            callbacks.Add(runnable);
            callbacksPending = true;
        }
    }

    public object getWaitForSeconds (int seconds)
    {
        return new WaitForSeconds (seconds);
    }
}
