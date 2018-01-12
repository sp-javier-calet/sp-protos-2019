#ifndef __sparta__SPUnityCrashReporterFacade__
#define __sparta__SPUnityCrashReporterFacade__

#include "SPUnityDefines.h"

typedef void (*SPUnityCrashReporterCallback)(const char* path);

extern "C" {

    EXPORT_API void SPUnityCrashReporter_Create(const char* path, const char* version, const char* fileSeparator, const char* crashExtension,
                                                     const char* logExtension, SPUnityCrashReporterCallback callback);
    EXPORT_API void SPUnityCrashReporter_Enable();
    EXPORT_API void SPUnityCrashReporter_Disable();
    EXPORT_API void SPUnityCrashReporter_ForceCrash();
}

#endif
