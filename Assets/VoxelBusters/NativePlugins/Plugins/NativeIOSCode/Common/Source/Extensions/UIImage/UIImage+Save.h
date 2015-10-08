//
//  UIImage+Save.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 20/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface UIImage (Save)

- (NSString *)saveImageToDocumentsDirectory;
- (NSString *)saveImageToDocumentsDirectory:(float)scale;

@end
