#include "SPUnityKeychainFacade.h"
#include "KeychainItemWrapper.h"
#include <string.h>

KeychainItemWrapper* SPUnityKeychainCreateWrapper(SPUnityKeychainItemStruct item, OSStatus* status)
{
    NSString* accessGroup = [NSString stringWithUTF8String:item.accessGroup];
    NSString* identifier = [NSString stringWithUTF8String:item.id];
    NSString* service = [NSString stringWithUTF8String:item.service];
    return [[KeychainItemWrapper alloc]
    initWithIdentifier:identifier
               service:service
           accessGroup:accessGroup
                 error:status];
}

char* SPUnityKeychainCreateString(const char* str)
{
    if(str == nullptr)
    {
        return nullptr;
    }
    char* nstr = (char*)malloc(sizeof(char)*(strlen(str)+1));
    strcpy(nstr, str);
    return nstr;
}

EXPORT_API int SPUnityKeychainSet(SPUnityKeychainItemStruct item, const char* value)
{
    OSStatus status;
    KeychainItemWrapper* wrapper = SPUnityKeychainCreateWrapper(item, &status);
    if(status != noErr)
    {
        return status;
    }
    [wrapper setObject:[NSString stringWithUTF8String:value]
            forKey:(__bridge id)kSecValueData
             error:&status];
    return status;
}

EXPORT_API char* SPUnityKeychainGet(SPUnityKeychainItemStruct item)
{
    OSStatus status;
    KeychainItemWrapper* wrapper = SPUnityKeychainCreateWrapper(item, &status);
    if(status != noErr)
    {
        return SPUnityKeychainCreateString(nullptr);
    }
    id value = [wrapper objectForKey:(__bridge id)kSecValueData];
    if(value == nil || ![value isKindOfClass:[NSString class]])
    {
        return SPUnityKeychainCreateString(nullptr);
    }
    else
    {
        return SPUnityKeychainCreateString(((NSString*)value).UTF8String);
    }
}

EXPORT_API int SPUnityKeychainClear(SPUnityKeychainItemStruct item)
{
    OSStatus status;
    KeychainItemWrapper* wrapper = SPUnityKeychainCreateWrapper(item, &status);
    if(status != noErr)
    {
        return status;
    }
    [wrapper resetKeychainItemWithError:&status];
    if(status == errSecItemNotFound)
    {
        status = noErr;
    }
    return status;
}

const char* kSPUnityKeychainSeedIdKey = "BundleSeedId";

EXPORT_API char* SPUnityKeychainGetDefaultAccessGroup()
{
    NSString* seedIdKey = [NSString stringWithUTF8String:kSPUnityKeychainSeedIdKey];
    NSDictionary *query = [NSDictionary dictionaryWithObjectsAndKeys:
                           (__bridge id)kSecClassGenericPassword, kSecClass,
                           seedIdKey, kSecAttrAccount,
                           @"", kSecAttrService,
                           (id)kCFBooleanTrue, kSecReturnAttributes,
                           nil];
    CFDictionaryRef result = nil;
    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, (CFTypeRef*)&result);
    if (status == errSecItemNotFound)
    {
        status = SecItemAdd((__bridge CFDictionaryRef)query, (CFTypeRef*)&result);
    }
    if (status != errSecSuccess)
    {
        return SPUnityKeychainCreateString(nullptr);
    }
    NSString* accessGroup = [NSString stringWithString:[(__bridge NSDictionary*)result
                                                        objectForKey:(__bridge id)kSecAttrAccessGroup]];
    CFRelease(result);
    if(accessGroup == nil)
    {
        return SPUnityKeychainCreateString(nullptr);
    }
    else
    {
        return SPUnityKeychainCreateString(accessGroup.UTF8String);
    }
}
