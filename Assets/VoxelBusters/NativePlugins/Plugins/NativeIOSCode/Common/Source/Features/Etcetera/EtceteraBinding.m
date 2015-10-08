//
//  EtceteraBinding.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 10/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "EtceteraBinding.h"

char* getUUID ()
{
	// Better to own the object
	const char *uuid	= [[Utility GetUUID] UTF8String];
	
	// Return c value
	return CStringCopy(uuid);
}

void setApplicationIconBadgeNumber (int badgeNumber)
{
	[[UIApplication sharedApplication] setApplicationIconBadgeNumber:badgeNumber];
}