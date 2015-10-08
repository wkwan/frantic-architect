//
//  WhatsAppActivity.m
//  Unity-iPhone
//
//  Created by Ashwin kumar on 05/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "WhatsAppActivity.h"

// Identifier
NSString*  UIActivityTypePostToWhatsApp	= @"UIActivityTypePostToWhatsApp";

@implementation WhatsAppActivity

@synthesize imageToShare;
@synthesize textToShare;
@synthesize whatsAppShare;

- (void)dealloc
{
    self.imageToShare   = NULL;
    self.textToShare    = NULL;
 	self.whatsAppShare	= NULL;
	
    [super dealloc];
}

#pragma mark - Activity Information

+ (UIActivityCategory)activityCategory
{
    return UIActivityCategoryShare;
}

- (NSString *)activityType
{
    return UIActivityTypePostToWhatsApp;
}

- (NSString *)activityTitle
{
    return @"Post to WhatsApp";
}

- (UIImage *)activityImage
{
	NSString *imageName	= NULL;
	
//	For iPhone and iPod touch, images on iOS 7 should be 60 by 60 points; on earlier versions of iOS, you should use images no larger than 43 by 43 points. For iPad, images on iOS 7 should be 76 by 76 points; on earlier versions of iOS you should use images no larger than 60 by 60 points. On a device with Retina display, the number of pixels is doubled in each direction.
	
	// Above iOS 7
	if (SYSTEM_VERSION_GREATER_THAN_OR_EQUAL_TO(@"7.0"))
	{
		if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad)
		{
			imageName	= @"Whatsapp_76_76.png";
		}
		else
		{
			imageName	= @"Whatsapp_60_60.png";
		}
	}
	// Pre iOS 7
	else
	{
		if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad)
		{
			imageName	= @"Whatsapp_60_60.png";
		}
		else
		{
			imageName	= @"Whatsapp_43_43.png";
		}
	}
	
	return [UIImage imageNamed:@"whatsapp_iPhone.png"];
}

#pragma mark - Performing Activity

- (BOOL)canPerformWithActivityItems:(NSArray *)activityItems
{
    NSString *message   = NULL;
    bool containsImage  = FALSE;
    
    for (id item in activityItems)
    {
        if ([item isKindOfClass:[NSString class]])
            message         = item;
        else if ([item isKindOfClass:[UIImage class]])
            containsImage   = TRUE;
    }
	
	
	// Create whats app instance
	if ([self whatsAppShare] == NULL)
	{
		self.whatsAppShare	= [[[WhatsAppShare alloc] init] autorelease];
	}
    
    // Check if sharing image using whatsapp is possible or not
    if (containsImage && [self.whatsAppShare canShareImage])
        return YES;
    
    // Check if sharing text using whatsapp is possible or not
    if (!IsNullOrEmpty(message) && [self.whatsAppShare canShareTextMessage])
        return YES;
    
    return NO;
}

- (UIViewController *)activityViewController
{
    return nil;
}

- (void)prepareWithActivityItems:(NSArray *)activityItems
{
    NSString *message   = NULL;
    UIImage *image      = NULL;
    
    for (id item in activityItems)
    {
        if ([item isKindOfClass:[NSString class]])
            message = item;
        else if ([item isKindOfClass:[UIImage class]])
            image   = item;
    }
    
    // Cache activity items
    self.imageToShare	= image;
    self.textToShare	= message;
}

- (void)performActivity
{
    // Share image
    if (self.imageToShare)
    {
		[self.whatsAppShare shareImage:self.imageToShare completion:^(bool isCompleted) {
			// Finished activity
			[self activityDidFinish:isCompleted];
		}];
        
		return;
    }
	
    // Share text
    if (self.textToShare)
    {
		[self.whatsAppShare shareTextMessage:self.textToShare completion:^(bool isCompleted) {
			// Finished activity
			[self activityDidFinish:isCompleted];
		}];
		
		return;
    }
}

@end