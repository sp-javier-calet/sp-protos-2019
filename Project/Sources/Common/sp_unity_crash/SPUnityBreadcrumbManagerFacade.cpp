#include "SPUnityBreadcrumbManagerFacade.h"
#include "BreadcrumbManager.hpp"
#include <string>

EXPORT_API void SPUnityBreadcrumbManager_SetMaxLogs(int maxLogs)
{
    BreadcrumbManager::getInstance().setMaxLogs(maxLogs);
}

EXPORT_API void SPUnityBreadcrumbManager_SetDumpFilePath(const char* directory, const char* file)
{
    BreadcrumbManager::getInstance().setDumpFilePath(std::string(directory), std::string(file));
}

EXPORT_API void SPUnityBreadcrumbManager_Log(const char* info)
{
    BreadcrumbManager::getInstance().leaveBreadcrumb(info);
}

EXPORT_API void SPUnityBreadcrumbManager_DumpToFile()
{
    BreadcrumbManager::getInstance().dumpToFile();
}

EXPORT_API void SPUnityBreadcrumbManager_Clear()
{
    BreadcrumbManager::getInstance().clear();
}
