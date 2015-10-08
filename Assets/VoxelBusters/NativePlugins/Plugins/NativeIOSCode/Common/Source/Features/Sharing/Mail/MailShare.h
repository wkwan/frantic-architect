//
//  MailShare.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 13/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <MessageUI/MessageUI.h>

typedef void (^MailShareCompletionHandler)(MFMailComposeResult);

@interface MailShare : NSObject <MFMailComposeViewControllerDelegate, UINavigationControllerDelegate>

// Properties
@property(nonatomic, copy)	MailShareCompletionHandler shareCompletionHandler;

// Related to sharing
- (BOOL)canSendMail;
- (void)sendMailWithSubject:(NSString *)subject
					andBody:(NSString *)body
                     isHtml:(BOOL)isHTML
			   toRecipients:(NSArray *)recipients
        alongWithAttachment:(NSData *)attachmentData
					 ofType:(NSString *)mimeType
           havingFileNameAs:(NSString *)filename
				 completion:(MailShareCompletionHandler)completion;

@end
