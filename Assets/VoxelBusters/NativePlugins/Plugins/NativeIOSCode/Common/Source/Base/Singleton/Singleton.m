//
//  Singleton.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 22/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "Singleton.h"
#import "SingletonContainer.h"

@implementation Singleton

+ (id)Instance
{
    return [SingletonContainer GetSingletonInstance:self];
}

@end
