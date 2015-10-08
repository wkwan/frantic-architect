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
//			"input": "text field one"
//		}

		protected override void ParseSingleFieldPromptClosedData (IDictionary _dataDict, out string _buttonPressed, out string _inputText)
		{
			_buttonPressed	= _dataDict[EditorUIHandler.kButtonPressed] as string;
			_inputText		= _dataDict[EditorUIHandler.kInput] as string;
		}

//		{
//			"button-pressed": "okie",
//			"username": "username",
//			"password": "password"
//		}

		protected override void ParseLoginPromptClosedData (IDictionary _dataDict, out string _buttonPressed, out string _usernameText, out string _passwordText)
		{
			_buttonPressed	= _dataDict[EditorUIHandler.kButtonPressed] as string;
			_usernameText	= _dataDict[EditorUIHandler.kUserName] as string;
			_passwordText	= _dataDict[EditorUIHandler.kPassword] as string;
		}

		#endregion
	}
}
#endif