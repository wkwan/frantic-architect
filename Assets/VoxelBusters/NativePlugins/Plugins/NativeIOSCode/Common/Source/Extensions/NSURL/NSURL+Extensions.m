//
//  NSURL+Extensions.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 21/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "NSURL+Extensions.h"

@implementation NSURL (Extensions)

+ (NSURL *)createURLWithString:(NSString *)URLString
{
	if ([URLString hasPrefix:@"file://"])
		return [NSURL fileURLWithPath:URLString];
	else
		return [NSURL URLWithString:URLString];
}

@end