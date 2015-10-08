using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using VoxelBusters.Utility;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	public partial class UIIOS : UI
	{
		#region Native Methods
		
		[DllImport("__Internal")]
		private static extern void showAlertDialog (string _title, string _message, string _buttonsList, string _callerTag);

		#endregion

		#region Alert Dialog API's

		protected override void ShowAlertDialogWithMultipleButtons (string _title, string _message, string[] _buttonsList, string _callbackTag)
		{
			base.ShowAlertDialogWithMultipleButtons(_title, _message, _buttonsList, _callbackTag);

			// Native method is called
			showAlertDialog(_title, _message, _buttonsList.ToJSON(), _callbackTag);
		}
		
		#endregion
	}
}
#endif