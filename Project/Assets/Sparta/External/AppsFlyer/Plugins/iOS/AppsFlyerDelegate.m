//
//  AppsFlyerDelegate.m
//  
//
//  Created by Golan on 6/21/15.
//
//

#import "AppsFlyerDelegate.h"

@implementation AppsFlyerDelegate


- (void) onConversionDataReceived:(NSDictionary*) installData {
    
    NSString *conversionDataResult = [self getJsonStringFromDictionary:installData];
    NSLog (@"AppsFlyerDelegate onConversionDataReceived = %@", conversionDataResult);
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_CONVERSION, [conversionDataResult UTF8String]);
}

- (void) onConversionDataRequestFailure:(NSError *)error {
    
    NSString *errDesc = [error localizedDescription];
    NSLog (@"AppsFlyerDelegate onConversionDataRequestFailure = %@", errDesc);
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_CONVERSION_ERROR, [errDesc UTF8String]);
}

- (void) onAppOpenAttribution:(NSDictionary*) attributionData {
    NSString *attrData = [self getJsonStringFromDictionary:attributionData];
    NSLog (@"AppsFlyerDelegate onConversionDataReceived = %@", attrData);
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_RETARGETTING, [attrData UTF8String]);
}

- (void) onAppOpenAttributionFailure:(NSError *)error{
    NSString *errDesc = [error localizedDescription];
    NSLog (@"AppsFlyerDelegate onAppOpenAttributionFailure = %@", errDesc);
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_RETARGETTING_ERROR, [errDesc UTF8String]);
    
}


- (NSString *) getJsonStringFromDictionary:(NSDictionary *)dict {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict
                                                       options:0
                                                         error:&error];
    
    if (!jsonData) {
        NSLog(@"JSON error: %@", error);
        return nil;
    } else {
        
        NSString *JSONString = [[NSString alloc] initWithBytes:[jsonData bytes] length:[jsonData length] encoding:NSUTF8StringEncoding];
        NSLog(@"JSON OUTPUT: %@", JSONString);
        return JSONString;
    }
}


@end
