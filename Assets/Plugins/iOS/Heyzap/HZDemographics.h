//
//  HZDemographics.h
//  Heyzap
//
//  Created by Maximilian Tagher on 12/2/15.
//  Copyright © 2015 Heyzap. All rights reserved.
//

#import <Foundation/Foundation.h>

@class CLLocation;

/**
 *  Set the properties on this class to pass information about the user to each of the mediated ad networks.
 */
@interface HZDemographics : NSObject

/**
 *  The user's current location.
 *
 *  Networks who use this information: AdColony, AdMob, AppLovin, InMobi.
 */
@property (nonatomic) CLLocation *location;

@end
