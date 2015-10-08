using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class UIEditor : UI 
	{
		#region Alert Dialog API's
		
		protected override void ShowAlertDialogWithMultipleButtons (string _title, string _message, string[] _buttonsList, string _callbackTag)
		{
			base.ShowAlertDialogWithMultipleButtons(_title, _message, _buttonsList, _callbackTag);

			//Use GUI for Alerts
			EditorUIHandler.Instance.ShowAlertDialogWithMultipleButtons(_title, _message, _buttonsList, _callbackTag, GetGUISkin());
		}
		
		#endregion
	}
}
#endif