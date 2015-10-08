//
//  NetworkConnectivityBinding.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 11/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "NetworkConnectivityBinding.h"
#import "NetworkConnectivityHandler.h"

void setNewIPAddress (const char *newIPAddress)
{
	[[NetworkConnectivityHandler Instance] setNewIPAddress:ConvertToNSString(newIPAddress)];
}
