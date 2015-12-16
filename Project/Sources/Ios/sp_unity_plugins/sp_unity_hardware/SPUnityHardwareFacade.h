#ifndef __SPUnityHardwareFacade__
#define __SPUnityHardwareFacade__

#include <stdint.h>

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

extern "C"
{
    EXPORT_API char* SPUnityHardwareGetDeviceString();
    EXPORT_API char* SPUnityHardwareGetDevicePlatformVersion();
    EXPORT_API char* SPUnityHardwareGetDeviceAdvertisingId();
    EXPORT_API bool SPUnityHardwareGetDeviceAdvertisingIdEnabled();
    EXPORT_API bool SPUnityHardwareGetDeviceRooted();

    EXPORT_API uint64_t SPUnityHardwareGetTotalMemory();
    EXPORT_API uint64_t SPUnityHardwareGetFreeMemory();
    EXPORT_API uint64_t SPUnityHardwareGetUsedMemory();
    EXPORT_API uint64_t SPUnityHardwareGetActiveMemory();

    EXPORT_API char* SPUnityHardwareGetAppId();
    EXPORT_API char* SPUnityHardwareGetAppVersion();
    EXPORT_API char* SPUnityHardwareGetAppShortVersion();
    EXPORT_API char* SPUnityHardwareGetAppLanguage();
    EXPORT_API char* SPUnityHardwareGetAppCountry();

    EXPORT_API char* SPUnityHardwareGetNetworkConnectivity();
    EXPORT_API char* SPUnityHardwareGetNetworkProxy();
    EXPORT_API char* SPUnityHardwareGetNetworkIpAddress();

    EXPORT_API uint64_t SPUnityHardwareGetTotalStorage();
    EXPORT_API uint64_t SPUnityHardwareGetFreeStorage();
    EXPORT_API uint64_t SPUnityHardwareGetUsedStorage();
}

#endif
