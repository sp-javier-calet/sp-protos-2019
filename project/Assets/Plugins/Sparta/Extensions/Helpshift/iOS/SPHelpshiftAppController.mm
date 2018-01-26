
#import <SPUnityPlugins/SPAppControllerDelegate.h>
#import <Foundation/Foundation.h>
#import "HelpshiftCore.h"
#import "HelpshiftSupport.h"
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

    [HelpshiftCore initializeWithProvider:[HelpshiftAll sharedInstance]];
    [HelpshiftCore installForApiKey:apiKey domainName:domainName appID:appId withOptions:configDictionary];
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



@interface SPHelpshiftAppController : NSObject<SPAppControllerDelegate>

@end

@implementation SPHelpshiftAppController

- (void) application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {

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

        // NSDictionary *installConfig = @{}; //Set your custom configuration

        // [HelpshiftCore initializeWithProvider:[HelpshiftAll sharedInstance]];
        // [HelpshiftCore installForApiKey:@"<your_api_key>" domainName:@"<your_domain_name>.helpshift.com" appID:@"<your_app_id>" withOptions:installConfig];
    }
}

- (void) application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    // Helpshift::register device token with Helpshift for Push Notification. Please make sure you've initialized Helpshift
    // in Obj-C
    [HelpshiftCore registerDeviceToken:deviceToken];
}

- (BOOL) application:application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void (^)())completionHandler {
    return [HelpshiftCore handleEventsForBackgroundURLSession:identifier completionHandler:completionHandler];
}

- (void) application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo {
    [HelpshiftCore handleRemoteNotification:userInfo withController:application.keyWindow.rootViewController];
}

- (void) application:(UIApplication *)application didReceiveLocalNotification:(UILocalNotification *)notification {
    [HelpshiftCore handleLocalNotification:notification withController:application.keyWindow.rootViewController];
}

- (BOOL) application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forRemoteNotification:(NSDictionary *)userInfo completionHandler:(void (^)())completionHandler {
    return [HelpshiftCore handleInteractiveRemoteNotification:userInfo forAction:identifier completionHandler:completionHandler];
}

- (BOOL) application:(UIApplication *)application handleActionWithIdentifier:(NSString *)identifier forLocalNotification:(UILocalNotification *)notification completionHandler:(void (^)())completionHandler {
    return [HelpshiftCore handleInteractiveLocalNotification:notification forAction:identifier completionHandler:completionHandler];
}

- (BOOL) application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options {
    if([[url host] isEqualToString:@"helpshift"]) {
        NSArray *components = [[url path] componentsSeparatedByString:@"/"];
        if([components count] == 3) {
            if([[components objectAtIndex:1] isEqualToString:@"section"]) {
                [HelpshiftSupport showFAQSection:[components objectAtIndex:2] withController:[UIApplication sharedApplication].keyWindow.rootViewController withOptions:@{}];
            } else if([[components objectAtIndex:1] isEqualToString:@"faq"]) {
                [HelpshiftSupport showSingleFAQ:[components objectAtIndex:2] withController:[UIApplication sharedApplication].keyWindow.rootViewController withOptions:@{}];
            }
        }
        return TRUE;
    }
    return FALSE;
}

@end
