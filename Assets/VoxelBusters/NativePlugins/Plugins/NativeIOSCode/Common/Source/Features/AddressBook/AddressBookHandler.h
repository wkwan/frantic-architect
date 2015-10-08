//
//  AddressBookHandler.h
//  Unity-iPhone
//
//  Created by Ashwin kumar on 10/12/14.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <AddressBook/AddressBook.h>
#import "HandlerBase.h"

@interface AddressBookHandler : HandlerBase

// Requesting for contacts info
- (ABAuthorizationStatus)getAuthorizationStatus;
- (void)readContacts;

@end
