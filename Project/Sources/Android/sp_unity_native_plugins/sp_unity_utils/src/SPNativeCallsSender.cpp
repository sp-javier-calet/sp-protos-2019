
#include "SPNativeCallsSender.h"
#include <android/log.h>

#define LOG_TAG "SPNativeCallsSender"
#define  LogError(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

JavaVM* SPNativeCallsSender::_java = nullptr;
jclass SPNativeCallsSender::_jcls = nullptr;

void SPNativeCallsSender::setJava(JavaVM* java)
{
    if(java)
    {
        _java = java;
        JniEnv env(_java);
        jclass jcls = env->FindClass("es/socialpoint/unity/base/SPNativeCallsSender");
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

void SPNativeCallsSender::SendMessage(const std::string& method)
{
    SendMessage(method, std::string());
}

void SPNativeCallsSender::SendMessage(const std::string& method, const std::string& parameter)
{
    LogError("Trying to call UnitySendMessage %s(\"%s\")", method.c_str(), parameter.c_str());
    if(method.empty())
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
         "SendMessage", "(Ljava/lang/String;Ljava/lang/String;)V");
    jstring methodStr = env->NewStringUTF(method.c_str());
    jstring parameterStr = env->NewStringUTF(parameter.c_str());
    env->CallStaticVoidMethod(_jcls, jsendmsg, methodStr, parameterStr);
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
