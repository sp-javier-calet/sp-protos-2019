//
//  SPUnityNotifications.m
//  sp_unity_plugins
//
//  Created by Ivan Hernández on 26/9/16.
//
//

#include "SPUnityNotifications.h"

#import <UIKit/UIKit.h>

EXPORT_API void SPUnityNotificationsScheduleLocalNotification(SPUnityNotificationDataStruct data)
{
    UILocalNotification* local = [[UILocalNotification alloc] init];

    if(local == nil)
    {
        return;
    }

    local.fireDate = [NSDate dateWithTimeIntervalSinceNow:data.fireDelay];

    local.timeZone = [NSTimeZone defaultTimeZone];
    local.alertBody = [NSString stringWithUTF8String:data.message];
    local.alertAction = [NSString stringWithUTF8String:data.title];
    local.applicationIconBadgeNumber = 1;

    UIApplication* application = [UIApplication sharedApplication];
    // look for notifications scheduled before this one
    // this is not perfect since a push notification can be pushed in the middle
    // and there seems to be no way to detect that
    for(UILocalNotification* scheduled in application.scheduledLocalNotifications)
    {
        if([scheduled.fireDate compare:local.fireDate] == NSOrderedAscending)
        {
            local.applicationIconBadgeNumber++;
        }
    }

    [application scheduleLocalNotification:local];
}

EXPORT_API void SPUnityNotificationsPresentLocalNotification(SPUnityNotificationDataStruct data)
{
    UILocalNotification* local = [[UILocalNotification alloc] init];

    if(local == nil)
    {
        return;
    }

    local.timeZone = [NSTimeZone defaultTimeZone];
    local.alertBody = [NSString stringWithUTF8String:data.message];
    local.alertAction = [NSString stringWithUTF8String:data.title];
    local.applicationIconBadgeNumber = 1;

    UIApplication* application = [UIApplication sharedApplication];
    [application presentLocalNotificationNow:local];
}

EXPORT_API void SPUnityNotificationsCancelAllLocalNotifications()
{
    UIApplication* application = [UIApplication sharedApplication];
    NSArray* notifications = [application scheduledLocalNotifications];

    for(UILocalNotification* n in notifications)
    {
        [application cancelLocalNotification:n];
    }

    application.applicationIconBadgeNumber = 0;
}

EXPORT_API void SPUnityNotificationsClearAllLocalNotifications()
{
    UIApplication* application = [UIApplication sharedApplication];

    [application cancelAllLocalNotifications];

    application.applicationIconBadgeNumber = 0;
}

EXPORT_API void SPUnityNotificationsRegisterForNotifications()
{
    UIApplication* application = [UIApplication sharedApplication];
    if([application respondsToSelector:@selector(registerUserNotificationSettings:)])
    {
#ifdef __IPHONE_8_0
        UIUserNotificationSettings* settings =
          [UIUserNotificationSettings settingsForTypes:(UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeSound | UIRemoteNotificationTypeAlert)
                                            categories:nil];
        [application registerUserNotificationSettings:settings];
#endif
    }
    else
    {
        UIRemoteNotificationType types = UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeAlert | UIRemoteNotificationTypeSound;
        [application registerForRemoteNotificationTypes:types];
    }
}
