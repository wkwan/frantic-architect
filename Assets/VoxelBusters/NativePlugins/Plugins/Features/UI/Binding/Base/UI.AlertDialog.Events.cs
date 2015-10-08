using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class UI : MonoBehaviour 
	{
		#region Delegates
	
		/// <summary>
		/// Use this delegate type to get callback when the user clicks a button on an alert dialog.
		/// </summary>
		/// <param name="_buttonPressed">Title of the button that was pressed.</param>
		public delegate void AlertDialogCompletion (string _buttonPressed);

		#endregion

		#region Events
		
		private Dictionary<string, AlertDialogCompletion> 	m_alertDialogCallbackCollection	= new Dictionary<string, AlertDialogCompletion>();

		#endregion

		#region Native Callback Methods

		private void AlertDialogClosed (string _jsonStr)
		{
			IDictionary _jsonData	= JSONUtility.FromJSON(_jsonStr) as IDictionary;
			string _buttonPressed;
			string _callerTag;

			// Parse received data
			ParseAlertDialogDismissedData(_jsonData, out _buttonPressed, out _callerTag);
			Console.Log(Constants.kDebugTag, "[UI] Alert dialog closed, ButtonPressed=" + _buttonPressed);
			
			// Get callback
			AlertDialogCompletion _alertCompletionCallback	= GetAlertDialogCallback(_callerTag);
			
			// Completion callback is triggered
			if (_alertCompletionCallback != null)
				_alertCompletionCallback(_buttonPressed);
		}

		#endregion

		#region Parse Methods

		protected virtual void ParseAlertDialogDismissedData (IDictionary _dataDict, out string _buttonPressed, out string _callerTag)
		{
			_buttonPressed	= null;
			_callerTag		= null;
		}

		#endregion
	
		#region Callback Handler Methods
		
		private string CacheAlertDialogCallback (AlertDialogCompletion _newCallback)
		{
			if (_newCallback != null)
			{
				string _tag								= NPBinding.Utility.GetUUID();
				m_alertDialogCallbackCollection[_tag]	= _newCallback;

				return _tag;
			}

			return string.Empty;
		}

		protected AlertDialogCompletion GetAlertDialogCallback (string _tag)
		{
			if (!string.IsNullOrEmpty(_tag))
			{
				if (m_alertDialogCallbackCollection.ContainsKey(_tag))
				{
					return m_alertDialogCallbackCollection[_tag] as AlertDialogCompletion;
				}
			}

			return null;
		}
		
		#endregion
	}
}