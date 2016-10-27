//
//  ConnectionManager.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//
//

#include "ConnectionManager.hpp"
#include <limits.h>
#include <assert.h>

namespace
{
    static CurlConnection InvalidConnection(CurlConnection::kInvalidId);
}

ConnectionManager::ConnectionManager(bool isHttp2)
: _isHttp2(isHttp2)
{
}

int ConnectionManager::generateId()
{
    // Use negative ids for http2 (one bit to easily filter connections)
    int sign = _isHttp2 ? -1 : 1;
    int id = _counterConns * sign;
    
    // Increment id counter
    _counterConns = (_counterConns + 1) % INT_MAX;
    if(_counterConns == CurlConnection::kInvalidId)
    {
        _counterConns = 1; // Reset
    }
    
    return id;
}

int ConnectionManager::create()
{
    int id = generateId();
    bool added = add(id);
    if(!added)
    {
        return CurlConnection::kInvalidId;
    }

    return id;
}

bool ConnectionManager::add(int id)
{
    auto result = _map.insert(std::make_pair(id, std::unique_ptr<CurlConnection>(new CurlConnection(id))));
    if(result.second == false)
    {
        // This should not really happen. ID range is big enough and old connections should be removed when finished.
        assert(false && "A connection with the same ID already existed");
        return false;
    }
    return true;
}

CurlConnection& ConnectionManager::get(int id)
{
    auto it = _map.find(id);
    if(it != _map.end())
    {
        return *it->second;
    }
    return InvalidConnection;
}

bool ConnectionManager::remove(int id)
{
    auto it = _map.find(id);
    if(it == _map.end())
    {
        return false;
    }
    _map.erase(it);
    return true;
}
