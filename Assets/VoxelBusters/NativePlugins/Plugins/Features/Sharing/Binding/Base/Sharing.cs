using UnityEngine;
using System.Collections;
using System.IO;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;
using DownloadTexture = VoxelBusters.Utility.DownloadTexture;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	/// <summary>
	/// Sharing gives access to different forms of sharing. Can share text or image with Social network, Mail, SMS, WhatsApp.
	/// </summary>
	public partial class Sharing : MonoBehaviour 
	{
		#region Properties

		private eShareOptions[] m_socialNetworkExcludedList	= new eShareOptions[] { eShareOptions.MESSAGE,
																					eShareOptions.MAIL,
																					eShareOptions.WHATSAPP
																				};

		#endregion

		#region Social Network API's

		/// <summary>
		/// Shares the text message on social network.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareTextMessageOnSocialNetwork (string _message, SharingCompletion _onCompletion) 
		{
			ShareMessage(_message, m_socialNetworkExcludedList, _onCompletion);
		}
		
		/// <summary>
		/// Shares the given URL string on social network.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_URLString">URL string to share.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareURLOnSocialNetwork (string _message, string _URLString, SharingCompletion _onCompletion) 
		{
			ShareURL(_message, _URLString, m_socialNetworkExcludedList, _onCompletion);
		}

		/// <summary>
		/// Shares the current screenshot on social network.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareScreenShotOnSocialNetwork (string _message, SharingCompletion _onCompletion)
		{
			ShareScreenShot(_message, m_socialNetworkExcludedList, _onCompletion);
		}

		/// <summary>
		/// Shares the image on social network.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_texture">Texture to share.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareImageOnSocialNetwork (string _message, Texture2D _texture, SharingCompletion _onCompletion)
		{
			ShareImage(_message, _texture, m_socialNetworkExcludedList, _onCompletion);
		}

		/// <summary>
		/// Shares the image on social network.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_imagePath">Path where image exists that needs to be shared.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareImageOnSocialNetwork (string _message, string _imagePath, SharingCompletion _onCompletion)
		{
			ShareImageAtPath(_message, _imagePath, m_socialNetworkExcludedList, _onCompletion);
		}

		/// <summary>
		/// Shares the image on social network.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_imageByteArray">image byte array to create the image from.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareImageOnSocialNetwork (string _message, byte[] _imageByteArray, SharingCompletion _onCompletion)
		{
			Share(_message, null, _imageByteArray, m_socialNetworkExcludedList, _onCompletion);
		}

		#endregion

		#region API's

		/// <summary>
		/// Shares the message.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_excludedOptions">List of sharing services to exclude while sharing.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareMessage (string _message, eShareOptions[] _excludedOptions, SharingCompletion _onCompletion)
		{
			Share(_message, null, null, _excludedOptions, _onCompletion);
		}

		/// <summary>
		/// Shares the URL string.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_URLString">URL string to share.</param>
		/// <param name="_excludedOptions">List of sharing services to exclude while sharing.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareURL (string _message, string _URLString, eShareOptions[] _excludedOptions, SharingCompletion _onCompletion)
		{
			if (string.IsNullOrEmpty(_URLString))
			{
				Console.LogWarning(Constants.kDebugTag, "[Sharing] ShareURL, URL is null/empty");
			}

			Share(_message, _URLString, null, _excludedOptions, _onCompletion);
		}

		/// <summary>
		/// Shares the current screenshot.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_excludedOptions">List of sharing services to exclude while sharing.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareScreenShot (string _message, eShareOptions[] _excludedOptions, SharingCompletion _onCompletion)
		{
			// First capture screenshot
			StartCoroutine(TextureExtensions.TakeScreenshot((_texture)=>{

				// Share image
				ShareImage(_message, _texture, _excludedOptions, _onCompletion);
			}));
		}

		/// <summary>
		/// Shares the image.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_texture">Texture to share as image.</param>
		/// <param name="_excludedOptions">List of sharing services to exclude while sharing.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareImage (string _message, Texture2D _texture, eShareOptions[] _excludedOptions, SharingCompletion _onCompletion)
		{
			byte[] _imageByteArray	= null;

			if (_texture != null)
			{
				_imageByteArray	= _texture.EncodeToPNG();
			}
			else
			{
				Console.LogWarning(Constants.kDebugTag, "[Sharing] ShareImage, texure is null");
			}

			Share(_message, null, _imageByteArray, _excludedOptions, _onCompletion);
		}

		/// <summary>
		/// Shares the image at path.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_imagePath">Path where image exists that needs to be shared.</param>
		/// <param name="_excludedOptions">List of sharing services to exclude while sharing.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareImageAtPath (string _message, string _imagePath, eShareOptions[] _excludedOptions, SharingCompletion _onCompletion)
		{
			if (!File.Exists(_imagePath))
			{
				Console.LogWarning(Constants.kDebugTag, "[Sharing] ShareImageAtPath, path is invalid");
				ShareImage(_message, null, _excludedOptions, _onCompletion);
				return;
			}

			// Download image from given path
			URL _imagePathURL				= URL.FileURLWithPath(_imagePath);
			DownloadTexture _newDownload	= new DownloadTexture(_imagePathURL, true, false);
			_newDownload.OnCompletion		= (Texture2D _texture, string _error)=>{

				// Download went wrong
				if (!string.IsNullOrEmpty(_error))
				{
					Console.LogWarning(Constants.kDebugTag, "[Sharing] ShareImageAtPath, failed to download texture. Error=" + _error);
				}
					
				ShareImage(_message, _texture, _excludedOptions, _onCompletion);
			};
			
			// Start download
			_newDownload.StartRequest();
		}

		/// <summary>
		/// Share message , URL or image
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_URLString">URL string to share.</param>
		/// <param name="_imageByteArray">image byte array to create the image from.</param>
		/// <param name="_excludedOptions">List of sharing services to exclude while sharing.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void Share (string _message, string _URLString, byte[] _imageByteArray, eShareOptions[] _excludedOptions, SharingCompletion _onCompletion)
		{
			string _excludedOptionsJsonString	= null;
			
			if (_excludedOptions != null)
				_excludedOptionsJsonString	= _excludedOptions.ToJSON();

			Share(_message, _URLString, _imageByteArray, _excludedOptionsJsonString, _onCompletion);
		}

		protected virtual void Share (string _message, string _URLString, byte[] _imageByteArray, string _excludedOptionsJsonString, SharingCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();

			// Cache callback
			OnSharingFinished	= _onCompletion;
		}

		#endregion
	}
}