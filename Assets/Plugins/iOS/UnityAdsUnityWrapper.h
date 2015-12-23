//
//  UnityAdsUnityWrapper.h
//  Copyright (c) 2013 Unity Technologies. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UnityAds/UnityAds.h>

extern UIViewController* UnityGetGLViewController();

@interface UnityAdsUnityWrapper : NSObject <UnityAdsDelegate> {
}

- (id)initWithGameId:(NSString*)gameId testModeOn:(bool)testMode debugModeOn:(bool)debugMode withGameObjectName:(NSString*)gameObjectName withUnityVersion:(NSString *)unityVersion;

@end
