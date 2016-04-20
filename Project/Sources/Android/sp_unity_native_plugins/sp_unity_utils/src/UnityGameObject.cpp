
#include "UnityGameObject.h"
#include "JniObject.hpp"
#include <android/log.h>
#include <cassert>

#define LOG_TAG "UnityGameObject"

UnityGameObject::UnityGameObject(const std::string name)
{
    assert(!name.empty() && "Empty GameObject name");
    _objectName = name;
}

void UnityGameObject::SendMessage(const std::string& method)
{
    SendMessage(method, std::string());
}

void UnityGameObject::SendMessage(const std::string& method, const std::string& parameter)
{
    __android_log_print(ANDROID_LOG_INFO, LOG_TAG, "Trying to call UnitySendMessage %s.%s(\"%s\")", _objectName.c_str(), method.c_str(), parameter.c_str());
    assert(_java && "Empty JavaVM");

    JniObject player("com.unity3d.player.UnityPlayer");
    player.staticCallVoid("UnitySendMessage", _objectName, method, parameter);
}
