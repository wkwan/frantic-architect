//
//  SingletonContainer.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 23/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface SingletonContainer : NSObject

// Properties
@property(nonatomic, retain)	NSMutableDictionary		*instanceContainer;

// Static instance
+ (id)GetSingletonInstance:(Class)class;

@end
