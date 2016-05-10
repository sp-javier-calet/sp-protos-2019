#include "SPUnityBreadcrumbManager.hpp"

using namespace socialpoint;

/*
 * Exported interface
 */
extern "C"{
    SPUnityBreadcrumbManager* SPUnityBreadcrumbManager_Get()
    {
        return SPUnityBreadcrumbManager::getInstance();
    }

    void SPUnityBreadcrumbManager_Log(const char* info)
    {
        SPUnityBreadcrumbManager::getInstance()->leaveBreadcrumb(0, info);
    }
}
