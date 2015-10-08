//
//  DebugProBinding.m
//  Unity-iPhone
//
//  Created by Ashwin kumar on 14/03/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "DebugProBinding.h"

#define ToNSString(cString) (cString == NULL) ? NULL : [NSString stringWithUTF8String:cString]

void debugProLogMessage (const char* message, eConsoleLogType type, const char* stacktrace)
{
	NSLog(@"%s\n%s", message, stacktrace);
}