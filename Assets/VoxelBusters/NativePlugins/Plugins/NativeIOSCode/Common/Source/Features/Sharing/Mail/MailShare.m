//
//  MailShare.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 13/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "MailShare.h"

@implementation MailShare

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

- (BOOL)canSendMail
{
    bool canSend	= [MFMailComposeViewController canSendMail];
	NSLog(@"[MailShare] can send mail: %d", canSend);
	
	return canSend;
}

- (void)sendMailWithSubject:(NSString *)subject
					andBody:(NSString *)body
                     isHtml:(BOOL)isHTML
			   toRecipients:(NSArray *)recipients
        alongWithAttachment:(NSData *)attachmentData
					 ofType:(NSString *)mimeType
           havingFileNameAs:(NSString *)filename
				 completion:(MailShareCompletionHandler)completion
{
	// Reset completion block
	self.shareCompletionHandler	= completion;
	
	// Check if we can send mail, if not then send failed
	if (![self canSendMail])
	{
		[self mailComposeController:NULL
				didFinishWithResult:MFMailComposeResultFailed
							  error:NULL];
		return;
	}
	
	// User mail composer to send mail
	MFMailComposeViewController* controller = [[[MFMailComposeViewController alloc] init] autorelease];
	
	// Set subject, message, recipients
	[controller setSubject:subject];
	[controller setMessageBody:body isHTML:isHTML];
	[controller setToRecipients:recipients];
	
	// Add attachment
	if (attachmentData != NULL)
		[controller addAttachmentData:attachmentData mimeType:mimeType fileName:filename];
	
	// Set delegate
	[controller setMailComposeDelegate:self];
	
	// Present it
	[UnityGetGLViewController() presentViewController:controller
											 animated:YES
										   completion:NULL];
}

#pragma mark - Delegate

- (void)mailComposeController:(MFMailComposeViewController *)controller
          didFinishWithResult:(MFMailComposeResult)result
                        error:(NSError *)error
{
	NSLog(@"[MailShare] did finish with result: %d", result);
	
	// Completion block
	if ([self shareCompletionHandler] != NULL)
	{
		shareCompletionHandler(result);
		self.shareCompletionHandler = NULL;
	}
	
	// Dismiss
	if (controller)
		[controller dismissViewControllerAnimated:YES completion:Nil];
}

@end