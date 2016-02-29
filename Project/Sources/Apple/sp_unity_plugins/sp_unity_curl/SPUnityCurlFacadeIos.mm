#import <UIKit/UIKit.h>
#include "SPUnityCurlFacade.h"
#include "SPUnityCurlManager.h"
#include <mutex>

UIBackgroundTaskIdentifier bgTask = UIBackgroundTaskInvalid;

std::mutex AppPaused_mutex;
bool AppPaused = false;

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
    std::lock_guard<std::mutex> lk(AppPaused_mutex);
    AppPaused = paused;
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
                std::lock_guard<std::mutex> lk(AppPaused_mutex);
                if(AppPaused)
                {
                    SPUnityCurlUpdate(0);
                }
                else
                {
                    break;
                }
            }
            SPUnityCurlEndBackgroundTask();
        });
    }
    else
    {
        SPUnityCurlEndBackgroundTask();
    }
}

