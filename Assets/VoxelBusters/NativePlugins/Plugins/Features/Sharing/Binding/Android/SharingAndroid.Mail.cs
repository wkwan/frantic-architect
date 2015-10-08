using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingAndroid : Sharing 
	{
		public override bool IsMailServiceAvailable()
		{
			bool _canSendMail	= Plugin.Call<bool>(NativeInfo.Methods.CAN_SEND_MAIL);
			if(!_canSendMail)
			{
				Console.LogWarning(Constants.kDebugTag, "[Sharing:Mail] CanSendMail=" + _canSendMail);
			}

			return _canSendMail;
		}

		public override void SendMail (string _subject, string _body, bool _isHTMLBody, byte[] _attachmentByteArray, 
		                               string _mimeType, string _attachmentFileNameWithExtn, string[] _recipients, SharingCompletion _onCompletion)
		{
			base.SendMail(_subject, _body, _isHTMLBody, _attachmentByteArray, _mimeType, 
			              _attachmentFileNameWithExtn, _recipients, _onCompletion);

			if (IsMailServiceAvailable())
			{
				// Find attachment data array length
				int _attachmentByteArrayLength	= 0;
				
				if (_attachmentByteArray != null)
					_attachmentByteArrayLength	= _attachmentByteArray.Length;

				Plugin.Call(NativeInfo.Methods.SEND_MAIL, _subject, _body,
				            _isHTMLBody, _attachmentByteArray, _attachmentByteArrayLength,
				            _mimeType, _attachmentFileNameWithExtn, _recipients.ToJSON());
			}
		}
	}
}
#endif