using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins
{
	public partial class ApplicationSettings 
	{
		[System.Serializable]
		public class Features
		{
			#region Properties
			
			[SerializeField, ExecuteOnValueChange("OnApplicationConfigurationChanged")]
			private				bool			m_usesAddressBook = true;
			public				bool			UsesAddressBook
			{
				get
				{
					return m_usesAddressBook;
				}
			}

			#if !NATIVE_PLUGINS_LITE_VERSION
			[SerializeField, ExecuteOnValueChange("OnBillingConfigurationChanged")]
			private				bool			m_usesBilling = true;
			public				bool			UsesBilling
			{
				get
				{
					return m_usesBilling;
				}
			}
			
			[SerializeField, ExecuteOnValueChange("OnApplicationConfigurationChanged")]
			private				bool			m_usesMediaLibrary = true;
			public				bool			UsesMediaLibrary
			{
				get
				{
					return m_usesMediaLibrary;
				}
			}
			#endif

			[SerializeField, ExecuteOnValueChange("OnApplicationConfigurationChanged")]
			private				bool			m_usesNetworkConnectivity = true;
			public				bool			UsesNetworkConnectivity
			{
				get
				{
					return m_usesNetworkConnectivity;
				}
			}
			
			#if !NATIVE_PLUGINS_LITE_VERSION
			[SerializeField, ExecuteOnValueChange("OnApplicationConfigurationChanged")]
			private				bool			m_usesNotificationService = true;
			public				bool			UsesNotificationService
			{
				get
				{
					return m_usesNotificationService;
				}
			}
			#endif
			
			[SerializeField, ExecuteOnValueChange("OnApplicationConfigurationChanged")]
			private				bool			m_usesSharing = true;
			public				bool			UsesSharing
			{
				get
				{
					return m_usesSharing;
				}
			}

			#if !NATIVE_PLUGINS_LITE_VERSION
			[SerializeField, ExecuteOnValueChange("OnTwitterConfigurationChanged")]
			private				bool			m_usesTwitter = false;
			public				bool			UsesTwitter
			{
				get
				{
					return m_usesTwitter;
				}
			}

			[SerializeField, ExecuteOnValueChange("OnApplicationConfigurationChanged")]
			private				bool			m_usesGameServices = false;
			public				bool			UsesGameServices
			{
				get
				{
					return m_usesGameServices;
				}
			}
			
			#endif
			
			#endregion
		}
	}
}