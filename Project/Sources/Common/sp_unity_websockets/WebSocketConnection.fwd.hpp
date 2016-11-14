//
//  WebSocketConnection.fwd.hpp
//  sp_unity_plugins
//
//  Created by Fernando Serra on 11/11/16.
//
//

#pragma once

#include <string>
#include <iostream>

struct WebSocketConnectionInfo
{
    std::string scheme;
    std::string host;
    std::string path;
    int port;
};

inline std::ostream& operator<<(std::ostream& os, const WebSocketConnectionInfo& info)
{
    os << info.scheme << "://" << info.host << ':' << info.port << '/' << info.path;
    return os;
}
