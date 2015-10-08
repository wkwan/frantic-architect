//
//  UIImage+Resize.m
//  Unity-iPhone
//
//  Created by Ashwin kumar on 13/02/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "UIImage+Resize.h"

@implementation UIImage (Resize)

- (UIImage*)resize:(float)scale
{
	if (scale < 0)
		return NULL;
	
	CGSize imageSize		= self.size;
	CGSize scaledSize		= CGSizeMake(imageSize.width * scale, imageSize.height * scale);

	UIGraphicsBeginImageContext(scaledSize);
	[self drawInRect:CGRectMake(0, 0, scaledSize.width, scaledSize.height)];
	
	UIImage *destImage = UIGraphicsGetImageFromCurrentImageContext();
	UIGraphicsEndImageContext();
	
	return destImage;
}
@end
