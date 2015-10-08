//
//  UIDeviceOrientationManager.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 22/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "Singleton.h"
#include "LifeCycleListener.h"

@protocol UIDeviceOrientationObserver <NSObject>

- (void)didRotateToOrientation:(UIDeviceOrientation)toOrientation fromOrientation:(UIDeviceOrientation)fromOrientation;

@end

@interface UIDeviceOrientationManager : Singleton <LifeCycleListener>

// Properties
@property(nonatomic, retain) 	NSMutableArray		 	*observers;
@property(nonatomic)			UIDeviceOrientation		currentOrientation;

// Observer
- (void)setObserver:(id <UIDeviceOrientationObserver>)observer;

@end
