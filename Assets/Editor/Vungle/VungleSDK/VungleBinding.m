//
//  VungleBinding.m
//  VungleTest
//
//
#import <VungleSDK/VungleSDK.h>
#import "VungleManager.h"


// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Converts C style string to NSString as long as it isnt empty
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil

#define VUNGLE_API_KEY   @"vungle.api_endpoint"

void UnitySendMessage( const char * className, const char * methodName, const char * param );

static BOOL bInit = false;
void _vungleStartWithAppId( const char * appId, const char * pluginVersion )
{
	if (bInit)
		return;
	if( [[VungleSDK sharedSDK] respondsToSelector:@selector(setPluginName:version:)] )
		[[VungleSDK sharedSDK] performSelector:@selector(setPluginName:version:) withObject:@"unity" withObject:GetStringParam(pluginVersion)];

	[[VungleManager sharedManager] startWithAppId:GetStringParam( appId )];
	bInit = true;
    
    [[VungleSDK sharedSDK] setLoggingEnabled:true];
    [[VungleSDK sharedSDK] attachLogger:[VungleManager sharedManager]];
}


void _vungleSetSoundEnabled( BOOL enabled )
{
	[VungleSDK sharedSDK].muted = !enabled;
}


void _vungleEnableLogging( BOOL shouldEnable )
{
	[[VungleSDK sharedSDK] setLoggingEnabled:shouldEnable];
}


BOOL _vungleIsAdAvailable()
{
	return [[VungleSDK sharedSDK] isAdPlayable];
}

UIInterfaceOrientationMask makeOrientation(NSNumber* code) {
    UIInterfaceOrientationMask orientationMask;
    int i = [code intValue];
    switch( i )
    {
        case 0:
            orientationMask = UIInterfaceOrientationMaskPortrait;
            break;
        case 1:
            orientationMask = UIInterfaceOrientationMaskLandscapeLeft;
            break;
        case 2:
            orientationMask = UIInterfaceOrientationMaskLandscapeRight;
            break;
        case 3:
            orientationMask = UIInterfaceOrientationMaskPortraitUpsideDown;
            break;
        case 4:
            orientationMask = UIInterfaceOrientationMaskLandscape;
            break;
        case 5:
            orientationMask = UIInterfaceOrientationMaskAll;
            break;
        case 6:
            orientationMask = UIInterfaceOrientationMaskAllButUpsideDown;
            break;
        default:
            orientationMask = UIInterfaceOrientationMaskAllButUpsideDown;
    }
    return orientationMask;
}

void _vunglePlayAdWithOptions( BOOL incentivized, int orientation, const char * user)
{
    NSMutableDictionary *options = [NSMutableDictionary dictionary];
	[options setObject:@(incentivized) forKey:VunglePlayAdOptionKeyIncentivized];
    options[VunglePlayAdOptionKeyOrientations] = @(makeOrientation([NSNumber numberWithInteger: orientation]));

	NSString *userString = GetStringParamOrNil( user );
	if( userString )
		[options setObject:userString forKey:VunglePlayAdOptionKeyUser];

	[[VungleManager sharedManager] playAdWithOptions:options];
}

void _vunglePlayAdEx( BOOL incentivized, int orientation, BOOL large, const char * user,
                              const char * alerTitle, const char * alertText, const char * alertClose,
                              const char * alertContinue)
{
    NSMutableDictionary *options = [NSMutableDictionary dictionary];
    [options setObject:@(incentivized) forKey:VunglePlayAdOptionKeyIncentivized];
    options[VunglePlayAdOptionKeyOrientations] = @(makeOrientation([NSNumber numberWithInteger: orientation]));
    [options setObject:@(large) forKey:VunglePlayAdOptionKeyLargeButtons];
    
    NSString *userString = GetStringParamOrNil( user );
    if( userString )
        [options setObject:userString forKey:VunglePlayAdOptionKeyUser];
    
    NSString *alerTitleString = GetStringParamOrNil( alerTitle );
    if( alerTitleString )
        [options setObject:alerTitleString forKey:VunglePlayAdOptionKeyIncentivizedAlertTitleText];
    
    NSString *alertTextString = GetStringParamOrNil( alertText );
    if( alertTextString )
        [options setObject:alertTextString forKey:VunglePlayAdOptionKeyIncentivizedAlertBodyText];
    
    NSString *alertCloseString = GetStringParamOrNil( alertClose );
    if( alertCloseString )
        [options setObject:alertCloseString forKey:VunglePlayAdOptionKeyIncentivizedAlertCloseButtonText];
    
    NSString *alertContinueString = GetStringParamOrNil( alertContinue );
    if( alertContinueString )
        [options setObject:alertContinueString forKey:VunglePlayAdOptionKeyIncentivizedAlertContinueButtonText];
    
    [[VungleManager sharedManager] playAdWithOptions:options];
}

