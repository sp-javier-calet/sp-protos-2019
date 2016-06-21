#include "SPUnityBreadcrumbManager.hpp"

/*
 * Exported interface
 */
extern "C"
{
    void SPUnityBreadcrumbManager_SetMaxLogs(int maxLogs)
    {
        SPUnityBreadcrumbManager::getInstance().setMaxLogs(maxLogs);
    }

    void SPUnityBreadcrumbManager_SetDumpFilePath(const char* directory, const char* file)
    {
        SPUnityBreadcrumbManager::getInstance().setDumpFilePath(std::string(directory), std::string(file));
    }

    void SPUnityBreadcrumbManager_Log(const char* info)
    {
        SPUnityBreadcrumbManager::getInstance().leaveBreadcrumb(info);
    }

    void SPUnityBreadcrumbManager_DumpToFile()
    {
        SPUnityBreadcrumbManager::getInstance().dumpToFile();
    }

    void SPUnityBreadcrumbManager_Clear()
    {
        SPUnityBreadcrumbManager::getInstance().clear();
    }
}
