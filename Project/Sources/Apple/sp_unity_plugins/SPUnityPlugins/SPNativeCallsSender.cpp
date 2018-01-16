//
//  SPNativeCallsSender.cpp
//  sp_unity_plugins
//
//  Created by Miguel Janer on 27/5/16.
//
//

#include "SPNativeCallsSender.h"
#include "UnityGameObject.h"

namespace {
    std::string _gameObjectName;
    std::string _listenerMethodName;
    std::string _separator;
}


std::string SPNativeCallsSender::combineMethodMessage(const std::string& method, const std::string& msg)
{
    return method + _separator + msg;
}

void SPNativeCallsSender::SendMessage(const std::string& method)
{
    UnityGameObject(_gameObjectName).SendMessage(_listenerMethodName, combineMethodMessage(method, std::string()));
}

void SPNativeCallsSender::SendMessage(const std::string& method, const std::string& msg)
{
    UnityGameObject(_gameObjectName).SendMessage(_listenerMethodName, combineMethodMessage(method, msg));
}

extern "C" {
void SPNativeCallsSender_Init(const char* listenerObjectName, const char* listenerMethodName, const char* separator)
{
    _gameObjectName = listenerObjectName;
    _listenerMethodName = listenerMethodName;
    _separator = separator;
}
}
