#include "SPUnityHardwareFacade.h"
#import <AdSupport/ASIdentifierManager.h>
#import <Foundation/Foundation.h>
#import <SystemConfiguration/SystemConfiguration.h>
#import <UIKit/UIKit.h>
#include <arpa/inet.h>
#include <ifaddrs.h>
#include <mach/mach.h>
#include <mach/mach_host.h>
#include <netinet/in.h>
#include <string.h>
#include <sys/sysctl.h>
#include <sys/types.h>

char* SPUnityHardwareCreateString(const char* str)
{
    char* nstr = (char*)malloc(sizeof(char)*(strlen(str)+1));
    strcpy(nstr, str);
    return nstr;
}

EXPORT_API char* SPUnityHardwareGetDeviceString()
{
    size_t size;
    sysctlbyname("hw.machine", NULL, &size, NULL, 0);
    char *machine = (char*)malloc(size);
    sysctlbyname("hw.machine", machine, &size, NULL, 0);
    return machine;
}

EXPORT_API char* SPUnityHardwareGetDevicePlatformVersion()
{
    NSString* version = [[UIDevice currentDevice] systemVersion];
    if(version == nil)
    {
        return SPUnityHardwareCreateString("");
    }
    return SPUnityHardwareCreateString(version.UTF8String);
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
                return SPUnityHardwareCreateString("arm64-v8");
                break;
            default:
                return SPUnityHardwareCreateString("unknown");
                break;
        }
    }
    else if(type == CPU_TYPE_ARM)
    {
        switch(subtype)
        {
            case CPU_SUBTYPE_ARM_V4T:
                return SPUnityHardwareCreateString("arm-v4t");
                break;
            case CPU_SUBTYPE_ARM_V6:
                return SPUnityHardwareCreateString("arm-v6");
                break;
            case CPU_SUBTYPE_ARM_V5TEJ:
                return SPUnityHardwareCreateString("arm-v5tej");
                break;
            case CPU_SUBTYPE_ARM_XSCALE:
                return SPUnityHardwareCreateString("arm-xscale");
                break;
            case CPU_SUBTYPE_ARM_V7:
                return SPUnityHardwareCreateString("arm-v7");
                break;
            case CPU_SUBTYPE_ARM_V7F:
                return SPUnityHardwareCreateString("arm-v7f");
                break;
            case CPU_SUBTYPE_ARM_V7S:
                return SPUnityHardwareCreateString("arm-v7s");
                break;
            case CPU_SUBTYPE_ARM_V6M:
                return SPUnityHardwareCreateString("arm-v6m");
                break;
            case CPU_SUBTYPE_ARM_V7M:
                return SPUnityHardwareCreateString("arm-v7m");
                break;
            case CPU_SUBTYPE_ARM_V7EM:
                return SPUnityHardwareCreateString("arm-v7em");
                break;
            default:
                return SPUnityHardwareCreateString("unknown");
                break;
        }
    }
    return SPUnityHardwareCreateString("unknown");
}

EXPORT_API char* SPUnityHardwareGetDeviceAdvertisingId()
{
    if(NSClassFromString(@"ASIdentifierManager"))
    {
        NSString* idfa = [ASIdentifierManager sharedManager].advertisingIdentifier.UUIDString;
        if(idfa != nil)
        {
            return SPUnityHardwareCreateString(idfa.UTF8String);
        }
    }
    return SPUnityHardwareCreateString("");
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
    FILE *f = fopen("/bin/bash", "r");
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
    
    if (host_statistics(host_port, HOST_VM_INFO, (host_info_t)&vm_stat, &host_size) != KERN_SUCCESS)
    {
        pagesize = 0;
    }
}

EXPORT_API uint64_t SPUnityHardwareGetTotalMemory()
{
    vm_statistics_data_t vm_stat;
    vm_size_t pagesize;
    SPUnityHardwareGetMemoryStatistics(vm_stat, pagesize);
    return (vm_stat.active_count + vm_stat.inactive_count + vm_stat.wire_count + vm_stat.free_count) * pagesize;
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
    NSString *id = [[NSBundle mainBundle] bundleIdentifier];
    if(id == nil)
    {
        return SPUnityHardwareCreateString("");
    }
    return SPUnityHardwareCreateString(id.UTF8String);
}

