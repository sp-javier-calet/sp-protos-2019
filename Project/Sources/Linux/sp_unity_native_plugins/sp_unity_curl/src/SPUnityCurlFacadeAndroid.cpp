#include "SPUnityCurlFacade.h"
#include "SPUnityCurlManager.h"
#include <mutex>
#include <thread>

std::mutex m;
bool AppPaused = false;

void curlUpdater_thread()
{
    while(SPUnityCurlRunning() > 0)
    {
        std::lock_guard<std::mutex> lk(m);
        if(AppPaused)
        {
            SPUnityCurlUpdate(0);
        }
        else
        {
            break;
        }
    }
}

EXPORT_API void SPUnityCurlOnApplicationPause(bool paused)
{
    if(SPUnityCurlRunning() == 0)
    {
        return;
    }

    if(paused)
    {
        std::lock_guard<std::mutex> lk(m);
        AppPaused = true;
        std::thread updater(curlUpdater_thread);
        updater.detach();
    }
    else
    {
        std::lock_guard<std::mutex> lk(m);
        AppPaused = false;
    }
}
