#include "SPUnityHardwareFacade.h"
#import <AdSupport/ASIdentifierManager.h>
#import <Foundation/Foundation.h>
#import <SystemConfiguration/SystemConfiguration.h>
#import <UIKit/UIKit.h>
#include "SPUnityNativeUtils.h"
#include <arpa/inet.h>
#include <ifaddrs.h>
#include <mach/mach.h>
#include <mach/mach_host.h>
#include <netinet/in.h>
#include <string.h>
#include <sys/sysctl.h>
#include <sys/types.h>

EXPORT_API char* SPUnityHardwareGetDeviceString()
{
    size_t size;
    sysctlbyname("hw.machine", NULL, &size, NULL, 0);
    char* machine = (char*)malloc(size);
    sysctlbyname("hw.machine", machine, &size, NULL, 0);
    return machine;
}

EXPORT_API char* SPUnityHardwareGetDevicePlatformVersion()
{
    NSString* version = [[UIDevice currentDevice] systemVersion];
    if(version == nil)
    {
        return SPUnityNativeUtils::createString("");
    }
    return SPUnityNativeUtils::createString(version.UTF8String);
}

EXPORT_API char* SPUnityHardwareGetDeviceArchitecture()
{
    size_t size;
    cpu_type_t type;
    cpu_subtype_t subtype;

    size = sizeof(type);
    sysctlbyname("hw.cputype", &type, &size, NULL, 0);

    size = sizeof(subtype);
    sysctlbyname("hw.cpusubtype", &subtype, &size, NULL, 0);

    // values for cputype and cpusubtype defined in mach/machine.h
    if(type == CPU_TYPE_ARM64)
    {
        switch(subtype)
        {
            case CPU_SUBTYPE_ARM64_V8:
                return SPUnityNativeUtils::createString("arm64-v8");
                break;
            default:
                return SPUnityNativeUtils::createString("arm64-unknown");
                break;
        }
    }
    else if(type == CPU_TYPE_ARM)
    {
        switch(subtype)
        {
            case CPU_SUBTYPE_ARM_V4T:
                return SPUnityNativeUtils::createString("arm-v4t");
                break;
            case CPU_SUBTYPE_ARM_V6:
                return SPUnityNativeUtils::createString("arm-v6");
                break;
            case CPU_SUBTYPE_ARM_V5TEJ:
                return SPUnityNativeUtils::createString("arm-v5tej");
                break;
            case CPU_SUBTYPE_ARM_XSCALE:
                return SPUnityNativeUtils::createString("arm-xscale");
                break;
            case CPU_SUBTYPE_ARM_V7:
                return SPUnityNativeUtils::createString("arm-v7");
                break;
            case CPU_SUBTYPE_ARM_V7F:
                return SPUnityNativeUtils::createString("arm-v7f");
                break;
            case CPU_SUBTYPE_ARM_V7S:
                return SPUnityNativeUtils::createString("arm-v7s");
                break;
            case CPU_SUBTYPE_ARM_V6M:
                return SPUnityNativeUtils::createString("arm-v6m");
                break;
            case CPU_SUBTYPE_ARM_V7M:
                return SPUnityNativeUtils::createString("arm-v7m");
                break;
            case CPU_SUBTYPE_ARM_V7EM:
                return SPUnityNativeUtils::createString("arm-v7em");
                break;
            default:
                return SPUnityNativeUtils::createString("arm-unknown");
                break;
        }
    }
    return SPUnityNativeUtils::createString("unknown");
}

EXPORT_API char* SPUnityHardwareGetDeviceAdvertisingId()
{
    if(NSClassFromString(@"ASIdentifierManager"))
    {
        NSString* idfa = [ASIdentifierManager sharedManager].advertisingIdentifier.UUIDString;
        if(idfa != nil)
        {
            return SPUnityNativeUtils::createString(idfa.UTF8String);
        }
    }
    return SPUnityNativeUtils::createString("");
}

EXPORT_API bool SPUnityHardwareGetDeviceAdvertisingIdEnabled()
{
    if(NSClassFromString(@"ASIdentifierManager"))
    {
        return [ASIdentifierManager sharedManager].isAdvertisingTrackingEnabled;
    }
    else
    {
        return true;
    }
}

