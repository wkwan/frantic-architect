//
//  NSData+UIImage.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 19/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface NSData (UIImage)

- (NSString *)saveImage;
- (BOOL)isPNG;
- (BOOL)isJPEG;

@end
