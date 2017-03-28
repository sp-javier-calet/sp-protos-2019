#import "UnityAppController.h"
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

@interface SPUnityAppControllerSubClass : UnityAppController

+ (void)load;

@end

@implementation SPUnityAppControllerSubClass

#pragma region - Controller Initialization

// AppReady flag defined in UnityAppController
extern bool _unityAppReady;

+ (void)load
{
    // Set this class as the main Application Controller
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

#pragma region - SubController Life Cycle Implementation

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    if([SPUnityApplication application:application didFinishLaunchingWithOptions:launchOptions])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:didFinishLaunchingWithOptions:)])
        {
            return [super application:application didFinishLaunchingWithOptions:launchOptions];
        }
    }
    
    return YES;
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    BOOL callSuper = [SPUnityApplication applicationWillResignActive:application];
    
    //aditional game loop to allow scripts response before being paused
    UnityBatchPlayerLoop();
    
    if(callSuper)
    {
        if([UnityAppController instancesRespondToSelector:@selector(applicationWillResignActive:)])
        {
            [super applicationWillResignActive:application];
        }
    }
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    if([SPUnityApplication applicationDidBecomeActive:application])
    {
        if([UnityAppController instancesRespondToSelector:@selector(applicationDidBecomeActive:)])
        {
            [super applicationDidBecomeActive:application];
        }
    }
}

- (void)applicationDidEnterBackground:(UIApplication*)application
{
    if([SPUnityApplication applicationDidEnterBackground:application])
    {
        if([UnityAppController instancesRespondToSelector:@selector(applicationDidEnterBackground:)])
        {
            [super applicationDidEnterBackground:application];
        }
    }
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
    if([SPUnityApplication applicationWillEnterForeground:application])
    {
        if([UnityAppController instancesRespondToSelector:@selector(applicationWillEnterForeground:)])
        {
            [super applicationWillEnterForeground:application];
        }
    }
}

- (void)applicationWillTerminate:(UIApplication*)application
{
    if([SPUnityApplication applicationWillTerminate:application])
    {
        if([UnityAppController instancesRespondToSelector:@selector(applicationWillTerminate:)])
        {
            [super applicationWillTerminate:application];
        }
    }
}

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
    if([SPUnityApplication application:application openURL:url sourceApplication:sourceApplication annotation:annotation])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:openURL:sourceApplication:annotation:)])
        {
            [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
        }
    }
    
    return YES;
}
#endif

- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler
{
    if([SPUnityApplication application:application continueUserActivity:userActivity restorationHandler:restorationHandler])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:continueUserActivity:restorationHandler:)])
        {
            [super application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
        }
    }
    
    return YES;
}

#pragma mark - Memory management

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    if([SPUnityApplication applicationDidReceiveMemoryWarning:application])
    {
        if([UnityAppController instancesRespondToSelector:@selector(applicationDidReceiveMemoryWarning:)])
        {
            [super applicationDidReceiveMemoryWarning:application];
        }
    }
}

#pragma mark - Notifications

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings// IOS 8.0
{
    if([SPUnityApplication application:application didRegisterUserNotificationSettings:notificationSettings])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:didRegisterUserNotificationSettings:)])
        {
            [super application:application didRegisterUserNotificationSettings:notificationSettings];
        }
    }
}
#endif

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken// IOS 3.0
{
    if([SPUnityApplication application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:didRegisterForRemoteNotificationsWithDeviceToken:)])
        {
            [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
        }
    }
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error// IOS 3.0
{
    if([SPUnityApplication application:application didFailToRegisterForRemoteNotificationsWithError:error])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:didFailToRegisterForRemoteNotificationsWithError:)])
        {
            [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
        }
    }
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo// IOS 3.0
{
    if([SPUnityApplication application:application didReceiveRemoteNotification:userInfo])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:didReceiveRemoteNotification:)])
        {
            [super application:application didReceiveRemoteNotification:userInfo];
        }
    }
}

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification// IOS 4.0
{
    if([SPUnityApplication application:application didReceiveLocalNotification:notification])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:didReceiveLocalNotification:)])
        {
            [super application:application didReceiveLocalNotification:notification];
        }
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
    
    if([SPUnityApplication application:application performActionForShortcutItem:shortcutItem completionHandler:callback])
    {
        if([UnityAppController instancesRespondToSelector:@selector(application:performActionForShortcutItem:completionHandler:)])
        {
            [super application:application performActionForShortcutItem:shortcutItem completionHandler:callback];
        }
    }
    
    callback(YES);
}
#endif

@end
