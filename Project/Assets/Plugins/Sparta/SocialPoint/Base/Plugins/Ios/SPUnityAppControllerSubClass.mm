#import "UnityAppController.h"

#if !UNITY_TVOS
#import <SPUnityPlugins/SPAppControllerDelegator.h>
#else
#import <SPUnityPlugins_tvOS/SPAppControllerDelegator.h>
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
    if(![super application:application didFinishLaunchingWithOptions:launchOptions])
    {
        return FALSE;
    }
    [_delegator application:application didFinishLaunchingWithOptions:launchOptions];
    return TRUE;
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    [_delegator applicationWillResignActive:application];

    //aditional game loop to allow scripts response before being paused
    UnityBatchPlayerLoop();

    [super applicationWillResignActive:application];
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    [_delegator applicationDidBecomeActive:application];
    [super applicationDidBecomeActive:application];
}

- (void)applicationDidEnterBackground:(UIApplication*)application
{
    [_delegator applicationDidEnterBackground:application];
    [super applicationDidEnterBackground:application];
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
    [_delegator applicationWillEnterForeground:application];
    [super applicationWillEnterForeground:application];
}

- (void)applicationWillTerminate:(UIApplication*)application
{
    [_delegator applicationWillTerminate:application];
    [super applicationWillTerminate:application];
}

- (BOOL)application:application openURL:(nonnull NSURL *)url options:(nonnull NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
    if([_delegator application:application openURL:url options:options])
    {
        return TRUE;
    }
    return [super application:application openURL:url options:options];
}


- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler
{
    if([_delegator application:application continueUserActivity:userActivity restorationHandler:restorationHandler])
    {
        return TRUE;
    }
    return [super application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
}

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [_delegator applicationDidReceiveMemoryWarning:application];
    [super applicationDidReceiveMemoryWarning:application];
}

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken// IOS 3.0
{
    [_delegator application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
    [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error// IOS 3.0
{
    [_delegator application:application didFailToRegisterForRemoteNotificationsWithError:error];
    [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo// IOS 3.0
{
    if([_delegator application:application didReceiveRemoteNotification:userInfo])
    {
        return;
    }
    [super application:application didReceiveRemoteNotification:userInfo];
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions options))completionHandler// IOS 10.0
{
    if([_delegator userNotificationCenter:center willPresentNotification:notification withCompletionHandler:completionHandler])
    {
        return;
    }
}

#if !UNITY_TVOS

- (void)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forRemoteNotification:(NSDictionary *)userInfo completionHandler:(void (^)(void))completionHandler
{
    if([_delegator application:application handleActionWithIdentifier:identifier forRemoteNotification:userInfo completionHandler:completionHandler])
    {
        return;
    }
    [super application:application handleActionWithIdentifier:identifier forRemoteNotification:userInfo completionHandler:completionHandler];
}

- (void)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forLocalNotification:(UILocalNotification *)notification completionHandler:(void (^)(void))completionHandler
{
    if([_delegator application:application handleActionWithIdentifier:identifier forLocalNotification:notification completionHandler:completionHandler])
    {
        return;
    }
    [super application:application handleActionWithIdentifier:identifier forLocalNotification:notification completionHandler:completionHandler];
}

- (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings// IOS 8.0
{
    [_delegator application:application didRegisterUserNotificationSettings:notificationSettings];
    [super application:application didRegisterUserNotificationSettings:notificationSettings];
}

- (void)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void(^)(void))completionHandler
{
    [_delegator userNotificationCenter:center didReceiveNotificationResponse:response withCompletionHandler:completionHandler];
}

- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification// IOS 4.0
{
    if([_delegator application:application didReceiveLocalNotification:notification])
    {
        return;
    }
    [super application:application didReceiveLocalNotification:notification];
}

- (void)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem completionHandler:(void (^)(BOOL))completionHandler
{
    if([_delegator application:application performActionForShortcutItem:shortcutItem completionHandler:completionHandler])
    {
        return;
    }
    [super application:application performActionForShortcutItem:shortcutItem completionHandler:completionHandler];
}

#endif

@end
