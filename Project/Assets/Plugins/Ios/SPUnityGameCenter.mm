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

NSURL* _publicKeyUrl;
NSData* _signature;
NSData* _salt;
uint64_t _timestamp;

void generateIdentityVerificationSignature()
{
    GKLocalPlayer* localPlayer = [GKLocalPlayer localPlayer];
    [localPlayer generateIdentityVerificationSignatureWithCompletionHandler:^(
                                                                              NSURL* publicKeyUrl, NSData* signature, NSData* salt, uint64_t timestamp, NSError* error){
        NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
        if(!error)
        {
            NSString *signatureStr = [signature base64Encoding];
            NSString *saltStr = [salt base64Encoding];
            [dict setObject:@false forKey:@"error"];
            [dict setObject:publicKeyUrl.absoluteString forKey:@"url"];
            [dict setObject:signatureStr forKey:@"signature"];
            [dict setObject:saltStr forKey:@"salt"];
            [dict setObject:[NSNumber numberWithLong:timestamp] forKey:@"timestamp"];
        }
        else
        {
            [dict setObject:@true forKey:@"error"];
            [dict setObject:[@(error.code) stringValue] forKey:@"errorCode"];
            [dict setObject:error.localizedDescription forKey:@"errorMessage"];
        }
    
        if([NSJSONSerialization isValidJSONObject:dict])
        {
            NSData *json = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:nil];
            NSString *jsonString = [[NSString alloc] initWithData:json encoding:NSUTF8StringEncoding];
            UnitySendMessage(_gameObjectName.c_str(), kNotifyMethod.c_str(), [jsonString UTF8String]);
        }
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
