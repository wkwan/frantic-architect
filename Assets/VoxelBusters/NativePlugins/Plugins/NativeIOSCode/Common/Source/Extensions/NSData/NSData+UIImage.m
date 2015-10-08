//
//  NSData+UIImage.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 19/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "NSData+UIImage.h"

@implementation NSData (UIImage)

- (NSString *)saveImage
{
    NSString *imageName         =  NULL;
	
	if ([self isPNG])
		imageName = [[Utility GetUUID] stringByAppendingString:@".png"];
	else if ([self isJPEG])
		imageName = [[Utility GetUUID] stringByAppendingString:@".jpg"];
	else
		return kNSStringDefault;
    
    // Now, we have to find the documents directory so we can save it
    NSArray *paths              = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentsDir      = [paths objectAtIndex:0];
    
    // Now we get the full path to the file
    NSString *finalPath         = [documentsDir stringByAppendingPathComponent:imageName];
    
    // Write it to disk
    [self writeToFile:finalPath atomically:NO];
	
	return finalPath;
}

- (BOOL)isPNG
{
	uint8_t firstByte;
	
	// Read first byte
	[self getBytes:&firstByte length:1];
	
	if (firstByte == 0x89)
		return YES;
	
	return NO;
}

- (BOOL)isJPEG
{
	uint8_t firstByte;
	
	// Read first byte
	[self getBytes:&firstByte length:1];
	
	if (firstByte == 0xFF)
		return YES;
	
	return NO;
}

@end
