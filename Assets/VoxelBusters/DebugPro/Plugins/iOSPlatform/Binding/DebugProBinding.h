//
//  DebugProBinding.h
//  Unity-iPhone
//
//  Created by Ashwin kumar on 14/03/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

enum eConsoleLogType
{
	ERROR		= 1 << 0,
	ASSERT		= 1 << 1,
	WARNING		= 1 << 2,
	INFO		= 1 << 3,
	EXCEPTION	= 1 << 4
};
typedef enum eConsoleLogType eConsoleLogType;
	
UIKIT_EXTERN void debugProLogMessage (const char* message, eConsoleLogType type, const char* stacktrace);