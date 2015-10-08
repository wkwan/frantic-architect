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
//			"input": "text field one"
//		}

		protected override void ParseSingleFieldPromptClosedData (IDictionary _dataDict, out string _buttonPressed, out string _inputText)
		{
			_buttonPressed	= _dataDict["button-pressed"] as string;
			_inputText		= _dataDict["input"] as string;
		}

//		{
//			"button-pressed": "okie",
//			"username": "username",
//			"password": "password"
//		}

		protected override void ParseLoginPromptClosedData (IDictionary _dataDict, out string _buttonPressed, out string _usernameText, out string _passwordText)
		{
			_buttonPressed	= _dataDict["button-pressed"] as string;
			_usernameText	= _dataDict["username"] as string;
			_passwordText	= _dataDict["password"] as string;
		}

		#endregion
	}
}
#endif