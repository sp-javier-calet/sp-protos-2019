//
//  SPUnityWebSockets.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 31/10/16.
//
//

// Which platform we are on?
#if _MSC_VER
#define UNITY_WIN 1
#else
#define UNITY_OSX 1
#endif

// Attribute to make function be exported from a plugin
#if UNITY_WIN
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API
#endif

#include "WebSocketConnection.hpp"


/*
 * Native interface
 */
extern "C"
{
}