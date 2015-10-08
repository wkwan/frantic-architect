//
//  WhatsAppShare.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 13/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "WhatsAppShare.h"
#import "UIHandler.h"

@implementation WhatsAppShare

@synthesize documentInteractionController;
@synthesize shareCompletionHandler;
@synthesize didCompleteSharing;

- (id)init
{
	self	= [super init];
	
	if (self)
	{
		self.documentInteractionController	= NULL;
		self.shareCompletionHandler			= NULL;
		self.didCompleteSharing				= NO;
	}
	
	return self;
}

- (void)dealloc
{
	self.documentInteractionController	= NULL;
	self.shareCompletionHandler			= NULL;
	
	[super dealloc];
}

#pragma mark - Sharing

- (bool)canShareTextMessage
{
    bool canSend	= [[UIApplication sharedApplication] canOpenURL:[self getWhatsAppURLToShareTextMessage:@""]];
	NSLog(@"[WhatsAppShare] can share message: %d", canSend);

	return canSend;
}

- (bool)canShareImage
{
    bool canSend	= [[UIApplication sharedApplication] canOpenURL:[NSURL URLWithString:@"whatsapp://app"]];
	NSLog(@"[WhatsAppShare] can send image: %d", canSend);
	
	return canSend;
}

- (void)shareTextMessage:(NSString *)message
			  completion:(WhatsAppShareCompletionHandler)completion
{
	NSLog(@"[WhatsAppShare] sharing message: %@", message);
	bool canShare	= [self canShareTextMessage];
	
	// Open app to share
	if (canShare)
		[[UIApplication sharedApplication] openURL:[self getWhatsAppURLToShareTextMessage:message]];
		
	// Completion block
	if (completion != NULL)
		completion(canShare);
}

- (void)shareImage:(UIImage *)image
		completion:(WhatsAppShareCompletionHandler)completion
{
	NSLog(@"[WhatsAppShare] sharing image");
	
	// Reset flag
	[self setDidCompleteSharing:NO];
	
	// Cache completion
	self.shareCompletionHandler	= completion;
	
	// Sharing image failed as its null
	if (![self canShareImage] || image == NULL)
	{
		NSLog(@"[WhatsAppShare] sharing failed");
		
		shareCompletionHandler(NO);
		self.shareCompletionHandler	= NULL;
		return;
	}
	
	// Save image to path in documents directory
	NSString *savePath							= [NSHomeDirectory() stringByAppendingPathComponent:@"Documents/whatsAppTmp.wai"];
	[UIImageJPEGRepresentation(image, 1.0) writeToFile:savePath atomically:YES];
    
	// Create interaction controller
	self.documentInteractionController          = [UIDocumentInteractionController interactionControllerWithURL:[NSURL fileURLWithPath:savePath]];
	self.documentInteractionController.UTI      = @"net.whatsapp.image";
	self.documentInteractionController.delegate = self;
	
	// Present
	CGRect menuRect;
	menuRect.origin	= [[UIHandler Instance] popoverPoint];
	menuRect.size	= CGSizeMake(1, 1);
	[self.documentInteractionController presentOpenInMenuFromRect:menuRect
														   inView:UnityGetGLView()
														 animated:YES];
}

#pragma mark - Delegate

- (void)documentInteractionController:(UIDocumentInteractionController *)controller
           didEndSendingToApplication:(NSString *)application
{
	[self setDidCompleteSharing:YES];
}

- (void)documentInteractionControllerDidDismissOpenInMenu:(UIDocumentInteractionController *)controller
{
	NSLog(@"[WhatsAppShare] did dismiss menu, sharing is completed: %d", self.didCompleteSharing);
	
	// Completion block
    if ([self shareCompletionHandler] != NULL)
	{
		self.shareCompletionHandler(self.didCompleteSharing);
		self.shareCompletionHandler		= NULL;
	}
    
    // Release document interactor
    self.documentInteractionController  = NULL;
}

#pragma mark - Misc

- (NSURL *)getWhatsAppURLToShareTextMessage:(NSString *)message
{
    NSString *URLStr    = [NSString stringWithFormat:@"whatsapp://send?text=%@", message];
    NSURL *URL          = [NSURL URLWithString:[URLStr stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding]];
    
    return URL;
}

@end
