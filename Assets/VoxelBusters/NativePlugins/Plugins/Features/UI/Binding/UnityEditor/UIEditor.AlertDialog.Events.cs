using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class UIEditor : UI 
	{
		#region Parse Methods

//		{
//			"button-pressed": "okie",
//			"caller": "caller"
//		}

		protected override void ParseAlertDialogDismissedData (IDictionary _dataDict, out string _buttonPressed, out string _callerTag)
		{
			_buttonPressed	= _dataDict[EditorUIHandler.kButtonPressed] as string;
			_callerTag		= _dataDict[EditorUIHandler.kCaller] as string;
		}

		#endregion
	}
}
#endif