#include "SPUnityBreadcrumbManager.hpp"
using namespace socialpoint;

/*
 * Exported interface
 */
extern "C"
{
    SPUnityBreadcrumbManager* SPUnityBreadcrumbManager_Get()
    {
        return SPUnityBreadcrumbManager::getInstance();
    }

    void SPUnityBreadcrumbManager_Log(const char* info)
    {
        SPUnityBreadcrumbManager::getInstance()->leaveBreadcrumb(info);
    }

    void SPUnityBreadcrumbManager_SetMaxLogs(int maxLogs)
    {
        SPUnityBreadcrumbManager::getInstance()->setMaxLogs(maxLogs);
    }

    void SPUnityBreadcrumbManager_Clear()
    {
        SPUnityBreadcrumbManager::getInstance()->clear();
    }
}
