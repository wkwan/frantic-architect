/*
 * ChartboostManager.h
 * Chartboost
 *
 * Copyright 2011 Chartboost. All rights reserved.
 */

#import <Foundation/Foundation.h>

#if !defined(CB_UNITY_SDK_VERSION_STRING)
  #define CB_UNITY_SDK_VERSION_STRING @"6.0.2"
#endif


@interface ChartBoostManager : NSObject

@property (nonatomic) BOOL shouldPauseClick;
@property (nonatomic) BOOL shouldRequestFirstSession;

// Properties used by delegates
@property (nonatomic) BOOL hasCheckedWithUnityToDisplayInterstitial;
@property (nonatomic) BOOL hasCheckedWithUnityToDisplayRewardedVideo;
@property (nonatomic) BOOL hasCheckedWithUnityToDisplayMoreApps;
@property (nonatomic) BOOL unityResponseShouldDisplayInterstitial;
@property (nonatomic) BOOL unityResponseShouldDisplayRewardedVideo;
@property (nonatomic) BOOL unityResponseShouldDisplayMoreApps;

@property (nonatomic, retain) NSString *gameObjectName;

+ (ChartBoostManager*)sharedManager;

- (void)startChartBoostWithAppId:(NSString*)appId appSignature:(NSString*)appSignature unityVersion:(NSString *)unityVersion;

@end
