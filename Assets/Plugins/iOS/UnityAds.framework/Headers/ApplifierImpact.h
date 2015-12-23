//
//  ApplifierImpact.h
//  Copyright (c) 2012 Unity Technologies. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>

extern NSString * const kApplifierImpactRewardItemPictureKey;
extern NSString * const kApplifierImpactRewardItemNameKey;

extern NSString * const kApplifierImpactOptionNoOfferscreenKey;
extern NSString * const kApplifierImpactOptionOpenAnimatedKey;
extern NSString * const kApplifierImpactOptionGamerSIDKey;
extern NSString * const kApplifierImpactOptionMuteVideoSounds;
extern NSString * const kApplifierImpactOptionVideoUsesDeviceOrientation;

@class ApplifierImpact;

@protocol ApplifierImpactDelegate <NSObject>

@required
- (void)applifierImpact:(ApplifierImpact *)applifierImpact completedVideoWithRewardItemKey:(NSString *)rewardItemKey videoWasSkipped:(BOOL)skipped;

@optional
- (void)applifierImpactWillOpen:(ApplifierImpact *)applifierImpact;
- (void)applifierImpactDidOpen:(ApplifierImpact *)applifierImpact;
- (void)applifierImpactWillClose:(ApplifierImpact *)applifierImpact;
- (void)applifierImpactDidClose:(ApplifierImpact *)applifierImpact;
- (void)applifierImpactWillLeaveApplication:(ApplifierImpact *)applifierImpact;
- (void)applifierImpactVideoStarted:(ApplifierImpact *)applifierImpact;
- (void)applifierImpactCampaignsAreAvailable:(ApplifierImpact *)applifierImpact;
- (void)applifierImpactCampaignsFetchFailed:(ApplifierImpact *)applifierImpact;
@end

@interface ApplifierImpact : NSObject

@property (nonatomic, weak) id<ApplifierImpactDelegate> delegate;

+ (ApplifierImpact *)sharedInstance;
+ (BOOL)isSupported;
+ (NSString *)getSDKVersion;

- (void)setTestDeveloperId:(NSString *)developerId;
- (void)setTestOptionsId:(NSString *)optionsId;
- (void)setDebugMode:(BOOL)debugMode;
- (void)setTestMode:(BOOL)testModeEnabled;

- (BOOL)isDebugMode;
- (BOOL)startWithGameId:(NSString *)gameId andViewController:(UIViewController *)viewController;
- (BOOL)startWithGameId:(NSString *)gameId;
- (void)setViewController:(UIViewController *)viewController showImmediatelyInNewController:(BOOL)applyImpact;
- (BOOL)canShowCampaigns;
- (BOOL)canShowImpact;
- (BOOL)setZone:(NSString *)zoneId;
- (BOOL)setZone:(NSString *)zoneId withRewardItem:(NSString *)rewardItemKey;
- (BOOL)showImpact:(NSDictionary *)options;
- (BOOL)showImpact;
- (BOOL)hideImpact;
- (void)stopAll;
- (BOOL)hasMultipleRewardItems;
- (NSArray *)getRewardItemKeys;
- (NSString *)getDefaultRewardItemKey;
- (NSString *)getCurrentRewardItemKey;
- (BOOL)setRewardItemKey:(NSString *)rewardItemKey;
- (void)setDefaultRewardItemAsRewardItem;
- (NSDictionary *)getRewardItemDetailsWithKey:(NSString *)rewardItemKey;
@end
