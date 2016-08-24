#include "JniEnv.h"

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
