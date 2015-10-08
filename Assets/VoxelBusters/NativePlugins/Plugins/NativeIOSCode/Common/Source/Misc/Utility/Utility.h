//
//  Utility.h
//  Unity-iPhone
//
//  Created by Ashwin kumar on 06/12/14.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

// String
UIKIT_EXTERN NSString*			kNSStringDefault;
UIKIT_EXTERN const char*		kCStringDefault;
UIKIT_EXTERN char* 				CStringCopy (const char* str);
UIKIT_EXTERN NSString* 			ConvertToNSString (const char * jsonCharArray);
UIKIT_EXTERN bool 				IsNullOrEmpty (NSString* string);

// Json
UIKIT_EXTERN id 				FromJson (const char * jsonCharArray);
UIKIT_EXTERN NSArray* 			ConvertToNSArray (const char * jsonCharArray);
UIKIT_EXTERN const char* 		ToJsonCString (id object);

// Rect operations
UIKIT_EXTERN CGRect 			ScreenBounds();
UIKIT_EXTERN CGRect 			GetUsableScreenSpace ();
UIKIT_EXTERN CGRect 			ConvertToNormalisedRect (CGRect inputRect);
UIKIT_EXTERN CGRect 			ConvertToScreenSpace (CGRect normalisedRect);
UIKIT_EXTERN UIEdgeInsets 		GetEdgeInsetsRelativeToScreenBounds (CGRect frame);
UIKIT_EXTERN CGRect				GetFrameRelativeToScreenBounds (UIEdgeInsets edgeInsets);

// Device
UIKIT_EXTERN bool				IsIpadInterface();

typedef void (^ImageCompletionHandler)(const void *, NSInteger);

@interface Utility : NSObject

// Related to UIImage
+ (UIImage *)CreateImageFromByteArray:(void*)byteArray ofLength:(int)length;
+ (void)GetImageBytes:(NSString *)imageName completion:(ImageCompletionHandler)completion;

// Related to date time
+ (NSString *)ConvertNSDateToNSString:(NSDate*)date;

// NSData
+ (NSData *)CreateNSDataFromByteArray:(void*)byteArray ofLength:(int)length;

// Misc
+ (NSString *)GetUUID;

@end
