#import "UnityAppController.h"

#if !UNITY_TVOS
#import <SPUnityPlugins/SPAppControllerDelegator.h>
#import <SPUnityPlugins/UnityGameObject.h>
#else
#import <SPUnityPlugins_tvOS/SPAppControllerDelegator.h>
#import <SPUnityPlugins_tvOS/UnityGameObject.h>
#endif

#import <UserNotifications/UserNotifications.h>

@interface SPUnityAppControllerSubClass : UnityAppController<UNUserNotificationCenterDelegate>
+ (void)load;
@end

@implementation SPUnityAppControllerSubClass
{
    SPAppControllerDelegator* _delegator;
}

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

}

- (id)init
{
    self = [super init];
    if(self != nil)
    {
        _delegator = [[SPAppControllerDelegator alloc] init];
        [_delegator addAllDelegates];
    }
    return self;
}

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    UNUserNotificationCenter.currentNotificationCenter.delegate = self;
    if([UnityAppController instancesRespondToSelector:@selector(application:didFinishLaunchingWithOptions:)])
    {
        if(![super application:application didFinishLaunchingWithOptions:launchOptions])
        {
            return FALSE;
        }
    }
    [_delegator application:application didFinishLaunchingWithOptions:launchOptions];
    return TRUE;
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    [_delegator applicationWillResignActive:application];

    //aditional game loop to allow scripts response before being paused
    UnityBatchPlayerLoop();

    if([UnityAppController instancesRespondToSelector:@selector(applicationWillResignActive:)])
    {
        [super applicationWillResignActive:application];
    }
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    [_delegator applicationDidBecomeActive:application];
    if([UnityAppController instancesRespondToSelector:@selector(applicationDidBecomeActive:)])
    {
        [super applicationDidBecomeActive:application];
    }
}

- (void)applicationDidEnterBackground:(UIApplication*)application
{
    [_delegator applicationDidEnterBackground:application];
    if([UnityAppController instancesRespondToSelector:@selector(applicationDidEnterBackground:)])
    {
        [super applicationDidEnterBackground:application];
    }
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
    [_delegator applicationWillEnterForeground:application];
    if([UnityAppController instancesRespondToSelector:@selector(applicationWillEnterForeground:)])
    {
        [super applicationWillEnterForeground:application];
    }
}

- (void)applicationWillTerminate:(UIApplication*)application
{
    [_delegator applicationWillTerminate:application];
    if([UnityAppController instancesRespondToSelector:@selector(applicationWillTerminate:)])
    {
        [super applicationWillTerminate:application];
    }
}

- (BOOL)application:application openURL:(nonnull NSURL *)url options:(nonnull NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
    if([_delegator application:application openURL:url options:options])
    {
        return TRUE;
    }
    if([UnityAppController instancesRespondToSelector:@selector(application:openURL:options:)])
    {
        return [super application:application openURL:url options:options];
    }
    return FALSE;
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
{
    if([_delegator application:application openURL:url sourceApplication:sourceApplication annotation:annotation])
    {
        return TRUE;
    }
    if([UnityAppController instancesRespondToSelector:@selector(application:openURL:sourceApplication:annotation:)])
    {
        return [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    }
    return FALSE;
}


- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler
{
    if([_delegator application:application continueUserActivity:userActivity restorationHandler:restorationHandler])
    {
        return TRUE;
    }
    if([UnityAppController instancesRespondToSelector:@selector(application:continueUserActivity:restorationHandler:)])
    {
        return [super application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
    }
    return FALSE;
}

- (void)application:(UIApplication *)application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void (^)(void))completionHandler
{
    if([_delegator application:application handleEventsForBackgroundURLSession:identifier completionHandler:completionHandler])
    {
        return;
    }
    if([UnityAppController instancesRespondToSelector:@selector(application:handleEventsForBackgroundURLSession:completionHandler:)])
    {
        [super application:application handleEventsForBackgroundURLSession:identifier completionHandler:completionHandler];
    }
    else
    {
        completionHandler();
    }
}

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [_delegator applicationDidReceiveMemoryWarning:application];
    if([UnityAppController instancesRespondToSelector:@selector(application:applicationDidReceiveMemoryWarning:)])
    {
        [super applicationDidReceiveMemoryWarning:application];
    }
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
    [_delegator application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
    if([UnityAppController instancesRespondToSelector:@selector(application:didRegisterForRemoteNotificationsWithDeviceToken:)])
    {
        [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
    }
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    [_delegator application:application didFailToRegisterForRemoteNotificationsWithError:error];
    if([UnityAppController instancesRespondToSelector:@selector(application:didFailToRegisterForRemoteNotificationsWithError:)])
    {
        [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
    }
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    [_delegator application:application didReceiveRemoteNotification:userInfo];
    if([UnityAppController instancesRespondToSelector:@selector(application:didReceiveRemoteNotification:)])
    {
        [super application:application didReceiveRemoteNotification:userInfo];
    }
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions options))completionHandler
{
    if([_delegator userNotificationCenter:center willPresentNotification:notification withCompletionHandler:completionHandler])
    {
        return;
    }
    completionHandler(UNNotificationPresentationOptionNone);
}

#if !UNITY_TVOS

- (void)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forRemoteNotification:(NSDictionary *)userInfo completionHandler:(void (^)(void))completionHandler
{
    if([_delegator application:application handleActionWithIdentifier:identifier forRemoteNotification:userInfo completionHandler:completionHandler])
    {
        return;
    }
    if([UnityAppController instancesRespondToSelector:@selector(application:handleActionWithIdentifier:forRemoteNotification:completionHandler:)])
    {
        [super application:application handleActionWithIdentifier:identifier forRemoteNotification:userInfo completionHandler:completionHandler];
        return;
    }
    completionHandler();
}

- (void)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forLocalNotification:(UILocalNotification *)notification completionHandler:(void (^)(void))completionHandler
{
    if([_delegator application:application handleActionWithIdentifier:identifier forLocalNotification:notification completionHandler:completionHandler])
    {
        return;
    }
    if([UnityAppController instancesRespondToSelector:@selector(application:handleActionWithIdentifier:forLocalNotification:completionHandler:)])
    {
        [super application:application handleActionWithIdentifier:identifier forLocalNotification:notification completionHandler:completionHandler];
        return;
    }
    completionHandler();
}

- (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings
{
    [_delegator application:application didRegisterUserNotificationSettings:notificationSettings];
    if([UnityAppController instancesRespondToSelector:@selector(application:didRegisterUserNotificationSettings:)])
    {
        [super application:application didRegisterUserNotificationSettings:notificationSettings];
    }
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void(^)(void))completionHandler
{
    if([_delegator userNotificationCenter:center didReceiveNotificationResponse:response withCompletionHandler:completionHandler])
    {
        return;
    }
    completionHandler();
}

- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification
{
    [_delegator application:application didReceiveLocalNotification:notification];
    if([UnityAppController instancesRespondToSelector:@selector(application:didReceiveLocalNotification:)])
    {
        [super application:application didReceiveLocalNotification:notification];
    }
}

- (void)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem completionHandler:(void (^)(BOOL))completionHandler
{
    if([_delegator application:application performActionForShortcutItem:shortcutItem completionHandler:completionHandler])
    {
        return;
    }
    if([UnityAppController instancesRespondToSelector:@selector(application:performActionForShortcutItem:completionHandler:)])
    {
        [super application:application performActionForShortcutItem:shortcutItem completionHandler:completionHandler];
        return;
    }
    completionHandler(YES);
}

#endif

@end
