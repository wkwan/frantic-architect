//
//  UIImage+Orientation.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 20/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "UIImage+Orientation.h"

@implementation UIImage (Orientation)

- (UIImage *)fixOrientation
{
    UIImage             *newCopy        = NULL;
    CGRect              bounds          = CGRectZero;
    CGContextRef        context         = nil;
    CGImageRef          cgImageRef      = self.CGImage;
    CGRect              rect            = CGRectZero;
    CGAffineTransform   transform       = CGAffineTransformIdentity;
    UIImageOrientation  orientation     = [self imageOrientation];
    
    // Update rect width and height
    rect.size.width  = CGImageGetWidth(cgImageRef);
    rect.size.height = CGImageGetHeight(cgImageRef);
    
    // Update bounds
    bounds = rect;
    
    switch (orientation)
    {
        case UIImageOrientationUp:
            return self;
            
        case UIImageOrientationUpMirrored:
            transform = CGAffineTransformMakeTranslation(rect.size.width, 0.0);
            transform = CGAffineTransformScale(transform, -1.0, 1.0);
            break;
            
        case UIImageOrientationDown:
            transform = CGAffineTransformMakeTranslation(rect.size.width,
														 rect.size.height);
            transform = CGAffineTransformRotate(transform, M_PI);
            break;
            
        case UIImageOrientationDownMirrored:
            transform = CGAffineTransformMakeTranslation(0.0, rect.size.height);
            transform = CGAffineTransformScale(transform, 1.0, -1.0);
            break;
            
        case UIImageOrientationLeft:
            bounds    = [self swapWidthAndHeight:bounds];
            transform = CGAffineTransformMakeTranslation(0.0, rect.size.width);
            transform = CGAffineTransformRotate(transform, 3.0 * M_PI / 2.0);
            break;
            
        case UIImageOrientationLeftMirrored:
            bounds    = [self swapWidthAndHeight:bounds];
            transform = CGAffineTransformMakeTranslation(rect.size.height,
														 rect.size.width);
            transform = CGAffineTransformScale(transform, -1.0, 1.0);
            transform = CGAffineTransformRotate(transform, 3.0 * M_PI / 2.0);
            break;
            
        case UIImageOrientationRight:
            bounds    = [self swapWidthAndHeight:bounds];
            transform = CGAffineTransformMakeTranslation(rect.size.height, 0.0);
            transform = CGAffineTransformRotate(transform, M_PI / 2.0);
            break;
            
        case UIImageOrientationRightMirrored:
            bounds    = [self swapWidthAndHeight:bounds];
            transform = CGAffineTransformMakeScale(-1.0, 1.0);
            transform = CGAffineTransformRotate(transform, M_PI / 2.0);
            break;
            
        default:
            // orientation value supplied is invalid
            assert(false);
            return nil;
    }
    
    UIGraphicsBeginImageContext(bounds.size);
    context = UIGraphicsGetCurrentContext();
    
    switch (orientation)
    {
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            CGContextScaleCTM(context, -1.0, 1.0);
            CGContextTranslateCTM(context, -rect.size.height, 0.0);
            break;
            
        default:
            CGContextScaleCTM(context, 1.0, -1.0);
            CGContextTranslateCTM(context, 0.0, -rect.size.height);
            break;
    }
    
    CGContextConcatCTM(context, transform);
    CGContextDrawImage(UIGraphicsGetCurrentContext(), rect, cgImageRef);
    newCopy = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    
    return newCopy;
}

- (CGRect)swapWidthAndHeight:(CGRect)rect
{
    CGFloat  swap       = rect.size.width;
    rect.size.width     = rect.size.height;
    rect.size.height    = swap;
    
    return rect;
}

@end
