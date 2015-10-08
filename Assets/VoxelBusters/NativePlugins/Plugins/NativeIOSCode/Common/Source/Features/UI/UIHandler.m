//
//  UIHandler.m
//  Unity-iPhone
//
//  Created by Ashwin kumar on 17/12/14.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "UIHandler.h"

@implementation UIHandler

#define kAlertDialogClosed			"AlertDialogClosed"
#define kSingleFieldDialogClosed	"SingleFieldPromptDialogClosed"
#define kLoginPromptDialogClosed	"LoginPromptDialogClosed"

#define tAlertView            		99
#define tSingleFieldPrompt    		100
#define tLoginPrompt         		101

@synthesize popoverPoint;

#pragma mark - UI Dialogs

- (CustomAlertView *)createAlertViewWithTitle:(NSString *)title
                                      message:(NSString *)message
                                          tag:(int)tagValue
									 delegete:(id)delegate
									  buttons:(NSArray *)buttons
{
    CustomAlertView *alertView  = [[[CustomAlertView alloc] init] autorelease];
    alertView.title             = title;
    alertView.message           = message;
    
    // Set buttons
    for (NSString* button in buttons)
        [alertView addButtonWithTitle:button];
    
    // Set tag
    alertView.tag				= tagValue;
    
    // Set delegate
    alertView.delegate      	= self;
    
    return alertView;
}

- (void)showAlertViewWithTitle:(NSString *)title
                       message:(NSString *)message
                       buttons:(NSArray *)buttons
                  andCallerTag:(NSString*)cTag
{
    CustomAlertView *alertView      = [[self createAlertViewWithTitle:title
															  message:message
																  tag:tAlertView
															 delegete:self
															  buttons:buttons] retain];
    // Set caller tag
	if (cTag == NULL)
		cTag	= kNSStringDefault;
	
    [alertView setUserData:cTag];
    
    // Show
    [alertView show];
    
    // Release
    [alertView release];
}

- (void)showSingleFieldPromptWithTitle:(NSString *)title
                               message:(NSString *)message
                       placeHolderText:(NSString *)placeholder
                         ofSecuredType:(BOOL)useSecureText
                            andButtons:(NSArray *)buttons
{
    CustomAlertView *alertView      = [[self createAlertViewWithTitle:title
															  message:message
																  tag:tSingleFieldPrompt
															 delegete:self
															  buttons:buttons] retain];
    // Set alert style
    if (useSecureText)
        alertView.alertViewStyle    = UIAlertViewStyleSecureTextInput;
    else
        alertView.alertViewStyle    = UIAlertViewStylePlainTextInput;
    
    // Set placeholder text
    if (placeholder != NULL)
        [[alertView textFieldAtIndex:0] setPlaceholder:placeholder];
    
    // Show
    [alertView show];
    
    // Release
    [alertView release];
}

- (void)showLoginPromptWithTitle:(NSString *)title
                         message:(NSString *)message
                 placeHolderText:(NSString *)placeholder1 :(NSString *)placeholder2
                      andButtons:(NSArray *)buttons
{
    CustomAlertView *alertView      = [[self createAlertViewWithTitle:title
															  message:message
																  tag:tLoginPrompt
															 delegete:self
															  buttons:buttons] retain];
    
    // Set alert style
    alertView.alertViewStyle        = UIAlertViewStyleLoginAndPasswordInput;
    
    // Set placeholder text
    if (placeholder1 != NULL)
        [[alertView textFieldAtIndex:0] setPlaceholder:placeholder1];
    
    if (placeholder2 != NULL)
        [[alertView textFieldAtIndex:1] setPlaceholder:placeholder2];
    
    // Show
    [alertView show];
    
    // Release
    [alertView release];
}

#pragma mark - Popover

- (void)setPopoverPoint:(CGPoint)newPoint
{
	NSLog(@"[UIHandler] setting popover point to (%f, %f)", newPoint.x, newPoint.y);
	popoverPoint = newPoint;
}

#pragma mark - Delegate

#define kButtonPressed	@"button-pressed"
#define kCaller			@"caller"
#define kInputText		@"input"
#define kUsernameText	@"username"
#define kPasswordText	@"password"


- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    NSString *buttonName            = [alertView buttonTitleAtIndex:buttonIndex];
    NSLog(@"[Dialogs] view was closed by pressing button %@", buttonName);
    
    // Alertview
    if (alertView.tag == tAlertView)
    {
        NSMutableDictionary *param 	= [NSMutableDictionary dictionary];
		param[kButtonPressed]   	= (buttonName != NULL) ? buttonName : kNSStringDefault;
        param[kCaller]       		= [(CustomAlertView *)alertView userData];
        
        // Notify unity
        NotifyEventListener(kAlertDialogClosed, ToJsonCString(param));
    }
    // Single field prompt
    else if (alertView.tag == tSingleFieldPrompt)
    {
        NSString *promptText        = [alertView textFieldAtIndex:0].text;
        if (promptText == NULL)
            promptText  = kNSStringDefault;
        
        NSMutableDictionary *param  = [NSMutableDictionary dictionary];
        param[kButtonPressed]    	= buttonName;
        param[kInputText]        	= promptText;
        
        // Notify unity
        NotifyEventListener(kSingleFieldDialogClosed, ToJsonCString(param));
    }
    // Login prompt
    else if (alertView.tag == tLoginPrompt)
    {
        NSString *usernameText    	= [alertView textFieldAtIndex:0].text;
        NSString *passwordText      = [alertView textFieldAtIndex:1].text;
       
        // Make sure string is not empty
        if (usernameText == NULL)
            usernameText	= kNSStringDefault;
        
        if (passwordText == NULL)
            passwordText    = kNSStringDefault;
        
        NSMutableDictionary *param  = [NSMutableDictionary dictionary];
        param[kButtonPressed]    	= buttonName;
        param[kUsernameText]        = usernameText;
        param[kPasswordText]     	= passwordText;
        
        // Notify unity
        NotifyEventListener(kLoginPromptDialogClosed, ToJsonCString(param));
    }
}
    
@end
