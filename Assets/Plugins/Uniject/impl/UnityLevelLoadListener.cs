using System;
using UnityEngine;
using Uniject;

namespace Uniject.Impl
{
    public class UnityLevelLoadListener : MonoBehaviour, ILevelLoadListener
    {
        private Action listener;
        public void registerListener (Action action)
        {
            this.listener = action;
        }
		
		void Start() {
			DontDestroyOnLoad(this.gameObject);
		}

        void OnLevelWasLoaded(int level) {
            if (listener != null) {
                listener ();
            }
        }
    }
}
