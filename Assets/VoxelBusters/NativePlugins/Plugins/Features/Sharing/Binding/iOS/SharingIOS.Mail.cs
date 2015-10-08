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
		private static extern bool canSendMail ();
		
		[DllImport("__Internal")]
		private static extern void sendMail (string _subject, 						string _body, 
		                                     bool _isHTMLBody, 						byte[] _attachmentByteArray, 
		                                     int _attachmentByteArrayLength, 		string _mimeType, 
		                                     string _attachmentFileNameWithExtn, 	string _recipients);

		#endregion

		#region Overriden API's 

		public override bool IsMailServiceAvailable ()
		{
			bool _canSendMail	= canSendMail();
			Console.Log(Constants.kDebugTag, "[Sharing:Mail] CanSendMail=" + _canSendMail);
			
			return _canSendMail;
		}
		
		public override void SendMail (string _subject, string _body, bool _isHTMLBody, byte[] _attachmentByteArray, 
		                               string _mimeType, string _attachmentFileNameWithExtn, string[] _recipients, SharingCompletion _onCompletion)
		{
			base.SendMail(_subject, _body, _isHTMLBody, _attachmentByteArray, _mimeType, 
			              _attachmentFileNameWithExtn, _recipients, _onCompletion);

			if (IsMailServiceAvailable())
			{
				// Attachment data array length
				int _attachmentByteArrayLength	= 0;
				
				if (_attachmentByteArray != null)
					_attachmentByteArrayLength	= _attachmentByteArray.Length;
				
				sendMail(_subject, _body, _isHTMLBody, _attachmentByteArray, _attachmentByteArrayLength, 	
				         _mimeType, _attachmentFileNameWithExtn, _recipients.ToJSON());
			}
		}
		
		#endregion
	}
}
#endif