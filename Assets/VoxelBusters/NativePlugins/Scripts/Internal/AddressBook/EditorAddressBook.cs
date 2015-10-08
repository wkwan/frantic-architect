using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;

#if UNITY_EDITOR
namespace VoxelBusters.NativePlugins.Internal
{
	public class EditorAddressBook : AdvancedScriptableObject <EditorAddressBook>
	{
		#region Properties
		
		[SerializeField]
		private				eABAuthorizationStatus		m_authorizationStatus;
		public 				eABAuthorizationStatus		AuthorizationStatus
		{
			get 
			{
				return m_authorizationStatus;
			}
			private set
			{
				m_authorizationStatus	= value;
			}
		}

		[SerializeField]
		private 			AddressBookContact[] 		m_contactsList					= new AddressBookContact[0];
		public 				AddressBookContact[]		ContactsList
		{
			get 
			{
				return m_contactsList;
			}
		}

		#endregion

		#region Constants

		// Event callbacks
		private const string			kABReadContactsFinishedEvent	= "ABReadContactsFinished";
		private const string			kABReadContactsFailedEvent		= "ABReadContactsFailed";

		#endregion

		#region Static Methods

		public static eABAuthorizationStatus GetAuthorizationStatus ()
		{
			return Instance.AuthorizationStatus;
		}

		public static void ReadContacts ()
		{
			if (NPBinding.AddressBook == null)
				return;

			eABAuthorizationStatus 	_authStatus		= GetAuthorizationStatus();

			if (_authStatus == eABAuthorizationStatus.DENIED || _authStatus == eABAuthorizationStatus.RESTRICTED)
			{
				// Send contacts read failed event
				NPBinding.AddressBook.InvokeMethod(kABReadContactsFailedEvent, ((int)_authStatus).ToString());
				return;
			}
			else if (_authStatus == eABAuthorizationStatus.NOT_DETERMINED)
			{
				string 				_message		= string.Format("{0} would like to access your contacts.", UnityEditor.PlayerSettings.productName);	
				string[]			_buttons		= new string[2] { "Ok", "Dont allow" };

				NPBinding.UI.ShowAlertDialogWithMultipleButtons(string.Empty, _message, _buttons, (string _pressedBtn)=>{

					if (_pressedBtn.Equals("Ok"))
					{
						Instance.AuthorizationStatus	= eABAuthorizationStatus.AUTHORIZED;

						// Read contacts
						OnAuthorizedToReadContacts();
					}
					else
					{
						Instance.AuthorizationStatus	= eABAuthorizationStatus.DENIED;

						// Send contacts read failed event
						NPBinding.AddressBook.InvokeMethod(kABReadContactsFailedEvent, ((int)Instance.AuthorizationStatus).ToString());
					}
				});
			}
			else
			{
				// Read contacts
				OnAuthorizedToReadContacts();
			}

		}

		private static void OnAuthorizedToReadContacts ()
		{
			AddressBookContact[] _contactsList		= Instance.ContactsList;
			int _totalContacts						= _contactsList.Length;
			AddressBookContact[] _contactsListCopy	= new AddressBookContact[_totalContacts];
			
			for (int _iter = 0; _iter < _totalContacts; _iter++)
			{
				_contactsListCopy[_iter]			= new EditorAddressBookContact(_contactsList[_iter]);
			}
			
			// Callback is sent to binding event listener
			NPBinding.AddressBook.InvokeMethod(kABReadContactsFinishedEvent, _contactsListCopy);
		}

		#endregion
	}
}
#endif