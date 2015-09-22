#import "SPUnityAppControllerSubClass.h"
#include <string>
#import <UIKit/UIKit.h>
#import <AdSupport/AdSupport.h>
#include <queue>

@implementation SPUnityAppControllerSubClass
{
    std::string _gameObjectName;
}

// AppReady flag defined in UnityAppController
extern bool _unityAppReady;

NSString* const kAppSourceKey = @"SourceApplicationKey";

// Event names. The names are defined by the Status Enum in IosAppEvents
static const std::string kStatusUpdateSource = "UPDATEDSOURCE";
static const std::string kStatusActive = "ACTIVE";
static const std::string kStatusWillGoForeground = "WILLGOFOREGROUND";
static const std::string kStatusBackground = "BACKGROUND";
static const std::string kStatusWillGoBackground = "WILLGOBACKGROUND";
static const std::string kStatusMemoryWarning = "MEMORYWARNING";
static const std::string kNotifyMethod = "NotifyStatus";

// Queue of events waiting to be notified
std::queue<std::string> _pendingEvents;

+(void)load
{
    extern const char* AppControllerClassName;
    AppControllerClassName = "SPUnityAppControllerSubClass";
}

- (void) notifyStatus:( std::string ) status
{
    // Add new status and flush all
    _pendingEvents.push(status);
    [self flush];
}

- (void)flush
{
    if(_unityAppReady && !_gameObjectName.empty())
    {
        while(!_pendingEvents.empty())
        {
            const std::string& status = _pendingEvents.front();
            _pendingEvents.pop();

            UnitySendMessage(_gameObjectName.c_str(), kNotifyMethod.c_str(), status.c_str());
        }
    }
}

-(NSString*) urlEncode:(id) object
{
    NSString *string = [NSString stringWithFormat: @"%@", object];
    return [string stringByAddingPercentEscapesUsingEncoding: NSUTF8StringEncoding];
}

-(void) clearSource
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    [defaults removeObjectForKey:kAppSourceKey];
    [defaults synchronize];
}

-(void) storeSource:(NSString*) url
{
    NSUserDefaults *defaults = [NSUserDefaults standardUserDefaults];
    [defaults setObject:url forKey:kAppSourceKey];
    [defaults synchronize];
}

-(void) storeSourceOptions:( NSDictionary* ) options withScheme:(NSString*)scheme
{
    if(options || scheme)
    {
        NSMutableArray *parts = [NSMutableArray array];
        for (id key in options) {
            id value = [options objectForKey: key];
            NSString *part = [NSString stringWithFormat: @"%@=%@",
                              [self urlEncode:key],
                              [self urlEncode:value]];
            [parts addObject: part];
        }

        NSString* url;
        if(scheme != nil)
        {
            url = [NSString stringWithFormat:@"%@%@%@", scheme, @"://", [parts componentsJoinedByString: @"&"]];
        } else {
            url = [parts componentsJoinedByString: @"&"];
        }

        [self storeSource:url];
    }
    else
    {
        [self clearSource];
    }
}

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    [super application:application didFinishLaunchingWithOptions:launchOptions];

    [self clearSource];

    if (&UIApplicationLaunchOptionsLocalNotificationKey != nil)
    {
        UILocalNotification *notification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];
        if (notification)
        {
            /* Notice that all event processing must be synchronous,
             * since the Source could change if there are any other  notification event */
            [self storeSourceOptions:notification.userInfo withScheme:@"local"];
        }
    }

    [self notifyStatus:kStatusUpdateSource];

    return YES;
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
{
    [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    [self storeSource:url.absoluteString];
    [self notifyStatus:kStatusUpdateSource];
    return YES;
}

-  (void)application:(UIApplication*)application didReceiveLocalNotification:(UILocalNotification *)notification
{
    [super application:application didReceiveLocalNotification:notification];
    [self storeSourceOptions:notification.userInfo withScheme:@"local"];
    [self notifyStatus:kStatusUpdateSource];
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    [super application:application didReceiveRemoteNotification:userInfo];
    [self storeSourceOptions:userInfo withScheme:@"push"];
    [self notifyStatus:kStatusUpdateSource];
}

- (void)applicationDidEnterBackground:(UIApplication *)application
{
    [super applicationDidEnterBackground:application];
    [self notifyStatus:kStatusBackground];
}

- (void)applicationWillEnterForeground:(UIApplication *)application
{
    [super applicationWillEnterForeground:application];
    // applicationWillEnterForeground: might sometimes arrive *before* actually initing unity (e.g. locking on startup)
    [self notifyStatus:kStatusWillGoForeground];
}

- (void)applicationDidBecomeActive:(UIApplication*)application
{
    [super applicationDidBecomeActive:application];
    [self notifyStatus:kStatusActive];
}

- (void)applicationWillResignActive:(UIApplication*)application
{
    [self notifyStatus:kStatusWillGoBackground];

    //aditional game loop to allow scripts response before being paused
    if(_unityAppReady)
    {
        UnityPlayerLoop();
    }
    [super applicationWillResignActive:application];
}

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    [super applicationDidReceiveMemoryWarning:application];
    [self notifyStatus:kStatusMemoryWarning];
}

- (void)setGameObjectName:(const char *)name
{
    _gameObjectName = name;
}


@end

extern "C" {
    void SPUnityAppEvents_Init(const char* gameObjectName)
    {
        SPUnityAppControllerSubClass* delegate = [[UIApplication sharedApplication] delegate];
        [delegate setGameObjectName:gameObjectName];
    }

    void SPUnityAppEvents_Flush()
    {
        SPUnityAppControllerSubClass* delegate = [[UIApplication sharedApplication] delegate];
        [delegate flush];
    }
}
