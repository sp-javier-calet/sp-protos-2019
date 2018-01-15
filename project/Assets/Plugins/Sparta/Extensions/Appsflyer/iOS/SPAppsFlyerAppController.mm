#import <SPUnityPlugins/SPAppControllerDelegate.h>

#import "AppsFlyerTracker.h"
#import "AppsFlyerDelegate.h"
#import "AppDelegateListener.h"

@interface SPAppsFlyerAppController : NSObject<SPAppControllerDelegate>
{
    BOOL _didEnterBackground;
}
@end

@implementation SPAppsFlyerAppController

- (instancetype)init
{
    NSLog(@"initializing AppsFlyerAppController");
    self = [super init];
    return self;
}

- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray *))restorationHandler {
    return [[AppsFlyerTracker sharedTracker] continueUserActivity:userActivity restorationHandler:restorationHandler];
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
    NSLog(@"got openURL: %@ %@", url, options);
    [[AppsFlyerTracker sharedTracker] handleOpenUrl:url options:options];
    return FALSE;
}

- (BOOL)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo {
    NSLog(@"got didReceiveRemoteNotification = %@", userInfo);

    //We don't want to enable Tracking App Launches from push notifications
    //[[AppsFlyerTracker sharedTracker] handlePushNotification:userInfo];
    return FALSE;
}

- (void)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    NSLog(@"got didFinishLaunching = %@", launchOptions);
    NSURL *url = launchOptions[@"url"];
    if (url != nil) {
        [self application:application openURL:url options:launchOptions];
    }
}

-(void)applicationDidBecomeActive:(UIApplication *)application {
    if (_didEnterBackground == TRUE) {
        NSLog(@"got didBecomeActive from background");

        //We don't want to Track App Launches every time we come from background, nor risk this being called before login
        //[[AppsFlyerTracker sharedTracker] trackAppLaunch];

        _didEnterBackground = FALSE;

    } else {
        NSLog(@"got didBecomeActive from start");
    }
}

-(void)applicationDidEnterBackground:(UIApplication *)application {
    _didEnterBackground = TRUE;
}

@end
