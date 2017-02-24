#import "SPUnityAppControllerSubClass.h"
#import <AdSupport/AdSupport.h>
#import <UIKit/UIKit.h>
#import <SPUnityPlugins/SPUnityApplication.h>
#if !UNITY_TVOS
#import <SPUnityPlugins/UnityGameObject.h>
#import <SPUnityPlugins/SPUnityNativeUtils.h>
#else
#import <SPUnityPlugins_tvOS/UnityGameObject.h>
#import <SPUnityPlugins_tvOS/SPUnityNativeUtils.h>
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
//    [super application:application didFinishLaunchingWithOptions:launchOptions];
//
//    [AppSourceUtils clearSource];
//
//#if !UNITY_TVOS
//    UILocalNotification* notification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];
//    if(notification)
//    {
//        /* Notice that all event processing must be synchronous,
//         * since the Source could change if there are any other notification event
//         */
//        [AppSourceUtils storeSourceOptions:notification.userInfo withScheme:@"local"];
//    }
//#endif
//
//#if !UNITY_TVOS
//    if(SPUnityNativeUtils::isSystemVersionGreaterThanOrEqualTo(SPUnityNativeUtils::kV9) && launchOptions != nil)
//    {
//        UIApplicationShortcutItem* shortcutItem = [launchOptions objectForKey:UIApplicationLaunchOptionsShortcutItemKey];
//
//        if(shortcutItem != nil)
//            [self storeForceTouchShortcut:shortcutItem];
//    }
//#endif
//    [AppEventsUtils notifyStatus:kStatusUpdateSource];
//
//    return YES;
    
    if([SPUnityApplication application:application didFinishLaunchingWithOptions:launchOptions])
    {
        [super application:application didFinishLaunchingWithOptions:launchOptions];
    }
    
    return YES;
}

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
//    [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
//
//    [AppSourceUtils storeSource:url.absoluteString];
//
//    [AppEventsUtils notifyStatus:kStatusUpdateSource];
//
//    return YES;
    
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
//    if(userAllowsNotifications())
//    {
//        onPermissionsGranted();
//    }
    
    if([SPUnityApplication application:application didRegisterUserNotificationSettings:notificationSettings])
    {
        [super application:application didRegisterUserNotificationSettings:notificationSettings];
    }
}
#endif

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken// IOS 3.0
{
//    [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
//
//    NSString* pushToken = [NSString stringWithFormat:@"%@", deviceToken];
//
//    // apple sends the token in " <token> " format
//    pushToken = [pushToken stringByReplacingOccurrencesOfString:@"<" withString:@""];
//    pushToken = [pushToken stringByReplacingOccurrencesOfString:@">" withString:@""];
//    pushToken = [pushToken stringByReplacingOccurrencesOfString:@" " withString:@""];
//
//    onRegisterForRemote([pushToken UTF8String]);
    
    if([SPUnityApplication application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken])
    {
        [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
    }
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error// IOS 3.0
{
//    [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
//
//    onRegisterForRemoteFailed([error.localizedDescription UTF8String]);
    
    if([SPUnityApplication application:application didFailToRegisterForRemoteNotificationsWithError:error])
    {
        [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
    }
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo// IOS 3.0
{
//    [super application:application didReceiveRemoteNotification:userInfo];
//
//    [AppSourceUtils storeSourceOptions:userInfo withScheme:@"push"];
//
//    [AppEventsUtils notifyStatus:kStatusUpdateSource];
    
    if([SPUnityApplication application:application didReceiveRemoteNotification:userInfo])
    {
        [super application:application didReceiveRemoteNotification:userInfo];
    }
}

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification// IOS 4.0
{
//    [super application:application didReceiveLocalNotification:notification];
//
//    [AppSourceUtils storeSourceOptions:notification.userInfo withScheme:@"local"];
//
//    [AppEventsUtils notifyStatus:kStatusUpdateSource];
    
    if([SPUnityApplication application:application didReceiveLocalNotification:notification])
    {
        [super application:application didReceiveLocalNotification:notification];
    }
}
#endif

#if !UNITY_TVOS
- (void)storeForceTouchShortcut:(UIApplicationShortcutItem*)shortcut
{
    NSDictionary* dictionary = @{kEventTypeKey : [shortcut type]};

    [AppSourceUtils storeSourceOptions:dictionary withScheme:@"appshortcut"];
}

- (void)application:(UIApplication*)application
  performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem
             completionHandler:(void (^)(BOOL))completionHandler
{
//    [self storeForceTouchShortcut:shortcutItem];
//
//    [AppEventsUtils notifyStatus:kStatusUpdateSource];
//
//    completionHandler(YES);
    
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
//    [super applicationDidEnterBackground:application];
//
//    [AppEventsUtils notifyStatus:kStatusBackground];
    
    if([SPUnityApplication applicationDidEnterBackground:application])
    {
        [super applicationDidEnterBackground:application];
    }
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
//#if !UNITY_TVOS
//    application.applicationIconBadgeNumber = 0;
//#endif
//
//    [super applicationWillEnterForeground:application];
//
//    // applicationWillEnterForeground: might sometimes arrive *before* actually
//    // initing unity (e.g. locking on startup)
//    [AppEventsUtils notifyStatus:kStatusWillGoForeground];
    
    if([SPUnityApplication applicationWillEnterForeground:application])
    {
        [super applicationWillEnterForeground:application];
    }
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
//    [super applicationDidBecomeActive:application];
//
//    [AppEventsUtils notifyStatus:kStatusActive];
    
    if([SPUnityApplication applicationDidBecomeActive:application])
    {
        [super applicationDidBecomeActive:application];
    }
}

- (void)applicationWillResignActive:(UIApplication*)application
{
//    [AppEventsUtils notifyStatus:kStatusWillGoBackground];
//
//    // aditional game loop to allow scripts response before being paused
//    UnityBatchPlayerLoop();
//
//    [super applicationWillResignActive:application];
    
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
//    [super applicationDidReceiveMemoryWarning:application];
//
//    [AppEventsUtils notifyStatus:kStatusMemoryWarning];
    
    if([SPUnityApplication applicationDidReceiveMemoryWarning:application])
    {
        [super applicationDidReceiveMemoryWarning:application];
    }
}


@end
