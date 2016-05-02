//
//  UnityGameObject.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez on 16/12/15.
//
//

#include "UnityGameObject.h"
#include <cassert>

UnityGameObject::SendMessageDelegate *UnityGameObject::_sendMessageDelegate;

void UnityGameObject::setSendMessageDelegate(const SendMessageDelegate& delegate)
{
    _sendMessageDelegate = new SendMessageDelegate(delegate);
}

UnityGameObject::UnityGameObject(const std::string name)
{

    assert(!name.empty() && "Empty GameObject name");
    _objectName = name;
}

void UnityGameObject::SendMessage(const std::string& method)
{
    SendMessage(method, std::string());
}

void UnityGameObject::SendMessage(const std::string& method, const std::string& msg)
{
    assert(_sendMessageDelegate && "Missing Send Message Delegate");
    if(_sendMessageDelegate && !_objectName.empty())
    {
        (*_sendMessageDelegate)(_objectName, method, msg);
    }
}
