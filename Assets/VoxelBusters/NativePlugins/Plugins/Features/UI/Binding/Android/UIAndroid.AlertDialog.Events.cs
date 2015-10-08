using UnityEngine;
using System.Collections;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	public partial class UIAndroid : UI 
	{
		#region Parse Methods

//		{
//			"button-pressed": "okie",
//			"caller": "caller"
//		}

		protected override void ParseAlertDialogDismissedData (IDictionary _dataDict, out string _buttonPressed, out string _callerTag)
		{
			_buttonPressed	= _dataDict["button-pressed"] as string;
			_callerTag		= _dataDict["caller"] as string;
		}

		#endregion
	}
}
#endif