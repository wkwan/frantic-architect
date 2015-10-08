//
//  UIBinding.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 11/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "UIBinding.h"
#import "UIHandler.h"

#pragma mark - UI Dialogs

void showAlertDialog (const char* title, const char* message,
                      const char* buttons, const char* callerTag)
{
    [[UIHandler Instance] showAlertViewWithTitle:ConvertToNSString(title)
										 message:ConvertToNSString(message)
										 buttons:ConvertToNSArray(buttons)
									andCallerTag:ConvertToNSString(callerTag)];
}

void showSingleFieldPromptDialog (const char* title, const char* message,
                                  const char* placeholder, bool useSecureText,
                                  const char* buttons)
{
    [[UIHandler Instance] showSingleFieldPromptWithTitle:ConvertToNSString(title)
												 message:ConvertToNSString(message)
										 placeHolderText:ConvertToNSString(placeholder)
										   ofSecuredType:useSecureText
											  andButtons:ConvertToNSArray(buttons)];
}

void showLoginPromptDialog (const char* title, const char* message,
                            const char* placeholder1, const char* placeholder2,
                            const char* buttons)
{
    [[UIHandler Instance] showLoginPromptWithTitle:ConvertToNSString(title)
										   message:ConvertToNSString(message)
								   placeHolderText:ConvertToNSString(placeholder1) :ConvertToNSString(placeholder2)
										andButtons:ConvertToNSArray(buttons)];
}

#pragma mark - Popover

void setPopoverPoint (float x, float y)
{
	float contentScale = [[UIScreen mainScreen] scale];

	[[UIHandler Instance] setPopoverPoint:CGPointMake(x / contentScale, y / contentScale)];
}