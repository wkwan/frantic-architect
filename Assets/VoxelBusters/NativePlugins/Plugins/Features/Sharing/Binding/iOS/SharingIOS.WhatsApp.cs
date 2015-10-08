using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingIOS : Sharing 
	{
		#region Native Methods
		
		[DllImport("__Internal")]
		private static extern bool canShareOnWhatsApp ();

		[DllImport("__Internal")]
		private static extern void shareTextMessageOnWhatsApp (string _message);
		
		[DllImport("__Internal")]
		private static extern void shareImageOnWhatsApp (byte[] _imageByteArray, int _byteArrayLength);
		
		#endregion
		
		#region Overriden API's 

		public override bool IsWhatsAppServiceAvailable ()
		{
			bool _canShare	= canShareOnWhatsApp();
			Console.Log(Constants.kDebugTag, "[Sharing:WhatsApp] CanShare=" + _canShare);

			return _canShare;
		}
		
		public override void ShareTextMessageOnWhatsApp (string _message, SharingCompletion _onCompletion)
		{
			base.ShareTextMessageOnWhatsApp(_message, _onCompletion);

			// Failed to share message
			if (string.IsNullOrEmpty(_message) || !IsWhatsAppServiceAvailable())
				return;

			// Native call
			shareTextMessageOnWhatsApp(_message);
		}

		public override void ShareImageOnWhatsApp (byte[] _imageByteArray, SharingCompletion _onCompletion)
		{
			base.ShareImageOnWhatsApp(_imageByteArray, _onCompletion);

			// Failed to share image
			if (_imageByteArray == null || !IsWhatsAppServiceAvailable())
				return;

			// Native call
			shareImageOnWhatsApp(_imageByteArray, _imageByteArray.Length);
		}

		#endregion
	}
}
#endif