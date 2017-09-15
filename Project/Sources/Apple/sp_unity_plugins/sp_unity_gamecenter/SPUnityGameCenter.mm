//
//  SPUnityGameCenter.m
//  Unity-iPhone
//
//  Created by Miguel Janer on 1/2/16.
//
#include <string>
#include "SPNativeCallsSender.h"
#import <GameKit/GameKit.h>

static const std::string kNotifyMethod = "Notify";

NSURL* _publicKeyUrl;
NSData* _signature;
NSData* _salt;
uint64_t _timestamp;

void generateIdentityVerificationSignature()
{
    GKLocalPlayer* localPlayer = [GKLocalPlayer localPlayer];
    [localPlayer generateIdentityVerificationSignatureWithCompletionHandler:^(NSURL* publicKeyUrl, NSData* signature, NSData* salt,
                                                                              uint64_t timestamp, NSError* error) {
      NSMutableDictionary* dict = [[NSMutableDictionary alloc] init];
      if(!error)
      {
          NSString* signatureStr = [signature base64EncodedStringWithOptions:0];
          NSString* saltStr = [salt base64EncodedStringWithOptions:0];
          [dict setObject:@false forKey:@"error"];
          [dict setObject:publicKeyUrl.absoluteString forKey:@"url"];
          [dict setObject:signatureStr forKey:@"signature"];
          [dict setObject:saltStr forKey:@"salt"];
          [dict setObject:[NSNumber numberWithLongLong:timestamp] forKey:@"timestamp"];
      }
      else
      {
          [dict setObject:@true forKey:@"error"];
          [dict setObject:[@(error.code) stringValue] forKey:@"errorCode"];
          [dict setObject:error.localizedDescription forKey:@"errorMessage"];
      }

      if([NSJSONSerialization isValidJSONObject:dict])
      {
          NSData* json = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:nil];
          NSString* jsonString = [[NSString alloc] initWithData:json encoding:NSUTF8StringEncoding];
          SPNativeCallsSender::SendMessage(kNotifyMethod, [jsonString UTF8String]);
      }
      else
      {
          SPNativeCallsSender::SendMessage(kNotifyMethod, "");
      }

    }];
}

extern "C" {
void SPUnityGameCenter_UserVerificationInit()
{
    generateIdentityVerificationSignature();
}
}
