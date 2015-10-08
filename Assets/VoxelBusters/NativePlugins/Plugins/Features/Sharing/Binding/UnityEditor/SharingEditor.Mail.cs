using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class SharingEditor : Sharing 
	{
		#region Overriden API's 

		public override bool IsMailServiceAvailable ()
		{	
			bool _canSendMail	= true;
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
				if (_attachmentByteArray != null)
					Console.LogWarning(Constants.kDebugTag, "[Sharing:Mail] Attachments are not supported in editor");

				string	_mailToAddress	= null;

				if (_recipients != null)
					_mailToAddress		= string.Join(",", _recipients);

				string	_mailToSubject	= EscapingString(_subject);
				string	_mailToBody		= EscapingString(_body);
				string	_mailToString	= string.Format("mailto:{0}?subject={1}&body={2}", _mailToAddress, _mailToSubject, _mailToBody);

				// Opens mail client
				Application.OpenURL(_mailToString);

				// Send event
				MailShareFinished(null);
			}
		}

		private string EscapingString (string _inputString)
		{
			return WWW.EscapeURL(_inputString).Replace("+","%20");
		}

		#endregion
	}
}
#endif