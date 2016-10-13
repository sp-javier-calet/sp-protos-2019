#import "SPUnityAppControllerSubClass.h"
#import <AdSupport/AdSupport.h>
#import <UIKit/UIKit.h>

#if !UNITY_TVOS
#import <SPUnityPlugins/UnityGameObject.h>
#import <SPUnityPlugins/SPUnityNativeUtils.h>
#else
#import <SPUnityPlugins_tvOS/UnityGameObject.h>
#import <SPUnityPlugins_tvOS/SPUnityNativeUtils.h>
#endif

#include <string>

@implementation SPUnityAppControllerSubClass

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
}


- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    [super application:application didFinishLaunchingWithOptions:launchOptions];

    [AppSourceUtils clearSource];

#if !UNITY_TVOS
    UILocalNotification* notification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];
    if(notification)
    {
        /* Notice that all event processing must be synchronous,
         * since the Source could change if there are any other notification event
         */
        [AppSourceUtils storeSourceOptions:notification.userInfo withScheme:@"local"];
    }
#endif

#if !UNITY_TVOS
    if(SPUnityNativeUtils::isSystemVersionGreaterThanOrEqualTo(SPUnityNativeUtils::kV9) && launchOptions != nil)
    {
        UIApplicationShortcutItem* shortcutItem = [launchOptions objectForKey:UIApplicationLaunchOptionsShortcutItemKey];

        if(shortcutItem != nil)
            [self storeForceTouchShortcut:shortcutItem];
    }
#endif
    [AppEventsUtils notifyStatus:kStatusUpdateSource];

    return YES;
}

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
    [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];

    [AppSourceUtils storeSource:url.absoluteString];

    [AppEventsUtils notifyStatus:kStatusUpdateSource];

    return YES;
}
#endif

#pragma mark - Notifications

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings// IOS 8.0
{
    if(userAllowsNotifications())
    {
        onPermissionsGranted();
    }
}
#endif

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken// IOS 3.0
{
    [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];

    NSString* pushToken = [NSString stringWithFormat:@"%@", deviceToken];

    // apple sends the token in " <token> " format
    pushToken = [pushToken stringByReplacingOccurrencesOfString:@"<" withString:@""];
    pushToken = [pushToken stringByReplacingOccurrencesOfString:@">" withString:@""];
    pushToken = [pushToken stringByReplacingOccurrencesOfString:@" " withString:@""];

    onRegisterForRemote([pushToken UTF8String]);
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error// IOS 3.0
{
    [super application:application didFailToRegisterForRemoteNotificationsWithError:error];

    onRegisterForRemoteFailed([error.localizedDescription UTF8String]);
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo// IOS 3.0
{
    [super application:application didReceiveRemoteNotification:userInfo];

    [AppSourceUtils storeSourceOptions:userInfo withScheme:@"push"];

    [AppEventsUtils notifyStatus:kStatusUpdateSource];
}

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification// IOS 4.0
{
    [super application:application didReceiveLocalNotification:notification];

    [AppSourceUtils storeSourceOptions:notification.userInfo withScheme:@"local"];

    [AppEventsUtils notifyStatus:kStatusUpdateSource];
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
    [self storeForceTouchShortcut:shortcutItem];

    [AppEventsUtils notifyStatus:kStatusUpdateSource];

    completionHandler(YES);
}
#endif

- (void)applicationDidEnterBackground:(UIApplication*)application
{
    [super applicationDidEnterBackground:application];

    [AppEventsUtils notifyStatus:kStatusBackground];
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
#if !UNITY_TVOS
    application.applicationIconBadgeNumber = 0;
#endif

    [super applicationWillEnterForeground:application];

    // applicationWillEnterForeground: might sometimes arrive *before* actually
    // initing unity (e.g. locking on startup)
    [AppEventsUtils notifyStatus:kStatusWillGoForeground];
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    [super applicationDidBecomeActive:application];

    [AppEventsUtils notifyStatus:kStatusActive];
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    [AppEventsUtils notifyStatus:kStatusWillGoBackground];

    // aditional game loop to allow scripts response before being paused
    UnityBatchPlayerLoop();

    [super applicationWillResignActive:application];
}

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [super applicationDidReceiveMemoryWarning:application];

    [AppEventsUtils notifyStatus:kStatusMemoryWarning];
}


@end
