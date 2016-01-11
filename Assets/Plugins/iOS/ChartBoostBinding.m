/*
 * ChartboostBinding.m
 * Chartboost
 *
 * Copyright 2011 Chartboost. All rights reserved.
 */

#import "ChartBoostManager.h"
#import "CBAnalytics.h"
#import "CBInPlay.h"


// Converts C style string to NSString
#define GetStringParam(_x_) (_x_ != NULL) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Converts C style string to NSString as long as it isnt empty
#define GetStringParamOrNil(_x_) (_x_ != NULL && strlen(_x_)) ? [NSString stringWithUTF8String:_x_] : nil

// InPlayAds Dictionary
NSMutableDictionary * InPlayAds = nil;

static char* MakeStringCopy(const char* string) {
    if (string == NULL) return NULL;
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

void _chartBoostInit(const char *appId, const char *appSignature, const char *unityVersion)
{
    [[ChartBoostManager sharedManager] startChartBoostWithAppId: GetStringParam(appId) 
        appSignature: GetStringParam(appSignature) 
        unityVersion: GetStringParam(unityVersion)];
}

BOOL _chartBoostIsAnyViewVisible()
{
    return [Chartboost isAnyViewVisible];
}

void _chartBoostCacheInterstitial(const char *location)
{
    [Chartboost cacheInterstitial: GetStringParamOrNil(location)];
}

BOOL _chartBoostHasInterstitial(const char *location)
{
	return [Chartboost hasInterstitial: GetStringParamOrNil(location)];
}

void _chartBoostShowInterstitial(const char *location)
{
    [Chartboost showInterstitial: GetStringParamOrNil(location)];
}


void _chartBoostCacheRewardedVideo(const char *location)
{
    [Chartboost cacheRewardedVideo: GetStringParamOrNil(location)];
}


BOOL _chartBoostHasRewardedVideo(const char *location)
{
	return [Chartboost hasRewardedVideo: GetStringParamOrNil(location)];
}


void _chartBoostShowRewardedVideo(const char *location)
{
    [Chartboost showRewardedVideo: GetStringParamOrNil(location)];
}


void _chartBoostCacheMoreApps(const char *location)
{
    [Chartboost cacheMoreApps: GetStringParamOrNil(location)];
}

BOOL _chartBoostHasMoreApps(const char *location)
{
	return [Chartboost hasMoreApps: GetStringParamOrNil(location)];
}

void _chartBoostShowMoreApps(const char *location)
{
    [Chartboost showMoreApps: GetStringParamOrNil(location)];
}

void _chartBoostCacheInPlay(const char *location)
{
    [Chartboost cacheInPlay: GetStringParamOrNil(location)];
}

BOOL _chartBoostHasInPlay(const char *location)
{
    return [Chartboost hasInPlay: GetStringParamOrNil(location)];
}

void * _chartBoostGetInPlay(const char *location)
{
    CBInPlay * inPlayAd = [Chartboost getInPlay: GetStringParamOrNil(location)];
    // If inPlay Ad isnt found just return a -1
    if(inPlayAd == NULL) {
        return NULL;
    }
    // Else return the address of the inPlayAd as an int, which can be used as a unique id
    // Also store the object in a dictionary so that it can later be deleted
    if (InPlayAds == nil) {
        InPlayAds = [[NSMutableDictionary alloc] init];
    }
    [InPlayAds setObject:inPlayAd forKey:[NSNumber numberWithInt:(long)inPlayAd]];
    return (void *)inPlayAd;
}

void _chartBoostInPlayClick(const void * uniqueId)
{
    CBInPlay * inPlayAd = (CBInPlay *)uniqueId;
    [inPlayAd click];
}

void _chartBoostInPlayShow(const void * uniqueId)
{
    CBInPlay * inPlayAd = (CBInPlay *)uniqueId;
    [inPlayAd show];
}

void * _chartBoostInPlayGetAppIcon(const void * uniqueId)
{
    CBInPlay * inPlayAd = (CBInPlay *)uniqueId;
    return (void *)[[inPlayAd appIcon] bytes];
}

int _chartBoostInPlayGetAppIconSize(const void * uniqueId)
{
    CBInPlay * inPlayAd = (CBInPlay *)uniqueId;
    return (int)[[inPlayAd appIcon] length];
}

char* _chartBoostInPlayGetAppName(const void * uniqueId)
{
    CBInPlay * inPlayAd = (CBInPlay *)uniqueId;
    return MakeStringCopy([inPlayAd appName].UTF8String);
}

void _chartBoostFreeInPlayObject(const void * uniqueId)
{
    [InPlayAds removeObjectForKey:[NSNumber numberWithInt:(long)uniqueId]];
}

void _chartBoostSetCustomId(const char *ID)
{
    [Chartboost setCustomId: GetStringParamOrNil(ID)];
}


void _chartBoostDidPassAgeGate(BOOL pass)
{
    [Chartboost didPassAgeGate: pass];
}

char* _chartBoostGetCustomId()
{
    return MakeStringCopy([Chartboost getCustomId].UTF8String);
}


void _chartBoostHandleOpenURL(const char *url, const char *sourceApp)
{
    NSString *urlString = GetStringParamOrNil(url);
    if (!urlString)
        return;
    [Chartboost handleOpenURL: [NSURL URLWithString: urlString] sourceApplication: GetStringParamOrNil(sourceApp)];
}

void _chartBoostSetShouldPauseClickForConfirmation(BOOL pause)
{
    [Chartboost setShouldPauseClickForConfirmation:pause];
}

void _chartBoostSetShouldRequestInterstitialsInFirstSession(BOOL request)
{
    [Chartboost setShouldRequestInterstitialsInFirstSession:request];
}

// Functions called by the delegates
void _chartBoostShouldDisplayInterstitialCallbackResult(BOOL result)
{
    [ChartBoostManager sharedManager].unityResponseShouldDisplayInterstitial = result;
    if(!result)
    {
        [ChartBoostManager sharedManager].hasCheckedWithUnityToDisplayInterstitial = NO;
    }
}

void _chartBoostShouldDisplayRewardedVideoCallbackResult(BOOL result)
{
    [ChartBoostManager sharedManager].unityResponseShouldDisplayRewardedVideo = result;
    if(!result)
    {
        [ChartBoostManager sharedManager].hasCheckedWithUnityToDisplayRewardedVideo = NO;
    }
}

void _chartBoostShouldDisplayMoreAppsCallbackResult(BOOL result)
{
    [ChartBoostManager sharedManager].unityResponseShouldDisplayMoreApps = result;
    if(!result)
    {
        [ChartBoostManager sharedManager].hasCheckedWithUnityToDisplayMoreApps = NO;
    }
}

BOOL _chartBoostGetAutoCacheAds()
{
    return [Chartboost getAutoCacheAds];
}

void _chartBoostSetAutoCacheAds(BOOL autoCacheAds)
{
    [Chartboost setAutoCacheAds:autoCacheAds];
}

void _chartBoostSetStatusBarBehavior(int statusBarBehavior)
{
	[Chartboost setStatusBarBehavior:(CBStatusBarBehavior)statusBarBehavior];
}

void _chartBoostSetShouldDisplayLoadingViewForMoreApps(BOOL shouldDisplay)
{
    [Chartboost setShouldDisplayLoadingViewForMoreApps:shouldDisplay];
}

void _chartBoostSetShouldPrefetchVideoContent(BOOL shouldPrefetch)
{
    [Chartboost setShouldPrefetchVideoContent:shouldPrefetch];
}

void _chartBoostTrackInAppPurchaseEvent(const char * receipt, const char * productTitle, const char * productDescription, const char * productPrice, const char * productCurrency, const char * productIdentifier)
{
    // The API was vague previously about what the receipt string meant.
    // We now expect a base64 encoded string.
    [CBAnalytics trackInAppPurchaseEventWithString:GetStringParamOrNil(receipt) productTitle:GetStringParamOrNil(productTitle) productDescription:GetStringParamOrNil(productDescription) productPrice:[NSDecimalNumber decimalNumberWithString:GetStringParamOrNil(productPrice)] productCurrency:GetStringParamOrNil(productCurrency) productIdentifier:GetStringParamOrNil(productIdentifier)];
}

void _chartBoostSetGameObjectName(const char *name)
{
    [ChartBoostManager sharedManager].gameObjectName = GetStringParam(name);
}

void _chartBoostTrackLevelInfo(const char * eventLabel, int levelType, int mainLevel, int subLevel, const char * description)
{
    [CBAnalytics trackLevelInfo:GetStringParamOrNil(eventLabel) eventField:(CBLevelType)levelType mainLevel:mainLevel subLevel:subLevel description:GetStringParamOrNil(description)];
}