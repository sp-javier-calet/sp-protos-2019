#ifndef __SPUnityCurlFacade__
#define __SPUnityCurlFacade__

// Which platform we are on?
#if _MSC_VER
#define UNITY_WIN 1
#else
#define UNITY_OSX 1
#endif

// Attribute to make function be exported from a plugin
#if UNITY_WIN
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API
#endif

#include <stdlib.h>

extern "C" {

//-----------------------------------------------------------------------------------------------------------------------------
// HttpClient
//-----------------------------------------------------------------------------------------------------------------------------

#pragma pack(1)
struct SPUnityKeychainItemStruct
{
    const char* id;
    const char* service;
    const char* accessGroup;
};

EXPORT_API int SPUnityKeychainSet(SPUnityKeychainItemStruct item, const char* value);
EXPORT_API char* SPUnityKeychainGet(SPUnityKeychainItemStruct item);
EXPORT_API int SPUnityKeychainClear(SPUnityKeychainItemStruct item);
EXPORT_API char* SPUnityKeychainGetDefaultAccessGroup();
}

#endif
