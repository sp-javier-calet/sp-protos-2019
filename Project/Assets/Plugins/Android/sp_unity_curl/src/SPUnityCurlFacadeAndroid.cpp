#include "SPUnityCurlFacade.h"
#include "SPUnityCurlManager.h"

EXPORT_API void SPUnityCurlOnApplicationPause(bool paused)
{
    if(paused)
    {
        while(SPUnityCurlRunning() > 0)
        {
            SPUnityCurlUpdate(0);
        }
    }
}

