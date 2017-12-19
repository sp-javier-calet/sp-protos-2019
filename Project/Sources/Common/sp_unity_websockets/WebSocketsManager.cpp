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
#include <cassert>

static int always_true_callback(X509_STORE_CTX* ctx, void* arg)
{
    return 1;
}

static int callback_websocket(struct lws* wsi, enum lws_callback_reasons reason, void* user, void* in, size_t len)
{
    if(reason != LWS_CALLBACK_GET_THREAD_ID)
    {
        lwsl_notice("REASON --- %d\n", reason);
    }

    if(reason == LWS_CALLBACK_OPENSSL_LOAD_EXTRA_CLIENT_VERIFY_CERTS)
    {
        SSL_CTX_set_cert_verify_callback((SSL_CTX*)user, always_true_callback, 0);
    }

    WebSocketsManager& manager = WebSocketsManager::get();
    WebSocketConnection* connection = manager.get(wsi);

    if(!connection)
    {
        return 0;
    }

    int n;
    int pRet = 0;

    switch(reason)
    {
        case LWS_CALLBACK_CLIENT_ESTABLISHED:
            lwsl_notice("Client has connected\n");
            connection->connectionEstablished();
            connection->resetPing();
            break;

        case LWS_CALLBACK_CLIENT_RECEIVE:
        {
            char* data = (char*)in;
            lwsl_notice("Client RX: %.*s\n", len, data);

            bool isFinalFrame = lws_remaining_packet_payload(wsi) == 0 && lws_is_final_fragment(wsi);
            connection->receivedData(std::string(data, len), isFinalFrame);

            break;
        }
        case LWS_CALLBACK_CLIENT_RECEIVE_PONG:
        {
            lwsl_notice("Client RX - PONG\n");
            connection->onPongReceived();
            break;
        }
        case LWS_CALLBACK_CLIENT_WRITEABLE:
        {
            if(manager.isSocketMarkedToClose(wsi))
            {
                lwsl_notice("Client closing socket %p\n", wsi);
                manager.removeSocketFromShouldCloseSet(wsi);
                connection->closeSocket();
                pRet = -1;
                break;
            }

            if(connection->hasDataToSend())
            {
                const std::string& dataToSend = connection->getNextDataToSend();
                size_t dataSize = dataToSend.size() - LWS_SEND_BUFFER_PRE_PADDING - LWS_SEND_BUFFER_POST_PADDING;
                const char* dataPointer = dataToSend.c_str();
                dataPointer += LWS_SEND_BUFFER_PRE_PADDING;
                lwsl_notice("Client TX: %.*s\n", dataSize, dataPointer);

                n = lws_write(wsi, (unsigned char*)dataPointer, dataSize, LWS_WRITE_TEXT);

                if(n < 0)
                {
                    lwsl_err("ERROR %d writing to socket, hanging up\n", n);
                    connection->connectionError((int)WebSocketConnection::Error::WriteError, "Error writing to socket, hanging up");
                    connection->closeSocket();
                    pRet = -1;
                }
                if(n < (int)dataSize)
                {
                    lwsl_err("Partial write\n");
                    connection->connectionError((int)WebSocketConnection::Error::StreamError, "Partial write");
                    connection->closeSocket();
                    pRet = -1;
                }

                connection->removeOldestData();
                if(connection->hasDataToSend())
                {
                    lws_callback_on_writable(wsi);
                }
            }
            else if(connection->checkAndDecrementPingCounter())
            {
                size_t dataSize = LWS_SEND_BUFFER_PRE_PADDING + LWS_SEND_BUFFER_POST_PADDING;
                unsigned char* data = new unsigned char[dataSize];
                lwsl_notice("Client TX - PING\n");
                n = lws_write(wsi, data + LWS_SEND_BUFFER_PRE_PADDING, 0, LWS_WRITE_PING);
                delete[] data;

                if(n < 0)
                {
                    lwsl_err("ERROR %d writing to socket, hanging up\n", n);
                    connection->connectionError((int)WebSocketConnection::Error::WriteError, "Error writing to socket, hanging up");
                    connection->closeSocket();
                    pRet = -1;
                }
                else
                {
                    if(connection->onPingSent())
                    {
                        connection->resetPing();

                        lwsl_err("ERROR: MAX PINGS REACHED\n");
                        connection->connectionError((int)WebSocketConnection::Error::MaxPings, "Max pings reached");
                        connection->closeSocket();
                        pRet = -1;
                    };
                }
            }
            break;
        }
        case LWS_CALLBACK_WSI_DESTROY:
        case LWS_CALLBACK_CLOSED:
            connection->closeSocket();
            pRet = -1;
            break;
        case LWS_CALLBACK_CLIENT_CONNECTION_ERROR:
            if(connection)
            {
                connection->connectionError((int)WebSocketConnection::Error::ConnectionError, "Connection Error");
                connection->closeSocket();
            }
            pRet = -1;
            break;
        default:
            break;
    }

    return pRet;
}

static struct lws_protocols protocols[] = {{
                                             "",                 /* name */
                                             callback_websocket, /* callback */
                                             0,                  /* per_session_data_size */
                                             65536,              /* rx_buffer_size */
                                             0,                  /* id */
                                             nullptr,            /* user */
                                           },
                                           {
                                             "wamp.2.json",      /* name */
                                             callback_websocket, /* callback */
                                             0,                  /* per_session_data_size */
                                             65536,              /* rx_buffer_size */
                                             0,                  /* id */
                                             nullptr,            /* user */
                                           },
                                           {
                                             nullptr, nullptr, 0, 0, 0, nullptr /* End of list */
                                           }};


