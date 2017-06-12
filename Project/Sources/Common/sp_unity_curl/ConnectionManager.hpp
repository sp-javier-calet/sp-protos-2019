//
//  ConnectionManager.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//
//

#ifndef __sparta__ConnectionManager__
#define __sparta__ConnectionManager__

#include <stdio.h>
#include <string>
#include <unordered_map>
#include <memory>

extern "C" {
#include <curl/curl.h>
}

/* Information associated with a specific easy handle */
struct CurlMessagesInfo
{
    std::string incoming;
    std::string outcoming;
};

struct CurlRequestInfo
{
    std::string url;
    std::string query;
    std::string method;
    std::string proxy;
    std::string headers;
    std::string body;
    int timeout;
    int activityTimeout;
};

struct CurlResponseInfo
{
    int code = 0;
    int errorCode = 0;
    double downloadSize = 0.0;
    double downloadSpeed = 0.0;
    double connectTime = 0.0;
    double totalTime = 0.0;
    std::string headers;
    std::string body;
    std::string error;
};

class CurlConnection
{
  public:
    static const int kInvalidId = 0;

    const int id;
    const bool isValid;

    CURL* easy = NULL;
    bool isActive = true;
    
    CurlRequestInfo request;
    CurlResponseInfo response;
    CurlMessagesInfo messages;

    CurlConnection(int pId)
    : id(pId)
    , isValid(pId != kInvalidId)
    {
    }
};

class ConnectionManager
{
  private:
    std::unordered_map<int, std::unique_ptr<CurlConnection>> _map;

    /* Must start with 1
     * ID must be different from 0 (INVALID_CONN_ID - reserved value for update when app is paused)
     */
    int _counterConns = 1;
    bool _isHttp2 = false;

    int generateId();
    bool add(int id);

  public:
    ConnectionManager(bool isHttp2);
    int create();
    bool remove(int id);
    CurlConnection& get(int id);
};

#endif /* __sparta__ConnectionManager__ */