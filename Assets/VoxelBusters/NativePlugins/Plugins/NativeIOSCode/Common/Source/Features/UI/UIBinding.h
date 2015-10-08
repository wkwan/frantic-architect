//
//  UIBinding.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 11/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

// Related to UI Dialogs
UIKIT_EXTERN void showAlertDialog (const char* title, 	const char* message,
								   const char* buttons, const char* callerTag);
UIKIT_EXTERN void showSingleFieldPromptDialog (const char* title, 		const char* message,
											   const char* placeholder, bool useSecureText,
											   const char* buttons);
UIKIT_EXTERN void showLoginPromptDialog (const char* title, 		const char* message,
										 const char* placeholder1, 	const char* placeholder2,
										 const char* buttons);

// Related to PopOver's
UIKIT_EXTERN void setPopoverPoint (float x, float y);