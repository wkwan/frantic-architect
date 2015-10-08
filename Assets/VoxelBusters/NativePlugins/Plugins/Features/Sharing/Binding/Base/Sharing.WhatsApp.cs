using UnityEngine;
using System.Collections;
using System.IO;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class Sharing : MonoBehaviour 
	{
		#region WhatsApp API's

		/// <summary>
		/// Determines whether whatsApp service is available.
		/// </summary>
		/// <returns><c>true</c> if whatsApp service is available; otherwise, <c>false</c>.</returns>
		public virtual bool IsWhatsAppServiceAvailable ()
		{
			bool _canShare	= false;
			Console.Log(Constants.kDebugTag, "[Sharing:WhatsApp] CanShare=" + _canShare);
			
			return _canShare;
		}
		
		/// <summary>
		/// Shares the text message on whatsApp.
		/// </summary>
		/// <param name="_message">Message to share.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public virtual void ShareTextMessageOnWhatsApp (string _message, SharingCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();
			
			// Cache callback
			OnSharingFinished	= _onCompletion;

			// Sharing on whatsapp isnt supported
			if (string.IsNullOrEmpty(_message) || !IsWhatsAppServiceAvailable())
			{
				Console.Log(Constants.kDebugTag, "[Sharing:WhatsApp] Failed to share text");
				WhatsAppShareFinished(WhatsAppShareFailedResponse());
				return;
			}
		}
		
		/// <summary>
		/// Shares the screenshot on whats app.
		/// </summary>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareScreenshotOnWhatsApp (SharingCompletion _onCompletion)
		{
			// First capture frame
			StartCoroutine(TextureExtensions.TakeScreenshot((_texture)=>{
				// Convert texture into byte array
				byte[] _imageByteArray	= _texture.EncodeToPNG();

				// Share
				ShareImageOnWhatsApp(_imageByteArray, _onCompletion);
			}));
		}
		
		/// <summary>
		/// Shares the image on whatsApp.
		/// </summary>
		/// <param name="_imagePath">Path of the image to be shared.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareImageOnWhatsApp (string _imagePath, SharingCompletion _onCompletion)
		{
			if (!File.Exists(_imagePath))
			{
				Console.LogError(Constants.kDebugTag, "[Sharing:WhatsApp] File doesnt exist. Path="  + _imagePath);
				ShareImageOnWhatsApp((byte[])null, _onCompletion);
				return;
			}

			// Get file data
			byte[] _imageByteArray	= FileOperations.ReadAllBytes(_imagePath);

			// Share
			ShareImageOnWhatsApp(_imageByteArray, _onCompletion);
		}
		
		/// <summary>
		/// Shares the image on whats app.
		/// </summary>
		/// <param name="_texture">Texture to take the image from.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void ShareImageOnWhatsApp (Texture2D _texture, SharingCompletion _onCompletion)
		{
			if (_texture == null)
			{
				Console.LogError(Constants.kDebugTag, "[Sharing:WhatsApp] Texture is null");
				ShareImageOnWhatsApp((byte[])null, _onCompletion);
				return;
			}

			// Convert texture into byte array
			byte[] _imageByteArray	= _texture.EncodeToPNG();
			
			// Share
			ShareImageOnWhatsApp(_imageByteArray, _onCompletion);
		}

		/// <summary>
		/// Shares the image on whats app.
		/// </summary>
		/// <param name="_imageByteArray">Image byte array to create the image from.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public virtual void ShareImageOnWhatsApp (byte[] _imageByteArray, SharingCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();
			
			// Cache callback
			OnSharingFinished	= _onCompletion;

			// Sharing on whatsapp isnt supported
			if (_imageByteArray == null || !IsWhatsAppServiceAvailable())
			{
				Console.LogError(Constants.kDebugTag, "[Sharing:WhatsApp] Failed to share image");
				WhatsAppShareFinished(WhatsAppShareFailedResponse());
				return;
			}		
		}
		
		#endregion
	}
}