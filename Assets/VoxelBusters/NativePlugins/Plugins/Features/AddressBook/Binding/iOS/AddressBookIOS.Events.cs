using UnityEngine;
using System.Collections;
using System;

#if UNITY_IOS
namespace VoxelBusters.NativePlugins
{
	using Internal;

	public partial class AddressBookIOS : AddressBook 
	{
		private enum iOSABAuthorizationStatus
		{
			kABAuthorizationStatusNotDetermined = 0,
			kABAuthorizationStatusRestricted,
			kABAuthorizationStatusDenied,
			kABAuthorizationStatusAuthorized
		};

		#region Parsing Methods

		protected override void ParseContactData (IDictionary _contactInfoDict, out AddressBookContact _contact)
		{
			_contact								= new iOSAddressBookContact(_contactInfoDict);
		}

		protected override void ParseAuthorizationStatusData (string _statusStr, out eABAuthorizationStatus _authStatus)
		{
			iOSABAuthorizationStatus _iOSAuthStatus	= ((iOSABAuthorizationStatus)int.Parse(_statusStr));

			// Set status
			_authStatus								= ConvertFromNativeAuthorizationStatus(_iOSAuthStatus);
		}

		private eABAuthorizationStatus ConvertFromNativeAuthorizationStatus (iOSABAuthorizationStatus _iOSAuthStatus)
		{
			switch (_iOSAuthStatus)
			{
			case iOSABAuthorizationStatus.kABAuthorizationStatusNotDetermined:
				return eABAuthorizationStatus.NOT_DETERMINED;

			case iOSABAuthorizationStatus.kABAuthorizationStatusRestricted:
				return eABAuthorizationStatus.RESTRICTED;

			case iOSABAuthorizationStatus.kABAuthorizationStatusDenied:
				return eABAuthorizationStatus.DENIED;

			case iOSABAuthorizationStatus.kABAuthorizationStatusAuthorized:
				return eABAuthorizationStatus.AUTHORIZED;

			default:
				throw new Exception("[AddressBook] Unsupported status.");
			}
		}

		#endregion
	}
}
#endif