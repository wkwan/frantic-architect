using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uniject;

namespace Unibill.Impl
{
    public class HTTPClient : IHTTPClient
	{
		private class PostRequest
		{
			public string url;
			public PostParameter[] parameters;
			public PostRequest(string url, params PostParameter[] parameters) {
				this.url = url;
				this.parameters = parameters;
			}
		}
		private Queue<PostRequest> events = new Queue<PostRequest>();

        public HTTPClient(IUtil util) {
            util.InitiateCoroutine (pump ());
		}

		public void doPost (string url, params PostParameter[] parameters)
		{
			events.Enqueue(new PostRequest(url, parameters));
		}
            
        private WaitForSeconds wait = new WaitForSeconds(5f);
		IEnumerator pump() {
			while(true) {
                while (events.Count > 0) {
					var e = events.Dequeue ();
					WWWForm form = new WWWForm ();
					for (int t = 0; t < e.parameters.Length; t++) {
						form.AddField (e.parameters[0].name, e.parameters [t].value);
					}
					WWW w = new WWW (e.url, form);
					yield return w;

                    if (!string.IsNullOrEmpty (w.error)) {
                        // Have another go.
                        events.Enqueue (e);
                        yield return new WaitForSeconds(60f);
                        break;
                    }
                }
                yield return wait;
			}
		}
	}
}
