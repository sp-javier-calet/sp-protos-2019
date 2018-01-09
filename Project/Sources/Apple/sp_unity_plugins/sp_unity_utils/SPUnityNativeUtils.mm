#include "SPUnityNativeUtils.h"
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>

#include <queue>
#include "UnityGameObject.h"


#pragma mark - Notifications

static std::string _deviceToken = "";
static std::string _registrationError = "";

bool userAllowsNotifications()
{
#if UNITY_TVOS
    return false;
#else
    if([[UIApplication sharedApplication] respondsToSelector:@selector(currentUserNotificationSettings)]) // ios8 >=
    {
        UIUserNotificationType notificationSelection = [[[UIApplication sharedApplication] currentUserNotificationSettings] types];

        return notificationSelection & UIUserNotificationTypeAlert;
    }
    else
    {
        UIRemoteNotificationType notificationSelection = [[UIApplication sharedApplication] enabledRemoteNotificationTypes];

        return notificationSelection & UIRemoteNotificationTypeAlert;
    }
#endif
}

void onPermissionsGranted()
{
    UIApplication* application = [UIApplication sharedApplication];
    if([application respondsToSelector:@selector(registerForRemoteNotifications)]) // ios8 >=
    {
        [application registerForRemoteNotifications];
    }
}

void onRegisterForRemote(const std::string& pushToken)
{
    _deviceToken = pushToken;
}

void onRegisterForRemoteFailed(const std::string& error)
{
    _registrationError = error;
}


#pragma mark - SystemVersion

const std::string SPUnityNativeUtils::kV6 = "6.0";
const std::string SPUnityNativeUtils::kV7 = "7.0";
const std::string SPUnityNativeUtils::kV8 = "8.0";
const std::string SPUnityNativeUtils::kV9 = "9.0";


bool SPUnityNativeUtils::isSystemVersionEqualTo(const std::string& version)
{
    return
      [[[UIDevice currentDevice] systemVersion] compare:[NSString stringWithUTF8String:version.c_str()] options:NSNumericSearch] == NSOrderedSame;
}

bool SPUnityNativeUtils::isSystemVersionGreaterThan(const std::string& version)
{
    return [[[UIDevice currentDevice] systemVersion] compare:[NSString stringWithUTF8String:version.c_str()] options:NSNumericSearch]
           == NSOrderedDescending;
}

bool SPUnityNativeUtils::isSystemVersionGreaterThanOrEqualTo(const std::string& version)
{
    return [[[UIDevice currentDevice] systemVersion] compare:[NSString stringWithUTF8String:version.c_str()] options:NSNumericSearch]
           != NSOrderedAscending;
}

bool SPUnityNativeUtils::isSystemVersionLessThan(const std::string& version)
{
    return [[[UIDevice currentDevice] systemVersion] compare:[NSString stringWithUTF8String:version.c_str()] options:NSNumericSearch]
           == NSOrderedAscending;
}

bool SPUnityNativeUtils::isSystemVersionLessThanOrEqualTo(const std::string& version)
{
    return [[[UIDevice currentDevice] systemVersion] compare:[NSString stringWithUTF8String:version.c_str()] options:NSNumericSearch]
           != NSOrderedDescending;
}

char* SPUnityNativeUtils::createString(const char* str)
{
    char* nstr = (char*)malloc(sizeof(char) * (strlen(str) + 1));
    strcpy(nstr, str);
    return nstr;
}


bool SPUnityNativeUtils::isNullOrEmpty(const char* str)
{
    return (str == nullptr || strlen(str) < 1);
}


#pragma mark - AppSourceUtils

@implementation AppSourceUtils

NSString* const kAppSourceKey = @"SourceApplicationKey";

+ (NSString*)urlEncode:(id)object
{
    NSString* string = [NSString stringWithFormat:@"%@", object];
    return [string stringByAddingPercentEscapesUsingEncoding:NSUTF8StringEncoding];
}

+ (void)clearSource
{
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
    [defaults removeObjectForKey:kAppSourceKey];
    [defaults synchronize];
}

+ (void)storeSource:(NSString*)url
{
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
    [defaults setObject:url forKey:kAppSourceKey];
    [defaults synchronize];
}

+ (void)storeSourceOptions:(NSDictionary*)options withScheme:(NSString*)scheme
{
    if(options || scheme)
    {
        NSMutableArray* parts = [NSMutableArray array];
        for(id key in options)
        {
            id value = [options objectForKey:key];
            NSString* part = [NSString stringWithFormat:@"%@=%@", [self urlEncode:key], [self urlEncode:value]];
            [parts addObject:part];
        }

        NSString* url;
        if(scheme != nil)
        {
            url = [NSString stringWithFormat:@"%@%@%@", scheme, @"://?", [parts componentsJoinedByString:@"&"]];
        }
        else
        {
            url = [parts componentsJoinedByString:@"&"];
        }

        [self storeSource:url];
    }
    else
    {
        [self clearSource];
    }
}

