using System;
using System.Collections;
using UnityEngine;
using Uniject;
using Unibill.Impl;

public class RemoteConfigFetcher : MonoBehaviour {

    public void Start() {
        DontDestroyOnLoad (this.gameObject);
    }

	public void Fetch(IStorage storage, string url, string key) {
		StartCoroutine (fetch(storage, url, key));
	}

	private IEnumerator fetch(IStorage storage, string url, string key) {
		var request = new WWW (url);
		log("Fetching latest Unibill config from " + url);
		while (!request.isDone) {
			yield return new WaitForSeconds (1);
		}

		if (!string.IsNullOrEmpty(request.error)) {
			log(string.Format("Failed to fetch inventory: {0}", request.error));
		} else {
			log("Fetched and stored latest inventory");
			storage.SetString(key, request.text);
		}
	}

	private void log(string message) {
		UnityEngine.Debug.Log ("UnibillConfigFetcher:" + message);
	}
}

