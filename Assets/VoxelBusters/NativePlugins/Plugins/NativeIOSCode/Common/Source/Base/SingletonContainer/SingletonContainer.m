//
//  SingletonContainer.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 23/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "SingletonContainer.h"

static SingletonContainer* sharedInstance = nil;

@implementation SingletonContainer

#pragma Properties

@synthesize instanceContainer;

#pragma Single Instance Methods

+ (SingletonContainer *)SharedInstance
{
    @synchronized([SingletonContainer class])
    {
        if (!sharedInstance)
            [[self alloc] init];
		
        return sharedInstance;
    }
	
    return nil;
}

+ (id)alloc
{
    @synchronized([SingletonContainer class])
    {
        NSAssert(sharedInstance == nil, @"Attempted to allocate a second instance of a singleton.");
        sharedInstance = [super alloc];
        return sharedInstance;
    }
	
    return nil;
}

- (id)init
{
	self	= [super init];
	
	if (self)
	{
		[self setInstanceContainer:[NSMutableDictionary dictionary]];
	}
	
	return self;
}

#pragma mark - Container Methods

+ (id)GetSingletonInstance:(Class)class
{
	// Get instance from container
	NSMutableDictionary *instanceContainer	= [[SingletonContainer SharedInstance] instanceContainer];
	NSString *className						= NSStringFromClass(class);
	id classInstance						= [instanceContainer objectForKey:className];
	
	@synchronized(class)
    {
        if (!classInstance)
		{
			// Create new instance
            classInstance	= [[[class alloc] init] autorelease];
			
			// Cache it in instance container
			[instanceContainer setObject:classInstance forKey:className];
		}
		
        return classInstance;
    }
	
	return NULL;
}

@end
