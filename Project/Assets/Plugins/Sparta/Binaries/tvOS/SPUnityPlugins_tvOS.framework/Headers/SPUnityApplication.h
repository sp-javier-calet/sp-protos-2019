//
//  SPUnityApplication.h
//  sp_unity_plugins
//
//  Created by Manuel Álvarez on 25/01/16.
//
//

#ifndef __SPUnityApplication__
#define __SPUnityApplication__

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@class SPUnitySubController;

@interface SPUnityApplication : NSObject

+ (void)setupApplication:(const bool*)unityIsReady;
+ (BOOL)isReady;
+ (BOOL)notifyControllers:(void(^)(SPUnitySubController* controller))action;

#pragma mark - Life Cycle

+ (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions;// IOS 3.0
+ (BOOL)applicationWillResignActive:(UIApplication*)application;
+ (BOOL)applicationDidBecomeActive:(UIApplication*)application;
+ (BOOL)applicationDidEnterBackground:(UIApplication*)application;// IOS 4.0
+ (BOOL)applicationWillEnterForeground:(UIApplication*)application;// IOS 4.0
+ (BOOL)applicationWillTerminate:(UIApplication*)application;
#if !UNITY_TVOS
+ (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation;// IOS 4.2
#endif
+ (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler;// IOS 8.0

#pragma mark - Memory management

+ (BOOL)applicationDidReceiveMemoryWarning:(UIApplication*)application;

#pragma mark - Notifications

#if !UNITY_TVOS
+ (BOOL)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings;// IOS 8.0
#endif
+ (BOOL)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken;// IOS 3.0
+ (BOOL)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error;// IOS 3.0
+ (BOOL)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo;// IOS 3.0
#if !UNITY_TVOS
+ (BOOL)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification;// IOS 4.0
#endif

#pragma mark - Shortcut items

#if !UNITY_TVOS
+ (BOOL)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem completionHandler:(void (^)(BOOL))completionHandler;// IOS 9.0
#endif

@end

#endif /* __SPUnityApplication__ */
