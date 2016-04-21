
#include "UnityGameObject.h"
#include <android/log.h>

#define LOG_TAG "UnityGameObject"
#define  LogError(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

JniEnv::JniEnv(JavaVM* java):
_java(java), _env(nullptr), _attached(false)
{
    if(_java->GetEnv((void**)&_env, JNI_VERSION_1_4) == JNI_EDETACHED)
    {
        _java->AttachCurrentThread(&_env, 0);
        _attached = true;
    }
}

JniEnv::~JniEnv()
{
    if(_attached)
    {
        _java->DetachCurrentThread();
    }
}

JNIEnv* JniEnv::operator->()
{
    return _env;
}

JniEnv::operator bool()
{
    return _env;
}

JavaVM* UnityGameObject::_java = nullptr;
jclass UnityGameObject::_jcls = nullptr;
jmethodID UnityGameObject::_jsendmsg = nullptr;

UnityGameObject::UnityGameObject(const std::string name)
{
    _objectName = name;
}

JniEnv UnityGameObject::getEnv()
{
    return JniEnv(_java);
}

void UnityGameObject::setJava(JavaVM* java)
{
    _java = java;
    JniEnv env = getEnv();
    _jcls = env->FindClass("com/unity3d/player/UnityPlayer");
    if(_jcls)
    {
        _jsendmsg = env->GetStaticMethodID(_jcls,
            "UnitySendMessage", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V");
    }
    LogError("Got refs UnityPlayer=%x and UnitySendMessage=%x", (uintptr_t)_jcls, (uintptr_t)_jsendmsg);
}

void UnityGameObject::SendMessage(const std::string& method)
{
    SendMessage(method, std::string());
}

void UnityGameObject::SendMessage(const std::string& method, const std::string& parameter)
{
    LogError("Trying to call UnitySendMessage %s.%s(\"%s\")", _objectName.c_str(), method.c_str(), parameter.c_str());
    if(_objectName.empty() || method.empty())
    {
        LogError("Invalid parameters.");
        return;
    }
    JniEnv env = getEnv();
    if(!env)
    {
        LogError("Could not get jni env.");
        return;
    }
    jstring objectStr = env->NewStringUTF(_objectName.c_str());
    jstring methodStr = env->NewStringUTF(method.c_str());
    jstring parameterStr = env->NewStringUTF(parameter.c_str());
    env->CallStaticVoidMethod(_jcls, _jsendmsg, objectStr, methodStr, parameterStr);
    jthrowable jexc = env->ExceptionOccurred();
    if (jexc)
    {
        jclass jcls = env->FindClass("java/lang/Throwable");
        jmethodID jmtd = env->GetMethodID(jcls, "toString", "()Ljava/lang/String;");
        jstring jstr = (jstring) env->CallObjectMethod(jexc, jmtd);
        const char *str = env->GetStringUTFChars(jstr, 0);
        env->ReleaseStringUTFChars(jstr, str);
        LogError("Unable to call UnitySendMessage: %s", str);
        env->DeleteLocalRef(jstr);
        // env->ExceptionDescribe();
        env->DeleteLocalRef(jexc);
        env->ExceptionClear();
    }
}
