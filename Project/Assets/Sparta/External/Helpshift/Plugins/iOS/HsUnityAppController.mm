//
//  HsUnityAppController.m
//
#import <SPUnityPlugins/SPUnitySubController.h>

#import <Foundation/Foundation.h>
#import "HelpshiftCore.h"
#import "HelpshiftSupport.h"
#import "UnityAppController.h"
#import "Helpshift.h"
#import "HelpshiftAll.h"

@interface HelpshiftUnityInstaller : NSObject

+ (BOOL) installDefault;

@end


@implementation HelpshiftUnityInstaller

+ (BOOL) validateInstallConfig:(NSString *)configuration {
    return (configuration != nil && [configuration isKindOfClass:[NSString class]] && configuration.length > 0 &&
            [configuration stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]].length > 0);
}

+ (NSDictionary*) readDefaultConfig {
    NSString *filePath = [[NSBundle mainBundle] pathForResource:@"HelpshiftInstallConfig" ofType:@"json"];

    if ([[NSFileManager defaultManager] fileExistsAtPath:filePath]) {
        NSString *myJSON = [[NSString alloc] initWithContentsOfFile:filePath encoding:NSUTF8StringEncoding error:NULL];
        NSError *error =  nil;
        NSMutableDictionary *installConfig = [[NSJSONSerialization JSONObjectWithData:[myJSON dataUsingEncoding:NSUTF8StringEncoding] options:NSJSONReadingAllowFragments error:&error] mutableCopy];

        if (installConfig == nil) {
            if (error != nil) {
                NSLog(@"Error occurred in loading install config %@", error.localizedDescription);
            }
            return nil;
        }

        NSString *apiKey = [installConfig objectForKey:@"__hs__apiKey"];
        NSString *appId = [installConfig objectForKey:@"__hs__appId"];
        NSString *domainName = [installConfig objectForKey:@"__hs__domainName"];

        if (![self validateInstallConfig:apiKey] ||
            ![self validateInstallConfig:appId] ||
            ![self validateInstallConfig:domainName]) {
            return nil;
        }
        return installConfig;
    }

    return nil;
}

+ (NSMutableDictionary*) dictionaryByRemovingNullValues:(NSDictionary*)inDict {
    NSMutableDictionary *output = [[NSMutableDictionary alloc] init];
    if (inDict) {
        for (NSString *key in inDict) {
            id value = [inDict objectForKey:key];
            if (value && ![value isKindOfClass:[NSNull class]]) {
                [output setObject:value forKey:key];
            }
        }
    }
    return output;
}

+ (void) initializeHelpshiftWithDefaultConfig:(NSDictionary*)config {
    NSMutableDictionary *configDictionary = [self dictionaryByRemovingNullValues:config];
    NSString *apiKey = [config objectForKey:@"__hs__apiKey"];
    NSString *appId = [config objectForKey:@"__hs__appId"];
    NSString *domainName = [config objectForKey:@"__hs__domainName"];

    [configDictionary removeObjectsForKeys:@[@"__hs__apiKey", @"__hs__appId", @"__hs__domainName"]];

    [Helpshift installForApiKey:apiKey domainName:domainName appID:appId withOptions:configDictionary];
}

+ (BOOL) installDefault {
    NSDictionary *installConfig = [self readDefaultConfig];
    if (installConfig != nil) {
        [self initializeHelpshiftWithDefaultConfig:installConfig];
        return YES;
    }
    return NO;
}

@end



@interface HsUnityAppController : SPUnitySubController

@end

@implementation HsUnityAppController : SPUnitySubController

