
#ifndef __sparta__SPUnityGameCenterFacade__
#define __sparta__SPUnityGameCenterFacade__

#include "SPUnityDefines.h"

typedef void (*SPUnityGameCenterUserVerificationCallback)(const char* data, const char* error);

extern "C" {    
    EXPORT_API void SPUnityGameCenter_GenerateUserVerification(SPUnityGameCenterUserVerificationCallback callback);
}

#endif
