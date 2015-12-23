//
//  HZMediatedNativeAdViewRegisterer.h
//  Heyzap
//
//  Created by Maximilian Tagher on 10/27/15.
//  Copyright © 2015 Heyzap. All rights reserved.
//

#import <UIKit/UIKit.h>

/**
 *  Methods in this protocol tell the SDK which views you're using to create your native ad.
 *
 *  For AdMob, this causes the view properties on AdMob's wrapper view (`GADNativeContentAdView`/`GADNativeAppInstallAdView`) to be set.
 *  For Facebook, views you register as `tappable` are passed to Facebook as clickable views.
 *  For Heyzap, views you register as `tappable` will present an `SKStoreProductViewController` when tapped.
 */
@protocol HZMediatedNativeAdViewRegisterer <NSObject>

- (void)registerTitleView:(UIView *)view tappable:(BOOL)tappable;
- (void)registerBodyView:(UIView *)view tappable:(BOOL)tappable;
- (void)registerIconView:(UIView *)view tappable:(BOOL)tappable;
- (void)registerCoverImageView:(UIView *)view tappable:(BOOL)tappable;
- (void)registerCallToActionView:(UIView *)view;
- (void)registerAdvertiserNameView:(UIView *)view tappable:(BOOL)tappable;

- (void)registerOtherView:(UIView *)view tappable:(BOOL)tappable;
- (void)registerOtherViews:(NSArray <UIView *>*)views tappable:(BOOL)tappable;

@end
