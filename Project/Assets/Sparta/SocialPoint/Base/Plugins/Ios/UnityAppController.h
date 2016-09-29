#pragma once

#import <UIKit/UIApplication.h>

const char* AppControllerClassName = "UnityAppController";

bool _unityAppReady = false;

// Fake UnityAppController to be able to compile and link SPUnityAppControllerSubClass on sp_unity_plugins.xcodeproj

@interface UnityAppController : NSObject<UIApplicationDelegate>
{
}

@end

void UnityBatchPlayerLoop();// batch mode like player loop, without rendering (usable for background processing)

void UnitySendMessage(const char* obj, const char* method, const char* msg);

@implementation UnityAppController

void UnityBatchPlayerLoop()
{
}

void UnitySendMessage(const char* obj, const char* method, const char* msg)
{
}

@end
