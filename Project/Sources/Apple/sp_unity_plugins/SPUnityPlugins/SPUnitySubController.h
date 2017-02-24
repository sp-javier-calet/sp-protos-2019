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

// MANU TODO: ORDER THIS LIKE SPUNITYAPPLICATION AND IMPLEMENT MISSING METHODS

- (void)alwaysCallSuper;
- (BOOL)finish;

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions;
- (BOOL)application:(UIApplication*)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo;
- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken;
- (void)application:(UIApplication*)application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void(^)())completionHandler;
- (void)application:(UIApplication*)application handleActionWithIdentifier:(NSString*)identifier forRemoteNotification:(NSDictionary*)userInfo completionHandler:(void(^)())completionHandler;


#if !UNITY_TVOS
- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification *)notification;
- (void)application:(UIApplication *)application handleActionWithIdentifier:(NSString*)identifier forLocalNotification:(UILocalNotification*)notification completionHandler:(void (^)())completionHandler;
#endif

- (void)applicationDidEnterBackground:(UIApplication *)application;
- (void)applicationWillEnterForeground:(UIApplication *)application;
- (void)applicationDidBecomeActive:(UIApplication*)application;
- (void)applicationWillResignActive:(UIApplication*)application;
- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application;

@end

#endif /* __SPUnitySubController__ */
