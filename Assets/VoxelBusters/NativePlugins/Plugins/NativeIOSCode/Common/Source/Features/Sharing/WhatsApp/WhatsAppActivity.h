//
//  WhatsAppActivity.h
//  Unity-iPhone
//
//  Created by Ashwin kumar on 05/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "WhatsAppShare.h"

// Identifier
UIKIT_EXTERN NSString*  		UIActivityTypePostToWhatsApp;

@interface WhatsAppActivity : UIActivity

// Properties
@property(nonatomic, retain)    UIImage         *imageToShare;
@property(nonatomic, retain)    NSString        *textToShare;
@property(nonatomic, retain)	WhatsAppShare	*whatsAppShare;

@end
