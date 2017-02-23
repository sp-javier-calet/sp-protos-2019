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
#import "SPUnitySubController.h"

@interface SPUnityApplication : NSObject

+ (void)setupApplication:(const bool*)unityIsReady;
+ (BOOL)isReady;
+ (BOOL)notifyControllers:(void(^)(SPUnitySubController* controller))action;

+ (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions;
+ (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
+ (BOOL)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken;
+ (BOOL)application:(UIApplication*)application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void(^)())completionHandler;
+ (BOOL)application:(UIApplication*)application handleActionWithIdentifier:(NSString*)identifier forRemoteNotification:(NSDictionary*)userInfo completionHandler:(void(^)())completionHandler;

#if !UNITY_TVOS
+ (BOOL)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification *)notification;
+ (BOOL)application:(UIApplication *)application handleActionWithIdentifier:(NSString*)identifier forLocalNotification:(UILocalNotification*)notification completionHandler:(void (^)())completionHandler;
#endif

+ (BOOL)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo;
+ (BOOL)applicationDidEnterBackground:(UIApplication *)application;
+ (BOOL)applicationWillEnterForeground:(UIApplication *)application;
+ (BOOL)applicationDidBecomeActive:(UIApplication*)application;
+ (BOOL)applicationWillResignActive:(UIApplication*)application;
+ (BOOL)applicationDidReceiveMemoryWarning:(UIApplication*)application;
@end

#endif /* __SPUnityApplication__ */
