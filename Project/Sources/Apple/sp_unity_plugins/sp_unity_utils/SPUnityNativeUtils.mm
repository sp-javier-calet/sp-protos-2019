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
}