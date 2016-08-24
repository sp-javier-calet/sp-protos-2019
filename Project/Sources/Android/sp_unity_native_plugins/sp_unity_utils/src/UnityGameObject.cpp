
#include "UnityGameObject.h"
#include <android/log.h>

#define LOG_TAG "UnityGameObject"
#define  LogError(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

JavaVM* UnityGameObject::_java = nullptr;
jclass UnityGameObject::_jcls = nullptr;

UnityGameObject::UnityGameObject(const std::string name)
{
    _objectName = name;
}

void UnityGameObject::setJava(JavaVM* java)
{
    if(java)
    {
        _java = java;
        JniEnv env(_java);
        jclass jcls = env->FindClass("es/socialpoint/unity/base/SPUnityActivity");
        _jcls = (jclass)env->NewGlobalRef(jcls);
        env->DeleteLocalRef(jcls);
        LogError("Got ref UnityPlayer=%x", (uintptr_t)_jcls);
    }
    else if(_java)
    {
        JniEnv env(_java);
        LogError("Cleaning up ref UnityPlayer=%x", (uintptr_t)_jcls);
        if(_jcls)
        {
            env->DeleteGlobalRef(_jcls);
        }

    }
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
    JniEnv env(_java);
    if(!env)
    {
        LogError("Could not get jni env.");
        return;
    }

    jmethodID jsendmsg = env->GetStaticMethodID(_jcls,
         "UnitySendMessage", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V");
    jstring objectStr = env->NewStringUTF(_objectName.c_str());
    jstring methodStr = env->NewStringUTF(method.c_str());
    jstring parameterStr = env->NewStringUTF(parameter.c_str());
    env->CallStaticVoidMethod(_jcls, jsendmsg, objectStr, methodStr, parameterStr);
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
        env->ExceptionDescribe();
        env->DeleteLocalRef(jexc);
        env->ExceptionClear();
    }
}
