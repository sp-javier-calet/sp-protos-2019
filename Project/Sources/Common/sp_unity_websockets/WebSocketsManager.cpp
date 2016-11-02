//
//  WebSocketsManager.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 2/11/16.
//

#include "WebSocketsManager.hpp"
#include "WebSocketConnection.hpp"
#include <string>
#include <sstream>


int WebSocketsManager::pingCounter = 0;
int WebSocketsManager::maxNumberOfPings = 3;

static int callback_websocket(struct libwebsocket_context* context, struct libwebsocket* wsi, enum libwebsocket_callback_reasons reason,
                              void* user, void* in, size_t len)
{
    WebSocketsManager& manager = WebSocketsManager::get();
    WebSocketConnection& connection =  manager.get(wsi);//(WebSocketConnection*)libwebsocket_context_user(context);
    
    int n;
    int pRet = 0;
    
    switch(reason)
    {
        case LWS_CALLBACK_CLIENT_ESTABLISHED:
            lwsl_notice("Client has connected\n");
            connection.connectionEstablished();
            WebSocketsManager::pingCounter = 0;
            break;
            
        case LWS_CALLBACK_CLIENT_RECEIVE:
        {
            char* data = (char*)in;
            lwsl_notice("Client RX: %.*s\n", len, data);
            
            bool isFinalFrame = libwebsockets_remaining_packet_payload(wsi) == 0 && libwebsocket_is_final_fragment(wsi);
            connection.receivedData(std::string(data, len), isFinalFrame);
            
            break;
        }
        case LWS_CALLBACK_CLIENT_RECEIVE_PONG:
        {
            lwsl_notice("Client RX - PONG\n");
            connection.receivedPong();
            WebSocketsManager::pingCounter = 0;
            break;
        }
        case LWS_CALLBACK_CLIENT_WRITEABLE:
        {
            if(manager.isSocketMarkedToClose(wsi))
            {
                lwsl_notice("Client closing socket %p\n", wsi);
                manager.removeSocketFromShouldCloseSet(wsi);
                connection.closeSocket();
                pRet = -1;
                break;
            }
            const std::string& dataToSend = connection.getNextDataToSend();
            if(!dataToSend.empty())
            {
                // TODO Padding
                size_t dataSize = dataToSend.size() - LWS_SEND_BUFFER_PRE_PADDING - LWS_SEND_BUFFER_POST_PADDING;
                lwsl_notice("Client TX: %.*s\n", dataSize, (unsigned char*)dataToSend.c_str());//getBytes(LWS_SEND_BUFFER_PRE_PADDING));
                
                n = libwebsocket_write(wsi, (unsigned char*)dataToSend.c_str(), dataSize, LWS_WRITE_TEXT);//->getBytes(LWS_SEND_BUFFER_PRE_PADDING), dataSize, LWS_WRITE_TEXT);
                
                if(n < 0)
                {
                    lwsl_err("ERROR %d writing to socket, hanging up\n", n);
                    connection.connectionError();
                    connection.closeSocket();
                    pRet = -1;
                }
                if(n < (int)dataSize)
                {
                    lwsl_err("Partial write\n");
                    connection.connectionError();
                    connection.closeSocket();
                    pRet = -1;
                }
                
                connection.removeOldestData();
                if(connection.hasDataToSend())
                {
                    libwebsocket_callback_on_writable(context, wsi);
                }
            }
            else if(connection.checkAndDecrementPingCounter())
            {
                size_t dataSize = LWS_SEND_BUFFER_PRE_PADDING + LWS_SEND_BUFFER_POST_PADDING;
                unsigned char* data = new unsigned char[dataSize];
                lwsl_notice("Client TX - PING\n");
                n = libwebsocket_write(wsi, data + LWS_SEND_BUFFER_PRE_PADDING, 0, LWS_WRITE_PING);
                delete[] data;
                
                if(n < 0)
                {
                    lwsl_err("ERROR %d writing to socket, hanging up\n", n);
                    connection.connectionError();
                    connection.closeSocket();
                    pRet = -1;
                }
                else
                {
                    ++WebSocketsManager::pingCounter;
                    
                    if(WebSocketsManager::pingCounter >= WebSocketsManager::maxNumberOfPings)
                    {
                        WebSocketsManager::pingCounter = 0;
                        lwsl_err("ERROR: MAX PINGS REACHED\n");
                        connection.connectionError();
                        connection.closeSocket();
                        pRet = -1;
                    }
                }
            }
            break;
        }
        case LWS_CALLBACK_CLOSED:
            connection.closeSocket();
            pRet = -1;
            break;
        case LWS_CALLBACK_CLIENT_CONNECTION_ERROR:
            connection.connectionError();
            connection.closeSocket();
            pRet = -1;
            break;
        default:
            break;
    }
    
    return pRet;
}

