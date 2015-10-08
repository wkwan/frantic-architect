//
//  UnityAdsUnityWrapper.m
//  Copyright (c) 2013 Unity Technologies. All rights reserved.
//

#import "UnityAdsUnityWrapper.h"
#if UNITY_VERSION >= 420
#import "UnityAppController.h"
#else
#import "AppController.h"
#endif

static UnityAdsUnityWrapper *unityAds = NULL;

void UnitySendMessage(const char* obj, const char* method, const char* msg);
#if UNITY_VERSION >= 500
void UnityPause(int pause);
#else
void UnityPause(bool pause);
#endif

extern "C" {
  NSString* UnityAdsCreateNSString (const char* string) {
    return string ? [NSString stringWithUTF8String: string] : [NSString stringWithUTF8String: ""];
  }
  
  char* UnityAdsMakeStringCopy (const char* string) {
    if (string == NULL)
      return NULL;
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
  }
}

@interface UnityAdsUnityWrapper () <UnityAdsDelegate>
@property (nonatomic, strong) NSString* gameObjectName;
@property (nonatomic, strong) NSString* gameId;
@property (nonatomic, assign) BOOL appSelectorActive;
@property (nonatomic, assign) BOOL videoPlaying;
@end

@implementation UnityAdsUnityWrapper

- (id)initWithGameId:(NSString*)gameId testModeOn:(bool)testMode debugModeOn:(bool)debugMode withGameObjectName:(NSString*)gameObjectName withUnityVersion:(NSString*)unityVersion {
  self = [super init];
  
  if (self != nil) {
    self.gameObjectName = gameObjectName;
    self.gameId = gameId;
    self.appSelectorActive = false;
    self.videoPlaying = false;
    
    [[UnityAds sharedInstance] setDelegate:self];
    [[UnityAds sharedInstance] setDebugMode:debugMode];
    [[UnityAds sharedInstance] setTestMode:testMode];
    [[UnityAds sharedInstance] setUnityVersion:unityVersion];
    [[UnityAds sharedInstance] startWithGameId:gameId andViewController:UnityGetGLViewController()];
  }
  
  return self;
}

- (void)unityAdsVideoCompleted:(NSString *)rewardItemKey skipped:(BOOL)skipped {
  self.videoPlaying = false;
  NSString *parameters = [NSString stringWithFormat:@"%@;%@", rewardItemKey, skipped ? @"true" : @"false"];
  UnitySendMessage(UnityAdsMakeStringCopy([self.gameObjectName UTF8String]), "onVideoCompleted", [parameters UTF8String]);
}

- (void)unityAdsWillShow {
  [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(didBecomeActive:) name:UIApplicationDidBecomeActiveNotification object:[UIApplication sharedApplication]];
  [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(willResignActive:) name:UIApplicationWillResignActiveNotification object:[UIApplication sharedApplication]];
}

- (void)unityAdsDidShow {
  UnitySendMessage(UnityAdsMakeStringCopy([self.gameObjectName UTF8String]), "onShow", "");
#if UNITY_VERSION >= 500
  UnityPause(1);
#else
  UnityPause(true);
#endif
}

- (void)unityAdsWillHide {
}

- (void)unityAdsDidHide {
  self.videoPlaying = false;
#if UNITY_VERSION >= 500
  UnityPause(0);
#else
  UnityPause(false);
#endif
  UnitySendMessage(UnityAdsMakeStringCopy([self.gameObjectName UTF8String]), "onHide", "");
  [[NSNotificationCenter defaultCenter] removeObserver:self name:UIApplicationDidBecomeActiveNotification object:nil];
  [[NSNotificationCenter defaultCenter] removeObserver:self name:UIApplicationWillResignActiveNotification object:nil];
}

- (void)unityAdsWillLeaveApplication {
}

- (void)unityAdsVideoStarted {
  self.videoPlaying = true;
  UnitySendMessage(UnityAdsMakeStringCopy([self.gameObjectName UTF8String]), "onVideoStarted", "");
}

- (void)unityAdsFetchCompleted {
  UnitySendMessage(UnityAdsMakeStringCopy([self.gameObjectName UTF8String]), "onFetchCompleted", "");
}

- (void)unityAdsFetchFailed {
  UnitySendMessage(UnityAdsMakeStringCopy([self.gameObjectName UTF8String]), "onFetchFailed", "");
}

- (void)didBecomeActive:(NSNotification*)notification {
  if(self.appSelectorActive && self.videoPlaying) {
    [[UnityAds sharedInstance] hide];
    if([[UnityAds sharedInstance] respondsToSelector:@selector(refreshAds)]) {
      [[UnityAds sharedInstance] performSelector:@selector(refreshAds)];
    }
  }
  self.appSelectorActive = false;
}

- (void)willResignActive:(NSNotification*)notification {
  self.appSelectorActive = true;
}

