using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	public partial class UIAndroid : UI 
	{
		#region Alert Dialog API's
		
		protected override void ShowAlertDialogWithMultipleButtons (string _title, string _message, string[] _buttonsList, string _callbackTag)
		{
			base.ShowAlertDialogWithMultipleButtons(_title, _message, _buttonsList, _callbackTag);

			// Native method is called
			Plugin.Call(NativeInfo.Methods.SHOW_ALERT_DIALOG, _title, _message, _buttonsList.ToJSON(), _callbackTag);
		}
		
		#endregion
	}
}
#endif