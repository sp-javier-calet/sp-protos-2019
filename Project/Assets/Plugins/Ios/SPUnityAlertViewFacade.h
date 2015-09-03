#ifndef __SPUnityAlertViewFacade__
#define __SPUnityAlertViewFacade__

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
    struct SPUnityAlertViewDataStruct
    {
        const char* message;
        const char* title;
        const char* signature;
        const char* buttons;
        const char* objectname;
        bool input;
    };

    EXPORT_API void SPUnityAlertViewShow(SPUnityAlertViewDataStruct data);
    EXPORT_API void SPUnityAlertViewHide();
}


#endif
