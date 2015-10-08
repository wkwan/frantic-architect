using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	public partial class UIAndroid : UI 
	{
		#region Prompt Dialog API's

		protected override void ShowSingleFieldPromptDialog (string _title, string _message, string _placeholder, bool _useSecureText, string[] _buttonsList, SingleFieldPromptCompletion _onCompletion)
		{
			base.ShowSingleFieldPromptDialog (_title, _message, _placeholder, _useSecureText, _buttonsList, _onCompletion);

			// Show prompt
			Plugin.Call(NativeInfo.Methods.SHOW_SINGLE_FIELD_PROMPT, _title, _message, _placeholder, _useSecureText, _buttonsList.ToJSON());
		}
		
		public override void ShowLoginPromptDialog (string _title, string _message, string _placeholder1, string _placeholder2, string[] _buttonsList, LoginPromptCompletion _onCompletion)
		{
			base.ShowLoginPromptDialog (_title, _message, _placeholder1, _placeholder2, _buttonsList, _onCompletion);
			
			// Show prompt
			Plugin.Call(NativeInfo.Methods.SHOW_LOGIN_PROMPT, _title, _message, _placeholder1, _placeholder2, _buttonsList.ToJSON());
		}
		
		#endregion
	}
}
#endif