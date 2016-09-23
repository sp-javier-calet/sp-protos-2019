//
//  CurlClient.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//

#ifndef __sparta__CurlClient__
#define __sparta__CurlClient__

#include <stdio.h>
#include <cstdint>
#include <string>
#include "Mutex.hpp"
#include "ConnectionManager.hpp"
#include "Certificate.hpp"

extern "C" {
    #include "curl.h"
}

struct CurlRequest
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

struct CurlMessage
{
    const uint8_t* message;
    int messageLength;
};


class CurlClient
{
private:
    Mutex curlUpdateLock;
    ConnectionManager _connections;
    Certificate _certificate;
    
    CURLM* _multi;
    bool _supportsHttp2;
    int _running;
    bool _verbose;
    
    const uint8_t* _pinnedPublicKey;
    size_t _pinnedPublicKeySize;
    
    CURL* create(CurlRequest req);
    
public:
    CurlClient(bool enableHttp2);
    ~CurlClient();

    bool isRunning();
    
    bool send(CurlRequest req);
    void update();
    bool update(int id);
    bool isFinished(int id);
    
    int createConnection();
    bool destroyConnection(int id);
    
    bool sendStreamMessage(int id, CurlMessage data);
    int getStreamMessageLenght(int id);
    void getStreamMessage(int id, char* data);
    
    double getTime(int id);
    double getTotalTime(int id);
    double getDownloadSize(int id);
    double getDownloadSpeed(int id);
    int getResponseCode(int id);
    int getErrorCode(int id);
    
    bool getError(int id, char* data);
    bool getBody(int id, char* data);
    bool getHeaders(int id, char* data);
    
    int getErrorLength(int id);
    int getBodyLength(int id);
    int getHeadersLength(int id);
    
    void setConfig(const std::string& name);
};

#endif /* __sparta__CurlClient__ */
