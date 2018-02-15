#ifndef __SPUnityNativeUtils__
#define __SPUnityNativeUtils__

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

#include <string>
#import <Foundation/NSString.h>


#pragma mark - Notifications

bool userAllowsNotifications();
void onPermissionsGranted();
void onRegisterForRemote(const std::string& pushToken);
void onRegisterForRemoteFailed(const std::string& error);


#pragma mark - ForceTouch

struct ForceTouchShortcutItem
{
    const char* Type;
    const char* Title;
    const char* Subtitle;
    const char* IconPath;
};


#pragma mark - SystemVersion

class SPUnityNativeUtils
{
  public:
    static const std::string kV6;
    static const std::string kV7;
    static const std::string kV8;
    static const std::string kV9;

    static bool isSystemVersionEqualTo(const std::string& version);
    static bool isSystemVersionGreaterThan(const std::string& version);
    static bool isSystemVersionGreaterThanOrEqualTo(const std::string& version);
    static bool isSystemVersionLessThan(const std::string& version);
    static bool isSystemVersionLessThanOrEqualTo(const std::string& version);

    static char* createString(const char* str);
    static bool isNullOrEmpty(const char* str);
};


#pragma mark - AppSourceUtils

// Event names. The names are defined by the Status Enum in IosAppEvents
static const std::string kStatusUpdateSource = "UPDATEDSOURCE";
static const std::string kStatusActive = "ACTIVE";
static const std::string kStatusWillGoForeground = "WILLGOFOREGROUND";
static const std::string kStatusBackground = "BACKGROUND";
static const std::string kStatusWillGoBackground = "WILLGOBACKGROUND";
static const std::string kStatusMemoryWarning = "MEMORYWARNING";
static const std::string kNotifyMethod = "NotifyStatus";

NSString* const kEventTypeKey = @"event_type";

@interface AppSourceUtils : NSObject

+ (void)clearSource;
+ (void)storeSource:(NSString*)url;
+ (void)storeSourceOptions:(NSDictionary*)options withScheme:(NSString*)scheme;

@end

#pragma mark - AppEventsUtils

@interface AppEventsUtils : NSObject

+ (void)notifyStatus:(std::string)status;

@end


#pragma mark - SPUnityMethods

extern "C" {
EXPORT_API bool SPUnityNativeUtilsIsInstalled(const char* appId);
EXPORT_API bool SPUnityNativeUtilsUserAllowNotification();
EXPORT_API void SPUnitySetForceTouchShortcutItems(ForceTouchShortcutItem* shortcuts, int itemsCount);
EXPORT_API void SPUnityAppEvents_Init(const char* gameObjectName);
EXPORT_API void SPUnityAppEvents_Flush();
EXPORT_API char* SPUnityNotificationsDeviceToken();
EXPORT_API char* SPUnityNotificationsRegistrationError();
EXPORT_API bool SPUnityNativeUtilsSupportsReviewDialog();
EXPORT_API void SPUnityNativeUtilsDisplayReviewDialog();
EXPORT_API void SPUnityNativeUtilsClearDataAndKillApp();
}

#endif
