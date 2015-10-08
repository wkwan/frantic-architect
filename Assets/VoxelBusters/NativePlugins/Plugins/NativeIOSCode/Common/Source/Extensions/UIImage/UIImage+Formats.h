//
//  UIImage+Formats.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 20/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

enum UIImageFomatType
{
	UIImageFomatJPEG,
	UIImageFomatPNG
};
typedef enum UIImageFomatType UIImageFomatType;

@interface UIImage (Formats)

- (UIImageFomatType)imageFormat;

@end
