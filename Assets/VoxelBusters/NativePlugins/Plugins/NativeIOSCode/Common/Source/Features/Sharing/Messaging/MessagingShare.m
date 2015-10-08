//
//  MessagingShare.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 13/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "MessagingShare.h"

@implementation MessagingShare

@synthesize shareCompletionHandler;

- (id)init
{
	self	= [super init];
	
	if (self)
	{
		self.shareCompletionHandler	= NULL;
	}
	
	return self;
}

- (void)dealloc
{
	self.shareCompletionHandler	= NULL;
	
	[super dealloc];
}

#pragma mark - Sharing

- (BOOL)isMessagingAvailable
{
    bool canSend	= [MFMessageComposeViewController canSendText];
	NSLog(@"[MessagingShare] can send message: %d", canSend);
	
    return canSend;
}

- (void)sendMessageWithBody:(NSString *)body
			   toRecipients:(NSArray *)recipients
				 completion:(MessagingShareCompletionHandler)completion
{
	self.shareCompletionHandler	= [completion copy];

	if (![self isMessagingAvailable])
	{
		[self messageComposeViewController:NULL didFinishWithResult:MessageComposeResultFailed];
		return;
	}
	
    // Create controller
	MFMessageComposeViewController *controller 	= [[[MFMessageComposeViewController alloc] init] autorelease];
	
	// Add body and set delegate
	controller.body                     		= body;
	controller.messageComposeDelegate   		= self;
	
	// Add recipients
	controller.recipients              		 	= recipients;
	
	// Present
	[UnityGetGLViewController() presentViewController:controller
											 animated:YES
										   completion:NULL];
}

#pragma mark - Delegate

- (void)messageComposeViewController:(MFMessageComposeViewController *)controller
				 didFinishWithResult:(MessageComposeResult)result
{
	NSLog(@"[MessagingShare] did finish with result: %d", result);
	
	// Completion block
	if ([self shareCompletionHandler] != NULL)
	{
		self.shareCompletionHandler(result);
		self.shareCompletionHandler	= NULL;
	}
	
	// Dismiss
	if (controller)
		[controller dismissViewControllerAnimated:YES completion:Nil];
}

@end