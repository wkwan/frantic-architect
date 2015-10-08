//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections;
using UnityEngine;
using Unibill.Impl;

namespace Uniject {
    public interface IUtil {
        T[] getAnyComponentsOfType<T>() where T : class;
        string loadedLevelName();
        RuntimePlatform Platform { get; }

        bool IsEditor { get; }
        string persistentDataPath { get; }
        DateTime currentTime { get; }
        string DeviceModel { get; }
        string DeviceName { get; }
        DeviceType DeviceType { get; }
        string OperatingSystem { get; }
        object InitiateCoroutine(IEnumerator start);
        object getWaitForSeconds (int seconds);
        void InitiateCoroutine(IEnumerator start, int delayInSeconds);
        void RunOnThreadPool(Action runnable);
        void RunOnMainThread(Action runnable);
    }
}
