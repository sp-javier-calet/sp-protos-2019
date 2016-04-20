#include <jni.h>
#include "JniObject.hpp"

jint JNI_OnLoad(JavaVM* vm, void* reserved)
{
    Jni::get().setJava(vm);
    return JNI_VERSION_1_4;
}
