//
//  GKAchievementReporter.mm
//
//
//  Created by Oscar Fridh on 24/06/15.
//
//

#import <GameKit/GameKit.h>

extern "C"
{
    void _ReportAchievement(const char *achievementID, float progress, bool showsCompletionBanner) {
        
        GKAchievement *achievement = [[GKAchievement alloc] initWithIdentifier:[NSString stringWithUTF8String:achievementID]];
        achievement.percentComplete = progress;
        achievement.showsCompletionBanner = showsCompletionBanner;
        
        
        [GKAchievement reportAchievements:@[achievement] withCompletionHandler:^(NSError *error) {
            
            if(error != nil) {
                
                NSLog(@"Error reporting achievement: %@", error.localizedDescription);
                
            }
            
        }];
        
    }
}