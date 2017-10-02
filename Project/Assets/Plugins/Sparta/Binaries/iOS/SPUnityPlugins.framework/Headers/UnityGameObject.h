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
#include <functional>

class UnityGameObject
{
  public:
    typedef std::function<void(std::string, std::string, std::string)> SendMessageDelegate;

  private:
    static SendMessageDelegate* _sendMessageDelegate;

    std::string _objectName;

  public:
    static void setSendMessageDelegate(const SendMessageDelegate& delegate);

    UnityGameObject(const std::string name);

    void SendMessage(const std::string& method);
    void SendMessage(const std::string& method, const std::string& msg);
};
#endif /* __UnityGameObject__ */
