//
//  LibWebsocketTestClient.hpp
//  sp_unity_plugins
//
//  Created by Fernando Serra on 10/11/16.
//
//

#pragma once

#include <string>

class LibWebsocketTestClient
{
  public:
    void run(const std::string& scheme, const std::string& host, const std::string& path, int port);
    void run();
};
