@interface HZUnityAdapterChartboostProxy : NSProxy

+ (void)showInterstitial:(NSString *)location;
+ (void)cacheInterstitial:(NSString *)location;
+ (BOOL)hasInterstitial:(NSString *)location;

@end