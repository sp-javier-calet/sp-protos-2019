//
//  CurlTestClient.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 27/09/16.
//
//

#include "CurlTestClient.hpp"
#include "CurlClient.hpp"
#include <vector>
#include <map>
#include <ctime>
#include <string>
#include <sstream>
#include <fstream>
#include <iostream>
#include <vector>

enum class RequestType
{
    Clock, Echo
};

struct RequestUrl
{
    std::string url;
    std::string method;
    std::string data;
};

std::map<RequestType, RequestUrl> requestUrls {
    { RequestType::Clock, {"https://http2.golang.org/clockstream", "GET", ""} },
    { RequestType::Echo, {"https://http2.golang.org/ECHO", "PUT", "hello" } } };

struct Config
{
    int updateRate = 300;
    int sendDeferredTime = 5;
    int timeout = 30000;
    bool destroyConnectionsOnFinish = true;
    bool singleDeferred = false;
    bool verbose = true;
    std::string proxy = "192.168.91.144:8888";
    std::vector<RequestType> initialRequests{ RequestType::Clock };
    std::vector<RequestType> deferredRequests{ RequestType::Echo, RequestType::Clock };
};

typedef CurlRequest Request;

struct Response
{
    int cId = 0;
    bool finished = false;
    int responseCode = 0;
    std::string headers;
    int headersReceived = 0;
    std::string body;
    int bodyReceived = 0;
    std::string error;
    int errorCode = 0;
    int errorReceived = 0;
    bool connectionDestroyed = false;
};

CurlClient* _client(nullptr);
Config cfg;

Request data_create(int connection, const std::string& url, const std::string& method, const std::string& body)
{
    Request cdata;
    
    cdata.id = connection;
    cdata.url = url.c_str();
    cdata.query = "";
    cdata.method = method.c_str();
    cdata.timeout = cfg.timeout;
    cdata.activityTimeout = cfg.timeout;
    cdata.proxy = cfg.proxy.c_str();
    cdata.headers = "";
    
    if(!body.empty())
    {
        cdata.body = (const uint8_t*)body.c_str();
        cdata.bodyLength = (int)body.length();
    }
    else
    {
        cdata.body = nullptr;
        cdata.bodyLength = 0;
    }
    
    return cdata;
}

void createConnectionByType(RequestType type, std::vector<int>& connections)
{
    RequestUrl& r = requestUrls[type];
    
    printf("### Adding request %s\n", r.url.c_str());
    int conn = _client->createConnection();
    connections.push_back(conn);
    bool send = _client->send(data_create(conn, r.url, r.method, r.data));
    printf("Curl Send %s, result: %d\n", r.url.c_str(), send);
}

void clearLog()
{
    std::ofstream ss;
    ss.open("/tmp/sp_unity_curl_app.log", std::ofstream::out | std::ofstream::app);
    ss << "\n\n\n";
    ss.close();
}

void CurlTestClient::run()
{
    _client = new CurlClient(true);
    _client->setVerbose(cfg.verbose);
    
    std::vector<int> connections;
    std::vector<int> finishedConnections;
    std::map<int, Response> responses;
    
    for(auto t : cfg.initialRequests)
    {
        createConnectionByType(t, connections);
    }
    
    std::time_t sendStart = std::time(0);
    std::time_t updateStart = std::time(0);
    do
    {
        float tRate  = 1.0f/cfg.updateRate;
        if(std::time(0) - updateStart > tRate)
        {
            clearLog();
            updateStart = std::time(0);
            
            if(connections.size())
            {
                _client->update();
            }
            
            for(auto cId : connections)
            {
                Response& response = responses[cId];
                response.cId = cId;
                response.finished = _client->isFinished(cId);
                response.responseCode = _client->getResponseCode(cId);
                
                // receivedata
                
                int headersL = _client->getHeadersLength(cId);
                if(headersL != response.headers.length())
                {
                    char* buffer = new char[headersL];
                    _client->getHeaders(cId, buffer);
                    response.headersReceived++;
                    response.headers = std::string(buffer, headersL);
                    delete[] buffer;
                }
                
                int bodyL = _client->getBodyLength(cId);
                if(bodyL != response.body.length())
                {
                    char* buffer = new char[bodyL];
                    _client->getBody(cId, buffer);
                    response.bodyReceived++;
                    response.body = std::string(buffer, bodyL);
                    
                    delete[] buffer;
                }
                
                int msgL = _client->getStreamMessageLenght(cId);
                if(msgL > 0)
                {
                    char* buffer = new char[msgL];
                    _client->getStreamMessage(cId, buffer);
                    delete[] buffer;
                }
                
                int errL = _client->getErrorLength(cId);
                if(errL != response.error.length())
                {
                    char* buffer = new char[errL];
                    _client->getError(cId, buffer);
                    response.errorCode = _client->getErrorCode(cId);
                    response.errorReceived++;
                    response.error = std::string(buffer, errL);
                    delete[] buffer;
                }
                
                if(response.finished && cfg.destroyConnectionsOnFinish)
                {
                    bool removed = _client->destroyConnection(cId);
                    printf("### Finished request %d. Removed %d\n", cId, removed);
                    
                    response.connectionDestroyed = removed;
                    finishedConnections.push_back(cId);
                }
            }
        }
        
        // Add deferred requests
        static bool flag = false;
        if(std::time(0) - sendStart > cfg.sendDeferredTime && (!cfg.singleDeferred || !flag))
        {
            flag = true;
            sendStart = std::time(0);
            
            for(auto t : cfg.deferredRequests)
            {
                createConnectionByType(t, connections);
            }
        }
        
        // Remove destroyed connections from main list
        if(finishedConnections.size())
        {
            for(int finished : finishedConnections)
            {
                connections.erase(std::remove(connections.begin(), connections.end(), finished), connections.end());
            }
            finishedConnections.clear();
        }
    } while(1);
}