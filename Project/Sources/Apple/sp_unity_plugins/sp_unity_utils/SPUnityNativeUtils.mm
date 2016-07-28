#include "SPUnityNativeUtils.h"
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

EXPORT_API bool SPUnityNativeUtilsIsInstalled(const char* appId)
{
    NSURL* url = [NSURL URLWithString:[NSString stringWithCString:appId encoding:NSUTF8StringEncoding]];
    return [[UIApplication sharedApplication] canOpenURL:url];
}

EXPORT_API bool SPUnityNativeUtilsUserAllowNotification()
{
#if UNITY_TVOS
    return false;
#else
    if([[UIApplication sharedApplication] respondsToSelector:@selector(currentUserNotificationSettings)])
    {
        // Ios 8
        UIUserNotificationType notificationSelection = [[[UIApplication sharedApplication] currentUserNotificationSettings] types];

        return notificationSelection & UIRemoteNotificationTypeAlert;
    }
    else
    {
        UIRemoteNotificationType notificationSelection = [[UIApplication sharedApplication] enabledRemoteNotificationTypes];

        return notificationSelection & UIRemoteNotificationTypeAlert;
    }
#endif
}

const std::string SPUnityNativeUtils::kV6 = "6.0";
const std::string SPUnityNativeUtils::kV7 = "7.0";
const std::string SPUnityNativeUtils::kV8 = "8.0";
const std::string SPUnityNativeUtils::kV9 = "9.0";


bool SPUnityNativeUtils::isSystemVersionEqualTo(const std::string& version)
{
    return [[[UIDevice currentDevice] systemVersion] compare:[NSString stringWithUTF8String:version.c_str()] options:NSNumericSearch]
           == NSOrderedSame;
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