extern "C" {
  void UnityAdsInit (const char *gameId, bool testMode, bool debugMode, const char *gameObjectName, const char* unityVersion) {
    if (unityAds == NULL) {
      unityAds = [[UnityAdsUnityWrapper alloc] initWithGameId:UnityAdsCreateNSString(gameId) testModeOn:testMode debugModeOn:debugMode withGameObjectName:UnityAdsCreateNSString(gameObjectName) withUnityVersion:UnityAdsCreateNSString(unityVersion)];
    }
  }

 	bool UnityAdsCanShowZone (const char * rawZoneId) {
    NSString * zoneId = UnityAdsCreateNSString(rawZoneId);

    return [[UnityAds sharedInstance] canShowZone:zoneId];
  }
 
	bool UnityAdsShow (const char * rawZoneId, const char * rawRewardItemKey, const char * rawOptionsString) {
    NSString * zoneId = UnityAdsCreateNSString(rawZoneId);
    NSString * rewardItemKey = UnityAdsCreateNSString(rawRewardItemKey);
    NSString * optionsString = UnityAdsCreateNSString(rawOptionsString);
    
    NSMutableDictionary *optionsDictionary = nil;
    if([optionsString length] > 0) {
      optionsDictionary = [[NSMutableDictionary alloc] init];
      [[optionsString componentsSeparatedByString:@","] enumerateObjectsUsingBlock:^(id rawOptionPair, NSUInteger idx, BOOL *stop) {
        NSArray *optionPair = [rawOptionPair componentsSeparatedByString:@":"];
        [optionsDictionary setValue:optionPair[1] forKey:optionPair[0]];
      }];
    }
    
    if ([[UnityAds sharedInstance] canShowZone:zoneId]) {
      if([zoneId length] > 0) {
        if([rewardItemKey length] > 0) {
          [[UnityAds sharedInstance] setZone:zoneId withRewardItem:rewardItemKey];
        } else {
          [[UnityAds sharedInstance] setZone:zoneId];
        }
      }

      [[UnityAds sharedInstance] setViewController:UnityGetGLViewController()];
      return [[UnityAds sharedInstance] show:optionsDictionary];
    }
    
    return false;
  }
	
	void UnityAdsHide () {
    [[UnityAds sharedInstance] hide];
  }
	
	bool UnityAdsIsSupported () {
    return [UnityAds isSupported];
  }
	
	const char* UnityAdsGetSDKVersion () {
    return UnityAdsMakeStringCopy([[UnityAds getSDKVersion] UTF8String]);
  }
  
	bool UnityAdsCanShow () {
    return [[UnityAds sharedInstance] canShow];
  }

	bool UnityAdsHasMultipleRewardItems () {
    return [[UnityAds sharedInstance] hasMultipleRewardItems];
  }
	
	const char* UnityAdsGetRewardItemKeys () {
    NSArray *keys = [[UnityAds sharedInstance] getRewardItemKeys];
    NSString *keyString = @"";
    
    for (NSString *key in keys) {
      if ([keyString length] <= 0) {
        keyString = [NSString stringWithFormat:@"%@", key];
      }
      else {
        keyString = [NSString stringWithFormat:@"%@;%@", keyString, key];
      }
    }
    
    return UnityAdsMakeStringCopy([keyString UTF8String]);
  }
  
	const char* UnityAdsGetDefaultRewardItemKey () {
    return UnityAdsMakeStringCopy([[[UnityAds sharedInstance] getDefaultRewardItemKey] UTF8String]);
  }
  
	const char* UnityAdsGetCurrentRewardItemKey () {
    return UnityAdsMakeStringCopy([[[UnityAds sharedInstance] getCurrentRewardItemKey] UTF8String]);
  }
  
	bool UnityAdsSetRewardItemKey (const char *rewardItemKey) {
    return [[UnityAds sharedInstance] setRewardItemKey:UnityAdsCreateNSString(rewardItemKey)];
  }
	
	void UnityAdsSetDefaultRewardItemAsRewardItem () {
    [[UnityAds sharedInstance] setDefaultRewardItemAsRewardItem];
  }
  
	const char* UnityAdsGetRewardItemDetailsWithKey (const char *rewardItemKey) {
    if (rewardItemKey != NULL) {
      NSDictionary *details = [[UnityAds sharedInstance] getRewardItemDetailsWithKey:UnityAdsCreateNSString(rewardItemKey)];
      return UnityAdsMakeStringCopy([[NSString stringWithFormat:@"%@;%@", [details objectForKey:kUnityAdsRewardItemNameKey], [details objectForKey:kUnityAdsRewardItemPictureKey]] UTF8String]);
    }
    return UnityAdsMakeStringCopy("");
  }
  
  const char *UnityAdsGetRewardItemDetailsKeys () {
    return UnityAdsMakeStringCopy([[NSString stringWithFormat:@"%@;%@", kUnityAdsRewardItemNameKey, kUnityAdsRewardItemPictureKey] UTF8String]);
  }
  
  void UnityAdsSetDebugMode(bool debugMode) {
    [[UnityAds sharedInstance] setDebugMode:debugMode];
  }

  void UnityAdsEnableUnityDeveloperInternalTestMode () {
  	[[UnityAds sharedInstance] enableUnityDeveloperInternalTestMode];
  }

  void UnityAdsSetCampaignDataURL (const char *campaignDataUrl) {
    [[UnityAds sharedInstance] setCampaignDataURL:UnityAdsCreateNSString(campaignDataUrl)];
  }

}

@end