- (BOOL) application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {

    /*
     Starting Unity 2.3.0 release onwards, the Helpshift install call through scripts is deprecated for both
     default configuration as well as custom configuration. It is mandatory to initialize Helpshift in Objective-C
     files only.

     1) If you already follow the initialization process for Helpshift through Objective-C, you can continue
     with your implementation for Helpshift initialization.
     2) If you initialize Helpshift through C# scripts using default configuration, please leave the initialization with
     default config below unchanged.
     3) If you initialize Helpshift through C# scripts using custom configuration, please set your custom configuration below
     as installConfig dictionary and uncomment the code below for default install.
     installForApiKey:domainName:appId:withOptions

     */

    BOOL defaultConfigInstall = [HelpshiftUnityInstaller installDefault];
    if (defaultConfigInstall == NO) {

        // Default config is not set correctly. If you intend to use default configuration, please check api key, app id and
        // domain name are set correctly in the default config. Otherwise you can initialize Helpshift with custom
        // configuration by uncommenting the code below.

        NSDictionary *installConfig = @{}; //Set your custom configuration

        // [HelpshiftCore initializeWithProvider:[HelpshiftAll sharedInstance]];
        // [HelpshiftCore installForApiKey:@"<your_api_key>" domainName:@"<your_domain_name>.helpshift.com" appID:@"<your_app_id>" withOptions:installConfig];
    }

    return [super application:application didFinishLaunchingWithOptions:launchOptions];
}

- (void) application:(UIApplication *)app didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    [super application:app didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];

    // Helpshift::register device token with Helpshift for Push Notification. Please make sure you've initialized Helpshift
    // in Obj-C
    [HelpshiftCore registerDeviceToken:deviceToken];
}

- (void) application:(UIApplication *)application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void (^)())completionHandler {
    if (![HelpshiftCore handleEventsForBackgroundURLSession:identifier completionHandler:completionHandler]) {
        // Handle events for background url session. Once you have implemented this function in UnityAppController, uncomment
        // the code below and comment the call completionHandler();

        //[super application:application handleEventsForBackgroundURLSession:identifier completionHandler:completionHandler];
        completionHandler();
    }
}

- (void) application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo {
    if (![HelpshiftCore handleRemoteNotification:userInfo withController:[UIApplication sharedApplication].keyWindow.rootViewController]) {
        [super application:application didReceiveRemoteNotification:userInfo];
    }
}

- (void) application:(UIApplication *)application didReceiveLocalNotification:(UILocalNotification *)notification {
    if (![HelpshiftCore handleLocalNotification:notification withController:[UIApplication sharedApplication].keyWindow.rootViewController]) {
        [super application:application didReceiveLocalNotification:notification];
    }
}

- (void) application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forRemoteNotification:(NSDictionary *)userInfo completionHandler:(void (^)())completionHandler {

    if (![HelpshiftCore handleInteractiveRemoteNotification:userInfo forAction:identifier completionHandler:completionHandler]) {

        // Handle action with identifier. Once you have implemented this function in UnityAppController, uncomment
        // the code below and comment the call completionHandler();

        //[super application:application handleActionWithIdentifier:identifier forRemoteNotification:userInfo completionHandler:completionHandler];
        completionHandler();
    }
}

- (void) application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forLocalNotification:(UILocalNotification *)notification completionHandler:(void (^)())completionHandler {
    if(![HelpshiftCore handleInteractiveLocalNotification:notification forAction:identifier completionHandler:completionHandler]) {
        // Handle action with identifier. Once you have implemented this function in UnityAppController, uncomment
        // the code below and comment the call completionHandler();

        //[super application:application handleActionWithIdentifier:identifier forLocalNotification:notification completionHandler:completionHandler];
        completionHandler();
    }
}

- (BOOL) application:(UIApplication *)application openURL:(nonnull NSURL *)url sourceApplication:(nullable NSString *)sourceApplication annotation:(nonnull id)annotation {
    if([[url host] isEqualToString:@"helpshift"]) {
        NSArray *components = [[url path] componentsSeparatedByString:@"/"];
        if([components count] == 3) {
            if([[components objectAtIndex:1] isEqualToString:@"section"]) {
                [HelpshiftSupport showFAQSection:[components objectAtIndex:2] withController:[UIApplication sharedApplication].keyWindow.rootViewController withOptions:@{}];
            } else if([[components objectAtIndex:1] isEqualToString:@"faq"]) {
                [HelpshiftSupport showSingleFAQ:[components objectAtIndex:2] withController:[UIApplication sharedApplication].keyWindow.rootViewController withOptions:@{}];
            }
        }
        return true;
    }
    return [super application:application
                      openURL:url
            sourceApplication:sourceApplication
                   annotation:annotation];
}

@end

