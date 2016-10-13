#import <UIKit/UIKit.h>
#include <mutex>
#include "CurlClient.hpp"

UIBackgroundTaskIdentifier bgTask = UIBackgroundTaskInvalid;
std::mutex appPausedMutex;
bool appPaused = false;

extern "C"
{
    void SPUnityCurlEndBackgroundTask()
    {
        if(bgTask != UIBackgroundTaskInvalid)
        {
            UIApplication* app = [UIApplication sharedApplication];
            [app endBackgroundTask:bgTask];
            bgTask = UIBackgroundTaskInvalid;
        }
    }
    
    void SPUnityCurlOnApplicationPause(CurlClient* client, bool paused)
    {
        std::lock_guard<std::mutex> lk(appPausedMutex);
        appPaused = paused;
        
        if(!client->isRunning())
        {
            return;
        }
        
        UIApplication* app = [UIApplication sharedApplication];
        if(paused)
        {
            bgTask = [app beginBackgroundTaskWithName:@"SPUnityCurl"
                                    expirationHandler:^{
                                        SPUnityCurlEndBackgroundTask();
                                    }];
            
            dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
                while(client->isRunning())
                {
                    std::lock_guard<std::mutex> lk(appPausedMutex);
                    if(appPaused)
                    {
                        client->update();
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
}