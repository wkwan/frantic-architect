using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

#if UNITY_EDITOR
using System.Xml;

namespace VoxelBusters.NativePlugins
{
	public class NPAndroidManifestGenerator : AndroidManifestGenerator
	{
		#region Properties

		private			ApplicationSettings.Features		m_supportedFeatures;

		#endregion

		#region Constructors

		public NPAndroidManifestGenerator ()
		{
			m_supportedFeatures	= NPSettings.Application.SupportedFeatures;
		}

		#endregion

		#region Application Methods

		protected override void WriteApplicationProperties (XmlWriter _xmlWriter)
		{
			WriteActivityInfo(_xmlWriter);
			WriteProviderInfo(_xmlWriter);
			WriteReceiverInfo(_xmlWriter);
			WriteServiceInfo(_xmlWriter);
			WriteMetaInfo(_xmlWriter);
		}

		private void WriteActivityInfo (XmlWriter _xmlWriter)
		{
			#if !NATIVE_PLUGINS_LITE_VERSION
			// Billing
			if (m_supportedFeatures.UsesBilling)
			{
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.billing.serviceprovider.google.GoogleBillingActivity",
						      _theme:			"@style/FloatingActivityTheme",
				              _comment:			"Billing : Activity used for purchase view");
			}
			
			// Media
			if (m_supportedFeatures.UsesMediaLibrary)
			{
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.medialibrary.MediaLibraryActivity",
				              _theme:			"@style/FloatingActivityTheme",
				              _orientation:		"sensor",
				              _configChanges:	"keyboardHidden|orientation|screenSize",
				              _comment:			"MediaLibrary : Generic helper activity");
				
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.medialibrary.GalleryVideoLauncherActivity",
				              _theme:			"@style/FloatingActivityTheme",
				              _orientation:		"sensor",
				              _comment:			"MediaLibrary : Used for Launching video from gallery");
				
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.medialibrary.YoutubePlayerActivity",
				              _comment:			"MediaLibrary : Youtube player activity");
			}

			// Notifications
			if (m_supportedFeatures.UsesNotificationService)
			{
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.notification.core.ApplicationLauncherFromNotification",
				              _theme:			"@style/FloatingActivityTheme",
				              _comment:			"Application Launcher - Notifications : Used as a proxy to capture triggered notification.");
			}
			
			

			// Twitter
			if (m_supportedFeatures.UsesTwitter)
			{
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.socialnetwork.twitter.TwitterHelperActivity",
				              _theme:			"@style/FloatingActivityTheme",
				              _comment:			"SocialNetworking - Twitter : Generic helper activity");
			}

			if (m_supportedFeatures.UsesGameServices)
			{
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.gameservices.serviceprovider.google.GooglePlayGameUIActivity",
				              _theme:			"@style/FloatingActivityTheme", 
				              _comment:			"Game Play Services helper activity");
			}

			#endif
			
			// Sharing
			if (m_supportedFeatures.UsesSharing)
			{
				WriteActivity(_xmlWriter:		_xmlWriter,
				              _name:			"com.voxelbusters.nativeplugins.features.sharing.SharingActivity",
				              _theme:			"@style/FloatingActivityTheme", 
				              _comment:			"Sharing");
			}

			
			
			
			//Common required activities

			//UIActivity
			WriteActivity(_xmlWriter:		_xmlWriter,
			              _name:			"com.voxelbusters.nativeplugins.features.ui.UiActivity",
			              _theme:			"@style/FloatingActivityTheme",
			              _comment:			"UI  : Generic helper activity for launching Dialogs");
		}

		private void WriteProviderInfo (XmlWriter _xmlWriter)
		{
			// Provider
			_xmlWriter.WriteComment("Custom File Provider. Sharing from internal folders  \"com.voxelbusters.nativeplugins.extensions.FileProviderExtended\"");
			_xmlWriter.WriteStartElement("provider");
			{
				_xmlWriter.WriteAttributeString("android:name", 				"com.voxelbusters.nativeplugins.extensions.FileProviderExtended");
				_xmlWriter.WriteAttributeString("android:authorities", 			string.Format("{0}.fileprovider", PlayerSettings.GetBundleIdentifier()));
				_xmlWriter.WriteAttributeString("android:exported", 			"false");
				_xmlWriter.WriteAttributeString("android:grantUriPermissions", 	"true");
				
				_xmlWriter.WriteStartElement("meta-data");
				{
					_xmlWriter.WriteAttributeString("android:name", 			"android.support.FILE_PROVIDER_PATHS");
					_xmlWriter.WriteAttributeString("android:resource", 		"@xml/nativeplugins_file_paths");
				}
				_xmlWriter.WriteEndElement();
			}
			_xmlWriter.WriteEndElement();
		}
		
		private void WriteReceiverInfo (XmlWriter _xmlWriter)
		{
			#if !NATIVE_PLUGINS_LITE_VERSION
			// GCM receiver
			if (m_supportedFeatures.UsesNotificationService)
			{
				_xmlWriter.WriteComment("Notifications : GCM Receiver");
				_xmlWriter.WriteStartElement("receiver");
				{
					_xmlWriter.WriteAttributeString("android:name", 			"com.voxelbusters.nativeplugins.features.notification.serviceprovider.gcm.GCMBroadcastReceiver");
					_xmlWriter.WriteAttributeString("android:permission", 		"com.google.android.c2dm.permission.SEND");
					
					_xmlWriter.WriteStartElement("intent-filter");
					{
						WriteAction(_xmlWriter:		_xmlWriter,
						            _name:			"com.google.android.c2dm.intent.RECEIVE");

						WriteAction(_xmlWriter:		_xmlWriter,
						            _name:			"com.google.android.c2dm.intent.REGISTRATION");

						WriteCategory(_xmlWriter:	_xmlWriter,
						              _name:		PlayerSettings.GetBundleIdentifier());
					}
					_xmlWriter.WriteEndElement();
				}
				_xmlWriter.WriteEndElement();
			}
			
			// Local notification receiver
			if (m_supportedFeatures.UsesNotificationService)
			{
				_xmlWriter.WriteComment("Notifications : Receiver for alarm to help Local Notifications");
				_xmlWriter.WriteStartElement("receiver");
				{
					_xmlWriter.WriteAttributeString("android:name", 			"com.voxelbusters.nativeplugins.features.notification.core.AlarmEventReceiver");
				}
				_xmlWriter.WriteEndElement();
			}
			#endif
		}

		private void WriteServiceInfo (XmlWriter _xmlWriter)
		{
			#if !NATIVE_PLUGINS_LITE_VERSION
			if (m_supportedFeatures.UsesNotificationService)
			{
				WriteService(_xmlWriter:	_xmlWriter, 
				             _name:			"com.voxelbusters.nativeplugins.features.notification.serviceprovider.gcm.GCMIntentService",
				             _comment:		"Notifications : GCM Service");
			}
			#endif
		}

		private void WriteMetaInfo (XmlWriter _xmlWriter)
		{
			#if !NATIVE_PLUGINS_LITE_VERSION
			if (m_supportedFeatures.UsesGameServices)
			{
				_xmlWriter.WriteStartElement("meta-data");
				{
					_xmlWriter.WriteAttributeString("android:name", 	"com.google.android.gms.games.APP_ID");
					_xmlWriter.WriteAttributeString("android:value", string.Format("\\ {0}", NPSettings.GameServicesSettings.Android.PlayServicesApplicationID));// \ added because its getting considered as integer when added from xml instead of string.
				}
				_xmlWriter.WriteEndElement();

			}
			#endif
		}

		#endregion

		#region Permission Methods

		protected override void WritePermissions (XmlWriter _xmlWriter)
		{
			if (m_supportedFeatures.UsesAddressBook)
			{
				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.READ_CONTACTS", 
				                    _comment: 	"Address Book");
			}

			if (m_supportedFeatures.UsesNetworkConnectivity)
			{
				
				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.ACCESS_NETWORK_STATE",
				                    _comment: 	"Network Connectivity");
			}
			

			#if !NATIVE_PLUGINS_LITE_VERSION
			if (m_supportedFeatures.UsesBilling)
			{
				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"com.android.vending.BILLING", 	
				                    _comment: 	"Billing");
			}

			
			if (m_supportedFeatures.UsesMediaLibrary)
			{
				WriteUsesPermission(_xmlWriter:	_xmlWriter, 
				                    _name: 		"android.permission.CAMERA", 
				                    _features:	new Feature[] { 
													new Feature("android.hardware.camera", false), 
													new Feature("android.hardware.camera.autofocus", false)}, 	
				                    _comment:	"Media Library");

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"com.google.android.apps.photos.permission.GOOGLE_PHOTOS");

			}

			if (m_supportedFeatures.UsesNotificationService)
			{
				WritePermission(_xmlWriter:			_xmlWriter, 	
				                _name: 				string.Format("{0}.permission.C2D_MESSAGE", PlayerSettings.GetBundleIdentifier()),
				                _protectionLevel:	"signature", 
				                _comment: 			"For enabling GCM");

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.GET_ACCOUNTS");

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.WAKE_LOCK");

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		string.Format("{0}.permission.C2D_MESSAGE", PlayerSettings.GetBundleIdentifier()));

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"com.google.android.c2dm.permission.RECEIVE");

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.VIBRATE", 	
				                    _comment: 	"Notifications : If vibration is required for notification");
			}

			if(m_supportedFeatures.UsesGameServices)
			{
				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
					                _name: 		"com.google.android.providers.gsf.permission.READ_GSERVICES", 	
					                _comment: 	"GameServices : For getting content provider access.");

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.GET_ACCOUNTS");

				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.USE_CREDENTIALS");
			}

			#endif


			//Write common permissions here

			//Internet access - Add by default as many features need this.
			WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
			                    _name: 		"android.permission.INTERNET",
			                    _comment:	"Required for internet access");

			//Storage Access
			if(	m_supportedFeatures.UsesSharing 
			   	#if !NATIVE_PLUGINS_LITE_VERSION
				||m_supportedFeatures.UsesMediaLibrary
				||m_supportedFeatures.UsesTwitter
				#endif
				)	
			{
				WriteUsesPermission(_xmlWriter:	_xmlWriter,
				                    _name: 		"android.permission.WRITE_EXTERNAL_STORAGE", 	
				                    _comment:	"For Saving to external directory - Save to Gallery Feature in MediaLibrary / Used for sharing");
				
				WriteUsesPermission(_xmlWriter:	_xmlWriter, 	
				                    _name: 		"android.permission.READ_EXTERNAL_STORAGE");
			}

			
		

		}

		#endregion
	}
}
#endif