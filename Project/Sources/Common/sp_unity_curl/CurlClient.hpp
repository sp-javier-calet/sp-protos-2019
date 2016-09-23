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


class CurlClient
{
private:
    CURLM* _multi;
    Mutex curlUpdateLock;
    ConnectionManager _connections;
    bool _supportsHttp2;
    int _running;
    
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
