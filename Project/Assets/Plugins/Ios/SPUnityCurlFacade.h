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


extern "C"
{
    struct SPUnityCurlRequestStruct
    {
        intptr_t id;
        const char* url;
        const char* query;
        const char* method;
        intptr_t timeout;
        intptr_t activityTimeout;
        const char* proxy;
        const char* headers;
        const char* body;
        intptr_t bodyLength;

    };

    EXPORT_API intptr_t SPUnityCurlRunning();

    EXPORT_API intptr_t SPUnityCurlCreateConn();
    EXPORT_API void SPUnityCurlDestroyConn(intptr_t id);

    EXPORT_API intptr_t SPUnityCurlSend(SPUnityCurlRequestStruct data);
    EXPORT_API intptr_t SPUnityCurlUpdate(intptr_t id);

    EXPORT_API intptr_t SPUnityCurlGetCode(intptr_t id);

    EXPORT_API void SPUnityCurlGetError(intptr_t id, char* data);
    EXPORT_API void SPUnityCurlGetBody(intptr_t id, char* data);
    EXPORT_API void SPUnityCurlGetHeaders(intptr_t id, char* data);

    EXPORT_API intptr_t SPUnityCurlGetErrorLength(intptr_t id);
    EXPORT_API intptr_t SPUnityCurlGetBodyLength(intptr_t id);
    EXPORT_API intptr_t SPUnityCurlGetHeadersLength(intptr_t id);
    EXPORT_API intptr_t SPUnityCurlGetDownloadSize(intptr_t id);
    EXPORT_API intptr_t SPUnityCurlGetDownloadSpeed(intptr_t id);

    EXPORT_API void SPUnityCurlInit();
    EXPORT_API void SPUnityCurlDestroy();

    // needs to be implemented for each platform
    EXPORT_API void SPUnityCurlOnApplicationPause(bool paused);

}


#endif