@end


#pragma mark - AppEventsUtils

@implementation AppEventsUtils

// AppReady flag defined in UnityAppController
extern bool _unityAppReady;

static std::string _gameObjectName = "";

// Queue of events waiting to be notified
std::queue<std::string> _pendingEvents;


+ (void)notifyStatus:(std::string)status
{
    // Add new status and flush all
    _pendingEvents.push(status);
    [self flush];
}

+ (void)flush
{
    if(_unityAppReady && !_gameObjectName.empty())
    {
        UnityGameObject go(_gameObjectName);
        while(!_pendingEvents.empty())
        {
            go.SendMessage(kNotifyMethod, _pendingEvents.front());
            _pendingEvents.pop();
        }
    }
}

+ (void)setGameObjectName:(const char*)name
{
    _gameObjectName = name;
}

@end


#pragma mark - SPUnityMethods

EXPORT_API bool SPUnityNativeUtilsIsInstalled(const char* appId)
{
    NSURL* url = [NSURL URLWithString:[NSString stringWithCString:appId encoding:NSUTF8StringEncoding]];
    return [[UIApplication sharedApplication] canOpenURL:url];
}

EXPORT_API bool SPUnityNativeUtilsUserAllowNotification()
{
    return userAllowsNotifications();
}

#if !UNITY_TVOS
EXPORT_API void SPUnitySetForceTouchShortcutItems(ForceTouchShortcutItem* shortcuts, int itemsCount)
{
    if(!SPUnityNativeUtils::isSystemVersionGreaterThanOrEqualTo(SPUnityNativeUtils::kV9))
    {
        return;
    }

    NSMutableArray<UIApplicationShortcutItem*>* items = [NSMutableArray arrayWithCapacity:itemsCount];

    for(int i = 0; i < itemsCount; ++i)
    {
        ForceTouchShortcutItem& shortcut = shortcuts[i];

        NSString* type = [NSString stringWithUTF8String:shortcut.Type];
        NSString* title = [NSString stringWithUTF8String:shortcut.Title];
        NSString* subtitle = SPUnityNativeUtils::isNullOrEmpty(shortcut.Subtitle) ? nil : [NSString stringWithUTF8String:shortcut.Subtitle];
        UIApplicationShortcutIcon* icon = SPUnityNativeUtils::isNullOrEmpty(shortcut.IconPath)
                                            ? nil
                                            : [UIApplicationShortcutIcon iconWithTemplateImageName:[NSString stringWithUTF8String:shortcut.IconPath]];
        UIApplicationShortcutItem* item =
          [[UIApplicationShortcutItem alloc] initWithType:type localizedTitle:title localizedSubtitle:subtitle icon:icon userInfo:nil];

        [items addObject:item];
    }

    [[UIApplication sharedApplication] setShortcutItems:items];
}
#endif

EXPORT_API void SPUnityAppEvents_Init(const char* gameObjectName)
{
    [AppEventsUtils setGameObjectName:gameObjectName];
}

EXPORT_API void SPUnityAppEvents_Flush()
{
    [AppEventsUtils flush];
}

EXPORT_API char* SPUnityNotificationsDeviceToken()
{
    return SPUnityNativeUtils::createString(_deviceToken.c_str());
}

EXPORT_API char* SPUnityNotificationsRegistrationError()
{
    return SPUnityNativeUtils::createString(_registrationError.c_str());
}

EXPORT_API bool SPUnityNativeUtilsSupportsReviewDialog()
{
#if !UNITY_TVOS && __IPHONE_OS_VERSION_MAX_ALLOWED >= 103000
    if(NSStringFromClass([SKStoreReviewController class]) != nil)
    {
        return true;
    }
#endif
    return false;
}

EXPORT_API void SPUnityNativeUtilsDisplayReviewDialog()
{
#if !UNITY_TVOS && __IPHONE_OS_VERSION_MAX_ALLOWED >= 103000
    if(NSStringFromClass([SKStoreReviewController class]) != nil)
    {
        [SKStoreReviewController requestReview];
    }
#endif
}

void clearUserDefaults()
{
    NSString* appDomain = [[NSBundle mainBundle] bundleIdentifier];
    [[NSUserDefaults standardUserDefaults] removePersistentDomainForName:appDomain];
    [[NSUserDefaults standardUserDefaults] synchronize];
}

EXPORT_API void SPUnityNativeUtilsClearDataAndKillApp()
{
#if !UNITY_TVOS
    clearUserDefaults();
    exit(0);
#endif
}
