//
//  UIHandler.h
//  Unity-iPhone
//
//  Created by Ashwin kumar on 17/12/14.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "HandlerBase.h"
#import "CustomAlertView.h"

@interface UIHandler : HandlerBase <UIAlertViewDelegate>

// Properties
@property(nonatomic)	CGPoint		popoverPoint;

// Related to UI Dialogs
- (CustomAlertView *)createAlertViewWithTitle:(NSString *)title
                                      message:(NSString *)message
                                          tag:(int)tagValue
									 delegete:(id)delegate
									  buttons:(NSArray *)buttons;

- (void)showAlertViewWithTitle:(NSString *)title
                       message:(NSString *)message
                       buttons:(NSArray *)buttons
                  andCallerTag:(NSString*)cTag;

- (void)showSingleFieldPromptWithTitle:(NSString *)title
                               message:(NSString *)message
                       placeHolderText:(NSString *)placeholder
                         ofSecuredType:(BOOL)useSecureText
                            andButtons:(NSArray *)buttons;

- (void)showLoginPromptWithTitle:(NSString *)title
                         message:(NSString *)message
                 placeHolderText:(NSString *)placeholder1 :(NSString *)placeholder2
                      andButtons:(NSArray *)buttons;

@end
