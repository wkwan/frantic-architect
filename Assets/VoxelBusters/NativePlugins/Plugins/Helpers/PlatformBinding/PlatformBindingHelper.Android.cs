using UnityEngine;
using System.Collections;

namespace VoxelBusters.NativePlugins
{
	public partial class PlatformBindingHelper : MonoBehaviour 
	{
	#if UNITY_ANDROID && !UNITY_EDITOR
	
		#region Platform Native Info
		
		class NativeInfo
		{
			//Handler class name
			public class Class
			{
				public const string NATIVE_BINDING_NAME				= "com.voxelbusters.NativeBinding";
			}
			
			//For holding method names
			public class Methods
			{
				public const string ON_PAUSE		 			= "onApplicationPause";
				public const string ON_RESUME		 			= "onApplicationResume";
				public const string ON_QUIT		 				= "onApplicationQuit";
				public const string ENABLE_DEBUG				= "enableDebug";
			}
		}
		
		#endregion
		
		#region  Required Variables
		

		private AndroidJavaClass 	m_nativeBinding;
		private AndroidJavaClass  	PluginNativeBinding
		{
			get 
			{ 
				if(m_nativeBinding == null)
				{
					m_nativeBinding = AndroidPluginUtility.CreateClassObject(NativeInfo.Class.NATIVE_BINDING_NAME);
				}
				return m_nativeBinding; 
			}
			
			set
			{
				m_nativeBinding = value;
			}
		}

		#endregion

		#region Methods
	
		private void InitializeAndroidSettings()
		{
			bool _isDebugMode = NPSettings.Application.IsDebugMode;

			PluginNativeBinding.CallStatic(NativeInfo.Methods.ENABLE_DEBUG, _isDebugMode);
		}
	
		private void OnApplicationPause(bool paused)
		{
			if(paused)
			{
				PluginNativeBinding.CallStatic(NativeInfo.Methods.ON_PAUSE);
			}
			else
			{
				PluginNativeBinding.CallStatic(NativeInfo.Methods.ON_RESUME);
			}
		}
	
		private void OnApplicationQuit()
		{
			PluginNativeBinding.CallStatic(NativeInfo.Methods.ON_QUIT);
		}
		
		#endregion
	
	#endif
	
	}
}