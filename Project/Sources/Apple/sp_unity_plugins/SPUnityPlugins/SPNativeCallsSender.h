//
//  SPNativeCallsSender.h
//  sp_unity_plugins
//
//  Created by Miguel Janer on 27/5/16.
//
//

#ifndef SPNativeCallsSender_
#define SPNativeCallsSender_

#include <string>

class SPNativeCallsSender
{
  private:
    static std::string combineMethodMessage(const std::string& method, const std::string& msg);

  public:
    static void SendMessage(const std::string& method);
    static void SendMessage(const std::string& method, const std::string& msg);
};
#endif /* SPNativeCallsSender_ */
