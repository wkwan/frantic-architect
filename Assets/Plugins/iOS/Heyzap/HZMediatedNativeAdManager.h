//
//  HZMediatedNativeAdManager.h
//  Heyzap
//
//  Created by Maximilian Tagher on 11/20/15.
//  Copyright © 2015 Heyzap. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "HZFetchOptions.h"

@class HZMediatedNativeAd;

NS_ASSUME_NONNULL_BEGIN

@interface HZMediatedNativeAdManager : NSObject

+ (void)fetchNativeAdWithOptions:(HZFetchOptions  * _Nullable)fetchOptions;
+ (HZMediatedNativeAd * _Nullable)getNextNativeAdForTag:(NSString * _Nullable)tag error:(NSError **)error;

NS_ASSUME_NONNULL_END

@end
