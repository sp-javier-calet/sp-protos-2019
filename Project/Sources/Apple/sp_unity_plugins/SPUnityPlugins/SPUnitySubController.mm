//
//  SPUnitySubController.m
//  sp_unity_plugins
//
//  Created by Manuel Álvarez on 25/01/16.
//
//

#import "SPUnitySubController.h"
#import <Foundation/Foundation.h>

@implementation SPUnitySubController
{
}

BOOL _calledSuper;
BOOL _defaultBehavior = NO;

-(void)alwaysCallSuper
{
    _defaultBehavior = YES;
    _calledSuper =  YES;
}

-(BOOL)finish
{
    // Reset called super flag and return current value
    BOOL called = _calledSuper;
    _calledSuper = _defaultBehavior;
    return called;
}

#pragma mark - Life Cycle

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    _calledSuper = YES;
    return YES;
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    _calledSuper = YES;
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    _calledSuper = YES;
}

- (void)applicationDidEnterBackground:(UIApplication *)application
{
    _calledSuper = YES;
}

- (void)applicationWillEnterForeground:(UIApplication *)application
{
    _calledSuper = YES;
}

- (BOOL)applicationWillTerminate:(UIApplication*)application
{
    _calledSuper = YES;
    return YES;
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
{
    _calledSuper = YES;
    return YES;
}

- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler;
{
    _calledSuper = YES;
    return YES;
}

#pragma mark - Memory management

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    _calledSuper = YES;
}

#pragma mark - Notifications

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings
{
    _calledSuper = YES;
    return YES;
}
#endif

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken
{
    _calledSuper = YES;
}

- (BOOL)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    _calledSuper = YES;
    return YES;
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    _calledSuper = YES;
}

#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification *)notification
{
    _calledSuper = YES;
}
#endif

#pragma mark - Shortcut items

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem
{
    _calledSuper = YES;
    return YES;
}
#endif

@end
