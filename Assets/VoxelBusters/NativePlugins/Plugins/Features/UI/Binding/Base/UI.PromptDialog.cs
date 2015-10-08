using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins
{
	public partial class UI : MonoBehaviour 
	{
		#region Prompt Dialog API's

		/// <summary>
		/// Prompt dialog that allows the user to enter text.
		/// </summary>
		/// <param name="_title">Text that appears in title bar.</param>
		/// <param name="_message">Descriptive text that provides more details.</param>
		/// <param name="_placeholder">String that is displayed when there is no other text in the textfield.</param>
		/// <param name="_buttonsList">Title of action buttons.</param>
		/// <param name="_onCompletion">Calls the delegate when action button is pressed.</param>
		public void ShowSingleFieldPromptDialogWithPlainText (string _title, string _message, string _placeholder, string[] _buttonsList, SingleFieldPromptCompletion _onCompletion)
		{
			ShowSingleFieldPromptDialog(_title, _message, _placeholder, false, _buttonsList, _onCompletion);
		}

		/// <summary>
		/// Prompt dialog that allows the user to enter text. The text field is obscured.
		/// </summary>
		/// <param name="_title">Text that appears in title bar.</param>
		/// <param name="_message">Descriptive text that provides more details.</param>
		/// <param name="_placeholder">String that is displayed when there is no other text in the textfield.</param>
		/// <param name="_buttonsList">Title of action buttons.</param>
		/// <param name="_onCompletion">Calls the delegate when action button is pressed.</param>
		public void ShowSingleFieldPromptDialogWithSecuredText (string _title, string _message, string _placeholder, string[] _buttonsList, SingleFieldPromptCompletion _onCompletion)
		{
			ShowSingleFieldPromptDialog(_title, _message, _placeholder, true, _buttonsList, _onCompletion);
		}
		
		protected virtual void ShowSingleFieldPromptDialog (string _title, string _message, string _placeholder, bool _useSecureText, string[] _buttonsList, SingleFieldPromptCompletion _onCompletion)
		{
			// Cache callback
			OnSingleFieldPromptClosed	= _onCompletion;
		}

		/// <summary>
		/// Prompt dialog that allows the user to enter login identifier and password.
		/// </summary>
		/// <param name="_title">Text that appears in title bar.</param>
		/// <param name="_message">Descriptive text that provides more details.</param>
		/// <param name="_usernamePlaceHolder">String that is displayed when there is no other text in the username textfield.</param>
		/// <param name="_passwordPlaceHolder">String that is displayed when there is no other text in the password textfield.</param>
		/// <param name="_buttonsList">Title of action buttons.</param>
		/// <param name="_onCompletion">Calls the delegate when action button is pressed.</param>
		public virtual void ShowLoginPromptDialog (string _title, string _message, string _usernamePlaceHolder, string _passwordPlaceHolder, string[] _buttonsList, LoginPromptCompletion _onCompletion)
		{
			// Cache callback
			OnLoginPromptClosed			= _onCompletion;
		}
		
		#endregion
	}
}