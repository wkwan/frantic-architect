using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class AddressBookAndroid : AddressBook 
	{
		#region Platform Native Info
		
		class NativeInfo
		{
			// Handler class name
			public class Class
			{
				public const string NAME			= "com.voxelbusters.nativeplugins.features.addressbook.AddressBookHandler";
			}
			
			// For holding method names
			public class Methods
			{
				public const string READ_CONTACTS	= "readContacts";
				public const string IS_AUTHORIZED	= "isAuthorized";
				
				
			}
		}
		
		#endregion
		
		#region  Required Variables
		
		private AndroidJavaObject 	m_plugin;
		private AndroidJavaObject  	Plugin
		{
			get 
			{ 
				if(m_plugin == null)
				{
					Console.LogError(Constants.kDebugTag, "[AddressBook] Plugin class not intialized!");
				}
				return m_plugin; 
			}
			
			set
			{
				m_plugin = value;
			}
		}
		
		#endregion
		
		#region Constructors
		
		AddressBookAndroid()
		{
			Plugin = AndroidPluginUtility.GetSingletonInstance(NativeInfo.Class.NAME);
		}
		
		#endregion
		
		#region Overriden API's

		public override eABAuthorizationStatus GetAuthorizationStatus ()
		{
			bool _accessGranted = Plugin.Call<bool>(NativeInfo.Methods.IS_AUTHORIZED);

			if(_accessGranted)
			{
				return eABAuthorizationStatus.AUTHORIZED;
			}
			else
			{
				return eABAuthorizationStatus.DENIED;
			}
		}
		
		public override void ReadContacts (ReadContactsCompletion _onCompletion)
		{
			base.ReadContacts(_onCompletion);

			// Native method is called
			Plugin.Call(NativeInfo.Methods.READ_CONTACTS);
		}
		
		#endregion
	}
}
#endif