static struct libwebsocket_protocols protocols[] = {{
    "",                 /* name */
    callback_websocket, /* callback */
    0,                  /* per_session_data_size */
    65536,              /* rx_buffer_size */
    0,                  /* no_buffer_all_partial_tx */
    nullptr,            /* owning_server */
    0,                  /* protocol_index */
},
{
    "wamp.2.json",      /* name */
    callback_websocket, /* callback */
    0,                  /* per_session_data_size */
    65536,              /* rx_buffer_size */
    0,                  /* no_buffer_all_partial_tx */
    nullptr,            /* owning_server */
    0,                  /* protocol_index */
},
{
    NULL, NULL, 0, 0, 0, nullptr, 0 /* End of list */
}};


WebSocketsManager::WebSocketsManager()
: _context(nullptr)
, _socketTimeout(0.1f)
{
    lws_set_log_level(0x0, NULL);
}

WebSocketsManager::~WebSocketsManager()
{
    if(_context)
    {
        libwebsocket_context_destroy(_context);
        _context = nullptr;
    }
}

WebSocketsManager& WebSocketsManager::get()
{
    static WebSocketsManager _instance;
    return _instance;
}

void WebSocketsManager::setLogLevelMax()
{
    /*
     Set the logging level to almost maximum (0x3FF is maximum)
     The bit disabled corresponds to PARSER logs because it logs to much PARSER things
     1   ERR
     2   WARN
     4   NOTICE
     8   INFO
     16  DEBUG
     32  PARSER
     64  HEADER
     128 EXTENSION
     256 CLIENT
     512 LATENCY
     */
    lws_set_log_level(0x3DF, NULL);
}

void WebSocketsManager::setLogLevelNone()
{
    lws_set_log_level(0x0, NULL);
}

void WebSocketsManager::setTimeout(float timeout)
{
    _socketTimeout = timeout;
}

void WebSocketsManager::checkAndCreateContext()
{
    if(!_context)
    {
        int listen_port = CONTEXT_PORT_NO_LISTEN;
        
        struct lws_context_creation_info info;
        memset(&info, 0, sizeof info);
        info.port = listen_port;
        info.iface = NULL;
        info.protocols = protocols;
        info.extensions = NULL;
        info.gid = -1;
        info.uid = -1;
        info.options = 0;
        info.ka_time = 5000;
        info.ka_probes = 5;
        info.ka_interval = 5000;
        info.user = this;
        info.http_proxy_address = "";
        _context = libwebsocket_create_context(&info);
        
        // TODO start
        //_dispatcher->dispatch(std::bind(&WebSocketsManager::loop, this));
    }
}

