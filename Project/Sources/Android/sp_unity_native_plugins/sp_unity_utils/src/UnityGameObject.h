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

class UnityGameObject
{
private:
    std::string _objectName;
    static JNIEnv* _env;
public:
    UnityGameObject(const std::string name);
    void SendMessage(const std::string& method);
    void SendMessage(const std::string& method, const std::string& msg);
    static void SetJniEnv(JNIEnv* env);
};
#endif /* __UnityGameObject__ */
