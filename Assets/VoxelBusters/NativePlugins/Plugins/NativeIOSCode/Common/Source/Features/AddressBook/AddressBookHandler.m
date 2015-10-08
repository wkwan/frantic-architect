//
//  AddressBookHandler.m
//  Unity-iPhone
//
//  Created by Ashwin kumar on 10/12/14.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "AddressBookHandler.h"
#import "NSData+UIImage.h"

@implementation AddressBookHandler

#define kABReadContactsFinished		"ABReadContactsFinished"
#define kABReadContactsFailed 		"ABReadContactsFailed"

#pragma mark contacts

- (ABAuthorizationStatus)getAuthorizationStatus
{
	return ABAddressBookGetAuthorizationStatus();
}

- (void)readContacts
{
    CFErrorRef *error               = nil;
    ABAddressBookRef addressBook    = ABAddressBookCreateWithOptions(NULL, error);
	
	// For iOS 6+
	if (ABAddressBookRequestAccessWithCompletion != NULL)
    {
		ABAuthorizationStatus authorisationStatus	= ABAddressBookGetAuthorizationStatus();
		NSLog(@"[AddressBookHandler] authorisation status %ld", (long)authorisationStatus);
		
		// Not determined
		if (authorisationStatus == kABAuthorizationStatusNotDetermined)
		{
			// Retain addressbook reference
			ABAddressBookRequestAccessWithCompletion(CFRetain(addressBook), ^(bool granted, CFErrorRef error) {
				
				// Read contacts only if permission is granted
				if (granted)
					[self onReadPermissionGranted:addressBook];
				else
					[self onReadPermissionDenied:ABAddressBookGetAuthorizationStatus()];
				
				// Release addressbook reference
				CFRelease(addressBook);
			});
		}
		// Authorised
		else if (authorisationStatus == kABAuthorizationStatusAuthorized)
		{
			[self onReadPermissionGranted:addressBook];
		}
		// Not allowed to access
		else if (authorisationStatus == kABAuthorizationStatusDenied || authorisationStatus == kABAuthorizationStatusRestricted)
		{
			[self onReadPermissionDenied:authorisationStatus];
		}
    }
    // For iOS 5 and older
    else
    {
		NSLog(@"[AddressBookHandler] skipping authorisation");
		[self onReadPermissionGranted:addressBook];
    }
	
	// Release
	if (addressBook)
		CFRelease(addressBook);
}


#define kLastName			@"last-name"
#define kImagePath			@"image-path"
#define kFirstName			@"first-name"
#define kPhoneNumList		@"phone-number-list"
#define kEmailIDList		@"emailID-list"

- (void)onReadPermissionGranted:(ABAddressBookRef)addressBook
{
	NSLog(@"[AddressBookHandler] permission granted to read contacts");
	CFArrayRef allPeople            = ABAddressBookCopyArrayOfAllPeopleInSourceWithSortOrdering(addressBook, nil, kABPersonFirstNameProperty);
	CFIndex totalContacts           = CFArrayGetCount(allPeople);
	NSMutableArray *contactsList    = [NSMutableArray arrayWithCapacity:totalContacts];
	
	for (int iter = 0; iter < totalContacts; iter++)
	{
		ABRecordRef person                      = CFArrayGetValueAtIndex(allPeople, iter);
		NSMutableDictionary *eachContactData    = [NSMutableDictionary dictionary];

		NSString *firstName 					= CFBridgingRelease(ABRecordCopyValue(person, kABPersonFirstNameProperty));
		NSString *lastName  					= CFBridgingRelease(ABRecordCopyValue(person, kABPersonLastNameProperty));

		if (firstName)
		{
			eachContactData[kFirstName]	= firstName;
		}
		
		if (lastName)
		{
			eachContactData[kLastName]	= lastName;
		}
		
		// Contacts image
		CFDataRef imageDataRef			= ABPersonCopyImageData(person);
		NSString* contactImgPath 		= kNSStringDefault;
		
		if (imageDataRef)
		{
			NSData *imageData			= (NSData *)imageDataRef;
			
			// Save image to documents
			contactImgPath  			= [imageData saveImage];
			
			// Release
			CFRelease(imageDataRef);
		}
		
		// Add image path to the contact info dictionary
		eachContactData[kImagePath]		= contactImgPath;
		
		// Get phone number
		NSMutableArray *phoneNumbers    = [NSMutableArray array];
		ABMultiValueRef multiPhoneRef   = ABRecordCopyValue(person, kABPersonPhoneProperty);
		CFIndex phoneNumberCount        = ABMultiValueGetCount(multiPhoneRef);
		
		for (CFIndex pIter = 0; pIter < phoneNumberCount; pIter++)
		{
			CFStringRef phoneNumberRef  = ABMultiValueCopyValueAtIndex(multiPhoneRef, pIter);
			NSString *phoneNumber		= (NSString *)phoneNumberRef;
			
			if (phoneNumber != NULL)
			{
				NSString *formattedNo	= [phoneNumber stringByReplacingOccurrencesOfString:@"[^0-9]"
																				 withString:@""
																					options:NSRegularExpressionSearch
																					  range:NSMakeRange(0, [(NSString *)phoneNumberRef length])];

				// Add phone#
				[phoneNumbers addObject:formattedNo];
				
				// Release
				CFRelease(phoneNumberRef);
			}
		}
		
		// Add phone list to contact info dictionary
		eachContactData[kPhoneNumList]   = phoneNumbers;
		
		// Release
		if (multiPhoneRef)
			CFRelease(multiPhoneRef);
		
		// Get email address
		NSMutableArray *contactEmails   = [NSMutableArray array];
		ABMultiValueRef multiEmailRef   = ABRecordCopyValue(person, kABPersonEmailProperty);
		CFIndex emaildIdCount			= ABMultiValueGetCount(multiEmailRef);
		
		for (CFIndex i = 0; i < emaildIdCount; i++)
		{
			CFStringRef contactEmailRef = ABMultiValueCopyValueAtIndex(multiEmailRef, i);
			NSString *contactEmail      = (NSString *)contactEmailRef;
			
			if (contactEmail != NULL)
			{
				// Add email id
				[contactEmails addObject:contactEmail];
				
				// Release
				CFRelease(contactEmailRef);
			}
		}
		
		// Add email list to contact info dictionary
		eachContactData[kEmailIDList]	= contactEmails;
		
		// Release
		if (multiEmailRef)
			CFRelease(multiEmailRef);
		
		// Add contact info to the list
		[contactsList addObject:eachContactData];
	}
	
	// Release
	CFRelease(allPeople);
	
	// Notify unity
	NSLog(@"[AddressBookHandler] fetched %lu contacts", (unsigned long)[contactsList count]);
	NotifyEventListener(kABReadContactsFinished, ToJsonCString(contactsList));
}

- (void)onReadPermissionDenied:(ABAuthorizationStatus)authorisationStatus
{
	NSLog(@"[AddressBookHandler] permission denied to read contacts");
	const char* status	= [[NSString stringWithFormat:@"%ld", (long)authorisationStatus] UTF8String];
	
	// Notify unity
	NotifyEventListener(kABReadContactsFailed, status);
}

@end
