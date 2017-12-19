//
//  SPUnitySubController.h
//  sp_unity_plugins
//
//  Created by Manuel Álvarez on 21/01/16.
//
//

#ifndef __SPUnitySubController__
#define __SPUnitySubController__

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface SPUnitySubController : NSObject

#pragma mark - Controller setup

- (void)alwaysCallSuper;
- (BOOL)finish;

#pragma mark - Life Cycle

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions;
- (void)applicationWillResignActive:(UIApplication*)application;
- (void)applicationDidBecomeActive:(UIApplication*)application;
- (void)applicationDidEnterBackground:(UIApplication *)application;
- (void)applicationWillEnterForeground:(UIApplication *)application;
- (BOOL)applicationWillTerminate:(UIApplication *)application;
#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
#endif
- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler;

#pragma mark - Memory management

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application;

#pragma mark - Notifications

#if !UNITY_TVOS
- (BOOL)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings;
#endif
- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken;
- (BOOL)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error;
- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo;
#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification *)notification;
#endif

#pragma mark - Shortcut items

#if !UNITY_TVOS
- (void)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem completionHandler:(void (^)(BOOL))completionHandler;
#endif

@end

#endif /* __SPUnitySubController__ */
