//
//  HZMediatedNativeAd.h
//  Heyzap
//
//  Created by Maximilian Tagher on 10/23/15.
//  Copyright © 2015 Heyzap. All rights reserved.
//

#import <Foundation/Foundation.h>

/**
 *  This enum lists the types of mediated ads. This can be useful if you want to handle certain native ad providers differently (e.g. apply some special UI to cross-promo ads), or can be used in conjunction with `underlyingNativeAd` to access network-specific native ad properties.
 */
typedef NS_ENUM(NSUInteger, HZMediatedNativeAdType) {
    HZMediatedNativeAdTypeFacebook,
    HZMediatedNativeAdTypeAdMobContent,
    HZMediatedNativeAdTypeAdMobAppInstall,
    HZMediatedNativeAdTypeHeyzap,
    HZMediatedNativeAdTypeHeyzapCrossPromo,
};

/**
 *  A preferred orientation for a native ad image.
 */
typedef NS_ENUM(NSUInteger, HZPreferredImageOrientation) {
    HZPreferredImageOrientationLandscape,
    HZPreferredImageOrientationPortrait,
};

NS_ASSUME_NONNULL_BEGIN

/**
 *  A notification sent when the user sees the mediated native ad. The `object` associated with the notification is the `HZMediatedNativeAd` instance.
 *
 *  @see hasHadImpression
 */
extern NSString * const HZMediatedNativeAdImpressionNotification;
/**
 *  A notification sent when the mediated native ad is clicked. The `object` associated with the notification is the `HZMediatedNativeAd` instance.
 *
 *  @see hasBeenClicked
 */
extern NSString * const HZMediatedNativeAdClickNotification;

@protocol HZMediatedNativeAdViewRegisterer;
@class HZNativeAdImage;

/**
 *  This class provides a common interface to different native ads. Recommending usage:
 *
 *  1. After receiving your native ad, configure it (set the `presentingViewController`, set the Facebook configuration properties if desired).
 *  2. Add the wrapper view to your view hierarchy. Add your native ad content as subviews to the wrapper view.
 *  3. Call `registerViews:` to set which view is used to display which property of the native ad.
 *
 *  See Heyzap's online docs for more details.
 */
@interface HZMediatedNativeAd : NSObject

#pragma mark - Native Ad Properties

/**
 *  A title conveying the main content of the ad. A title for a business's ad might be "ProShred® Paper Shredding", while an app's title might be the name of the app.
 */
@property (nonatomic, readonly, nullable) NSString *title;

/**
 *  A longer portion of the ad's content. A body for a paper shredding business might be "On-Site Document Destruction. Secure, Convenient, ISO Certified.", whereas a body for a game's ad might be the game's description.
 */
@property (nonatomic, readonly, nullable) NSString *body;

/**
 *  A very short phrase prompting the user to act. A call-to-action for a website might be "Visit Site", while an ad for a game might be "Install Now".
 */
@property (nonatomic, readonly, nullable) NSString *callToAction;

/**
 *  An icon (for apps) or logo (for content ads) associated with the ad.
 *
 *  @warning AdMob does not provide the height and width of the image, so these values will be 0 when using AdMob; you must use a hard-coded height and width if using AdMob, or check its size after loading the image from the network.
 */
@property (nonatomic, readonly, nullable) HZNativeAdImage *iconImage;

/**
 *  Returns a large image to use in your native ad.
 *
 *  @param preferredOrientation Whether you'd prefer a landscape or portrait image. The default is landscape. This parameter is only used for Heyzap; Facebook only offers one cover image, and AdMob requires that you specify the image orientation when requesting the ad (see `admobPreferredImageOrientation` in `HZFetchOptions`).
 *
 *  @return The image, or `nil` if none was available.
 *
 *  @warning AdMob does not provide the height and width of the image, so these values will be 0 when using AdMob; use a view that properly handles arbitrarily sized images (e.g. `UIImageView` with `contentMode` set to ``UIViewContentModeScaleAspectFit`), or check the image's size after loading it from the network.
 */
- (HZNativeAdImage * _Nullable)coverImageWithPreferredOrientation:(HZPreferredImageOrientation)preferredOrientation;

#pragma mark - Displaying the Native Ad

/**
 *  The view controller to present the web browser/app store from when the native ad is clicked. This property *must* be set to handle clicks properly. Set this property immediately after receiving your native ad.
 */
@property (nonatomic, weak, nullable) UIViewController *presentingViewController;

/**
 *  Tells the SDK which views are being used for your native ad.
 *
 *  @param block The block gives an object conforming to the `HZMediatedNativeAdViewRegisterer` protocol, which you can use to tell the SDK which views you use to display which native ad properties.
 *
 *  @see HZMediatedNativeAdViewRegisterer
 */
- (void)registerViews:(void(^)(id<HZMediatedNativeAdViewRegisterer> _Nonnull))block;

/**
 *  Each view you use as part of your native ad must be a subview of this view.
 *
 *  Implementation Details:
 *
 *  For AdMob, a `GADNativeContentAdView` or `GADNativeAppInstallAdView` is returned.
 *  For Facebook, the wrapper view is passed to `registerViewForInteraction:...`
 *  For Heyzap, the wrapper view automatically handles impression and click reporting.
 *
 *  @return The `UIView` to place your views in.
 */
- (UIView * _Nonnull)wrapperView;

#pragma mark - Native Ad Metadata

/**
 *  The name of the network we're mediating. This value will be one of the `HZNetwork` constants in HeyzapAds.h.
 */
@property (nonatomic, readonly, nonnull) NSString *mediatedNetwork;

/**
 *  The type of ad returned.
 *
 *  @see HZMediatedNativeAdType
 */
@property (nonatomic, readonly) HZMediatedNativeAdType adType;

/**
 *  The native ad we're mediating. Accessing this directly can be an effective way to get network-specific native ad properties.
 */
@property (nonatomic, readonly, nonnull) id underlyingNativeAd;

/**
 *  Whether the user has seen the ad.
 */
@property (nonatomic, readonly) BOOL hasHadImpression;

/**
 *  Whether the ad has been clicked yet.
 *
 *  @warning Because AdMob does not provide enough information to distinguish a click on the ad choices view and a click on the native ad content, all clicks are assumed to be on the native ad content.
 */
@property (nonatomic, readonly) BOOL hasBeenClicked;

@property (nonatomic, readonly, nonnull) NSString *tag;


#pragma mark - Facebook Configuration

// Set these properties immediately after receiving your native ad.

/**
 *  Whether or not to show an `FBAdChoicesView` in the wrapper view. Defaults to `YES`.
 *
 *  If you set this value to `NO`, you are responsible for displaying such a view.
 *  See https://developers.facebook.com/docs/audience-network/guidelines/native-ads for Facebook's native ad requirements.
 */
@property (nonatomic) BOOL shouldShowFacebookAdChoicesView;

/**
 *  The preferred corner to place the `FBAdChoicesView`. The default value is `UIRectCornerTopRight`.
 *
 *  @note `UIRectCornerAllCorners`, or any other bitmasked value, is not supported.
 */
@property (nonatomic) UIRectCorner facebookAdChoicesViewCorner;

#pragma mark - Utility

NSString * _Nonnull NSStringFromHZMediatedNativeAdType(HZMediatedNativeAdType adType);

@end

NS_ASSUME_NONNULL_END
