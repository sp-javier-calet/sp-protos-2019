//
//  WebSocketsManager.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 2/11/16.
//

#ifndef __sparta__WebSocketsManager__
#define __sparta__WebSocketsManager__

#include "libwebsockets.h"
#include <map>
#include <set>
#include <string>

struct WebSocketConnectionInfo;
class WebSocketConnection;

class WebSocketsManager
{
  public:
    static int pingCounter;
    static int maxNumberOfPings;

    struct ProxySettings
    {
        ProxySettings(std::string host, int port)
        : host(host)
        , port(port)
        {
        }
        std::string host;
        int port;
    };

  private:
    WebSocketsManager();
    ~WebSocketsManager();

    lws_context* _context;
    lws_vhost* _vhost;

    std::map<lws*, WebSocketConnection*> _mapConnection;

    std::set<lws*> _setSocketsShouldClose;

    ProxySettings _proxy;

    void checkAndCreateContext();

    /**
     * Connect thread synchronously. Do not call this in main thread to avoid blocking the game
     */
    lws* connectSocketToUrl(const WebSocketConnectionInfo& pUrl, const WebSocketConnection* connection);

  public:
    static WebSocketsManager& get();

    void dataReadyToSendOnConnection(WebSocketConnection* connection);

    void markSocketToClose(lws* wsi);
    bool isSocketMarkedToClose(lws* wsi);
    void removeSocketFromShouldCloseSet(lws* wsi);

    void setLogLevelMax();
    void setLogLevelNone();

    void setProxySettings(ProxySettings proxy);

    WebSocketConnection* get(lws* wsi);
    void connect(WebSocketConnection* connection);
    void connect(WebSocketConnection* connection, int idx);
    void update();

    void remove(WebSocketConnection* connection);
};


#endif /* defined(__sparta__WebSocketsManager__) */
