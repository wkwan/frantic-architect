using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins.Internal
{
	public sealed class AndroidAddressBookContact : AddressBookContact
	{
//		{
//			"emailID-list": [
//			    "joey@actingclass.com"
//			    ],
//			"image-path": "/storage/emulated/0/Android/data/com.company.product/files/1.png",
//			"last-name": "Joey",
//			"phone-number-list": [
//			    "911"
//			    ],
//			"first-name": "Tribbiani"
//		}

		#region Constants

		private const string	kDisplayName		= "display-name";
		private const string	kFamilyName			= "family-name";
		private const string	kGivenName			= "given-name";
		private const string	kImagePath			= "image-path";
		private const string	kPhoneNumList		= "phone-number-list";
		private const string	kEmailList			= "email-list";
		
		#endregion

		#region Constructors

		public AndroidAddressBookContact (IDictionary _contactInfoJsontDict)
		{
			//If Given Name is available set it. Else pick from Display Name
			string _displayName = _contactInfoJsontDict.GetIfAvailable<string>(kDisplayName);
			string _givenName 	= _contactInfoJsontDict.GetIfAvailable<string>(kGivenName);
			string _familyName 	= _contactInfoJsontDict.GetIfAvailable<string>(kFamilyName);
		
			if(string.IsNullOrEmpty(_displayName))
			{
				_displayName = "";
			}

			FirstName 		= _givenName;
			LastName 		= _familyName;
			ImagePath		= _contactInfoJsontDict[kImagePath] as string;
			PhoneNumberList	= new List<string>();
			EmailIDList		= new List<string>();
			
			// Add phone numbers
			IList _phoneNumJsonList	= _contactInfoJsontDict[kPhoneNumList] as IList;
			foreach (string _phoneNo in _phoneNumJsonList)
				PhoneNumberList.Add(_phoneNo);
			
			// Add email id's
			IList _emailIDJsonList	= _contactInfoJsontDict[kEmailList] as IList;
			foreach (string _emailID in _emailIDJsonList)
				EmailIDList.Add(_emailID);
		}

		#endregion
	}
}