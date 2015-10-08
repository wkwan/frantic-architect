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
		#region API's

		/// <summary>
		/// Determines whether mail service is available.
		/// </summary>
		/// <returns><c>true</c> if this mail service is available; otherwise, <c>false</c>.</returns>
		public virtual bool IsMailServiceAvailable ()
		{
			bool _canSendMail	= false;
			Console.Log(Constants.kDebugTag, "[Sharing:Mail] CanSendMail=" + _canSendMail);
			
			return _canSendMail;
		}
		
		/// <summary>
		/// Sends the plain text mail.
		/// </summary>
		/// <param name="_subject">Subject of the mail.</param>
		/// <param name="_body">Body of the mail.</param>
		/// <param name="_recipients">List of receipients.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void SendPlainTextMail (string _subject, string _body, string[] _recipients, 
		                               SharingCompletion _onCompletion) 
		{
			SendMail(_subject, _body, false, null, string.Empty, 
			         string.Empty, _recipients, _onCompletion);
		}
		
		/// <summary>
		/// Sends the HTML text mail.
		/// </summary>
		/// <param name="_subject">Subject of the mail.</param>
		/// <param name="_htmlBody">HTML string for body of the mail.</param>
		/// <param name="_recipients">List of receipients.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void SendHTMLTextMail (string _subject, string _htmlBody, string[] _recipients, 
		                              SharingCompletion _onCompletion) 
		{
			SendMail(_subject, _htmlBody, true, null, string.Empty, 
			         string.Empty, _recipients, _onCompletion);
		}
		
		/// <summary>
		/// Sends the mail with screenshot.
		/// </summary>
		/// <param name="_subject">Subject of mail.</param>
		/// <param name="_body">Body of the mail.</param>
		/// <param name="_isHTMLBody">Indicates if the sent body string is HTML string.</param>
		/// <param name="_recipients">List of receipients.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void SendMailWithScreenshot (string _subject, string _body, bool _isHTMLBody, 
		                                    string[] _recipients, SharingCompletion _onCompletion) 
		{
			// First capture frame
			StartCoroutine(TextureExtensions.TakeScreenshot((_texture)=>{
				// Convert texture into byte array
				byte[] _imageByteArray	= _texture.EncodeToPNG();
				
				SendMail(_subject, _body, _isHTMLBody, _imageByteArray, 
				         MIMEType.kPNG , "Screenshot.png", _recipients, _onCompletion);
			}));
		}

		/// <summary>
		/// Sends the mail with texture as image.
		/// </summary>
		/// <param name="_subject">Subject of mail.</param>
		/// <param name="_body">Body of the mail.</param>
		/// <param name="_isHTMLBody">Indicates if the sent body string is HTML string.</param>
		/// <param name="_texture">Texture to create the image from.</param>
		/// <param name="_recipients">List of receipients.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void SendMailWithTexture (string _subject, string _body, bool _isHTMLBody, 
		                                 Texture2D _texture, string[] _recipients, SharingCompletion _onCompletion) 
		{
			byte[] _imageByteArray	= null;
			string _mimeType		= null;
			string _attachmentName	= null;

			// Convert texture into byte array
			if (_texture != null)
			{
				_imageByteArray	= _texture.EncodeToPNG();
				_attachmentName	= "texture.png";
				_mimeType		= MIMEType.kPNG;
			}
			else
			{
				Console.LogWarning(Constants.kDebugTag, "[Sharing:Mail] Sending mail with no attachments, attachment is null");
			}
			
			SendMail(_subject, _body, _isHTMLBody, _imageByteArray, 
			         _mimeType, _attachmentName, _recipients, _onCompletion);
		}
		
		/// <summary>
		/// Sends the mail with an attachment.
		/// </summary>
		/// <param name="_subject">Subject of mail.</param>
		/// <param name="_body">Body of the mail.</param>
		/// <param name="_isHTMLBody">Indicates if the sent body string is HTML string.</param>
		/// <param name="_attachmentPath">Path to attachment file.</param>
		/// <param name="_mimeType">MIME type of attachment. ex: image/png, application/pdf .</param>
		/// <param name="_recipients">List of receipients.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public void SendMailWithAttachment (string _subject, string _body, bool _isHTMLBody, 
		                                    string _attachmentPath, string _mimeType, string[] _recipients, SharingCompletion _onCompletion) 
		{
			byte[] _attachmentByteArray	= null;
			string _filename			= null;

			// File exists
			if (File.Exists(_attachmentPath))
			{
				_attachmentByteArray	= FileOperations.ReadAllBytes(_attachmentPath);
				_filename				= Path.GetFileName(_attachmentPath);
			}
			else
			{
				Console.LogWarning(Constants.kDebugTag, "[Sharing:Mail] Sending file with no attachments, file doesnt exist at path="  + _attachmentPath);
			}

			SendMail(_subject, _body, _isHTMLBody, _attachmentByteArray,
			         _mimeType, _filename, _recipients, _onCompletion);
		}
		
		/// <summary>
		/// Sends the mail with attachment from byte array.
		/// </summary>
		/// <param name="_subject">Subject of mail.</param>
		/// <param name="_body">Body of the mail.</param>
		/// <param name="_isHTMLBody">Indicates if the sent body string is HTML string.</param>
		/// <param name="_attachmentByteArray">Attachment byte array.</param>
		/// <param name="_mimeType">MIME type of attachment. ex: image/png, application/pdf .</param>
		/// <param name="_attachmentFileNameWithExtn">Attachment file name with extension.</param>
		/// <param name="_recipients">List of receipients.</param>
		/// <param name="_onCompletion">Callback to be triggered when sharing action completes.</param>
		public virtual void SendMail (string _subject, string _body, bool _isHTMLBody, byte[] _attachmentByteArray, 
		                              string _mimeType, string _attachmentFileNameWithExtn, string[] _recipients, SharingCompletion _onCompletion)
		{
			// Pause unity player
			this.PauseUnity();
			
			// Cache callback
			OnSharingFinished	= _onCompletion;

			// Cant send mail
			if (!IsMailServiceAvailable())
			{
				MailShareFinished(MailShareFailedResponse());
				return;
			}
		}
		
		#endregion
	}
}
