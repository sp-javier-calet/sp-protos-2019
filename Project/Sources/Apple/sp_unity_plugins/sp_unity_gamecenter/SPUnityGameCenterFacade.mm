
#include "SPUnityGameCenterFacade.h"
#import <GameKit/GameKit.h>

EXPORT_API void SPUnityGameCenter_GenerateUserVerification(SPUnityGameCenterUserVerificationCallback callback)
{
    GKLocalPlayer* localPlayer = [GKLocalPlayer localPlayer];
    [localPlayer generateIdentityVerificationSignatureWithCompletionHandler:^(NSURL* publicKeyUrl, NSData* signature, NSData* salt,
                                                                              uint64_t timestamp, NSError* error) {
        if(error)
        {
            if(callback != nullptr)
            {
                callback(nil, error.localizedDescription.UTF8String);
            }
            return;
        }

        NSMutableDictionary* dict = [[NSMutableDictionary alloc] init];
        NSString* signatureStr = [signature base64EncodedStringWithOptions:0];
        NSString* saltStr = [salt base64EncodedStringWithOptions:0];
        [dict setObject:@false forKey:@"error"];
        [dict setObject:publicKeyUrl.absoluteString forKey:@"url"];
        [dict setObject:signatureStr forKey:@"signature"];
        [dict setObject:saltStr forKey:@"salt"];
        [dict setObject:[NSNumber numberWithLongLong:timestamp] forKey:@"timestamp"];
        NSString* jsonString = NULL;
        if([NSJSONSerialization isValidJSONObject:dict])
        {
            NSData* json = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:nil];
            jsonString = [[NSString alloc] initWithData:json encoding:NSUTF8StringEncoding];
        }
        
        if(callback != nullptr)
        {
            callback(jsonString.UTF8String, nil);
        }
        
    }];
}
