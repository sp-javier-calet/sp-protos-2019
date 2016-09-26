//
//  SPUnityCurl.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//
//

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
#include "CurlClient.hpp"


/*
 * Native interface
 */
extern "C"
{   
    EXPORT_API int SPUnityCurlCreateConn(CurlClient* client)
    {
        return client->createConnection();
    }
    
    EXPORT_API void SPUnityCurlDestroyConn(CurlClient* client, int id)
    {
        client->destroyConnection(id);
    }
    
    EXPORT_API int SPUnityCurlSend(CurlClient* client, CurlRequest req)
    {
        return client->send(req);
    }
    
    EXPORT_API int SPUnityCurlSendStreamMessage(CurlClient* client, int id, CurlMessage msg)
    {
        return client->sendStreamMessage(id, msg);
    }
    
    EXPORT_API int SPUnityCurlUpdate(CurlClient* client, int id)
    {
        return client->update(id);
    }
    
    EXPORT_API double SPUnityCurlGetConnectTime(CurlClient* client, int id)
    {
        return client->getTime(id);
    }
    
    EXPORT_API double SPUnityCurlGetTotalTime(CurlClient* client, int id)
    {
        return client->getTotalTime(id);
    }
    
    EXPORT_API double SPUnityCurlGetDownloadSize(CurlClient* client, int id)
    {
        return client->getDownloadSize(id);
    }
    
    EXPORT_API double SPUnityCurlGetDownloadSpeed(CurlClient* client, int id)
    {
        return client->getDownloadSpeed(id);
    }
    
    EXPORT_API int SPUnityCurlGetResponseCode(CurlClient* client, int id)
    {
        return client->getResponseCode(id);
    }
    
    EXPORT_API int SPUnityCurlGetErrorCode(CurlClient* client, int id)
    {
        return client->getErrorCode(id);
    }
    
    EXPORT_API void SPUnityCurlGetError(CurlClient* client, int id, char* data)
    {
        client->getError(id, data);
    }
    
    EXPORT_API void SPUnityCurlGetBody(CurlClient* client, int id, char* data)
    {
        client->getBody(id, data);
    }
    
    EXPORT_API void SPUnityCurlGetHeaders(CurlClient* client, int id, char* data)
    {
        client->getHeaders(id, data);
    }
    
    EXPORT_API void SPUnityCurlGetStreamMessage(CurlClient* client, int id, char* data)
    {
        client->getStreamMessage(id, data);
    }
    
    EXPORT_API int SPUnityCurlGetErrorLength(CurlClient* client, int id)
    {
        return client->getErrorLength(id);
    }
    
    EXPORT_API int SPUnityCurlGetBodyLength(CurlClient* client, int id)
    {
        return client->getBodyLength(id);
    }
    
    EXPORT_API int SPUnityCurlGetHeadersLength(CurlClient* client, int id)
    {
        return client->getHeadersLength(id);
    }
    
    EXPORT_API int SPUnityCurlGetStreamMessageLenght(CurlClient* client, int id)
    {
        return client->getStreamMessageLenght(id);
    }
    
    EXPORT_API CurlClient* SPUnityCurlCreate(bool enableHttp2)
    {
        return new CurlClient(enableHttp2);
    }
    
    EXPORT_API void SPUnityCurlDestroy(CurlClient* client)
    {
        delete client;
    }
    
    EXPORT_API void SPUnityCurlSetConfig(CurlClient* client, const char* name)
    {
        client->setConfig(name);
    }
    
    // needs to be implemented for each platform
    EXPORT_API void SPUnityCurlOnApplicationPause(CurlClient* client, bool paused);
}