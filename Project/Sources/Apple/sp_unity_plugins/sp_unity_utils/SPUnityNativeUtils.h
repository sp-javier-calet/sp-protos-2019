#ifndef __SPUnityNativeUtils__
#define __SPUnityNativeUtils__

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
    EXPORT_API bool SPUnityNativeUtilsIsInstalled(const char* appId);
    EXPORT_API bool SPUnityNativeUtilsUserAllowNotification();
}

#endif
