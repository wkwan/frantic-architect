using UnityEngine;
using System.Collections;
using VoxelBusters.NativePlugins;
using VoxelBusters.Utility;
using VoxelBusters.AssetStoreProductUtility.Demo;

namespace VoxelBusters.NativePlugins.Demo
{
	public class SharingDemo : DemoSubMenu 
	{
		#region Properties

		// Related to sharing
		[SerializeField]
		private string					m_shareMessage		= "share message";
		[SerializeField]
		private string					m_shareURL			= "http://www.google.com";
		[SerializeField]
		private eShareOptions[]			m_excludedOptions	= new eShareOptions[0];

		// Related to SMS
		[SerializeField]
		private string					m_smsBody			= "SMS body holds text message that needs to be sent to recipients";
		[SerializeField]
		private string[] 				m_smsRecipients;
		
		// Related to mail
		[SerializeField]
		private string					m_plainTextMailBody	= "This is plain text mail";
		[SerializeField]
		private string					m_htmlMailBody		= "<html><body><h1>Hello</h1></body></html>";
		[SerializeField]
		private string[] 				m_mailRecipients;
		
		[Tooltip ("This demo consideres image relative to Application.persistentDataPath")]
		[SerializeField]
		private string 					m_sampleImageAtDataPath;

		#endregion

		#region Message Sharing

		private bool IsMessagingServiceAvailable()
		{
			return NPBinding.Sharing.IsMessagingServiceAvailable();
		}

		private void SendTextMessage()
		{
			NPBinding.Sharing.SendTextMessage(m_smsBody, m_smsRecipients, FinishedSharing);
		}
		
		#endregion
		
		#region Mail Sharing
		
		private bool IsMailServiceAvailable()
		{
			return NPBinding.Sharing.IsMailServiceAvailable();
		}

		private void SendPlainTextMail()
		{
			NPBinding.Sharing.SendPlainTextMail("Demo", m_plainTextMailBody, m_mailRecipients, FinishedSharing);
		}

		private void SendHTMLTextMail () 
		{
			NPBinding.Sharing.SendHTMLTextMail("Demo", m_htmlMailBody, m_mailRecipients, FinishedSharing);
		}

		private void SendMailWithScreenshot()
		{

			NPBinding.Sharing.SendMailWithScreenshot("Demo",
			                                         m_plainTextMailBody,
			                                         false,
			                                         m_mailRecipients,
			                                         FinishedSharing);

		}

		private void SendMailWithAttachment()
		{
			NPBinding.Sharing.SendMailWithAttachment(	"Demo",
				                                         m_htmlMailBody,
				                                         true,
			                                         	 Application.persistentDataPath + "/" + m_sampleImageAtDataPath,
				                                         "image/png", 
				                                         m_mailRecipients,
				                                         FinishedSharing);
		}

		private bool IsWhatsAppServiceAvailable ()
		{
			return NPBinding.Sharing.IsWhatsAppServiceAvailable();
		}

		#endregion
		
		#region WhatsApp Sharing

		private void ShareTextMessageOnWhatsApp()
		{
			NPBinding.Sharing.ShareTextMessageOnWhatsApp(m_shareMessage, FinishedSharing);			
		}

		private void ShareScreenshotOnWhatsApp()
		{
			NPBinding.Sharing.ShareScreenshotOnWhatsApp(FinishedSharing);
		}
		
		private void ShareImageOnWhatsApp()
		{
			NPBinding.Sharing.ShareImageOnWhatsApp(Application.persistentDataPath + "/" + m_sampleImageAtDataPath, FinishedSharing);
		}
	
		#endregion
		
		#region Sharing On Social Network
		
		private void ShareTextMessageOnSocialNetwork()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();

			// Share
			NPBinding.Sharing.ShareTextMessageOnSocialNetwork(m_shareMessage, 	FinishedSharing);
		}

		private void ShareURLOnSocialNetwork()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();
			
