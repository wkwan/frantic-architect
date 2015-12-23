//
//  HeyzapUnitySDK.m
//
//  Copyright 2015 Smart Balloon, Inc. All Rights Reserved
//
//  Permission is hereby granted, free of charge, to any person
//  obtaining a copy of this software and associated documentation
//  files (the "Software"), to deal in the Software without
//  restriction, including without limitation the rights to use,
//  copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following
//  conditions:
//
//  The above copyright notice and this permission notice shall be
//  included in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//  OTHER DEALINGS IN THE SOFTWARE.
//

#import "HeyzapAds.h"
#import "HZInterstitialAd.h"
#import "HZVideoAd.h"
#import "HZIncentivizedAd.h"
#import "HZBannerAd.h"
#import "HZUnityAdapterChartboostProxy.h"
#import "HZLog.h"

extern void UnitySendMessage(const char *, const char *, const char *);

#define HZ_FRAMEWORK_NAME @"unity3d"

#define HZ_VIDEO_KLASS @"HZVideoAd"
#define HZ_INTERSTITIAL_KLASS @"HZInterstitialAd"
#define HZ_INCENTIVIZED_KLASS @"HZIncentivizedAd"
#define HZ_BANNER_KLASS @"HZBannerAd"

@interface HeyzapUnityAdDelegate : NSObject<HZAdsDelegate,HZIncentivizedAdDelegate,HZBannerAdDelegate>

@property (nonatomic, strong) NSString *klassName;

- (id) initWithKlassName: (NSString *) klassName;
- (void) sendMessageForKlass: (NSString *) klass withMessage: (NSString *) message andTag: (NSString *) tag;

@end

@implementation HeyzapUnityAdDelegate

- (id) initWithKlassName: (NSString *) klassName {
    self = [super init];
    if (self) {
        _klassName = klassName;
    }
    
    return self;
}

- (void) didReceiveAdWithTag:(NSString *)tag { [self sendMessageForKlass: self.klassName withMessage: @"available" andTag: tag]; }

- (void) didFailToReceiveAdWithTag:(NSString *)tag { [self sendMessageForKlass: self.klassName withMessage: @"fetch_failed" andTag: tag]; }

- (void) didShowAdWithTag:(NSString *)tag { [self sendMessageForKlass: self.klassName withMessage: @"show" andTag: tag]; }

- (void) didHideAdWithTag:(NSString *)tag { [self sendMessageForKlass: self.klassName withMessage:  @"hide" andTag: tag]; }

- (void) didFailToShowAdWithTag:(NSString *)tag andError:(NSError *)error { [self sendMessageForKlass: self.klassName withMessage:  @"failed" andTag: tag]; }

- (void) didClickAdWithTag:(NSString *)tag { [self sendMessageForKlass: self.klassName withMessage:  @"click" andTag: tag]; }

- (void) didCompleteAdWithTag: (NSString *) tag { [self sendMessageForKlass: self.klassName withMessage:  @"incentivized_result_complete" andTag: tag]; }

- (void) didFailToCompleteAdWithTag: (NSString *) tag { [self sendMessageForKlass: self.klassName withMessage:  @"incentivized_result_incomplete" andTag: tag]; }

- (void) willStartAudio { [self sendMessageForKlass: self.klassName  withMessage: @"audio_starting" andTag:  @""]; }

- (void) didFinishAudio { [self sendMessageForKlass: self.klassName withMessage:  @"audio_finished" andTag:  @""]; }

- (void)bannerDidReceiveAd:(HZBannerAd *)banner {
    [self sendMessageForKlass:self.klassName withMessage:@"loaded" andTag:banner.options.tag];
}

- (void)bannerDidFailToReceiveAd:(HZBannerAd *)banner error:(NSError *)error {
    if (banner != nil) {
        [self sendMessageForKlass:self.klassName withMessage:@"error" andTag:banner.options.tag];
    } else {
        [self sendMessageForKlass:self.klassName withMessage: @"error" andTag: @""];
    }
}

- (void)bannerWasClicked:(HZBannerAd *)banner {
    [self sendMessageForKlass:self.klassName withMessage:@"click" andTag:banner.options.tag];
}

- (void) sendMessageForKlass: (NSString *) klass withMessage: (NSString *) message andTag: (NSString *) tag {
    NSString *unityMessage = [NSString stringWithFormat: @"%@,%@", message, tag];
    UnitySendMessage([klass UTF8String], "SetCallback", [unityMessage UTF8String]);
}

@end

static HeyzapUnityAdDelegate *HZInterstitialDelegate = nil;
static HeyzapUnityAdDelegate *HZIncentivizedDelegate = nil;
static HeyzapUnityAdDelegate *HZVideoDelegate = nil;
static HeyzapUnityAdDelegate *HZBannerDelegate = nil;

static HZBannerAd *HZCurrentBannerAd = nil;