WebSocketsManager::WebSocketsManager()
: _context(nullptr)
, _proxy("", -1)
, _vhost(nullptr)
{
    lws_set_log_level(0x0, NULL);
}

WebSocketsManager::~WebSocketsManager()
{
    if(_context)
    {
        lws_context_destroy(_context);
        _context = nullptr;
        _vhost = nullptr;
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
        info.options = LWS_SERVER_OPTION_EXPLICIT_VHOSTS | LWS_SERVER_OPTION_SKIP_SERVER_CANONICAL_NAME | LWS_SERVER_OPTION_DO_SSL_GLOBAL_INIT;
        info.ka_time = 5000;
        info.ka_probes = 5;
        info.ka_interval = 5000;
        info.user = this;

        _context = lws_create_context(&info);
        _vhost = lws_create_vhost(_context, &info);
    }
}

lws* WebSocketsManager::connectSocketToUrl(const WebSocketConnectionInfo& pUrl, const WebSocketConnection* connection)
{
    lws* socket = nullptr;

    int use_ssl = 0;
    if(pUrl.scheme == "wss")
    {
        use_ssl = connection->getAllowSelfSignedCertificates() ? 2 : 1; /* 2 = allow self signed certs, 1 = encrypted */
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
        lws_client_connect_info ccinfo;
        ccinfo.context = _context;
        ccinfo.address = pUrl.host.c_str();
        ccinfo.port = pUrl.port;
        ccinfo.ssl_connection = use_ssl;
        ccinfo.path = pUrl.path.c_str();
        ccinfo.host = pUrl.host.c_str();
        ccinfo.origin = connection->getOrigin().c_str();
        ccinfo.protocol = suppProtocolsCStr;
        ccinfo.ietf_version_or_minus_one = -1;
        ccinfo.client_exts = nullptr;
        ccinfo.method = nullptr;
        ccinfo.parent_wsi = nullptr;
        ccinfo.uri_replace_from = nullptr;
        ccinfo.uri_replace_to = nullptr;
        ccinfo.vhost = _vhost;

        if(!_proxy.host.empty() && _proxy.port != -1)
        {
            std::ostringstream proxyAddr;
            proxyAddr << _proxy.host << ":" << _proxy.port;
            lws_set_proxy(_vhost, proxyAddr.str().c_str());
        }
        lwsl_notice("Client connecting to %s:%u%s use_ssl:%d ....\n", pUrl.host.c_str(), pUrl.port, pUrl.path.c_str(), use_ssl);
        socket = lws_client_connect_via_info(&ccinfo);
    }
    else
    {
        lwsl_err("libwebsocket init failed\n");
    }

    return socket;
}

void WebSocketsManager::setProxySettings(WebSocketsManager::ProxySettings proxy)
{
    _proxy = std::move(proxy);
}

WebSocketConnection* WebSocketsManager::get(lws* wsi)
{
    auto itr = _mapConnection.find(wsi);
    if(itr != _mapConnection.end())
    {
        return itr->second;
    }
    else
    {
        return nullptr;
    }
}

void WebSocketsManager::connect(WebSocketConnection* connection)
{
    assert(!connection->getWebsocket());
    const std::vector<WebSocketConnectionInfo>& vecUrls = connection->getVecUrls();
    assert(!vecUrls.empty());

    size_t currentUrlIndex = connection->getCurrentUrlIndex() % vecUrls.size();
    const WebSocketConnectionInfo& currentUrl = vecUrls[currentUrlIndex];

    std::stringstream ss;
    ss << currentUrl;
    std::string urlStr = ss.str();

    currentUrlIndex++;
    connection->setCurrentUrlIndex(currentUrlIndex);

    checkAndCreateContext();

    lws* websocket = connectSocketToUrl(currentUrl, connection);

    if(websocket)
    {
        lwsl_notice("Wsi %p added to connection map\n", websocket);
        connection->setWebsocket(websocket);
        _mapConnection.insert(std::make_pair(websocket, connection));
        lwsl_notice("Client connected to %s\n", urlStr.c_str());
    }
    else if(currentUrlIndex < vecUrls.size())
    {
        lwsl_err("Client failed to connect to %s\n", urlStr.c_str());
        connect(connection);
    }
    else
    {
        currentUrlIndex = 0;
        connection->connectionError((int)WebSocketConnection::Error::ConnectionError, "Connection Error. Cannot connect to any of the specified URLs");
        connection->setState(WebSocketConnection::State::Closed);
    }
}

void WebSocketsManager::dataReadyToSendOnConnection(WebSocketConnection* connection)
{
    lwsl_notice("Wsi %p has data ready to send\n", connection->getWebsocket());
    if(_context && connection->getWebsocket())
    {
        lws_callback_on_writable(connection->getWebsocket());
    }
}

void WebSocketsManager::markSocketToClose(lws* wsi)
{
    lwsl_notice("Wsi %p market to close\n", wsi);
    _setSocketsShouldClose.insert(wsi);
    if(_context)
    {
        lws_callback_on_writable(wsi);
    }
}

bool WebSocketsManager::isSocketMarkedToClose(lws* wsi)
{
    return _setSocketsShouldClose.count(wsi) != 0;
}

void WebSocketsManager::removeSocketFromShouldCloseSet(lws* wsi)
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
    lws_service(_context, 0);
}
