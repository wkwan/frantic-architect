using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelBusters.Utility;

namespace VoxelBusters.NativePlugins.Internal
{
	public sealed class iOSAddressBookContact : AddressBookContact 
	{
//		{
//			"emailID-list": [
//			    "joey@actingclass.com"
//			    ],
//			"image-path": "file://localhost/var/mobile/Applications/ED14DC3D-37C4-44D2-B750-CC595DE48D78/Documents/207D078C-5AE6-4B7F-BEA2-EE1045B27C82.jpg",
//			"last-name": "Joey",
//			"phone-number-list": [
//			    "911"
//			    ],
//			"first-name": "Tribbiani"
//		}

		#region Constants

		private const string 		kLastName			= "last-name";
		private const string 		kImagePath			= "image-path";		
		private const string 		kFirstName			= "first-name";
		private const string 		kPhoneNumList		= "phone-number-list";
		private const string 		kEmailIDList		= "emailID-list";
		
		#endregion

		#region Constructors

		public iOSAddressBookContact (IDictionary _contactInfoJsontDict)
		{
			FirstName		= _contactInfoJsontDict[kFirstName] as string;
			LastName		= _contactInfoJsontDict[kLastName] as string;
			ImagePath		= _contactInfoJsontDict[kImagePath] as string;
			PhoneNumberList	= new List<string>();
			EmailIDList		= new List<string>();
			
			// Add phone numbers
			IList _phoneNumJsonList	= _contactInfoJsontDict[kPhoneNumList] as IList;
			foreach (string _phoneNo in _phoneNumJsonList)
				PhoneNumberList.Add(_phoneNo);
			
			// Add email id's
			IList _emailIDJsonList	= _contactInfoJsontDict[kEmailIDList] as IList;
			foreach (string _emailID in _emailIDJsonList)
				EmailIDList.Add(_emailID);
		}

		#endregion
	}
}