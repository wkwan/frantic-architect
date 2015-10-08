//
//  NPSharingHandler.h
//  Unity-iPhone
//
//  Created by Ashwin kumar on 05/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "HandlerBase.h"
#import "MailShare.h"
#import "MessagingShare.h"
#import "WhatsAppShare.h"

enum eShareOptions
{
    UNDEFINED   = 0,
    MESSAGE,
    MAIL,
    FB,
    TWITTER,
    WHATSAPP
};
typedef enum eShareOptions eShareOptions;

@interface NPSharingHandler : HandlerBase

// Properties
@property(nonatomic, retain)	UIPopoverController *popoverController;
@property(nonatomic, retain)	MailShare			*mailShare;
@property(nonatomic, retain)	MessagingShare		*messagingShare;
@property(nonatomic, retain)	WhatsAppShare		*whatsAppShare;

// Static methods
+ (MailShare *)MailShare;
+ (MessagingShare *)MessagingShare;
+ (WhatsAppShare *)WhatsAppShare;

// Related to share sheet
- (void)shareMessage:(NSString *)message
                 URL:(NSString *)URLString
            andImage:(UIImage *)image
 withExcludedSharing:(NSArray *)excludedOptions;

@end
