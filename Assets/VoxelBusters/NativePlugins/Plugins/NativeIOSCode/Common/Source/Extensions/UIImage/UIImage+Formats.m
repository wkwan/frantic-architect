//
//  UIImage+Formats.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 20/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "UIImage+Formats.h"

@implementation UIImage (Formats)

- (UIImageFomatType)imageFormat
{
	CGImageAlphaInfo imgAlpha = CGImageGetAlphaInfo(self.CGImage);
	
    // Is this an image with transparency (i.e. do we need to save as PNG?)
    if ((imgAlpha == kCGImageAlphaNone) || (imgAlpha == kCGImageAlphaNoneSkipFirst) || (imgAlpha == kCGImageAlphaNoneSkipLast))
		return UIImageFomatJPEG;
    else
		return UIImageFomatPNG;
}

@end
