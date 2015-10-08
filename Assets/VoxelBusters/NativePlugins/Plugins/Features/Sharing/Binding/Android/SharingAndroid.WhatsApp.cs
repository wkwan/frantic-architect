using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingAndroid : Sharing 
	{
		#region Overriden API's 

		public override bool IsWhatsAppServiceAvailable ()
		{
			bool _canShare	=  Plugin.Call<bool>(NativeInfo.Methods.CAN_SHARE_ON_WHATS_APP);

			if(!_canShare)
			{
				Console.Log(Constants.kDebugTag, "[Sharing:WhatsApp] CanShare=" + _canShare);
			}
			
			return _canShare;
		}

		public override void ShareTextMessageOnWhatsApp (string _message, SharingCompletion _onCompletion)
		{
			base.ShareTextMessageOnWhatsApp(_message, _onCompletion);
			
			// Failed to share message
			if (string.IsNullOrEmpty(_message) || !IsWhatsAppServiceAvailable())
				return;

			Plugin.Call(NativeInfo.Methods.SHARE_ON_WHATS_APP, _message, null, 0);
		}
		
		public override void ShareImageOnWhatsApp (byte[] _imageByteArray, SharingCompletion _onCompletion)
		{
			base.ShareImageOnWhatsApp(_imageByteArray, _onCompletion);
			
			// Failed to share image
			if (_imageByteArray == null || !IsWhatsAppServiceAvailable())
				return;

			Plugin.Call(NativeInfo.Methods.SHARE_ON_WHATS_APP, null, _imageByteArray, _imageByteArray.Length);
		}
		
		#endregion
	}
}
#endif