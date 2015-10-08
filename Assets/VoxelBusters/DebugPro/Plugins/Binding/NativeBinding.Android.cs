using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO.Internal;

#if UNITY_ANDROID
using System.Collections.Generic;

namespace VoxelBusters.DebugPRO.Internal
{
	public partial class NativeBinding : MonoBehaviour 
	{
		#region Native Methods

		class NativeInfo
		{
			public class Class
			{
				public const string NATIVE_BINDING_NAME		= "com.voxelbusters.NativeBinding";
			}
			
			//For holding method names
			public class Methods
			{
				public const string LOG		 			= "logMessage";		
			}
		}

		private static AndroidJavaClass 	m_nativeBinding;
		private static AndroidJavaClass  	PluginNativeBinding
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

		private static Dictionary<eConsoleLogType, string> kLogTypeMap = new Dictionary<eConsoleLogType, string>()
		{
			{ eConsoleLogType.ERROR		, "ERROR"},
			{ eConsoleLogType.ASSERT	, "ASSERT"},
			{ eConsoleLogType.WARNING	, "WARNING"},
			{ eConsoleLogType.INFO		, "INFO"},
			{ eConsoleLogType.EXCEPTION	, "EXCEPTION"},
		};
		

		#endregion
	}
}
#endif