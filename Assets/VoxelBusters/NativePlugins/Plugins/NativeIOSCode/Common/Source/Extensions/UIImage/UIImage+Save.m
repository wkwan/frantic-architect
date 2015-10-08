//
//  UIImage+Save.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 20/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "UIImage+Save.h"
#import "NSData+UIImage.h"
#import "UIImage+Formats.h"
#import "UIImage+Resize.h"

@implementation UIImage (Save)

- (NSString *)saveImageToDocumentsDirectory
{
	return [self saveImageToDocumentsDirectory:1];
}

- (NSString *)saveImageToDocumentsDirectory:(float)scale
{
	UIImage *resizedImage	= [self resize:scale];
	
	if (resizedImage == NULL)
		return kNSStringDefault;
	
	if ([self imageFormat] == UIImageFomatPNG)
		return [UIImagePNGRepresentation(resizedImage) saveImage];
	else
		return [UIImageJPEGRepresentation(resizedImage, 1) saveImage];
}

@end
