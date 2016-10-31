//
//  WebsSocktetsClient.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 31/10/16.
//

#ifndef __sparta__WebSocketsClient__
#define __sparta__WebSocketsClient__

#include <string>

struct lws;

struct WebSocketMessage
{
    const uint8_t* message;
    int messageLength;
};

class WebSocketsClient
{
private:
    
    bool _allowSelfSignedCertificates;
    std::string _url;
    lws* _websocket;
    
public:
    WebSocketsClient(std::string url);
    ~WebSocketsClient();
    
    void addProtocol(std::string protocol);
    
    void update();
    
    bool sendStreamMessage(WebSocketMessage data);
    int getStreamMessageLenght();
    void getStreamMessage(char* data);
    
    bool isConnected();
    bool isConnecting();
    
    void connect();
    void disconnect();
    void send(std::string data);
    void ping();

};

#endif /* __sparta__WebSocketsClient__ */
