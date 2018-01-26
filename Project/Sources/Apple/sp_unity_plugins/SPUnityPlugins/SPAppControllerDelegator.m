#import "SPAppControllerDelegator.h"
#import "SPAppControllerDelegate.h"
#import <objc/message.h>


@implementation SPAppControllerDelegator
{
    NSMutableArray* _delegates;
}

- (id)init
{
    self = [super init];
    if(self != nil)
    {
        self.enabled = TRUE;
        _delegates = [[NSMutableArray alloc] init];
    }
    return self;
}

- (void)addAllDelegates
{
    Protocol *protocol = @protocol(SPAppControllerDelegate);
    int numClasses = objc_getClassList(NULL, 0);
    if(numClasses > 0)
    {
        Class *classes = NULL;
        classes = (__unsafe_unretained Class *)malloc(sizeof(Class) * numClasses);
        numClasses = objc_getClassList(classes, numClasses);
        for (int idx = 0; idx < numClasses; idx++)
        {
            Class class = classes[idx];
            if (class_getClassMethod(class, @selector(conformsToProtocol:)) && [class conformsToProtocol:protocol])
            {
                NSLog(@"registering SPAppControllerDelegate %@", NSStringFromClass(class));
                id delegate = [[class alloc] init];
                [self addDelegate:delegate];
            }
        }
        free(classes);
    }
}

- (BOOL)addDelegate:(id<SPAppControllerDelegate>)delegate
{
    if([_delegates containsObject:delegate])
    {
        return FALSE;
    }
    [_delegates addObject:delegate];
    return TRUE;
}

- (BOOL)removeDelegate:(id<SPAppControllerDelegate>)delegate
{
    if(![_delegates containsObject:delegate])
    {
        return FALSE;
    }
    [_delegates removeObject:delegate];
    return TRUE;
}

#pragma mark - Life Cycle

- (void)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:didFinishLaunchingWithOptions:)])
            {
                [delegate application:application didFinishLaunchingWithOptions:launchOptions];
            }
        }
    }
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(applicationWillResignActive:)])
            {
                [delegate applicationWillResignActive:application];
            }
        }
    }
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(applicationDidBecomeActive:)])
            {
                [delegate applicationDidBecomeActive:application];
            }
        }
    }
}

- (void)applicationDidEnterBackground:(UIApplication*)application
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(applicationDidEnterBackground:)])
            {
                [delegate applicationDidEnterBackground:application];
            }
        }
    }
}

- (void)applicationWillEnterForeground:(UIApplication*)application
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(applicationWillEnterForeground:)])
            {
                [delegate applicationWillEnterForeground:application];
            }
        }
    }
}

- (void)applicationWillTerminate:(UIApplication*)application
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(applicationWillTerminate:)])
            {
                [delegate applicationWillTerminate:application];
            }
        }
    }
}

- (BOOL)application:(UIApplication*)application continueUserActivity:(NSUserActivity*)userActivity restorationHandler:(void (^)(NSArray* restorableObjects))restorationHandler
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:continueUserActivity:restorationHandler:)])
            {
                if([delegate application:application continueUserActivity:userActivity restorationHandler:restorationHandler])
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}


- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey, id> *)options
{
    BOOL handled = FALSE;
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:openURL:options:)])
            {
                if([delegate application:application openURL:url options:options])
                {
                    handled = TRUE;
                }
            }
        }
    }
    return handled;
}

- (BOOL)application:(UIApplication *)application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void (^)(void))completionHandler
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:handleEventsForBackgroundURLSession:completionHandler:)])
            {
                if([delegate application:application handleEventsForBackgroundURLSession:identifier completionHandler:completionHandler])
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}

#pragma mark - Memory management

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(applicationDidReceiveMemoryWarning:)])
            {
                [delegate applicationDidReceiveMemoryWarning:application];
            }
        }
    }
}

#pragma mark - Notifications

#if !TARGET_OS_TV
- (void)application:(UIApplication*)application didRegisterUserNotificationSettings:(UIUserNotificationSettings*)notificationSettings
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:didRegisterUserNotificationSettings:)])
            {
                [delegate application:application didRegisterUserNotificationSettings:notificationSettings];
            }
        }
    }
}
#endif

- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:didRegisterForRemoteNotificationsWithDeviceToken:)])
            {
                [delegate application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
            }
        }
    }
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:didFailToRegisterForRemoteNotificationsWithError:)])
            {
                [delegate application:application didFailToRegisterForRemoteNotificationsWithError:error];
            }
        }
    }
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:didReceiveRemoteNotification:)])
            {
                [delegate application:application didReceiveRemoteNotification:userInfo];
            }
        }
    }
}

- (BOOL)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions options))completionHandler
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(userNotificationCenter:willPresentNotification:withCompletionHandler:)])
            {
                if([delegate userNotificationCenter:center willPresentNotification:notification withCompletionHandler:completionHandler])
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}

#if !TARGET_OS_TV

- (BOOL)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forRemoteNotification:(NSDictionary *)userInfo completionHandler:(void (^)(void))completionHandler
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:handleActionWithIdentifier:forRemoteNotification:completionHandler:)])
            {
                if([delegate application:application handleActionWithIdentifier:identifier forRemoteNotification:userInfo completionHandler:completionHandler])
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}

- (BOOL)application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forLocalNotification:(UILocalNotification *)notification completionHandler:(void (^)(void))completionHandler
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:handleActionWithIdentifier:forLocalNotification:completionHandler:)])
            {
                if([delegate application:application handleActionWithIdentifier:identifier forLocalNotification:notification completionHandler:completionHandler])
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}

- (BOOL)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void(^)(void))completionHandler
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:)])
            {
                if([delegate userNotificationCenter:center didReceiveNotificationResponse:response withCompletionHandler:completionHandler])
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}

- (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification*)notification
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:didReceiveLocalNotification:)])
            {
                [delegate application:application didReceiveLocalNotification:notification];
            }
        }
    }
}
#endif

#pragma mark - Shortcut items

#if !TARGET_OS_TV
- (BOOL)application:(UIApplication*)application performActionForShortcutItem:(UIApplicationShortcutItem*)shortcutItem completionHandler:(void (^)(BOOL))completionHandler
{
    if(self.enabled)
    {
        for(id<SPAppControllerDelegate> delegate in _delegates)
        {
            if([delegate respondsToSelector:@selector(application:performActionForShortcutItem:completionHandler:)])
            {
                if([delegate application:application performActionForShortcutItem:shortcutItem completionHandler:completionHandler])
                {
                    return TRUE;
                }
            }
        }
    }
    return FALSE;
}
#endif

@end

