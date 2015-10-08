//
//  Utility.m
//  Unity-iPhone
//
//  Created by Ashwin kumar on 06/12/14.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "Utility.h"

#pragma mark - String

NSString const*	kNSStringDefault	= @"";
const char*		kCStringDefault		= "";

NSString* ConvertToNSString (const char * jsonCharArray)
{
	if (jsonCharArray == NULL)
		return NULL;
	else
		return [NSString stringWithUTF8String:jsonCharArray];
}

bool IsNullOrEmpty (NSString* string)
{
	if (string == NULL)
		return TRUE;
	else
		return [string isEqualToString:@""];
}

char* CStringCopy (const char* str)
{
	if (str == NULL)
		return NULL;
	
	char* strCopy = (char*)malloc(strlen(str) + 1);
	strcpy(strCopy, str);
	
	return strCopy;
}

#pragma mark - Json

id FromJson (const char * jsonCharArray)
{
	NSString* jsonString   = ConvertToNSString(jsonCharArray);
	NSData* jsonData       = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
	
	if (jsonData != NULL)
	{
		return [NSJSONSerialization JSONObjectWithData:jsonData
											   options:0
												 error:nil];
	}
	
	return NULL;
}

NSArray* ConvertToNSArray (const char * jsonCharArray)
{
	if (jsonCharArray == NULL)
		return [NSArray array];
	else
		return (NSArray *)FromJson(jsonCharArray);
}

const char* ToJsonCString (id object)
{
	
	NSError *error = NULL;
	
	if (object != NULL)
	{
		NSData *jsonData        = [NSJSONSerialization dataWithJSONObject:object
																  options:0
																	error:&error];
		NSString* jsonString    = [[[NSString alloc] initWithData:jsonData
														 encoding:NSUTF8StringEncoding] autorelease];
		
		return [jsonString UTF8String];
	}
	
	return "{}";
}

#pragma mark - Rect operations

CGRect ScreenBounds ()
{
	CGRect bounds		= [UIScreen mainScreen].applicationFrame;

	// Pre iOS 8, swap width and height
	if (SYSTEM_VERSION_LESS_THAN(@"8.0"))
	{
		if (UIInterfaceOrientationIsLandscape([UIApplication sharedApplication].statusBarOrientation))
		{
			bounds.size	= CGSizeMake(CGRectGetHeight(bounds), CGRectGetWidth(bounds));
		}
	}
	
	return bounds;
}

CGRect GetUsableScreenSpace ()
{
	CGRect bounds		= ScreenBounds();
	
	UINavigationController *navController	= UnityGetGLViewController().navigationController;
	if (navController != NULL && !navController.navigationBarHidden)
	{
		float navBarHeight			= CGRectGetHeight([[navController navigationBar] frame]);
		
		// Update origin and height
		bounds.origin.y		+= navBarHeight;
		bounds.size.height	-= navBarHeight;
	};
	
	return bounds;
}

CGRect ConvertToNormalisedRect (CGRect inputRect)
{
	CGSize screenSize			= ScreenBounds().size;

	CGRect normalisedRect;
	normalisedRect.origin.x		= inputRect.origin.x / screenSize.width;
	normalisedRect.origin.y		= inputRect.origin.y / screenSize.height;
	normalisedRect.size.width	= inputRect.size.width / screenSize.width;
	normalisedRect.size.height	= inputRect.size.height / screenSize.height;
	
	return normalisedRect;
}

CGRect ConvertToScreenSpace (CGRect normalisedRect)
{
	CGSize screenSize	= ScreenBounds().size;
	
	CGRect rect;
	rect.origin.x		= normalisedRect.origin.x * screenSize.width;
	rect.origin.y		= normalisedRect.origin.y * screenSize.height;
	rect.size.width		= normalisedRect.size.width * screenSize.width;
	rect.size.height	= normalisedRect.size.height * screenSize.height;
	
	return rect;
}

UIEdgeInsets GetEdgeInsetsRelativeToScreenBounds (CGRect frame)
{
	CGRect screenBounds	= GetUsableScreenSpace();
	UIEdgeInsets edgeInsets;
	
	edgeInsets.left		= CGRectGetMinX(screenBounds) - CGRectGetMinX(frame);
	edgeInsets.top		= CGRectGetMinY(screenBounds) - CGRectGetMinY(frame);
	edgeInsets.right	= CGRectGetMaxX(screenBounds) - CGRectGetMaxX(frame);
	edgeInsets.bottom	= CGRectGetMaxY(screenBounds) - CGRectGetMaxY(frame);
	
	return edgeInsets;
}

CGRect GetFrameRelativeToScreenBounds (UIEdgeInsets edgeInsets)
{
	return UIEdgeInsetsInsetRect(GetUsableScreenSpace(), edgeInsets);
}

#pragma mark - Device

bool IsIpadInterface ()
{
	return (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad);
}

@implementation Utility

#pragma mark - UIImage

+ (UIImage *)CreateImageFromByteArray:(void*)byteArray ofLength:(int)length
{
    if (length <= 0)
        return NULL;
    else
        return [UIImage imageWithData:[NSData dataWithBytes:(void*)byteArray length:length]];
}

+ (void)GetImageBytes:(NSString *)imageName completion:(ImageCompletionHandler)completion
{
	UIImage *image		= [UIImage imageNamed:imageName];
	NSData *imageData	= UIImageJPEGRepresentation(image, 1);
	
	// Assign value
	if (completion)
		completion([imageData bytes], [imageData length]);
}

#pragma mark - NSDate

+ (NSString *)ConvertNSDateToNSString:(NSDate*)date
{
	if (date == NULL)
		return NULL;
	
	NSDateFormatter *dateFormatter = [[NSDateFormatter alloc] init];
	[dateFormatter setTimeZone:[NSTimeZone timeZoneWithAbbreviation:@"UTC"]];
	[dateFormatter setDateFormat:@"yyyy-MM-dd HH:mm:ss Z"];

    return [dateFormatter stringFromDate:date];
}

#pragma mark - NSData

+ (NSData *)CreateNSDataFromByteArray:(void*)byteArray ofLength:(int)length
{
    if (length <= 0)
        return NULL;
    else
        return [NSData dataWithBytes:(void*)byteArray length:length];
}

#pragma mark misc

+ (NSString *)GetUUID
{
    CFUUIDRef uuidRef           = CFUUIDCreate(NULL);
    CFStringRef uuidStringRef   = CFUUIDCreateString(NULL, uuidRef);
    CFRelease(uuidRef);
    
	NSLog(@"[Utility] UUID: %@", uuidStringRef);
    return [(NSString *)uuidStringRef autorelease];
}

@end