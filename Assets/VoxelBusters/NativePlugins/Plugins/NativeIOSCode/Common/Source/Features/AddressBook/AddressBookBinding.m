//
//  AddressBookBinding.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 10/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "AddressBookBinding.h"
#import "AddressBookHandler.h"

int getAuthorizationStatus ()
{
	return (int)[[AddressBookHandler Instance] getAuthorizationStatus];
}

void readContacts ()
{
    [[AddressBookHandler Instance] readContacts];
}
