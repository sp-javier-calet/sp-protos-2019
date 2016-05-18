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

#include <string>

extern "C"
{
    EXPORT_API bool SPUnityNativeUtilsIsInstalled(const char* appId);
    EXPORT_API bool SPUnityNativeUtilsUserAllowNotification();
}

class SPUnityNativeUtils
{
    public:
        static const std::string kV6;
        static const std::string kV7;
        static const std::string kV8;
        static const std::string kV9;
    
        static bool isSystemVersionEqualTo(const std::string& version);
        static bool isSystemVersionGreaterThan(const std::string& version);
        static bool isSystemVersionGreaterThanOrEqualTo(const std::string& version);
        static bool isSystemVersionLessThan(const std::string& version);
        static bool isSystemVersionLessThanOrEqualTo(const std::string& version);
};

#endif
