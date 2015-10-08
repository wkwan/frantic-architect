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
		#region Platform Native Info
		
		class NativeInfo
		{
			// Handler class name
			public class Class
			{
				public const string NAME								= "com.voxelbusters.nativeplugins.features.sharing.SharingHandler";
			}
			
			// For holding method names
			public class Methods
			{
				public const string SHARE		 						= "share";
				public const string CAN_SEND_MAIL						= "canSendMail";
				public const string SEND_MAIL							= "sendMail";
				public const string CAN_SEND_SMS						= "canSendSms";
				public const string SEND_SMS							= "sendSms";

				public const string CAN_SHARE_ON_WHATS_APP				= "canShareOnWhatsApp";
				public const string SHARE_ON_WHATS_APP					= "shareOnWhatsApp";
			}
		}
		
		#endregion
		
		#region  Required Variables
		
		private AndroidJavaObject m_plugin;
		private AndroidJavaObject Plugin
		{
			get 
			{ 
				if(m_plugin == null)
				{
					Console.LogError(Constants.kDebugTag, "[Sharing] Plugin class not intialized!");
				}
				return m_plugin; 
			}
			
			set
			{
				m_plugin = value;
			}
		}
		
		private bool m_registeredForLocalNotifications;

		#endregion
		
		#region Constructors
		
		SharingAndroid()
		{
			Plugin = AndroidPluginUtility.GetSingletonInstance(NativeInfo.Class.NAME);
		}
		
		#endregion

		#region Overriden API's
		
		protected override void Share (string _message, string _URLString, byte[] _imageByteArray, string _excludedOptionsJsonString, SharingCompletion _onCompletion)
		{
			base.Share(_message, _URLString, _imageByteArray, _excludedOptionsJsonString, _onCompletion);
			
			// Get image byte array length
			int _byteArrayLength	= 0;
			
			if (_imageByteArray != null)
				_byteArrayLength	= _imageByteArray.Length;
			
			Plugin.Call(NativeInfo.Methods.SHARE, _message, _URLString, _imageByteArray, _byteArrayLength, _excludedOptionsJsonString);
		}
		
		#endregion
	}
}
#endif