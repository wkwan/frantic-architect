using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class UIEditor : UI 
	{
		#region Prompt Dialog API's

		protected override void ShowSingleFieldPromptDialog (string _title, string _message, string _placeholder, bool _useSecureText, string[] _buttonsList, SingleFieldPromptCompletion _onCompletion)
		{
			base.ShowSingleFieldPromptDialog (_title, _message, _placeholder, _useSecureText, _buttonsList, _onCompletion);
						
			EditorUIHandler.Instance.ShowSingleFieldPromptDialog (_title, _message, _placeholder, _useSecureText, _buttonsList, GetGUISkin());
		}
		
		public override void ShowLoginPromptDialog (string _title, string _message, string _placeholder1, string _placeholder2, string[] _buttonsList, LoginPromptCompletion _onCompletion)
		{
			base.ShowLoginPromptDialog (_title, _message, _placeholder1, _placeholder2, _buttonsList, _onCompletion);
			
			EditorUIHandler.Instance.ShowLoginPromptDialog (_title, _message, _placeholder1, _placeholder2, _buttonsList, GetGUISkin());
		}
		
		#endregion
	}
}
#endif