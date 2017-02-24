//
//  SPUnityApplication.mm
//  sp_unity_plugins
//
//  Created by Manuel Álvarez on 25/01/16.
//
//

#import "SPUnityApplication.h"
#import "SPUnitySubController.h"
#import <objc/runtime.h>
#include <vector>

// MANU TODO: IMPLEMENT MISSING METHODS

@implementation SPUnityApplication
{
}

const static bool kDefaultUnityIsReady = false;
const bool* _unityIsReady = &kDefaultUnityIsReady;

/* Use a pointer instead of static memory, since c++ static initializers 
 * could be called after obj-c classes initialization, where this is used. */
static std::vector<SPUnitySubController*>* _controllers = nullptr;

+(void)initialize
{
    _controllers = new std::vector<SPUnitySubController*>();
    
    // Check for SPUnitySubController subclasses and register the existing ones
    
    // MANU TODO: IMPLEMENT SPUnityAppEvents WITH THE CURRENT CODE
    
    NSArray* extensions = @[ @"SPUnityAppEvents",
                             @"HsUnityAppController"];
    
//    for(NSString* className in extensions)
//    {
//        id objClass = NSClassFromString(className);
//        if(objClass != nil)
//        {
//            [objClass superclass];
//        }
//    }
    id parentClass = objc_getClass("SPUnitySubController");
    
    for(NSString* className in extensions)
    {
        id currClass = objc_getClass([className UTF8String]);
        Class superClass = currClass;
        do
        {
            superClass = class_getSuperclass(superClass);
        } while(superClass && superClass != parentClass);
        
        if (superClass != nil)
        {
            id controller = [[currClass alloc] init];
            _controllers->push_back(controller);
        }
    }
}

+(void)setupApplication:(const bool*)unityIsReady
{
    _unityIsReady = unityIsReady;
}

+(BOOL)isReady
{
    assert(_unityIsReady != &kDefaultUnityIsReady && "Uninitialized Unity Application. Is 'setupApplication' called?");
    return *_unityIsReady;
}

+(BOOL)notifyControllers:(void(^)(SPUnitySubController* controller))action
{
    BOOL callSuper = YES;
    assert(_controllers && "Uninitialized Unity Application");
    for(auto controller : *_controllers)
    {
        action(controller);
        callSuper &= [controller finish];
    }
    return callSuper;
}

#pragma mark - Life Cycle

+ (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller application:application didFinishLaunchingWithOptions:launchOptions];
    }];
}

+ (BOOL)applicationWillResignActive:(UIApplication*)application
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller applicationWillResignActive:application];
    }];
}

+ (BOOL)applicationDidBecomeActive:(UIApplication*)application
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller applicationDidBecomeActive:application];
    }];
}

+ (BOOL)applicationDidEnterBackground:(UIApplication*)application
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller applicationDidEnterBackground:application];
    }];
}

+ (BOOL)applicationWillEnterForeground:(UIApplication*)application
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller applicationWillEnterForeground:application];
    }];
}

+ (void)applicationWillTerminate:(UIApplication*)application
{
    
}

#if !UNITY_TVOS
+ (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    }];
}
#endif

// MANU TODO: IMPLEMENT THIS METHOD NEEDED BY APPSFLYER

//+ (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler
//{
//    
//}

#pragma mark - Memory management

+ (BOOL)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller applicationDidReceiveMemoryWarning:application];
    }];
}

#pragma mark - Notifications

#if !UNITY_TVOS
+ (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings
{
    
}
#endif

+ (BOOL)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
    }];
}

+ (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    
}

+ (BOOL)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller application:application didReceiveRemoteNotification:userInfo];
    }];
}

#if !UNITY_TVOS
+ (BOOL)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification
{
    return [self notifyControllers:^(SPUnitySubController* controller){
        [controller application:application didReceiveLocalNotification:notification];
    }];
}
#endif

#pragma mark - Shortcut items

#if !UNITY_TVOS
+ (void)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem
{
    
}
#endif

@end

