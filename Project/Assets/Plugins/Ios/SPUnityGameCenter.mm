//
//  SPUnityGameCenter.m
//  Unity-iPhone
//
//  Created by Miguel Janer on 1/2/16.
//
#include <string>
#import <GameKit/GameKit.h>

std::string _gameObjectName;
static const std::string kNotifyMethod = "Notify";

void generateIdentityVerificationSignature()
{
    GKLocalPlayer* localPlayer = [GKLocalPlayer localPlayer];
    [localPlayer generateIdentityVerificationSignatureWithCompletionHandler:^(
                                                                              NSURL* publicKeyUrl, NSData* signature, NSData* salt, uint64_t timestamp, NSError* error){
        if(!error)
        {
            //serialize json with content and sent it
            NSDictionary *dict = @{@"url":publicKeyUrl.absoluteString,
                                   @"signature":signature,
                                   @"salt":salt,
                                   @"timestamp":[NSNumber numberWithLong:timestamp]};
            if([NSJSONSerialization isValidJSONObject:dict])
            {
                NSData *json = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:nil];
                NSString *jsonString = [[NSString alloc] initWithData:json encoding:NSUTF8StringEncoding];
                UnitySendMessage(_gameObjectName.c_str(), kNotifyMethod.c_str(), [jsonString UTF8String]);
            }
            
        }
        UnitySendMessage(_gameObjectName.c_str(), kNotifyMethod.c_str(), "failed");
        
    }];
}

extern "C"
{
    void SPUnityGameCenterUserVerification_Init(const char* gameObjectName)
    {
        _gameObjectName = gameObjectName;
        generateIdentityVerificationSignature();
    }
}
