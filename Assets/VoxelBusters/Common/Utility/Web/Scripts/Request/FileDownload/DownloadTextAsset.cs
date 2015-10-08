using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExifLibrary;

namespace VoxelBusters.Utility
{
	public class DownloadTextAsset : Request 
	{
		#region Delegates

		public delegate void Completion (string _text, string _error);

		#endregion

		#region Properties

		public Completion OnCompletion
		{
			get;
			set;
		}

		#endregion

		#region Constructors
		
		public DownloadTextAsset (URL _URL, bool _isAsynchronous, bool _autoFixOrientation) : base(_URL, _isAsynchronous)
		{
			WWWObject	= new WWW(_URL.URLString);
		}
		
		#endregion
		
		#region Handling Response

		protected override void OnFetchingResponse ()
		{			
			Debug.Log("[DownloadTextAsset] Did finish downloading, Error=" + WWWObject.error);

			// Callback isnt set
			if (OnCompletion == null)
				return;

			// Valid data
			if (string.IsNullOrEmpty(WWWObject.error))
			{
				OnCompletion(WWWObject.text, null);
			}
			// Encountered error while downloading
			else
			{
				OnCompletion(null, WWWObject.error);
			}
		}

		#endregion
	}
}
