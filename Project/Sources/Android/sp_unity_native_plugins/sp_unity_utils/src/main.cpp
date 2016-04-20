#include <jni.h>
#include "UnityGameObject.h"

jint JNI_OnLoad(JavaVM* vm, void* reserved)
{
  JNIEnv* env = 0;
  vm->AttachCurrentThread(&env, 0);
  UnityGameObject::SetJniEnv(env);
  return JNI_VERSION_1_6;
}
