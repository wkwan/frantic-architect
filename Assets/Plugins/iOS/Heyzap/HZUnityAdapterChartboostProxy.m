#import "HZUnityAdapterChartboostProxy.h"

#pragma clang diagnostic ignored "-Wincomplete-implementation"
@implementation HZUnityAdapterChartboostProxy

+ (id)forwardingTargetForSelector:(SEL)aSelector
{
    return NSClassFromString(@"Chartboost");
}

@end