EXPORT_API bool SPUnityHardwareGetDeviceRooted()
{
#if TARGET_IPHONE_SIMULATOR
    return false;
#else
    FILE* f = fopen("/bin/bash", "r");
    bool rooted = errno != ENOENT;
    fclose(f);
    return rooted;
#endif
}

void SPUnityHardwareGetMemoryStatistics(vm_statistics_data_t& vm_stat, vm_size_t& pagesize)
{
    mach_port_t host_port;
    mach_msg_type_number_t host_size;

    host_port = mach_host_self();
    host_size = sizeof(vm_statistics_data_t) / sizeof(integer_t);
    host_page_size(host_port, &pagesize);

    if(host_statistics(host_port, HOST_VM_INFO, (host_info_t)&vm_stat, &host_size) != KERN_SUCCESS)
    {
        pagesize = 0;
    }
}

EXPORT_API uint64_t SPUnityHardwareGetTotalMemory()
{
    return [NSProcessInfo processInfo].physicalMemory;
}

EXPORT_API uint64_t SPUnityHardwareGetFreeMemory()
{
    vm_statistics_data_t vm_stat;
    vm_size_t pagesize;
    SPUnityHardwareGetMemoryStatistics(vm_stat, pagesize);
    return (vm_stat.free_count) * pagesize;
}

EXPORT_API uint64_t SPUnityHardwareGetUsedMemory()
{
    vm_statistics_data_t vm_stat;
    vm_size_t pagesize = 0;
    SPUnityHardwareGetMemoryStatistics(vm_stat, pagesize);
    return (vm_stat.active_count + vm_stat.inactive_count + vm_stat.wire_count) * pagesize;
}

EXPORT_API uint64_t SPUnityHardwareGetActiveMemory()
{
    vm_statistics_data_t vm_stat;
    vm_size_t pagesize = 0;
    SPUnityHardwareGetMemoryStatistics(vm_stat, pagesize);
    return (vm_stat.active_count) * pagesize;
}

EXPORT_API char* SPUnityHardwareGetAppId()
{
    NSString* id = [[NSBundle mainBundle] bundleIdentifier];
    if(id == nil)
    {
        return SPUnityNativeUtils::createString("");
    }
    return SPUnityNativeUtils::createString(id.UTF8String);
}

char* SPUnityHardwareGetBundleInfoString(const char* name)
{
    NSDictionary* info = [[NSBundle mainBundle] infoDictionary];
    NSString* value = [info objectForKey:[NSString stringWithUTF8String:name]];
    if(value == nil)
    {
        return SPUnityNativeUtils::createString("");
    }
    return SPUnityNativeUtils::createString(value.UTF8String);
}

EXPORT_API char* SPUnityHardwareGetAppVersion()
{
    return SPUnityHardwareGetBundleInfoString("CFBundleVersion");
}

EXPORT_API char* SPUnityHardwareGetAppShortVersion()
{
    return SPUnityHardwareGetBundleInfoString("CFBundleShortVersionString");
}

EXPORT_API char* SPUnityHardwareGetAppLanguage()
{
    NSArray* langs = [NSLocale preferredLanguages];
    for(NSString* language in langs)
    {
        NSLocale* locale = [NSLocale localeWithLocaleIdentifier:language];
        NSString* lang = [locale objectForKey:NSLocaleLanguageCode];
        NSString* script = [locale objectForKey:NSLocaleScriptCode];// this will return Hans or Hant for chinese language
        if([script length] != 0)
        {
            lang = [NSString
              stringWithFormat:@"%@-%@", lang, script];// this will effectively return zh-Hans and zh-Hant for simplified or traditional chinese
        }
        if(lang != nil)
        {
            return SPUnityNativeUtils::createString(lang.UTF8String);
        }
    }
    return SPUnityNativeUtils::createString("");
}

EXPORT_API char* SPUnityHardwareGetAppCountry()
{
    NSLocale* locale = [NSLocale currentLocale];
    NSString* country = [locale objectForKey:NSLocaleCountryCode];
    if(country == nil)
    {
        return SPUnityNativeUtils::createString("");
    }
    return SPUnityNativeUtils::createString(country.UTF8String);
}