char* SPUnityHardwareGetBundleInfoString(const char* name)
{
    NSDictionary* info = [[NSBundle mainBundle] infoDictionary];
    NSString* value = [info objectForKey:[NSString stringWithUTF8String:name]];
    if(value == nil)
    {
        return SPUnityHardwareCreateString("");
    }
    return SPUnityHardwareCreateString(value.UTF8String);
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
        NSString* script = [locale objectForKey:NSLocaleScriptCode]; // this will return Hans or Hant for chinese language
        if ([script length] != 0)
        {
            lang = [NSString stringWithFormat:@"%@-%@", lang, script]; //this will effectively return zh-Hans and zh-Hant for simplified or traditional chinese
        }
        if(lang != nil)
        {
            return SPUnityHardwareCreateString(lang.UTF8String);
        }
    }
    return SPUnityHardwareCreateString("");
}

EXPORT_API char* SPUnityHardwareGetAppCountry()
{
    NSLocale* locale = [NSLocale currentLocale];
    NSString* country = [locale objectForKey: NSLocaleCountryCode];
    if(country == nil)
    {
        return SPUnityHardwareCreateString("");
    }
    return SPUnityHardwareCreateString(country.UTF8String);
}

EXPORT_API char* SPUnityHardwareGetNetworkConnectivity()
{
    struct sockaddr_in zeroAddress;
    bzero(&zeroAddress, sizeof(zeroAddress));
    zeroAddress.sin_len = sizeof(zeroAddress);
    zeroAddress.sin_family = AF_INET;
    SCNetworkReachabilityRef ref = SCNetworkReachabilityCreateWithAddress(kCFAllocatorDefault, (const struct sockaddr*)&zeroAddress);
    SCNetworkReachabilityFlags flags = 0;
    
    if(SCNetworkReachabilityGetFlags(ref, &flags)) 
    {
        if((flags & kSCNetworkReachabilityFlagsReachable))
        {
            if((flags & kSCNetworkReachabilityFlagsIsWWAN))
            {
                return SPUnityHardwareCreateString("wwan");
            }
            else
            {
                return SPUnityHardwareCreateString("wifi");
            }
        }
        else
        {
            return SPUnityHardwareCreateString("none");
        }
    }
    
    return SPUnityHardwareCreateString("");
}

const size_t kProxyBufferLength = 4096;

EXPORT_API char* SPUnityHardwareGetNetworkProxy()
{
    CFDictionaryRef dicRef = CFNetworkCopySystemProxySettings();
    const CFStringRef cfstr = (const CFStringRef)CFDictionaryGetValue(dicRef, (const void*)kCFNetworkProxiesHTTPProxy);

    char host[kProxyBufferLength];
    memset(host, 0, kProxyBufferLength);
    
    if (cfstr)
    {
        if (CFStringGetCString(cfstr, host, kProxyBufferLength, kCFStringEncodingUTF8))
        {
        }
    }

    const CFNumberRef cfnum = (const CFNumberRef)CFDictionaryGetValue(dicRef, (const void*)kCFNetworkProxiesHTTPPort);
        
    if (cfnum)
    {
        SInt32 port;
        if (CFNumberGetValue(cfnum, kCFNumberSInt32Type, &port))
        {
            char proxy[kProxyBufferLength];
            sprintf(proxy, "%s:%d", host, (int)port);
            return SPUnityHardwareCreateString(proxy);
        }
    }

    return SPUnityHardwareCreateString(host);
}

EXPORT_API char* SPUnityHardwareGetNetworkIpAddress()
{
    struct ifaddrs* interfaces = NULL;
    struct ifaddrs* temp_addr = NULL;
    int success = 0;
    char* addr = NULL;
    success = getifaddrs(&interfaces);
    if (success == 0)
    {
        temp_addr = interfaces;
        while(temp_addr != NULL)
        {
            if(temp_addr->ifa_addr->sa_family == AF_INET)
            {
                if(strcmp(temp_addr->ifa_name, "en0") == 0)
                {
                    addr = inet_ntoa(((struct sockaddr_in *)temp_addr->ifa_addr)->sin_addr);
                    break;
                }
            }
            temp_addr = temp_addr->ifa_next;
        }
    }
    freeifaddrs(interfaces);
    if(addr == NULL)
    {
        return SPUnityHardwareCreateString("");
    }
    return SPUnityHardwareCreateString(addr);
}

uint64_t SPUnityHardwareGetStorageInfo(NSString* key)
{
    uint64_t value = 0;
    NSError* error = nil;
    NSArray* paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSDictionary* dictionary = [[NSFileManager defaultManager] attributesOfFileSystemForPath:[paths lastObject] error: &error];
    
    if (dictionary)
    {
        NSNumber* fileSystemSizeInBytes = [dictionary objectForKey: key];
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
