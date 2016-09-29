#ifndef __SPUnityNotifications__
#define __SPUnityNotifications__

// Which platform we are on?
#if _MSC_VER
#define UNITY_WIN 1
#else
#define UNITY_OSX 1
#endif

// Attribute to make function be exported from a plugin
#if UNITY_WIN
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API
#endif

extern "C" {
struct SPUnityNotificationDataStruct
{
    const char* message;
    const char* title;
    long fireDelay;
    int iconBadgeNumber;
};

EXPORT_API void SPUnityNotificationsScheduleLocalNotification(SPUnityNotificationDataStruct data);
EXPORT_API void SPUnityNotificationsPresentLocalNotification(SPUnityNotificationDataStruct data);
EXPORT_API void SPUnityNotificationsCancelAllLocalNotifications();
EXPORT_API void SPUnityNotificationsClearAllLocalNotifications();
EXPORT_API void SPUnityNotificationsRegisterForNotifications();
}


#endif
