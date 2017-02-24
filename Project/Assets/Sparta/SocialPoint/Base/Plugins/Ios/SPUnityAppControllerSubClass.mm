#import "SPUnityAppControllerSubClass.h"

#import <AdSupport/AdSupport.h>
#import <UIKit/UIKit.h>

#if !UNITY_TVOS
#import <SPUnityPlugins/UnityGameObject.h>
#import <SPUnityPlugins/SPUnityApplication.h>
#else
#import <SPUnityPlugins_tvOS/UnityGameObject.h>
#import <SPUnityPlugins_tvOS/SPUnityApplication.h>
#endif

#include <string>

@implementation SPUnityAppControllerSubClass

// MANU TODO: ORDER THIS FILE AS SPUnityApplication

// AppReady flag defined in UnityAppController
extern bool _unityAppReady;

+ (void)load
{
    extern const char* AppControllerClassName;
    AppControllerClassName = "SPUnityAppControllerSubClass";

    /**
     * Initialize library components
     */
    UnityGameObject::setSendMessageDelegate([](const std::string& name, const std::string& method, const std::string& message)
                                            {
                                                UnitySendMessage(name.c_str(), method.c_str(), message.c_str());
                                            });
    
    [SPUnityApplication setupApplication:&_unityAppReady];
}


- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    if([SPUnityApplication application:application didFinishLaunchingWithOptions:launchOptions])
    {
        [super application:application didFinishLaunchingWithOptions:launchOptions];
    }
    
    return YES;
}

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
    if([SPUnityApplication application:application openURL:url sourceApplication:sourceApplication annotation:annotation])
    {
        [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    }
    
    return YES;
}
#endif

#pragma mark - Notifications

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings// IOS 8.0
{
    if([SPUnityApplication application:application didRegisterUserNotificationSettings:notificationSettings])
    {
        [super application:application didRegisterUserNotificationSettings:notificationSettings];
    }
}
#endif

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken// IOS 3.0
{
    if([SPUnityApplication application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken])
    {
        [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
    }
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error// IOS 3.0
{
    if([SPUnityApplication application:application didFailToRegisterForRemoteNotificationsWithError:error])
    {
        [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
    }
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo// IOS 3.0
{
    if([SPUnityApplication application:application didReceiveRemoteNotification:userInfo])
    {
        [super application:application didReceiveRemoteNotification:userInfo];
    }
}

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification// IOS 4.0
{
    if([SPUnityApplication application:application didReceiveLocalNotification:notification])
    {
        [super application:application didReceiveLocalNotification:notification];
    }
}
#endif

#if !UNITY_TVOS
- (void)application:(UIApplication*)application
  performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem
             completionHandler:(void (^)(BOOL))completionHandler
{
    __block BOOL completionHandlerCalled = NO;
    
    auto callback = ^(BOOL result) {
        if(!completionHandlerCalled)
        {
            completionHandlerCalled = YES;
            completionHandler(result);
        }
    };
    
    if([SPUnityApplication application:application performActionForShortcutItem:shortcutItem])
    {
        [super application:application performActionForShortcutItem:shortcutItem completionHandler:callback];
    }
    
    callback(YES);
}
#endif

- (void)applicationDidEnterBackground:(UIApplication*)application
{
    if([SPUnityApplication applicationDidEnterBackground:application])
    {
        [super applicationDidEnterBackground:application];
    }
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
    if([SPUnityApplication applicationWillEnterForeground:application])
    {
        [super applicationWillEnterForeground:application];
    }
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    if([SPUnityApplication applicationDidBecomeActive:application])
    {
        [super applicationDidBecomeActive:application];
    }
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    BOOL callSuper = [SPUnityApplication applicationWillResignActive:application];
    
    //aditional game loop to allow scripts response before being paused
    UnityBatchPlayerLoop();
    
    if(callSuper)
    {
        [super applicationWillResignActive:application];
    }
}

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    if([SPUnityApplication applicationDidReceiveMemoryWarning:application])
    {
        [super applicationDidReceiveMemoryWarning:application];
    }
}

@end