			// Share
			NPBinding.Sharing.ShareURLOnSocialNetwork(m_shareMessage, 		m_shareURL, 
			                                          FinishedSharing);
		}
		private void ShareScreenShotOnSocialNetwork()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();
			
			// Share
			NPBinding.Sharing.ShareScreenShotOnSocialNetwork(m_shareMessage, FinishedSharing);
			
		}
		
		private void ShareImageOnSocialNetwork()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();
			
			// Share
			NPBinding.Sharing.ShareImageOnSocialNetwork(m_shareMessage, Application.persistentDataPath + "/" + m_sampleImageAtDataPath,
			                                            FinishedSharing);
			
		}
		
		#endregion
		
		#region Sharing

		private void ShareMessage()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();
			
			// Share
			NPBinding.Sharing.ShareMessage(m_shareMessage, 		m_excludedOptions, 
			                               FinishedSharing);
		}
		
		private void ShareURL()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();
			
			// Share
			NPBinding.Sharing.ShareURL(m_shareMessage,			m_shareURL, 				
			                           m_excludedOptions, 		FinishedSharing);
		}
		
		private void ShareScreenShot()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();
			
			// Share
			NPBinding.Sharing.ShareScreenShot(m_shareMessage, 	m_excludedOptions,
			                                  FinishedSharing);
		}
		
		private void ShareImageAtPath()
		{
			// Set popover to last touch position
			NPBinding.UI.SetPopoverPointAtLastTouchPosition();
			
			// Share
			NPBinding.Sharing.ShareImageAtPath(m_shareMessage, 		Application.persistentDataPath + "/" + m_sampleImageAtDataPath,
			                                   m_excludedOptions, 	FinishedSharing);
		}

		#endregion

		#region Callbacks
		
		private void FinishedSharing (eShareResult _result)
		{
			AddNewResult("Finished sharing");
			AppendResult("Share Result = " + _result);
		}

		#endregion

		#region UI

		protected override void OnGUIWindow ()
		{
			base.OnGUIWindow();

			RootScrollView.BeginScrollView();
			{
				DrawMessageSharing();
				DrawMailSharing();
				DrawWhatsAppSharing();
				DrawSocialNetworkingSharing();
				DrawGeneralSharing();
			}
			RootScrollView.EndScrollView();
			
			DrawResults();
			DrawPopButton();
		}

		private void DrawMessageSharing ()
		{	
			GUILayout.Label("Share via Message", kSubTitleStyle);
			
			if (GUILayout.Button("Is Messaging Available"))
			{
				AddNewResult("IsMessagingAvailable=" + IsMessagingServiceAvailable());
			}
			
			if (GUILayout.Button("Send Text Message"))
			{
				SendTextMessage();
			}
		}
			
		private void DrawMailSharing ()
		{
			GUILayout.Label("Share via Mail", kSubTitleStyle);
			
			if (GUILayout.Button("Is Mail Available"))
			{
				AddNewResult("Can Send Mail = " + IsMailServiceAvailable());
			}
			
			if (GUILayout.Button("Send Plain Text Mail"))
			{
				SendPlainTextMail();
			}
			
			if (GUILayout.Button("Send HTML Text Mail"))
			{
				SendHTMLTextMail();
			}
			
			if (GUILayout.Button("Send Mail With Screenshot"))
			{
				SendMailWithScreenshot();
			}
			
			if (GUILayout.Button("Send Mail With Attachment : Path"))
			{
				SendMailWithAttachment();
			}
		}

		private void DrawWhatsAppSharing ()
		{
			GUILayout.Label("Share on WhatsApp", kSubTitleStyle);
			
			if (GUILayout.Button("IsWhatsAppServiceAvailable"))
			{
				AddNewResult("Can Share On WhatsApp = " + IsWhatsAppServiceAvailable());
			}
			
			if (GUILayout.Button("ShareTextMessageOnWhatsApp"))
			{
				ShareTextMessageOnWhatsApp();
			}
			
			if (GUILayout.Button("ShareScreenshotOnWhatsApp"))
			{
				ShareScreenshotOnWhatsApp();
			}
			
			if (GUILayout.Button("ShareImageOnWhatsApp"))
			{
				ShareImageOnWhatsApp();
			}
		}

		private void DrawSocialNetworkingSharing ()
		{
			GUILayout.Label("Share on Social Network", kSubTitleStyle);
			
			if (GUILayout.Button("ShareTextMessageOnSocialNetwork"))
			{
				ShareTextMessageOnSocialNetwork();
			}
			
			if (GUILayout.Button("ShareURLOnSocialNetwork"))
			{
				ShareURLOnSocialNetwork();
			}
			
			if (GUILayout.Button("ShareScreenShotOnSocialNetwork"))
			{
				ShareScreenShotOnSocialNetwork();
			}
			
			if (GUILayout.Button("ShareImageOnSocialNetwork"))
			{
				ShareImageOnSocialNetwork();
			}
		}

		private void DrawGeneralSharing ()
		{
			GUILayout.Label("Share", kSubTitleStyle);
			
			if (GUILayout.Button("ShareMessage"))
			{
				ShareMessage();
			}
			
			if (GUILayout.Button("ShareURL"))
			{
				ShareURL();
			}
			
			if (GUILayout.Button("ShareScreenShot"))
			{
				ShareScreenShot();
			}
			
			if (GUILayout.Button("ShareImageAtPath"))
			{
				ShareImageAtPath();
			}
		}

		#endregion
	}
}