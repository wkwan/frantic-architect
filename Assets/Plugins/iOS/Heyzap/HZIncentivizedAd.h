/*
 * Copyright (c) 2015, Heyzap, Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 * * Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * * Redistributions in binary form must reproduce the above copyright
 *   notice, this list of conditions and the following disclaimer in the
 *   documentation and/or other materials provided with the distribution.
 *
 * * Neither the name of 'Heyzap, Inc.' nor the names of its contributors
 *   may be used to endorse or promote products derived from this software
 *   without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
 * TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


@protocol HZIncentivizedAdDelegate;

#import <Foundation/Foundation.h>
#import "HeyzapAds.h"
#import "HZShowOptions.h"

@class HZShowOptions;

/** HZIncentivizedAd is responsible for fetching and showing incentivized video ads. All methods on this class must be called from the main queue. */
@interface HZIncentivizedAd : NSObject

+ (void)setDelegate:(id<HZIncentivizedAdDelegate>)delegate;

/** Shows an incentivized video ad if one is available. */
+ (void) show;

/** Shows an incentivized video ad if one with the particlar tag is available.
  *
  * @param tag Tag name describing the location or context for the ad to be shown.
  */
+ (void) showForTag: (NSString *) tag;

/** Shows an incentivized video with the given options.
 *
 * @param options HZShowOptions object containing properties for configuring how the ad is shown.
 */
+ (void) showWithOptions: (HZShowOptions *) options;

/** Fetches an incentivized video ad from Heyzap. */
+ (void) fetch;

/**
 *  Fetches an incentivized video ad from Heyzap.
 *
 *  @param tag An identifier for the location/context of the ad which you can use to disable the ad from your dashboard.
 */

+ (void) fetchForTag:(NSString *)tag;

/**
 *  Fetches an incentivized video ad from Heyzap.
 *
 *  @param completion A block called when the video is fetched or fails to fetch. `result` states whether the fetch was sucessful; the error object describes the issue, if there was one.
 */
+ (void) fetchWithCompletion: (void (^)(BOOL result, NSError *error))completion;

/**
 *  Fetches an incentivized video ad from Heyzap with a tag.
 *  @param tag Tag name describing the location or context for the ad to be shown.
 *  @param completion A block called when the video is fetched or fails to fetch. `result` states whether the fetch was sucessful; the error object describes the issue, if there was one.
 */
+ (void) fetchForTag: (NSString *) tag withCompletion:(void (^)(BOOL, NSError *))completion;


/**
 *  Fetches an incentivized video ad for each of the given tags.
 *
 *  @param tags An NSArray of NSString* identifiers for the location of ads which you can use to disable ads from your dashboard.
 */
+ (void) fetchForTags:(NSArray *)tags;


/**
 *  Fetches an incentivized video ad for each of the given tags with an optional completion handler.
 *
 *  @param tag        An NSArray of NSString* identifiers for the location of ads which you can use to disable ads from your dashboard.
 *  @param completion A block called when an ad for each tag is fetched or fails to fetch. `result` states whether the fetch was sucessful; the error object describes the issue, if there was one.
 */
+ (void) fetchForTags:(NSArray *)tags withCompletion:(void (^)(BOOL result, NSError *error))completion;


/**
 *  Whether or not an incentivized video ad is ready to show.
 *
 *  @return If an incentivized video ad is ready to show.
 */
+ (BOOL) isAvailable;

/**
 *  Whether or not an incentivized video ad is ready to show for the given tag.
 *
 *  @param tag Tag name describing the location or context for the ad to be shown.
 *  
 *  @return If an incentivized video ad is ready to show.
 */
+ (BOOL) isAvailableForTag: (NSString *) tag;

/**
 *  (Optional) As a layer of added security, you can specify an identifier for the user. You can opt to receive a server-to-server callback with the provided userIdentifier.
 *
 *  @param userIdentifier Any unique identifier, like a username, email, or ID that your server-side database uses.
 *  @deprecated This method has been deprecated and may be removed in a future version of the SDK.
 */
+ (void) setUserIdentifier: (NSString *) userIdentifier __attribute__((deprecated("Please use the `incentivizedInfo` string that can be passed to calls to `showWithOptions:` instead if you want to pass information to your server regarding rewarded videos. More info about this feature can be found at https://developers.heyzap.com/docs/advanced-publishing ")));

+ (void) setCreativeID: (int) creativeID;
@end
