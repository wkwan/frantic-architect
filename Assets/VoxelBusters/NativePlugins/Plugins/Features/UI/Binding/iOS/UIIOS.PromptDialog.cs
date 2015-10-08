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
		private static extern void showSingleFieldPromptDialog (string _title, string _message, string _placeholder, bool _useSecureText, string _buttonsList);
		
		[DllImport("__Internal")]
		private static extern void showLoginPromptDialog (string _title, string _message, string _placeholder1, string _placeholder2, string _buttonsList);

		#endregion
		
		#region Prompt Dialog API's

		protected override void ShowSingleFieldPromptDialog (string _title, string _message, string _placeholder, bool _useSecureText, string[] _buttonsList, SingleFieldPromptCompletion _onCompletion)
		{
			base.ShowSingleFieldPromptDialog (_title, _message, _placeholder, _useSecureText, _buttonsList, _onCompletion);
			
			// Show prompt
			showSingleFieldPromptDialog(_title, _message, _placeholder, _useSecureText, _buttonsList.ToJSON());
		}
		
		public override void ShowLoginPromptDialog (string _title, string _message, string _placeholder1, string _placeholder2, string[] _buttonsList, LoginPromptCompletion _onCompletion)
		{
			base.ShowLoginPromptDialog (_title, _message, _placeholder1, _placeholder2, _buttonsList, _onCompletion);
			
			// Show prompt
			showLoginPromptDialog(_title, _message, _placeholder1, _placeholder2, _buttonsList.ToJSON());
		}

		#endregion
	}
}
#endif