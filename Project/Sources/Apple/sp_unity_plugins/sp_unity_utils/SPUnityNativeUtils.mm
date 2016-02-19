#include "SPUnityNativeUtils.h"
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

EXPORT_API bool SPUnityNativeUtilsIsInstalled(const char* appId)
{
    NSURL* url = [NSURL URLWithString:[NSString stringWithCString:appId encoding:NSUTF8StringEncoding]];
    return [[UIApplication sharedApplication] canOpenURL:url];
}
