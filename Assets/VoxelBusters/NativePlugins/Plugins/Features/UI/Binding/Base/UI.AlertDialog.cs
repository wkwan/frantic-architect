using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VoxelBusters.NativePlugins
{
	public partial class UI : MonoBehaviour 
	{
		#region Constants
			
		private const string kDefaultTextForButton = "Ok";

		#endregion

		#region Alert Dialog API's

		/// <summary>
		/// Shows the alert dialog with single button.
		/// </summary>
		/// <param name="_title">Text that appears in title bar.</param>
		/// <param name="_message">Descriptive text that provides more details.</param>
		/// <param name="_button">Title of action button.</param>
		/// <param name="_onCompletion">Calls the delegate when action button is pressed.</param>
		public void ShowAlertDialogWithSingleButton (string _title, string _message, string _button, AlertDialogCompletion _onCompletion)
		{
			ShowAlertDialogWithMultipleButtons(_title, _message, new string[] {_button }, _onCompletion); 
		}

		/// <summary>
		/// Shows the alert dialog with multiple buttons.
		/// </summary>
		/// <param name="_title">Text that appears in title bar.</param>
		/// <param name="_message">Descriptive text that provides more details.</param>
		/// <param name="_buttonsList">Title of action buttons.</param>
		/// <param name="_onCompletion">Calls the delegate when action button is pressed.</param>
		public void ShowAlertDialogWithMultipleButtons (string _title, string _message, string[] _buttonsList, AlertDialogCompletion _onCompletion)
		{
			// Cache callback
			string _callbackTag	= CacheAlertDialogCallback(_onCompletion);

			// Show alert
			ShowAlertDialogWithMultipleButtons(_title, _message, _buttonsList, _callbackTag);
		}

		protected virtual void ShowAlertDialogWithMultipleButtons (string _title, string _message, string[] _buttonsList, string _callbackTag)
		{
			if(_buttonsList == null || _buttonsList.Length == 0)
			{
				_buttonsList = new string[]{kDefaultTextForButton}; //Adding default text
			}
			else if(string.IsNullOrEmpty(_buttonsList[0]))
			{
				_buttonsList[0] = kDefaultTextForButton;
			}
		}

		#endregion
	}
}