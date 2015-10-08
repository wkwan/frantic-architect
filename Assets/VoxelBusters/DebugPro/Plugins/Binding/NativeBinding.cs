using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace VoxelBusters.DebugPRO.Internal
{
	public partial class NativeBinding : MonoBehaviour 
	{
		#region Native Methods

#if UNITY_IOS
		[DllImport("__Internal")]
		private static extern void debugProLogMessage (string _message, eConsoleLogType _type, string _stackTrace);
#endif

		#endregion

		#region Static Methods

		public static void Log (ConsoleLog _log)
		{
#if UNITY_IOS
			debugProLogMessage(_log.Message, _log.Type, _log.StackTrace);
#elif UNITY_ANDROID
			PluginNativeBinding.CallStatic(NativeInfo.Methods.LOG, _log.Message.ToBase64(), kLogTypeMap[_log.Type], _log.StackTrace.ToBase64());
#endif
		}

		#endregion
	}
}