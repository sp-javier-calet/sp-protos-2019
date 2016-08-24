#ifndef __JniEnv__
#define __JniEnv__

#include <jni.h>
class JniEnv
{
private:
    JavaVM* _java;
    JNIEnv* _env;
    bool _attached;
public:
    JniEnv(JavaVM* java);
    ~JniEnv();
    JNIEnv* operator->();
    operator bool();
};
#endif /* __JniEnv__ */