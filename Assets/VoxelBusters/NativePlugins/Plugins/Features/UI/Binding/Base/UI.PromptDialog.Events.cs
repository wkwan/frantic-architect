using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class UI : MonoBehaviour 
	{
		#region Delegates

		/// <summary>
		/// Use this delegate type to get callback when the user clicks a button on single field prompt dialog.
		/// </summary>
		/// <param name="_buttonPressed">Title of the button that was pressed.</param>
		/// <param name="_inputText">Text contents of textfield.</param>
		public delegate void SingleFieldPromptCompletion (string _buttonPressed, string _inputText);

		/// <summary>
		/// Use this delegate type to get callback when the user clicks a button on login prompt dialog.
		/// </summary>
		/// <param name="_buttonPressed">Title of the button that was pressed.</param>
		/// <param name="_usernameText">Text contents of login textfield.</param>
		/// <param name="_passwordText">Text contents of password textfield.</param>
		public delegate void LoginPromptCompletion (string _buttonPressed, string _usernameText, string _passwordText);
		
		#endregion
		
		#region Events
		
		protected SingleFieldPromptCompletion				OnSingleFieldPromptClosed;
		protected LoginPromptCompletion						OnLoginPromptClosed;
		
		#endregion
		
		#region Native Callback Methods
		
		private void SingleFieldPromptDialogClosed (string _jsonStr)
		{
			Console.Log(Constants.kDebugTag, "[UI] Single field prompt was dismissed");
			
			if (OnSingleFieldPromptClosed != null)
			{
				IDictionary _dataDict	= JSONUtility.FromJSON(_jsonStr) as IDictionary;
				string _buttonPressed;
				string _inputText;
				
				// Parse received data
				ParseSingleFieldPromptClosedData(_dataDict, out _buttonPressed, out _inputText);
				
				// Completion callback is triggered
				OnSingleFieldPromptClosed(_buttonPressed, _inputText);
			}
		}
		
		private void LoginPromptDialogClosed (string _jsonStr)
		{
			Console.Log(Constants.kDebugTag, "[UI] Login prompt was dismissed");
			
			if (OnLoginPromptClosed != null)
			{
				IDictionary _jsonData	= JSONUtility.FromJSON(_jsonStr) as IDictionary;
				string _buttonPressed;
				string _usernameText;
				string _passwordText;
				
				// Parse received data
				ParseLoginPromptClosedData(_jsonData, out _buttonPressed, out _usernameText, out _passwordText);
				
				// Completion callback is triggered
				OnLoginPromptClosed(_buttonPressed, _usernameText, _passwordText);
			}
		}
		
		#endregion

		#region Parse Methods

		protected virtual void ParseSingleFieldPromptClosedData (IDictionary _dataDict, out string _buttonPressed, out string _inputText)
		{
			_buttonPressed	= null;
			_inputText		= null;
		}

		protected virtual void ParseLoginPromptClosedData (IDictionary _dataDict, out string _buttonPressed, out string _usernameText, out string _passwordText)
		{
			_buttonPressed	= null;
			_usernameText	= null;
			_passwordText	= null;
		}

		#endregion
	}
}