#ifndef __sparta__SPAppControllerDelegate__
#define __sparta__SPAppControllerDelegate__

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <UserNotifications/UserNotifications.h>

@protocol SPAppControllerDelegate <NSObject>

@optional

#pragma mark - Life Cycle

- (void)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions;// IOS 3.0
- (void)applicationWillResignActive:(UIApplication*)application;
- (void)applicationDidBecomeActive:(UIApplication*)application;
- (void)applicationDidEnterBackground:(UIApplication*)application;// IOS 4.0
- (void)applicationWillEnterForeground:(UIApplication*)application;// IOS 4.0
- (void)applicationWillTerminate:(UIApplication*)application;

// return TRUE if delegate will handle user activity and optionally call restorationHandler
// do not call restorationHandler if you return FALSE
// when one delegate returns TRUE, other delegates and app controller should not be called, only one delegate can take care of the activity
- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler;// IOS 8.0

// return TRUE if delegate handled the URL
// in any case all delegates and app controller will be called, app controller should return TRUE if one or more of them return TRUE
- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey, id> *)options; // IOS 9.0

// this is maintained for compatibility reasons
// if delegate does not implement this, we should try to call application:openURL:options:
// return TRUE if delegate handled the URL
// in any case all delegates and app controller will be called, app controller should return TRUE if one or more of them return TRUE
- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation; // deprecated in IOS 9.0

// return TRUE if delegate will handle URL session and always call completionHandler
// do not call completionHandler if you return FALSE
// when one delegate returns TRUE, other delegates and app controller should not be called, only one delegate can take care of the URL session
// delegates should use unique identifiers to avoid collisions
- (BOOL)application:(UIApplication *)application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void (^)(void))completionHandler; // IOS 7.0

#pragma mark - Memory management

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application;

#pragma mark - Notifications

// return TRUE if delegate will handle notification and always call completionHandler
// do not call completionHandler if you return FALSE
// when one delegate returns TRUE, other delegates and app controller should not be called, only one delegate can take care of the notification
- (BOOL)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions options))completionHandler; // IOS 10.0
- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken;// IOS 3.0
- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error;// IOS 3.0
- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo;// IOS 3.0

#if !TARGET_OS_TV
// return TRUE if delegate will handle notification and always call completionHandler
// do not call completionHandler if you return FALSE
// when one delegate returns TRUE, other delegates and app controller should not be called, only one delegate can take care of the notification
- (BOOL)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forRemoteNotification:(NSDictionary *)userInfo completionHandler:(void (^)(void))completionHandler; // IOS 8.0

// return TRUE if delegate will handle notification and always call completionHandler
// do not call completionHandler if you return FALSE
// when one delegate returns TRUE, other delegates and app controller should not be called, only one delegate can take care of the notification
- (BOOL)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forLocalNotification:(UILocalNotification *)notification completionHandler:(void (^)(void))completionHandler; // IOS 8.0

// return TRUE if delegate will handle notification and always call completionHandler
// do not call completionHandler if you return FALSE
// when one delegate returns TRUE, other delegates and app controller should not be called, only one delegate can take care of the notification
- (BOOL)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void(^)(void))completionHandler; // IOS 10.0

- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification;// IOS 4.0
- (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings;// IOS 8.0
#endif

#pragma mark - Shortcut items

#if !TARGET_OS_TV
// return TRUE if delegate will handle shortcut and always call completionHandler with parameter TRUE if successful
// do not call completionHandler if you return FALSE
// when one delegate returns TRUE, other delegates and app controller should not be called, only one delegate can take care of the shortcut
- (BOOL)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem completionHandler:(void (^)(BOOL))completionHandler;// IOS 9.0
#endif

@end

#endif
