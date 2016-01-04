#include "SPUnityCurlFacade.h"
#include "SPUnityCurlManager.h"
#import <UIKit/UIKit.h>

UIBackgroundTaskIdentifier bgTask = UIBackgroundTaskInvalid;

void SPUnityCurlEndBackgroundTask()
{
    if(bgTask != UIBackgroundTaskInvalid)
    {
        UIApplication* app = [UIApplication sharedApplication];
        [app endBackgroundTask:bgTask];
        bgTask = UIBackgroundTaskInvalid;
    }
}

EXPORT_API void SPUnityCurlOnApplicationPause(bool paused)
{
    if(SPUnityCurlRunning() == 0)
    {
        return;
    }
    UIApplication* app = [UIApplication sharedApplication];
    if(paused)
    {
        bgTask = [app beginBackgroundTaskWithName:@"SPUnityCurl" expirationHandler:^{
            SPUnityCurlEndBackgroundTask();
        }];

        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
            while(SPUnityCurlRunning() > 0)
            {
                SPUnityCurlUpdate(0);
            }
            SPUnityCurlEndBackgroundTask();
        });
    }
    else
    {
        SPUnityCurlEndBackgroundTask();
    }
}

