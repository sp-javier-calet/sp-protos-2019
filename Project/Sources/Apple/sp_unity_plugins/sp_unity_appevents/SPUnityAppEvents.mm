//
//  SPUnityAppEvents.m
//  Unity-iPhone
//
//  Created by Manuel Álvarez on 21/01/16.
//
//

#import "SPUnityNativeUtils.h"
#import "SPUnityAppEvents.h"

@implementation SPUnityAppEvents
{
}

static SPUnityAppEvents* _instance;

- (id) init
{
    if(self = [super init])
    {
        _instance = self;
        [super alwaysCallSuper];
    }
    return self;
}


- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
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
        {
            [self storeForceTouchShortcut:shortcutItem];
        }
    }
#endif
    [AppEventsUtils notifyStatus:kStatusUpdateSource];
    
    return YES;
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
{
    [AppSourceUtils storeSource:url.absoluteString];
    [AppEventsUtils notifyStatus:kStatusUpdateSource];

    return YES;
}

#if !UNITY_TVOS
-  (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification *)notification
{
    [AppSourceUtils storeSourceOptions:notification.userInfo withScheme:@"local"];
    [AppEventsUtils notifyStatus:kStatusUpdateSource];
}
#endif

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings// IOS 8.0
{
    if(userAllowsNotifications())
    {
        onPermissionsGranted();
    }
    
    return YES;
}
#endif

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken// IOS 3.0
{
    NSString* pushToken = [NSString stringWithFormat:@"%@", deviceToken];
    
    // apple sends the token in " <token> " format
    pushToken = [pushToken stringByReplacingOccurrencesOfString:@"<" withString:@""];
    pushToken = [pushToken stringByReplacingOccurrencesOfString:@">" withString:@""];
    pushToken = [pushToken stringByReplacingOccurrencesOfString:@" " withString:@""];
    
    onRegisterForRemote([pushToken UTF8String]);
}

- (BOOL)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error// IOS 3.0
{
    onRegisterForRemoteFailed([error.localizedDescription UTF8String]);
    return YES;
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    [AppSourceUtils storeSourceOptions:userInfo withScheme:@"push"];
    [AppEventsUtils notifyStatus:kStatusUpdateSource];
}

- (void)applicationDidEnterBackground:(UIApplication *)application
{
    [AppEventsUtils notifyStatus:kStatusBackground];
}

- (void)applicationWillEnterForeground:(UIApplication *)application
{
    // applicationWillEnterForeground: might sometimes arrive *before* actually
    // initing unity (e.g. locking on startup)
    [AppEventsUtils notifyStatus:kStatusWillGoForeground];
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    [AppEventsUtils notifyStatus:kStatusActive];
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    [AppEventsUtils notifyStatus:kStatusWillGoBackground];
}

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [AppEventsUtils notifyStatus:kStatusMemoryWarning];
}


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

@end
