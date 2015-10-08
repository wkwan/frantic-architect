using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins.Internal
{
	public class EditorUIHandler
	{
		#region Constants

		// Event callbacks
		private const string kAlertDialogClosedEvent				= "AlertDialogClosed";
		private const string kSingleFieldPromptDialogClosedEvent	= "SingleFieldPromptDialogClosed";
		private const string kLoginPromptDialogClosedEvent			= "LoginPromptDialogClosed";

		// Data keys
		public const string kButtonPressed 							= "button-pressed";
		public const string kCaller 								= "caller";
		public const string kInput 									= "input";
		public const string kUserName 								= "username";
		public const string kPassword 								= "password";
		
		#endregion

		#region Singleton Instance

		private static EditorUIHandler m_instance = null;
		public static EditorUIHandler Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new EditorUIHandler();
				}

				return m_instance;
			}
		}

		private EditorUIHandler()
		{}

		#endregion

		#region Alert Dialog Methods
		
		public void ShowAlertDialogWithMultipleButtons (string _title, string _message, string[] _buttonsList, string _callbackTag, GUISkin _skin)
		{
			GUIAlertDialog _dialog = GUIAlertDialog.Create(_title, _message, _buttonsList, AlertDialogCallback);

			_dialog.UISkin 			= _skin;
			_dialog.Tag				= _callbackTag;	

			_dialog.Show();
		}

		#endregion

		#region Prompt Dialog Methods
		
		public void ShowSingleFieldPromptDialog (string _title, string _message, string _placeholder, bool _useSecureText, string[] _buttonsList, GUISkin _skin)
		{
			GUIPromptDialog.InputFieldElement[] _inputFields = new GUIPromptDialog.InputFieldElement[1];
 
			_inputFields[0] 			= new GUIPromptDialog.InputFieldElement();
			_inputFields[0].Set(_placeholder, _useSecureText, true);

			GUIPromptDialog _dialog 	= GUIPromptDialog.Create(_title, _message, _inputFields, _buttonsList, SingleFieldPromptCallback);
			_dialog.UISkin 				= _skin;
			
			_dialog.Show();
		}

		public void ShowLoginPromptDialog (string _title, string _message, string _placeholder1, string _placeholder2, string[] _buttonsList, GUISkin _skin)
		{
			GUIPromptDialog.InputFieldElement[] _inputFields = new GUIPromptDialog.InputFieldElement[2];
			
			_inputFields[0] = new GUIPromptDialog.InputFieldElement();
			_inputFields[0].Set(_placeholder1);

			_inputFields[1] = new GUIPromptDialog.InputFieldElement(); //Username
			_inputFields[1].Set(_placeholder2, true); //Password

			GUIPromptDialog _dialog 	= GUIPromptDialog.Create(_title, _message, _inputFields, _buttonsList, LoginPromptCallback);
			_dialog.UISkin 				= _skin;
			
			_dialog.Show();
		}

		#endregion

		#region Callbacks

		void AlertDialogCallback(string _selectedButton, string _callerTag)
		{
			Dictionary<string, string> _dataDict 	= new Dictionary<string, string>();
			_dataDict[kButtonPressed] 				= _selectedButton;
			_dataDict[kCaller]						= _callerTag;

			if (NPBinding.UI != null)
				NPBinding.UI.InvokeMethod(kAlertDialogClosedEvent, _dataDict.ToJSON());
		}

		void SingleFieldPromptCallback(string _selectedButton, GUIPromptDialog.InputFieldElement[] _inputList)
		{
			Dictionary<string, string> _dataDict 	= new Dictionary<string, string>();
			_dataDict[kButtonPressed] 				= _selectedButton;
		
			if(_inputList != null && _inputList[0] != null)
			{
				_dataDict[kInput]	= _inputList[0].GetCurrentText();
			}
			
			if (NPBinding.UI != null)
				NPBinding.UI.InvokeMethod(kSingleFieldPromptDialogClosedEvent, _dataDict.ToJSON());
		}


		void LoginPromptCallback(string _selectedButton, GUIPromptDialog.InputFieldElement[] _inputList)
		{
			Dictionary<string, string> _dataDict	= new Dictionary<string, string>();

			_dataDict[kButtonPressed] 				= _selectedButton;
			
			//Adding some default text here
			_dataDict[kUserName] 					= "";
			_dataDict[kPassword] 					= "";

			if(_inputList != null)
			{
				if(_inputList[0] != null)
				{	
					_dataDict[kUserName]	= _inputList[0].GetCurrentText();
				}

				if(_inputList[1] != null)
				{
					_dataDict[kPassword]	= _inputList[1].GetCurrentText();
				}
			}

			if (NPBinding.UI != null)
				NPBinding.UI.InvokeMethod(kLoginPromptDialogClosedEvent, _dataDict.ToJSON());
		}
			
		#endregion
	}
}
#endif