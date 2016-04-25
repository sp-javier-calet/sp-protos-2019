//
//  UnityGameObject.h
//  sp_unity_plugins
//
//  Created by Manuel Álvarez on 16/12/15.
//
//

#ifndef __UnityGameObject__
#define __UnityGameObject__

#include <string>
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

class UnityGameObject
{
private:
    std::string _objectName;
    static JavaVM* _java;
    static jclass _jcls;
public:
    UnityGameObject(const std::string name);
    static void setJava(JavaVM* java);
    void SendMessage(const std::string& method);
    void SendMessage(const std::string& method, const std::string& msg);
};
#endif /* __UnityGameObject__ */
