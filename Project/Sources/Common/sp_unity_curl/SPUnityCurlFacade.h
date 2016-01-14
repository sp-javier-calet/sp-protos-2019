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
#include <cstdint>

extern "C"
{
    struct SPUnityCurlRequestStruct
    {
        int id;
        const char* url;
        const char* query;
        const char* method;
        int timeout;
        int activityTimeout;
        const char* proxy;
        const char* headers;
        const uint8_t* body;
        int bodyLength;

    };

    EXPORT_API int SPUnityCurlRunning();

    EXPORT_API int SPUnityCurlCreateConn();
    EXPORT_API void SPUnityCurlDestroyConn(int id);

    EXPORT_API int SPUnityCurlSend(SPUnityCurlRequestStruct data);
    EXPORT_API int SPUnityCurlUpdate(int id);

    EXPORT_API double SPUnityCurlGetConnectTime(int id);
    EXPORT_API double SPUnityCurlGetTotalTime(int id);
    
    EXPORT_API int SPUnityCurlGetCode(int id);

    EXPORT_API void SPUnityCurlGetError(int id, char* data);
    EXPORT_API void SPUnityCurlGetBody(int id, char* data);
    EXPORT_API void SPUnityCurlGetHeaders(int id, char* data);

    EXPORT_API int SPUnityCurlGetErrorLength(int id);
    EXPORT_API int SPUnityCurlGetBodyLength(int id);
    EXPORT_API int SPUnityCurlGetHeadersLength(int id);
    EXPORT_API int SPUnityCurlGetDownloadSize(int id);
    EXPORT_API int SPUnityCurlGetDownloadSpeed(int id);

    EXPORT_API void SPUnityCurlInit();
    EXPORT_API void SPUnityCurlDestroy();

    // needs to be implemented for each platform
    EXPORT_API void SPUnityCurlOnApplicationPause(bool paused);

    EXPORT_API void SPUnityCurlSetCertificate(const char* data, size_t size);
}


#endif
