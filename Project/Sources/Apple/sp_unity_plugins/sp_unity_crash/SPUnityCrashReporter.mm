
#import <UIKit/UIKit.h>
#if !UNITY_TVOS
#include "CrashReporter.h"
#endif
#include <string>
#import <Foundation/Foundation.h>


class SPUnityCrashReporter
{
private:
    bool _enabled;
    std::string _crashDirectory;
    std::string _version;
    std::string _error;
    std::string _fileSeparator;
    std::string _crashExtension;
    
    static void onCrash(siginfo_t *info, ucontext_t *uap, void *context)
    {
        SPUnityCrashReporter* crashReporter = (SPUnityCrashReporter*) context;
        crashReporter->check();
    }
    
#if !UNITY_TVOS
    void setCrashCallback()
    {
        PLCrashReporterCallbacks* callbacks = new PLCrashReporterCallbacks();
        callbacks->version = 0;
        callbacks->context = this;
        callbacks->handleSignal = &onCrash;
        PLCrashReporter *reporter = [PLCrashReporter sharedReporter];
        [reporter setCrashCallbacks:callbacks];
    }
    
    void dumpCrash(PLCrashReport* plReport)
    {
        NSString* crashData = [PLCrashReportTextFormatter stringValueForCrashReport:plReport withTextFormat:PLCrashReportTextFormatiOS];
        if(_enabled && crashData)
        {
            NSString* timestamp = [NSString stringWithFormat:@"%ld", (time_t)round(plReport.systemInfo.timestamp.timeIntervalSince1970)];
            std::string filePath = _crashDirectory + [timestamp UTF8String] + _fileSeparator +
            _version + _crashExtension;
            [crashData writeToFile:[[NSString alloc] initWithUTF8String:filePath.c_str()]
                        atomically:YES encoding:NSUTF8StringEncoding error:nil];
        }
    }
    
    bool initializePLCrashReporter()
    {
        static bool crashReporterInitialized = false;
        
        if(!crashReporterInitialized)
        {
            PLCrashReporter *reporter = [PLCrashReporter sharedReporter];
            NSError* error = nil;
            
            // Callback must be set before enable crash reporter
            setCrashCallback();
            
            [reporter enableCrashReporterAndReturnError: &error];
            
            if (error)
            {
                _error = [error.localizedDescription UTF8String];
            }
            
            crashReporterInitialized = true;
        }
        return _error.empty();
    }
#endif
    
public:
    
    static SPUnityCrashReporter* getInstance()
    {
        static SPUnityCrashReporter instance;
        return &instance;
    }
    
    SPUnityCrashReporter()
    : _enabled(false)
    {
    }
    
    void setConfig(const std::string& path,
                   const std::string& version,
                   const std::string& fileSeparator,
                   const std::string& crashExtension)
    {
        _crashDirectory = path;
        _version = version;
        _fileSeparator = fileSeparator;
        _crashExtension = crashExtension;
    }
    
    bool enable()
    {
#if !UNITY_TVOS
        if(!_enabled)
        {
            _enabled = initializePLCrashReporter();
        }
        return _enabled;
#else
        return false;
#endif
    }
    
    bool disable()
    {
        /* PLCrashReporter cannot be actually disabled, but we can
         * avoid dumping crashes after disabling
         */
        _enabled = false;
        
        return true;
    }
    
    /*
     * Check for pending crashes and dump them to a file if necessary.
     * @return true if there is any pending crash
     */
    bool check()
    {
#if !UNITY_TVOS
        PLCrashReporter *reporter = [PLCrashReporter sharedReporter];
        
        if (![reporter hasPendingCrashReport])
        {
            return false;
        }
        
        NSError* error = nil;
        NSData* data = [reporter loadPendingCrashReportDataAndReturnError:&error];
        
        if(error)
        {
            [reporter purgePendingCrashReport];
            return true;
        }
        
        error = nil;
        PLCrashReport* plReport = [[PLCrashReport alloc] initWithData:data error:&error];
        
        if(error)
        {
            [reporter purgePendingCrashReport];
            return true;
        }
        
        [reporter purgePendingCrashReport];
        
        dumpCrash(plReport);
        
        return true;
#else
        return false;
#endif
    }
    
};


/*
 * Exported interface
 */
extern "C" {
    SPUnityCrashReporter* native_crashReporter_create(const char* path, const char* version,
                                                      const char* fileSeparator, const char* crashExtension,
                                                      const char* logExtension)
    {
        SPUnityCrashReporter* reporterInstance = SPUnityCrashReporter::getInstance();
        reporterInstance->setConfig(std::string(path), std::string(version),
                                    std::string(fileSeparator), std::string(crashExtension));
        
        return reporterInstance;
    }
    
    void native_crashReporter_enable(SPUnityCrashReporter* crashReporter)
    {
        crashReporter->enable();
    }
    
    void native_crashReporter_disable(SPUnityCrashReporter* crashReporter)
    {
        crashReporter->disable();
    }
    
    void native_crashReporter_destroy(SPUnityCrashReporter* crashReporter)
    {
        delete crashReporter;
    }
    
    void native_crashReporter_forceCrash()
    {
        *((unsigned int*)0) = 0xDEAD;
    }
}
