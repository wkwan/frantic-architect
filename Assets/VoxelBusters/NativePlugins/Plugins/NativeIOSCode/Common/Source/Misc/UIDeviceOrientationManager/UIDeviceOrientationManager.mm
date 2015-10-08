//
//  UIDeviceOrientationManager.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 22/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "UIDeviceOrientationManager.h"

@implementation UIDeviceOrientationManager

@synthesize observers;
@synthesize currentOrientation;

+ (void)load
{
	UnityRegisterLifeCycleListener([UIDeviceOrientationManager Instance]);
}

- (id)init
{
	self	= [super init];
	
	if (self)
	{
		// Initialise
		self.observers	= [NSMutableArray array];
	}
	
	return self;
}

- (void)dealloc
{
	// Remove notification observer
	UnityUnregisterLifeCycleListener(self);
	[[NSNotificationCenter defaultCenter] removeObserver:self
													name:UIDeviceOrientationDidChangeNotification
												  object:[UIDevice currentDevice]];
	// Release
	self.observers	= NULL;
	[super dealloc];
}

#pragma mark - Application Callback

- (void)didFinishLaunching:(NSNotification*)notification
{
	UIDevice *device	= [UIDevice currentDevice];
	
	// Cache orientation
	[self setCurrentOrientation:[device orientation]];
	
	// Add as observer
	[[NSNotificationCenter defaultCenter] addObserver:self
											 selector:@selector(orientationChanged:)
												 name:UIDeviceOrientationDidChangeNotification
											   object:device];
}

#pragma mark - Orientation

- (void)orientationChanged:(NSNotification *)note
{
	UIDeviceOrientation toOrientation	= [[note object] orientation];
	UIDeviceOrientation fromOrientation	= self.currentOrientation;
	NSLog(@"[OrientationManager] new orientation: %d", toOrientation);
	
	for (NSValue *nonRetainedObserver in self.observers)
    {
		id<UIDeviceOrientationObserver> observer	= [nonRetainedObserver nonretainedObjectValue];
		
		if (observer)
			[observer didRotateToOrientation:toOrientation fromOrientation:fromOrientation];
    }
	
	// Update value orientation
	self.currentOrientation	= toOrientation;
}

#pragma mark - Observer

- (void)setObserver:(id<UIDeviceOrientationObserver>)newObserver
{
	NSValue *observerValue	= [NSValue valueWithNonretainedObject:newObserver];
	
	if (![self.observers containsObject:observerValue])
	{
		[self.observers addObject:observerValue];
	}
}

- (void)removeObserver:(id<UIDeviceOrientationObserver>)observer
{
	NSValue *observerValue	= [NSValue valueWithNonretainedObject:observer];

	if ([self.observers containsObject:observerValue])
	{
		[self.observers removeObject:observerValue];
	}
}

@end
