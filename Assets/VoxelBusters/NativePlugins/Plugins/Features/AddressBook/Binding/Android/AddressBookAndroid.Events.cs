using UnityEngine;
using System.Collections;
using VoxelBusters.DebugPRO;

#if UNITY_ANDROID
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class AddressBookAndroid : AddressBook 
	{
		#region Parsing constants

		//Access status flags
		private const string	kAccessAuthorized	= "authorized";
		private const string	kAccessDenied		= "denied";
		private const string	kAccessRestricted	= "restricted";
		
		#endregion

		#region Parsing Methods

		protected override void ParseContactData (IDictionary _contactInfoDict, out AddressBookContact _contact)
		{
			_contact	= new AndroidAddressBookContact(_contactInfoDict);
		}

		protected override void ParseAuthorizationStatusData (string _statusStr, out eABAuthorizationStatus _authStatus)
		{
			if(kAccessAuthorized.Equals(_statusStr))
			{
				_authStatus = eABAuthorizationStatus.AUTHORIZED;
			}
			else if(kAccessDenied.Equals(_statusStr))
			{
				_authStatus = eABAuthorizationStatus.DENIED;
			}
			else if(kAccessRestricted.Equals(_statusStr))
			{
				_authStatus = eABAuthorizationStatus.RESTRICTED;
			}
			else
			{
				_authStatus = eABAuthorizationStatus.DENIED;
				Console.LogError(Constants.kDebugTag, "[AddressBook] Wrong parse status " + _statusStr + " " + "Cross check keys with native. Sending DENIED status by default");
			}
		}

		#endregion
	}
}
#endif