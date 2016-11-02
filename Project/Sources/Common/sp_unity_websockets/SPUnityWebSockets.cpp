//
//  SPUnityWebSockets.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 31/10/16.
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

#include "WebSocketConnection.hpp"


/*
 * Native interface
 */
extern "C"
{
    EXPORT_API WebSocketConnection* SPUnityWebSocketsCreate(char* url)
    {
        return new WebSocketConnection(std::string(url));
    }
    
    EXPORT_API void SPUnityWebSocketDestroy(WebSocketConnection* socket)
    {
        delete socket;
    }
    
    EXPORT_API void SPUnityWebSocketConnect(WebSocketConnection* socket)
    {
        socket->connect();
    }
    
    EXPORT_API bool SPUnityWebSocketIsConnected(WebSocketConnection* socket)
    {
        return socket->isConnected();
    }
    
    EXPORT_API bool SPUnityWebSocketIsConnecting(WebSocketConnection* socket)
    {
        return socket->isConnected();
    }
    
    EXPORT_API void SPUnityWebSocketDisconnect(WebSocketConnection* socket)
    {
        socket->disconnect();
    }
    
    EXPORT_API void SPUnityWebSocketPing(WebSocketConnection* socket)
    {
        socket->sendPing();
    }
    
    EXPORT_API void SPUnityWebSocketSend(WebSocketConnection* socket, char* data, size_t size)
    {
        socket->send(std::string(data, size));
    }
    
    EXPORT_API bool SPUnityWebSocketHasMessages(WebSocketConnection* socket)
    {
        return false; // TODO
    }
    
    EXPORT_API int SPUnityWebSocketGetMessageLength(WebSocketConnection* socket)
    {
        return 0; // TODO
    }
    
    EXPORT_API void SPUnityWebSocketGetMessage(WebSocketConnection* socket, char* data)
    {
        // TODO
    }
    
    EXPORT_API int SPUnityWebSocketGetErrorLenght(WebSocketConnection* socket)
    {
        return 0; // TODO
    }
    
    EXPORT_API int SPUnityWebSocketGetErrorCode(WebSocketConnection* socket)
    {
        return 0; // TODO
    }
    
    EXPORT_API void SPUnityWebSocketGetError(WebSocketConnection* socket, char* data)
    {
        // TODO
    }
    
    EXPORT_API void SPUnityWebSocketSetProxy(WebSocketConnection* socket, char* proxy)
    {
        // TODO
    }
    
    EXPORT_API void SPUnityWebSocketSetVerbose(WebSocketConnection* socket, bool verbose)
    {
    }
}