libwebsocket* WebSocketsManager::connectSocketToUrl(const std::string& pUrl, const WebSocketConnection* connection)
{
    std::string scheme;
    std::string host;
    uint port;
    std::string path;
    std::string proxy;
    libwebsocket* socket = nullptr;
    
    int use_ssl = 0;
    if(scheme == "wss")
    {
        use_ssl = connection->getAllowSelfSignedCertificates() ? 3 : 1; /* 2 = allow selfsigned, 3 = skip all kind of SSL errors */
    }
    
    // Comma-separated list of protocols being asked for from the server, or just one. The server will pick the one it likes best. If you don't
    // want to specify a protocol, which is legal, use NULL here.
    const char* suppProtocolsCStr = nullptr;
    std::string suppProtocolsStr = connection->getSuportedProtocolsString();
    if(!suppProtocolsStr.empty())
    {
        suppProtocolsCStr = suppProtocolsStr.c_str();
    }
    
    if(_context != nullptr)
    {
        /* TODO Proxy parameter
         if(networkInfo && !networkInfo->getProxyHost().empty() && networkInfo->getProxyPort() != -1)
        {
            std::ostringstream proxyAddr;
            proxyAddr << networkInfo->getProxyHost() << ":" << networkInfo->getProxyPort();
            libwebsocket_set_proxy(_context, proxyAddr.str().c_str());
        }
        */
        lwsl_notice("Client connecting to %s:%u%s use_ssl:%d ....\n", host.c_str(), proxy.c_str(), path.c_str(), use_ssl);
        socket = libwebsocket_client_connect(_context, host.c_str(), port, use_ssl, path.c_str(), host.c_str(),
                                           connection->getOrigin().c_str(), suppProtocolsCStr, -1);
    }
    else
    {
        lwsl_err("libwebsocket init failed\n");
    }
    
    return socket;
}

void WebSocketsManager::connect(WebSocketConnection* connection)
{
    assert(!connection->getWebsocket());
    const std::vector<std::string>& vecUrls = connection->getVecUrls();
    assert(!vecUrls.empty());
    
    size_t currentUrlIndex = connection->getCurrentUrlIndex() % vecUrls.size();
    std::string currentUrl = vecUrls[currentUrlIndex];
    currentUrlIndex++;
    connection->setCurrentUrlIndex(currentUrlIndex);
    connection->setUrl(currentUrl);
    
    checkAndCreateContext();
    
    libwebsocket* websocket = connectSocketToUrl(currentUrl, connection);
    
    if(websocket)
    {
        lwsl_notice("Wsi %p added to connection map\n", websocket);
        connection->setWebsocket(websocket);
        _mapConnection.insert(std::make_pair(websocket, connection));
        lwsl_notice("Client connected to %s\n", currentUrl.c_str());
    }
    else if(currentUrlIndex < vecUrls.size())
    {
        lwsl_err("Client failed to connect to %s\n", currentUrl.c_str());
        connect(connection);
    }
    else
    {
        currentUrlIndex = 0;
        connection->setState(WebSocketConnection::State::CLOSED);
    }
}

void WebSocketsManager::dataReadyToSendOnConnection(WebSocketConnection* connection)
{
    lwsl_notice("Wsi %p has data ready to send\n", connection->getWebsocket());
    if(_context && connection->getWebsocket())
    {
        libwebsocket_callback_on_writable(_context, connection->getWebsocket());
    }
}

void WebSocketsManager::markSocketToClose(libwebsocket* wsi)
{
    lwsl_notice("Wsi %p market to close\n", wsi);
    _setSocketsShouldClose.insert(wsi);
    if(_context)
    {
        libwebsocket_callback_on_writable(_context, wsi);
    }
}

bool WebSocketsManager::isSocketMarkedToClose(libwebsocket* wsi)
{
    return _setSocketsShouldClose.count(wsi) != 0;
}

void WebSocketsManager::removeSocketFromShouldCloseSet(libwebsocket* wsi)
{
    assert(_setSocketsShouldClose.count(wsi) != 0);
    _setSocketsShouldClose.erase(wsi);
}

void WebSocketsManager::remove(WebSocketConnection* connection)
{
    lwsl_notice("Wsi %p removed from connection map\n", connection->getWebsocket());
    assert(_mapConnection.count(connection->getWebsocket()) > 0);
    _mapConnection.erase(connection->getWebsocket());
}

void WebSocketsManager::update()
{
    libwebsocket_service(_context, _socketTimeout * 1000.0f);
}