extern "C" {
    void hz_ads_start_app(const char *publisher_id, HZAdOptions flags) {
        static dispatch_once_t onceToken;
        dispatch_once(&onceToken, ^{
            NSString *publisherID = [NSString stringWithUTF8String: publisher_id];
            
            [HeyzapAds startWithPublisherID: publisherID andOptions: flags andFramework: HZ_FRAMEWORK_NAME];
            
            HZIncentivizedDelegate = [[HeyzapUnityAdDelegate alloc] initWithKlassName: HZ_INCENTIVIZED_KLASS];
            [HZIncentivizedAd setDelegate: HZIncentivizedDelegate];
            
            HZInterstitialDelegate = [[HeyzapUnityAdDelegate alloc] initWithKlassName: HZ_INTERSTITIAL_KLASS];
            [HZInterstitialAd setDelegate: HZInterstitialDelegate];
            
            HZVideoDelegate = [[HeyzapUnityAdDelegate alloc] initWithKlassName: HZ_VIDEO_KLASS];
            [HZVideoAd setDelegate: HZVideoDelegate];
            
            HZBannerDelegate = [[HeyzapUnityAdDelegate alloc] initWithKlassName: HZ_BANNER_KLASS];
            
            [HeyzapAds networkCallbackWithBlock:^(NSString *network, NSString *callback) {
                NSString *unityMessage = [NSString stringWithFormat: @"%@,%@", network, callback];
                NSString *klassName = @"HeyzapAds";
                UnitySendMessage([klassName UTF8String], "SetNetworkCallbackMessage", [unityMessage UTF8String]);
            }];
        });
    }
    
    void hz_ads_show_interstitial(const char *tag) {
        [HZInterstitialAd showForTag: [NSString stringWithUTF8String: tag]];
    }
    
    void hz_ads_hide_interstitial(void) {
        //[HZInterstitialAd hide];
    }
    
    void hz_ads_fetch_interstitial(const char *tag) {
        [HZInterstitialAd fetchForTag: [NSString stringWithUTF8String: tag]];
    }
    
    bool hz_ads_interstitial_is_available(const char *tag) {
        return [HZInterstitialAd isAvailableForTag: [NSString stringWithUTF8String: tag]];
    }
    
    void hz_ads_show_video(const char *tag) {
        [HZVideoAd showForTag: [NSString stringWithUTF8String: tag]];
    }
    
    void hz_ads_hide_video(void) {
        //[HZVideoAd hide];
    }
    
    void hz_ads_fetch_video(const char *tag) {
        [HZVideoAd fetchForTag: [NSString stringWithUTF8String: tag]];
    }
    
    bool hz_ads_video_is_available(const char *tag) {
        return [HZVideoAd isAvailableForTag:[NSString stringWithUTF8String:tag]];
    }
    
    void hz_ads_show_incentivized(const char *tag) {
        [HZIncentivizedAd showForTag: [NSString stringWithUTF8String: tag]];
    }

    void hz_ads_show_incentivized_with_custom_info(const char *tag, const char *customInfo) {
        HZShowOptions *showOptions = [HZShowOptions new];
        showOptions.tag = [NSString stringWithUTF8String: tag];
        showOptions.incentivizedInfo = [NSString stringWithUTF8String: customInfo];
        [HZIncentivizedAd showWithOptions:showOptions];
    }
    
    void hz_ads_hide_incentivized() {
        //[HZIncentivizedAd hide];
    }
    
    void hz_ads_fetch_incentivized(const char *tag) {
        [HZIncentivizedAd fetchForTag: [NSString stringWithUTF8String: tag]];
    }
    
    bool hz_ads_incentivized_is_available(const char *tag) {
        return [HZIncentivizedAd isAvailableForTag: [NSString stringWithUTF8String: tag]];
    }
    
    void hz_ads_show_banner(const char *position, const char *tag) {
        if (!HZCurrentBannerAd) {
            HZBannerPosition pos = HZBannerPositionBottom;
            NSString *positionStr = [NSString stringWithUTF8String: position];
            if ([positionStr isEqualToString: @"top"]) {
                pos = HZBannerPositionTop;
            }
            
            HZBannerAdOptions *options = [[HZBannerAdOptions alloc] init];
            options.tag = [NSString stringWithUTF8String:tag];
            
            [HZBannerAd placeBannerInView:nil position:pos options:options success:^(HZBannerAd *banner) {
                if (!HZCurrentBannerAd) {
                    HZCurrentBannerAd = banner;
                    [HZCurrentBannerAd setDelegate: HZBannerDelegate];
                    [HZBannerDelegate sendMessageForKlass:[HZBannerDelegate klassName] withMessage:@"loaded" andTag:banner.options.tag];
                } else {
                    [banner removeFromSuperview];
                    NSLog(@"Requested a banner before the previous one was destroyed. Ignoring this request.");
                }

            } failure:^(NSError *error) {
                NSLog(@"Error fetching banner; error = %@",error);
                [HZBannerDelegate bannerDidFailToReceiveAd: nil error: error];
            }];
        } else {
            // Unhide the banner
            [HZCurrentBannerAd setHidden: NO];
        }
    }
    
    void hz_ads_hide_banner(void) {
        if (HZCurrentBannerAd) {
            [HZCurrentBannerAd setHidden: YES];
            
        } else {
            NSLog(@"Can't hide banner, there is no banner ad currently loaded.");
        }
    }
    
    void hz_ads_destroy_banner(void) {
        if (HZCurrentBannerAd) {
            [HZCurrentBannerAd removeFromSuperview];
            HZCurrentBannerAd = nil;
            
        } else {
            NSLog(@"Can't destroy banner, there is no banner ad currently loaded.");
        }
    }
    
    char * hz_ads_banner_dimensions(void) {
        if (HZCurrentBannerAd) {
            const char * dims = [[HZCurrentBannerAd dimensionsDescription] UTF8String];
            if (dims == NULL) {
                return NULL;
            }
            
            char* returnValue = (char*)malloc(strlen(dims) + 1);
            strcpy(returnValue, dims);
            return returnValue;
            
        } else {
            NSLog(@"Can't get banner dimensions, there is no banner ad currently loaded.");
        }
        
        return NULL;
    }
    
    char * hz_ads_get_remote_data(void){
      NSString *remoteData = [HeyzapAds getRemoteDataJsonString];
      const char* remoteString = [remoteData UTF8String];
      char* returnValue = (char*)malloc(sizeof(char)*(strlen(remoteString) + 1));
      strcpy(returnValue, remoteString);
      return returnValue;
    }
    
    void hz_ads_show_mediation_debug_view_controller(void) {
        [HeyzapAds presentMediationDebugViewController];
    }
    
    bool hz_ads_is_network_initialized(const char *network) {
        return [HeyzapAds isNetworkInitialized: [NSString stringWithUTF8String: network]];
    }
    
    void hz_pause_expensive_work(void) {
        [HeyzapAds pauseExpensiveWork];
    }
    
    void hz_resume_expensive_work(void) {
        [HeyzapAds resumeExpensiveWork];
    }
    
    void hz_ads_show_debug_logs(void) {
        [HZLog setDebugLevel:HZDebugLevelVerbose];
    }
    
    void hz_ads_hide_debug_logs(void) {
        [HZLog setDebugLevel:HZDebugLevelSilent];
    }
    
    void hz_ads_show_third_party_debug_logs(void) {
        [HZLog setThirdPartyLoggingEnabled:YES];
    }
    
    void hz_ads_hide_third_party_debug_logs(void) {
        [HZLog setThirdPartyLoggingEnabled:NO];
    }
    
    BOOL hz_chartboost_enabled(void) {
        return [HeyzapAds isNetworkInitialized:HZNetworkChartboost];
    }
    
    // Calling hz_fetch_chartboost_for_location recursively won't keep the `const char *` in memory
    // Since I want to call it recursively, I immediately convert to an `NSString *` in `hz_fetch_chartboost_for_location`
    void hz_fetch_chartboost_for_location_objc(NSString *location) {
        if (!hz_chartboost_enabled()) {
            HZDLog(@"Chartboost not enabled; retrying in 0.25 seconds");
            dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(0.25 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
                hz_fetch_chartboost_for_location_objc(location);
            });
            return;
        }
        HZDLog(@"Caching Chartboost interstitial for location: %@",location);
        [HZUnityAdapterChartboostProxy cacheInterstitial:location];
    }
    
    void hz_fetch_chartboost_for_location(const char *location) {
        NSString *nsLocation = [NSString stringWithUTF8String:location];
        hz_fetch_chartboost_for_location_objc(nsLocation);
    }
    
    bool hz_chartboost_is_available_for_location(const char *location) {
        NSString *nsLocation = [NSString stringWithUTF8String:location];
        if (!hz_chartboost_enabled()) {
            HZDLog(@"Chartboost ad is not available because it is not enabled");
            return NO;
        }
        const BOOL hasAd = [HZUnityAdapterChartboostProxy hasInterstitial:nsLocation];
        HZDLog(@"Chartboost says it has an ad = %i",hasAd);
        return hasAd;
    }
    
    void hz_show_chartboost_for_location(const char *location) {
        NSString *nsLocation = [NSString stringWithUTF8String:location];
        
        if (!hz_chartboost_enabled()) {
            HZDLog(@"Chartboost not enabled yet; not able to show ad.");
            return;
        }
        HZDLog(@"Requesting Chartboost show interstitial for location: %@",nsLocation);
        [HZUnityAdapterChartboostProxy showInterstitial:nsLocation];
    }
}
