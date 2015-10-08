using System;
using System.Collections;
using System.Collections.Generic;

namespace Uniject
{
    public interface IHTTPRequest {
        Dictionary<string, string> responseHeaders { get; }
        byte[] bytes { get; }
        string contentString { get; }
        string error { get; }
    }

    public interface IURLFetcher
    {
        /// <summary>
        /// GET a URL with a specific set of headers.
        /// </summary>
        /// <returns>
        /// A yieldable object, ie a WWW instance when running in Unity.
        /// Following the yield, getResponse() should be called.
        /// </returns>
        object doGet(string url, Dictionary<string, string> headers);
        object doPost (string url, Dictionary<string, string> parameters);
        IHTTPRequest getResponse();
    }
}
