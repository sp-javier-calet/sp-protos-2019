
#ifndef __sparta__SPUnityBreadcrumbManagerFacade__
#define __sparta__SPUnityBreadcrumbManagerFacade__

#include "SPUnityDefines.h"

extern "C" {
    EXPORT_API void SPUnityBreadcrumbManager_SetMaxLogs(int maxLogs);
    EXPORT_API void SPUnityBreadcrumbManager_SetDumpFilePath(const char* directory, const char* file);
    EXPORT_API void SPUnityBreadcrumbManager_Log(const char* info);
    EXPORT_API void SPUnityBreadcrumbManager_DumpToFile();
    EXPORT_API void SPUnityBreadcrumbManager_Clear();
}

#endif
