using UnityEngine;
using System.Collections;
using VoxelBusters.Utility;
using VoxelBusters.DebugPRO;

namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class AddressBook : MonoBehaviour 
	{
		#region Delegates

		/// <summary>
		/// Use this delegate type to get callback when reading contacts information completes.
		/// </summary>
		/// <param name="_authorizationStatus"> Authorization status for address book access .</param>
		/// <param name="_contactList"> List of contacts read from address book database.</param>
		public delegate void ReadContactsCompletion (eABAuthorizationStatus _authorizationStatus, AddressBookContact[] _contactList);

		#endregion

		#region Events
		
		protected ReadContactsCompletion		OnReadContactsFinished;
		
		#endregion

		#region Callback Methods

		private void ABReadContactsFinished (string _contactsJsonStr)
		{	
			IList _contactsJsonList				= JSONUtility.FromJSON(_contactsJsonStr) as IList;
			int _count							= _contactsJsonList.Count;
			AddressBookContact[] _contactsList	= new AddressBookContact[_count];
			
			for (int _iter = 0; _iter < _count; _iter++)
			{
				AddressBookContact _contact;
				IDictionary _contactInfoDict	= _contactsJsonList[_iter] as IDictionary;
				
				// Parse native data
				ParseContactData(_contactInfoDict, out _contact);
				
				// Add parsed object 
				_contactsList[_iter]			= _contact;
			}

			// Triggers event
			ABReadContactsFinished(_contactsList);
		}

		private void ABReadContactsFinished (AddressBookContact[] _contactsList)
		{
			Console.Log(Constants.kDebugTag, "[AddressBook] Reading contacts finished, Status=" + eABAuthorizationStatus.AUTHORIZED + " " + "Read contacts count=" + _contactsList.Length);
			
			// Invoke callback
			if (OnReadContactsFinished != null)
				OnReadContactsFinished(eABAuthorizationStatus.AUTHORIZED, _contactsList);
		}
		
		private void ABReadContactsFailed (string _statusStr)
		{
			eABAuthorizationStatus _authStatus;

			// Parse data
			ParseAuthorizationStatusData(_statusStr, out _authStatus);
			Console.Log(Constants.kDebugTag, "[AddressBook] Reading contacts failed, AuthorizationStatus=" + _authStatus + "Contacts=" + "NULL");

			// Invoke callback
			if (OnReadContactsFinished != null)
				OnReadContactsFinished(_authStatus, null);
		}

		#endregion

		#region Parsing Methods

		protected virtual void ParseContactData (IDictionary _contactInfoDict, out AddressBookContact _contact)
		{
			_contact	= null;
		}

		protected virtual void ParseAuthorizationStatusData (string _statusStr, out eABAuthorizationStatus _authStatus)
		{
			_authStatus	= eABAuthorizationStatus.RESTRICTED;
		}

		#endregion
	}
}