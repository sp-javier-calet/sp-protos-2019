#ifndef __SPNativeCallsSender__
#define __SPNativeCallsSender__

#include <string>
#include "JniEnv.h"

class SPNativeCallsSender
{
private:
    static JavaVM* _java;
    static jclass _jcls;
public:
    static void setJava(JavaVM* java);
    static void SendMessage(const std::string& method);
    static void SendMessage(const std::string& method, const std::string& msg);
};
#endif /* __SPNativeCallsSender__ */
