#include "SPUnityCurlFacade.h"
#include "SPUnityCurlManager.h"

EXPORT_API void SPUnityCurlOnApplicationPause(bool paused)
{
    if(SPUnityCurlRunning() == 0)
    {
        return;
    }

    // TODO Required?
}

