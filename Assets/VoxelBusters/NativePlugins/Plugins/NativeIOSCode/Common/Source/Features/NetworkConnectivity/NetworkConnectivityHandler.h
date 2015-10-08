//
//  NetworkConnectivityHandler.h
//  Unity-iPhone
//
//  Created by Ashwin kumar on 06/12/14.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "Reachability.h"
#import "HandlerBase.h"
#include <arpa/inet.h>

@interface NetworkConnectivityHandler : HandlerBase

// Properties
@property (nonatomic, assign)   bool                isHostReachable;
@property (nonatomic, assign)   bool                isWifiReachable;
@property (nonatomic, assign)   bool                isConnected;
@property (nonatomic, retain)   Reachability        *hostReachability;
@property (nonatomic, retain)   Reachability        *wifiReachability;

// Related to notification handlers
- (void)setNewIPAddress:(NSString *)newIPAddress;
- (void)reachabilityChanged:(NSNotification *)note;
- (void)updateInterfaceWithReachability:(Reachability *)curReach;
- (bool)checkIfReachable:(Reachability *)curReach;

@end