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
#include "WebSocketsManager.hpp"

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
        return socket->getState() == WebSocketConnection::State::Open;
    }
    
    EXPORT_API bool SPUnityWebSocketIsConnecting(WebSocketConnection* socket)
    {
        return socket->getState() == WebSocketConnection::State::Connecting;
    }
    
    EXPORT_API int SPUnityWebSocketGetState(WebSocketConnection* socket)
    {
        return (int)socket->getState();
    }
    
    EXPORT_API void SPUnityWebSocketDisconnect(WebSocketConnection* socket)
    {
        socket->disconnect();
    }
    
    EXPORT_API void SPUnityWebSocketUpdate(WebSocketConnection* socket)
    {
        WebSocketsManager::get().update();
    }
    
    EXPORT_API void SPUnityWebSocketPing(WebSocketConnection* socket)
    {
        socket->sendPing();
    }
    
    EXPORT_API void SPUnityWebSocketSend(WebSocketConnection* socket, const std::string data)
    {
        socket->send(data);
    }
    
    EXPORT_API int SPUnityWebSocketGetMessageLength(WebSocketConnection* socket)
    {
        return (int)socket->getMessage().size();
    }
    
    EXPORT_API bool SPUnityWebSocketGetMessage(WebSocketConnection* socket, char* data)
    {
        if(socket->hasMessages())
        {
            auto& msg = socket->getMessage();
            memcpy(data, msg.c_str(), msg.size() * sizeof(char));
            socket->removeOldestMessage();
            return true;
        }
        return false;
    }
    
    EXPORT_API int SPUnityWebSocketGetErrorLenght(WebSocketConnection* socket)
    {
        return (int)socket->getError().size();
    }
    
    EXPORT_API int SPUnityWebSocketGetErrorCode(WebSocketConnection* socket)
    {
        return socket->getErrorCode();
    }
    
    EXPORT_API bool SPUnityWebSocketGetError(WebSocketConnection* socket, char* data)
    {
        if(socket->hasError())
        {
            auto& err = socket->getError();
            memcpy(data, err.c_str(), err.size() * sizeof(char));
            socket->clearError();
            return true;
        }
        return false;
    }
    
    EXPORT_API void SPUnityWebSocketSetProxy(WebSocketConnection* socket, const std::string proxy)
    {
        // TODO
    }
    
    EXPORT_API void SPUnityWebSocketSetVerbose(bool verbose)
    {
        if(verbose)
        {
            WebSocketsManager::get().setLogLevelMax();
        }
        else
        {
            WebSocketsManager::get().setLogLevelNone();
        }
    }
}