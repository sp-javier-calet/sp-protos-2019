#include <jni.h>
#include "UnityGameObject.h"

jint JNI_OnLoad(JavaVM* vm, void* reserved)
{
    UnityGameObject::setJava(vm);
    return JNI_VERSION_1_4;
}

void JNI_OnUnload(JavaVM *vm, void *reserved)
{
    UnityGameObject::setJava(nullptr);
}
