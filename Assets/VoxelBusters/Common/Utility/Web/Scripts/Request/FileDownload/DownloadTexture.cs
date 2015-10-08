using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExifLibrary;

namespace VoxelBusters.Utility
{
	public class DownloadTexture : Request 
	{
		#region Delegates

		public delegate void Completion (Texture2D _texture, string _error);

		#endregion

		#region Properties

		public bool			AutoFixOrientation 
		{ 
			get; 
			set; 
		}

		public Completion	OnCompletion 
		{ 
			get; 
			set; 
		}

		#endregion

		#region Constructors

		public DownloadTexture (URL _URL, bool _isAsynchronous, bool _autoFixOrientation) : base(_URL, _isAsynchronous)
		{
			AutoFixOrientation	= _autoFixOrientation;
			WWWObject			= new WWW(_URL.URLString);
		}

		#endregion
		
		#region Overriden Methods

		protected override void OnFetchingResponse ()
		{
			Texture2D _finalTexture	= null;

			// Callbacks isnt set
			if (OnCompletion == null)
				return;

			// Encountered error while downloading texture
			if (!string.IsNullOrEmpty(WWWObject.error))
			{
				Debug.Log("[DownloadTexture] Error=" + WWWObject.error);
				OnCompletion(null, WWWObject.error);
				return;
			}

			// Fix orientation to normal
			#if !UNITY_WINRT
			if (AutoFixOrientation)
			{
				Stream  _textureStream 	= new MemoryStream(WWWObject.bytes);	

				ExifFile _exifFile 		= ExifFile.Read(_textureStream);
				
				if(_exifFile != null && _exifFile.Properties.ContainsKey(ExifTag.Orientation))
				{
					Orientation _orientation	= (Orientation)_exifFile.Properties[ExifTag.Orientation].Value;
					Debug.Log("[DownloadTexture] Orientation=" + _orientation);

					switch (_orientation)
					{
					case Orientation.Normal:
						// Original image is used
						_finalTexture	= WWWObject.texture;
						break;

					case Orientation.MirroredVertically:
						// Invert horizontally
						_finalTexture	= WWWObject.texture.MirrorTexture(true, false);
						break;

					case Orientation.Rotated180:
						// Invert horizontally as well as vertically 
						_finalTexture	= WWWObject.texture.MirrorTexture(true, true);
						break;

					case Orientation.MirroredHorizontally:
						// Invert vertically 
						_finalTexture	= WWWObject.texture.MirrorTexture(false, true);
						break;

					case Orientation.RotatedLeftAndMirroredVertically:
						// Invert horizontally and rotate by -90
						_finalTexture	= WWWObject.texture.MirrorTexture(true, false).Rotate(-90);
						break;

					case Orientation.RotatedRight:
						// Rotate by 90
						_finalTexture	= WWWObject.texture.Rotate(90);
						break;

					case Orientation.RotatedLeft:
						// Invert vertically and rotate by -90
						_finalTexture	= WWWObject.texture.MirrorTexture(false, true).Rotate(-90);
						break;

					case Orientation.RotatedRightAndMirroredVertically:
						// Rotate by -90
						_finalTexture	= WWWObject.texture.Rotate(-90);
						break;
					}
					
				}
				else
				{
					_finalTexture	= WWWObject.texture;
				}
			}
			// Use original image 
			else
			#endif
			{
				_finalTexture	= WWWObject.texture;
			}

			OnCompletion(_finalTexture, null);
		}

		#endregion
	}
}