void _vunglePlayAdWithOptionsEx( char* opt ) {
    NSObject* obj = [VungleManager objectFromJson:GetStringParam( opt )];
    if([obj isKindOfClass:[NSDictionary class]])
    {
        NSDictionary *from = obj;
        NSMutableDictionary *options = [NSMutableDictionary dictionary];
        options[VunglePlayAdOptionKeyIncentivized] = from[@"incentivized"];
        options[VunglePlayAdOptionKeyOrientations] = @(makeOrientation(from[@"orientation"]));
        options[VunglePlayAdOptionKeyLargeButtons] = from[@"large"];
        if (from[@"userTag"])
            options[VunglePlayAdOptionKeyUser]  = from[@"userTag"];
        if (from[@"alertTitle"])
            options[VunglePlayAdOptionKeyIncentivizedAlertTitleText] = from[@"alertTitle"];
        if (from[@"alertText"])
            options[VunglePlayAdOptionKeyIncentivizedAlertBodyText] = from[@"alertText"];
        if (from[@"closeText"])
            options[VunglePlayAdOptionKeyIncentivizedAlertCloseButtonText] = from[@"closeText"];
        if (from[@"continueText"])
            options[VunglePlayAdOptionKeyIncentivizedAlertContinueButtonText] = from[@"continueText"];
        options[VunglePlayAdOptionKeyPlacement] = from[@"placement"];
        NSMutableDictionary *extra = [NSMutableDictionary dictionary];
        if (from[@"key1"])
            extra[VunglePlayAdOptionKeyExtra1] = from[@"key1"];
        if (from[@"key2"])
            extra[VunglePlayAdOptionKeyExtra2] = from[@"key2"];
        if (from[@"key3"])
            extra[VunglePlayAdOptionKeyExtra3] = from[@"key3"];
        if (from[@"key4"])
            extra[VunglePlayAdOptionKeyExtra4] = from[@"key4"];
        if (from[@"key5"])
            extra[VunglePlayAdOptionKeyExtra5] = from[@"key5"];
        if (from[@"key6"])
            extra[VunglePlayAdOptionKeyExtra6] = from[@"key6"];
        if (from[@"key7"])
            extra[VunglePlayAdOptionKeyExtra7] = from[@"key7"];
        if (from[@"key8"])
            extra[VunglePlayAdOptionKeyExtra8] = from[@"key8"];
        options[VunglePlayAdOptionKeyExtraInfoDictionary] = extra;
        [[VungleManager sharedManager] playAdWithOptions:options];
    }
}

void _vungleClearCache( )
{
    [[VungleSDK sharedSDK] clearCache];
}

void _vungleClearSleep( )
{
    [[VungleSDK sharedSDK] clearSleep];
}

void _vungleSetEndPoint(const char * endPoint) {
    NSString *endPointString = GetStringParamOrNil( endPoint );
    if (endPointString) {
        NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
        [defaults setObject:endPointString forKey:VUNGLE_API_KEY];
    }
}

char* MakeStringCopy (const char* string) {
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

char * _vungleGetEndPoint() {
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
    return MakeStringCopy([[defaults objectForKey:VUNGLE_API_KEY] UTF8String]);
}


