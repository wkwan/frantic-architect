//
//  MessagingShare.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 13/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <MessageUI/MessageUI.h>

typedef void (^MessagingShareCompletionHandler)(MessageComposeResult);

@interface MessagingShare : NSObject <MFMessageComposeViewControllerDelegate>

// Properties
@property(nonatomic, copy)	MessagingShareCompletionHandler shareCompletionHandler;

// Related to sharing
- (BOOL)isMessagingAvailable;
- (void)sendMessageWithBody:(NSString *)body
			   toRecipients:(NSArray *)recipients
				 completion:(MessagingShareCompletionHandler)completion;

@end
