//
//  HZFetchOptions.h
//  Heyzap
//
//  Created by Monroe Ekilah on 8/26/15.
//  Copyright (c) 2015 Heyzap. All rights reserved.
//

#import <UIKit/UIKit.h>

typedef NS_ENUM(NSInteger, HZAdMobNativeAdImageOrientation) {
    HZAdMobNativeAdImageOrientationAny,
    HZAdMobNativeAdImageOrientationPortrait,
    HZAdMobNativeAdImageOrientationLandscape,
};

@interface HZFetchOptions : NSObject <NSCopying>

// Info passed to HeyzapMediation for a fetch
@property (nonatomic, strong, nullable) NSString *tag;
@property (nonatomic, nullable, copy) void (^completion)(BOOL result,  NSError * __nullable error );

#pragma mark - Native Ad Options

/**
 *  The view controller to fetch native ads for. Currently only AdMob uses this property. This property is required for AdMob.
 */
@property (nonatomic, weak) UIViewController *presentingViewController;

/**
 *  The number of unique native ads to fetch from each network. The default value is `@20`.
 *
 *  For Heyzap and Facebook, native ads can be requested in a large batch to ensure that each native ad is unique.
 *  This property is ignored for AdMob.
 */
@property (nonatomic, null_resettable) NSNumber *uniqueNativeAdsToFetch;

/**
 *  The type of AdMob native ads to request (App Install and/or Content ads)
 *
 *  You can populate this property using AdMob's constants, or with the functions:
 *      * `hzAdMobNativeAdTypeAppInstall`
 *      * `hzAdMobNativeAdTypeContent`
 */
@property (nonatomic, null_resettable) NSArray <NSString *> *admobNativeAdTypes;

/**
 *  The preferred orientation of images for AdMob native ads. Note that AdMob may only have images of one orientation, so you'll need to handle both landscape and portrait images regardless.
 *
 *  Defaults to `HZAdMobNativeAdImageOrientationAny`.
 */
@property (nonatomic) HZAdMobNativeAdImageOrientation admobPreferredImageOrientation;

NSArray <NSString *> * _Nonnull hzAllAdmobAdTypes(void);

NSString  * _Nonnull  hzAdMobNativeAdTypeAppInstall(void);
NSString * _Nonnull hzAdMobNativeAdTypeContent(void);

@end
