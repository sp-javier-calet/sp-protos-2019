
#include "UnityGameObject.h"
#include <android/log.h>
#include <cassert>

#define LOG_TAG "UnityGameObject"

JNIEnv* UnityGameObject::_env = nullptr;

UnityGameObject::UnityGameObject(const std::string name)
{
    assert(!name.empty() && "Empty GameObject name");
    _objectName = name;
}

void UnityGameObject::SetJniEnv(JNIEnv* env)
{
    _env = env;
}

void UnityGameObject::SendMessage(const std::string& method)
{
    SendMessage(method, std::string());
}

void UnityGameObject::SendMessage(const std::string& method, const std::string& parameter)
{
    assert(_env && "Empty JNI Environment");
	jclass UnityPlayer = _env->FindClass("com/unity3d/player/UnityPlayer");
	jmethodID UnitySendMessage = _env->GetStaticMethodID(UnityPlayer, "UnitySendMessage", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V");
	jthrowable exception = _env->ExceptionOccurred();
	if(exception)
	{
		_env->ExceptionDescribe();
		_env->DeleteLocalRef(exception);
		_env->ExceptionClear();
		return;
	}
	jstring objectStr = _env->NewStringUTF(_objectName.c_str());
	jstring methodStr = _env->NewStringUTF(method.c_str());
	jstring parameterStr = _env->NewStringUTF(parameter.c_str());
	_env->CallStaticVoidMethod(UnityPlayer, UnitySendMessage, objectStr, methodStr, parameterStr);
	exception = _env->ExceptionOccurred();
	if(exception)
	{
		_env->ExceptionDescribe();
		_env->DeleteLocalRef(exception);
		_env->ExceptionClear();
	}
}
