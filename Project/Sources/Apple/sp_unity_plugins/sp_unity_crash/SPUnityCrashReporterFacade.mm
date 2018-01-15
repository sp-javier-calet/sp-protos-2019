
#include "SPUnityCrashReporterFacade.h"
#import <UIKit/UIKit.h>
#if !TARGET_OS_TV
#include "CrashReporter.h"
#endif
#include <string>
#import <Foundation/Foundation.h>
#include "BreadcrumbManager.hpp"

namespace {

    class SPUnityCrashReporterBridge
    {
      private:
        bool _enabled;
        std::string _crashDirectory;
        std::string _version;
        std::string _error;
        std::string _fileSeparator;
        std::string _crashExtension;
        SPUnityCrashReporterCallback _callback;
        BreadcrumbManager& _breadcrumbManager;

        static void onCrash(siginfo_t* info, ucontext_t* uap, void* context)
        {
            SPUnityCrashReporterBridge* crashReporter = (SPUnityCrashReporterBridge*)context;
            crashReporter->check();
        }

    #if !TARGET_OS_TV
        void setCrashCallback()
        {
            PLCrashReporterCallbacks* callbacks = new PLCrashReporterCallbacks();
            callbacks->version = 0;
            callbacks->context = this;
            callbacks->handleSignal = &onCrash;
            PLCrashReporter* reporter = [PLCrashReporter sharedReporter];
            [reporter setCrashCallbacks:callbacks];
        }

        void dumpCrash(PLCrashReport* plReport)
        {
            NSString* crashData = [PLCrashReportTextFormatter stringValueForCrashReport:plReport withTextFormat:PLCrashReportTextFormatiOS];
            if(_enabled && crashData)
            {
                NSString* timestamp = [NSString stringWithFormat:@"%ld", (time_t)round(plReport.systemInfo.timestamp.timeIntervalSince1970)];
                std::string filePath = _crashDirectory + [timestamp UTF8String] + _fileSeparator + _version + _crashExtension;
                [crashData writeToFile:[[NSString alloc] initWithUTF8String:filePath.c_str()] atomically:YES encoding:NSUTF8StringEncoding error:nil];

                if(_callback != nullptr)
                {
                    _callback(filePath.c_str());
                }
            }
        }

        void dumpBreadcrumbs()
        {
            _breadcrumbManager.dumpToFile();
        }

        bool initializePLCrashReporter()
        {
            static bool crashReporterInitialized = false;

            if(!crashReporterInitialized)
            {
                PLCrashReporter* reporter = [PLCrashReporter sharedReporter];
                NSError* error = nil;

                // Callback must be set before enable crash reporter
                setCrashCallback();

                [reporter enableCrashReporterAndReturnError:&error];

                if(error)
                {
                    _error = [error.localizedDescription UTF8String];
                }

                crashReporterInitialized = true;
            }
            return _error.empty();
        }
    #endif

      public:

        SPUnityCrashReporterBridge()
        : _enabled(false)
        , _breadcrumbManager(BreadcrumbManager::getInstance())
        {
        }

        void setConfig(const std::string& path, const std::string& version, const std::string& fileSeparator, const std::string& crashExtension, SPUnityCrashReporterCallback callback)
        {
            _crashDirectory = path;
            _version = version;
            _fileSeparator = fileSeparator;
            _crashExtension = crashExtension;
            _callback = callback;
        }

        bool enable()
        {
    #if !TARGET_OS_TV
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
    #if !TARGET_OS_TV
            PLCrashReporter* reporter = [PLCrashReporter sharedReporter];

            if(![reporter hasPendingCrashReport])
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

            dumpBreadcrumbs();
            dumpCrash(plReport);

            return true;
    #else
            return false;
    #endif
        }
    };
    
    static SPUnityCrashReporterBridge* _bridge = nullptr;
}

EXPORT_API void SPUnityCrashReporter_Create(const char* path, const char* version, const char* fileSeparator, const char* crashExtension,
                                                 const char* logExtension, SPUnityCrashReporterCallback callback)
{
    if(_bridge == nullptr)
    {
        _bridge = new SPUnityCrashReporterBridge();
    }
    _bridge->setConfig(std::string(path), std::string(version), std::string(fileSeparator), std::string(crashExtension), callback);
}

EXPORT_API void SPUnityCrashReporter_Enable()
{
    if(_bridge != nullptr)
    {
        _bridge->enable();
    }
}

EXPORT_API void SPUnityCrashReporter_Disable()
{
    if(_bridge != nullptr)
    {
        _bridge->disable();
    }
}

EXPORT_API void SPUnityCrashReporter_ForceCrash()
{
    *((volatile unsigned int*)0) = 0xDEAD;
}
