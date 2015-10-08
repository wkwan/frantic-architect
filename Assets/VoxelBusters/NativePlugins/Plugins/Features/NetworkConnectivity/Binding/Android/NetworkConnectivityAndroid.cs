using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class NetworkConnectivityAndroid : NetworkConnectivity 
	{
		#region Key Constants //The same keys are used  by Native code as well
		
		private const string 	kHostName				= "host-name";
		private const string 	kTimeOutPeriod			= "time-out-period";
		private const string 	kMaxRetryCount			= "max-retry-count";
		private const string	kTimeGapBetweenPolling	= "time-gap-between-polling";

		#endregion

		#region Platform Native Info
		
		class NativeInfo
		{
			// Handler class name
			public class Class
			{
				public const string NAME				= "com.voxelbusters.nativeplugins.features.reachability.NetworkReachabilityHandler";
			}
			
			// For holding method names
			public class Methods
			{
				public const string INITIALIZE			= "initialize";
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
					Console.LogError(Constants.kDebugTag, "[NetworkConnectivity] Plugin class not intialized!");
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
		
		NetworkConnectivityAndroid()
		{
			Plugin = AndroidPluginUtility.GetSingletonInstance(NativeInfo.Class.NAME);
		}
		
		#endregion

		#region API

		public override void Initialise ()
		{
			NetworkConnectivitySettings _settings = NPSettings.NetworkConnectivity;

			Plugin.Call(NativeInfo.Methods.INITIALIZE);

			//Stop previous if any polling happening.
			StopCoroutine("MonitorNetworkConnectivity");
			StartCoroutine(MonitorNetworkConnectivity(_settings));
		}	

		#endregion

	}
}
#endif