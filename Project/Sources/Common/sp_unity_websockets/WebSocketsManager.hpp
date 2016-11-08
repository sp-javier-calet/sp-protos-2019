//
//  WebSocketsManager.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 2/11/16.
//

#ifndef __sparta__WebSocketsManager__
#define __sparta__WebSocketsManager__

#include <libwebsockets.h>
#include <map>
#include <set>

class WebSocketConnection;

class WebSocketsManager
{
public:
    static int pingCounter;
    static int maxNumberOfPings;
    
private:
    WebSocketsManager();
    ~WebSocketsManager();
    
    libwebsocket_context* _context;
    
    std::map<libwebsocket*, WebSocketConnection*> _mapConnection;
    
    std::set<libwebsocket*> _setSocketsShouldClose;
    
    void checkAndCreateContext();
    
    /**
     * Connect thread synchronously. Do not call this in main thread to avoid blocking the game
     */
    
    libwebsocket* connectSocketToUrl(const std::string& pUrl, const WebSocketConnection* connection);
    
public:
    static WebSocketsManager& get();
    
    void dataReadyToSendOnConnection(WebSocketConnection* connection);
    
    void markSocketToClose(libwebsocket* wsi);
    bool isSocketMarkedToClose(libwebsocket* wsi);
    void removeSocketFromShouldCloseSet(libwebsocket* wsi);
    
    void setLogLevelMax();
    void setLogLevelNone();

    WebSocketConnection& get(libwebsocket* wsi);
    void connect(WebSocketConnection* connection);
    void connect(WebSocketConnection* connection, int idx);
    void update();
    
    void remove(WebSocketConnection* connection);
};


#endif /* defined(__sparta__WebSocketsManager__) */
