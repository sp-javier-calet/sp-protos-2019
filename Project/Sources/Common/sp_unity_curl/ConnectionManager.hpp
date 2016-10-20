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
#include "curl.h"
}

/* Information associated with a specific easy handle */
struct SPUnityCurlMessagesInfo
{
    std::string incoming;
    std::string outcoming;
};

class CurlConnection
{
public:
    static const int kInvalidId = 0;
    
    const int id;
    const bool isValid;
    
    CURL* easy = NULL;
    int responseCode = 0;
    int errorCode = 0;
    std::string bodyBuffer;
    std::string headersBuffer;
    std::string errorBuffer;
    double downloadSize = 0.0;
    double downloadSpeed = 0.0;
    double connectTime = 0.0;
    double totalTime = 0.0;
    bool isActive = true;
    SPUnityCurlMessagesInfo messages;
    
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
