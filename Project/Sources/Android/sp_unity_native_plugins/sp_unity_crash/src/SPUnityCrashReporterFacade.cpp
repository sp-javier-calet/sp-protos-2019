#include "SPUnityCrashReporterFacade.h"
#include "BreadcrumbManager.hpp"
#include "SPUnityFileUtils.hpp"
#include <stdlib.h>
#include <sstream>
#include <cassert>
#include <chrono>
#include <ctime>
#include <pthread.h>
#include <android/log.h>

#if defined(__arm__)
/* google_breakpad is only supported in arm architectures
 * SPUnityCrashReporter cannot be enabled in x86 builds.
 */

    #include "client/linux/handler/exception_handler.h" // inclusion of linux header as told in README.ANDROID from google-breakpad
    #include "client/linux/handler/minidump_descriptor.h"
#else
    namespace google_breakpad
    {
        struct ExceptionHandler { };
    }
#endif

#define LOG_TAG "Unity"
#define  LogError(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

namespace {
    class SPUnityCrashReporterBridge
    {
    private:
        std::string _crashDirectory;
        std::string _version;
        std::string _error;
        std::string _fileSeparator;
        std::string _crashExtension;
        std::string _logExtension;
        SPUnityCrashReporterCallback _callback;
        BreadcrumbManager& _breadcrumbManager;

        google_breakpad::ExceptionHandler* _exceptionHandler;

        static void* callOnCrashDumpedThread(void *ctx)
        {
            CrashDumpedCallData* data = (CrashDumpedCallData*)ctx;
            LogError("OnCrashDumped");
            if(data->callback != nullptr)
            {
                data->callback(data->logPath.c_str());
            }
            delete data;
            return nullptr;
        }


#if defined(__arm__)
        static bool onCrash(const google_breakpad::MinidumpDescriptor& descriptor,
                          void* context,
                          bool succeeded)
        {
            if(context)
            {
                SPUnityCrashReporterBridge* crashReporter = static_cast<SPUnityCrashReporterBridge*>(context);
                crashReporter->dumpBreadcrumbs();
                crashReporter->dumpCrash(descriptor.path());
            }

            return false;
        }
#endif

    public:
        SPUnityCrashReporterBridge()
        : _exceptionHandler(nullptr)
        , _breadcrumbManager(BreadcrumbManager::getInstance())
        {
        }

        void setConfig(const std::string& path, const std::string& version, const std::string& fileSeparator, const std::string& crashExtension, const std::string& logExtension, SPUnityCrashReporterCallback callback)
        {
            _crashDirectory = path;
            _version = version;
            _fileSeparator = fileSeparator;
            _crashExtension = crashExtension;
            _logExtension = logExtension;
            _callback = callback;
        }

        bool enable()
        {
        #if defined(__arm__)
            if(!_exceptionHandler)
            {
                google_breakpad::MinidumpDescriptor descriptor(_crashDirectory);
                _exceptionHandler = new google_breakpad::ExceptionHandler(descriptor, NULL, SPUnityCrashReporterBridge::onCrash, this, true, -1);
            }
        #endif

            // Clear logcat
            std::string logcatCmd("logcat -c");
            system(logcatCmd.c_str());

            return _exceptionHandler != nullptr;
        }

        bool disable()
        {
            delete _exceptionHandler;
            _exceptionHandler = nullptr;
            return true;
        }

        struct CrashDumpedCallData
        {
            std::string logPath;
            SPUnityCrashReporterCallback callback;
        };

        void dumpCrash(const std::string& crashPath)
        {
            std::time_t epoch_time = std::chrono::system_clock::to_time_t(std::chrono::system_clock::now());

            // Convert to local time
            epoch_time = std::mktime(std::localtime(&epoch_time));

            std::stringstream ss;
            ss << epoch_time;
            std::string timestamp = ss.str();

            std::string filePath    (_crashDirectory + timestamp + _fileSeparator + _version);
            std::string newCrashPath(filePath + _crashExtension);
            std::string newLogPath  (filePath + _logExtension);

            // Move dmp file
            rename(crashPath.c_str(), newCrashPath.c_str());

            // Dump logcat
            std::string logcatCmd("logcat -d -t 200 -f " + newLogPath);
            system(logcatCmd.c_str());

            pthread_t thread;
            pthread_create(&thread, NULL, callOnCrashDumpedThread,
                new CrashDumpedCallData{ newCrashPath, _callback });
        }

        void dumpBreadcrumbs()
        {
            _breadcrumbManager.dumpToFile();
        }
    };

    static SPUnityCrashReporterBridge* _bridge;
}

EXPORT_API void SPUnityCrashReporter_Create(const char* crashPath,
                                                  const char* version,
                                                  const char* fileSeparator,
                                                  const char* crashExtension,
                                                  const char* logExtension,
                                                  SPUnityCrashReporterCallback callback)
{
    if(_bridge == nullptr)
    {
        _bridge = new SPUnityCrashReporterBridge();
    }
    _bridge->setConfig(std::string(crashPath),
                        std::string(version),
                        std::string(fileSeparator),
                        std::string(crashExtension),
                        std::string(logExtension),
                        callback);
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
    unsigned int* invalidAddress = 0;
    *(invalidAddress) = 0xDEAD;
}