EXPORT_API char* SPUnityHardwareGetNetworkConnectivity()
{
    SCNetworkReachabilityRef ref = SCNetworkReachabilityCreateWithName(kCFAllocatorDefault, [@"www.google.com" UTF8String]);
    SCNetworkReachabilityFlags flags = 0;

    if(SCNetworkReachabilityGetFlags(ref, &flags))
    {
        CFRelease(ref);
        if((flags & kSCNetworkReachabilityFlagsReachable))
        {
            if((flags & kSCNetworkReachabilityFlagsIsWWAN))
            {
                return SPUnityNativeUtils::createString("wwan");
            }
            else
            {
                return SPUnityNativeUtils::createString("wifi");
            }
        }
        else
        {
            return SPUnityNativeUtils::createString("none");
        }
    }
    CFRelease(ref);
    return SPUnityNativeUtils::createString("none");
}

const size_t kProxyBufferLength = 4096;

EXPORT_API char* SPUnityHardwareGetNetworkProxy()
{
    CFDictionaryRef dicRef = CFNetworkCopySystemProxySettings();
    const CFStringRef cfstr = (const CFStringRef)CFDictionaryGetValue(dicRef, (const void*)kCFNetworkProxiesHTTPProxy);

    char host[kProxyBufferLength];
    memset(host, 0, kProxyBufferLength);

    if(cfstr)
    {
        if(CFStringGetCString(cfstr, host, kProxyBufferLength, kCFStringEncodingUTF8))
        {
        }
    }

    const CFNumberRef cfnum = (const CFNumberRef)CFDictionaryGetValue(dicRef, (const void*)kCFNetworkProxiesHTTPPort);

    if(cfnum)
    {
        SInt32 port;
        if(CFNumberGetValue(cfnum, kCFNumberSInt32Type, &port))
        {
            char proxy[kProxyBufferLength];
            sprintf(proxy, "%s:%d", host, (int)port);
            return SPUnityNativeUtils::createString(proxy);
        }
    }

    return SPUnityNativeUtils::createString(host);
}

EXPORT_API char* SPUnityHardwareGetNetworkIpAddress()
{
    char* addr = nullptr;
    struct ifaddrs* interfaces = nullptr;
    struct ifaddrs* temp_addr = nullptr;
    int success = 0;

    success = getifaddrs(&interfaces);
    if(success == 0)
    {
        temp_addr = interfaces;
        while(temp_addr != nullptr)
        {
            if(temp_addr->ifa_addr->sa_family == AF_INET6)
            {
                // Check if interface is en0 which is the wifi connection on the iPhone
                if(strcmp(temp_addr->ifa_name, "en0") == 0)
                {
                    char str[INET6_ADDRSTRLEN];
                    inet_ntop(AF_INET6, &(((struct sockaddr_in6*)temp_addr->ifa_addr)->sin6_addr), str, INET6_ADDRSTRLEN);
                    addr = str;
                    break;
                }
            }
            else if(temp_addr->ifa_addr->sa_family == AF_INET)
            {
                if(strcmp(temp_addr->ifa_name, "en0") == 0)
                {
                    char str[INET_ADDRSTRLEN];
                    inet_ntop(AF_INET, &(((struct sockaddr_in*)temp_addr->ifa_addr)->sin_addr), str, INET_ADDRSTRLEN);
                    addr = str;
                    break;
                }
            }
            temp_addr = temp_addr->ifa_next;
        }
    }
    freeifaddrs(interfaces);
    if(addr == nullptr)
    {
        return SPUnityNativeUtils::createString("");
    }
    return SPUnityNativeUtils::createString(addr);
}

uint64_t SPUnityHardwareGetStorageInfo(NSString* key)
{
    uint64_t value = 0;
    NSError* error = nil;
    NSArray* paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSDictionary* dictionary = [[NSFileManager defaultManager] attributesOfFileSystemForPath:[paths lastObject] error:&error];

    if(dictionary)
    {
        NSNumber* fileSystemSizeInBytes = [dictionary objectForKey:key];
        value = [fileSystemSizeInBytes unsignedLongLongValue];
    }
    return value;
}

EXPORT_API uint64_t SPUnityHardwareGetTotalStorage()
{
    return SPUnityHardwareGetStorageInfo(NSFileSystemSize);
}

EXPORT_API uint64_t SPUnityHardwareGetFreeStorage()
{
    return SPUnityHardwareGetStorageInfo(NSFileSystemFreeSize);
}

EXPORT_API uint64_t SPUnityHardwareGetUsedStorage()
{
    return SPUnityHardwareGetTotalStorage() - SPUnityHardwareGetFreeStorage();
}
