#ifndef __SPUnityUtils__
#define __SPUnityUtils__

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
    EXPORT_API int SPUnityUtilsGetRandomInt();
    EXPORT_API unsigned int SPUnityUtilsGetRandomUnsignedInt();
    EXPORT_API int SPUnityUtilsGetRandomIntRange(int min, int max);
    EXPORT_API float SPUnityUtilsGetRandomFloatRange(float min, float max);
    EXPORT_API double SPUnityUtilsGetRandomDoubleRange(double min, double max);
}

#endif
