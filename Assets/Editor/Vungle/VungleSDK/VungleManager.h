//
//  VungleManager.h
//  VungleTest
//
//  Created by Mike Desaro on 6/5/12.
//  Copyright (c) 2012 prime31. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <VungleSDK/VungleSDK.h>


@interface VungleManager : NSObject <VungleSDKDelegate>


+ (VungleManager*)sharedManager;

+ (id)objectFromJson:(NSString*)json;

+ (NSString*)jsonFromObject:(id)object;


- (void)startWithAppId:(NSString*)appId;

- (void)playAdWithOptions:(NSDictionary*)options;


@end
