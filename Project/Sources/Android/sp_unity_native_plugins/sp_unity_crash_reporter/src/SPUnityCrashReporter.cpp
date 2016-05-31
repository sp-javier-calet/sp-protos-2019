#include <stdlib.h>
#include <sstream>
#include <cassert>
#include <chrono>
#include <ctime>
#include <pthread.h>
#include "UnityGameObject.h"
#include "SPUnityCrashReporter.hpp"
#include "SPUnityBreadcrumbManager.hpp"
#include "SPUnityFileUtils.hpp"
using namespace socialpoint;

/* google_breakpad is only supported in arm architectures
 * SPUnityCrashReporter cannot be enabled in x86 builds.
 */

#if defined(__arm__)
    #include "client/linux/handler/exception_handler.h" // inclusion of linux header as told in README.ANDROID from google-breakpad
    #include "client/linux/handler/minidump_descriptor.h"

    namespace
    {
        bool onCrash(const google_breakpad::MinidumpDescriptor& descriptor,
                          void* context,
                          bool succeeded)
        {
            if(context)
            {
                SPUnityCrashReporter* crashReporter = static_cast<SPUnityCrashReporter*>(context);
                crashReporter->dumpBreadcrumbs();
                crashReporter->dumpCrash(descriptor.path());
            }

            return false;
        }
    }
#else
    namespace google_breakpad
    {
        class ExceptionHandler
        {
        };
    }
#endif

SPUnityCrashReporter::SPUnityCrashReporter(const std::string& crashPath,
                                           const std::string& version,
                                           const std::string& fileSeparator,
                                           const std::string& crashExtension,
                                           const std::string& logExtension,
                                           const std::string& gameObject)
: _crashDirectory(crashPath)
, _version(version)
, _fileSeparator(fileSeparator)
, _crashExtension(crashExtension)
, _logExtension(logExtension)
, _gameObject(gameObject)
, _exceptionHandler(nullptr)
, _breadcrumbManager(socialpoint::SPUnityBreadcrumbManager::getInstance())
{
}

bool SPUnityCrashReporter::enable()
{
#if defined(__arm__)
    if(!_exceptionHandler)
    {
        google_breakpad::MinidumpDescriptor descriptor(_crashDirectory);
        _exceptionHandler = new google_breakpad::ExceptionHandler(descriptor, NULL, onCrash, this, true, -1);
    }
#endif

    // Clear logcat
    std::string logcatCmd("logcat -c");
    system(logcatCmd.c_str());

    return _exceptionHandler != nullptr;
}

bool SPUnityCrashReporter::disable()
{
    delete _exceptionHandler;
    _exceptionHandler = nullptr;
    return true;
}

struct CrashDumpedCallData
{
    std::string gameObject;
    std::string logPath;
};

void* callOnCrashDumpedThread(void *ctx)
{
    CrashDumpedCallData* data = (CrashDumpedCallData*)ctx;
    UnityGameObject(data->gameObject).SendMessage("OnCrashDumped", data->logPath);
    delete data;
    return nullptr;
}

//*** TEST
void* callOnDebugThread(void *ctx)
{
    CrashDumpedCallData* data = (CrashDumpedCallData*)ctx;
    UnityGameObject(data->gameObject).SendMessage("DebugLog", data->logPath);
    delete data;
    return nullptr;
}
//*** TEST
void DebugLog(const std::string& gameObject, const std::string& logMsg)
{
    if(!gameObject.empty())
    {
        pthread_t thread;
        pthread_create(&thread, NULL, callOnDebugThread,
            new CrashDumpedCallData{ gameObject, logMsg });
    }
}

void SPUnityCrashReporter::dumpCrash(const std::string& crashPath)
{
    //*** TEST
    DebugLog(_gameObject, "*** TEST Dumping Crash");

    std::time_t epoch_time = std::chrono::system_clock::to_time_t(std::chrono::system_clock::now());

    // Convert to local time
    epoch_time = std::mktime(std::localtime(&epoch_time));

    std::stringstream ss;
    ss << epoch_time;
    std::string timestamp = ss.str();

    std::string filePath    (_crashDirectory + timestamp + _fileSeparator + _version);
    std::string newCrashPath(filePath + _crashExtension);
    std::string newLogPath  (filePath + _logExtension);

    //*** TEST
    DebugLog(_gameObject, "*** TEST Creating crash file...");

    // Move dmp file
    rename(crashPath.c_str(), newCrashPath.c_str());

    //*** TEST
    DebugLog(_gameObject, "*** TEST Created");

    //*** TEST
    DebugLog(_gameObject, "*** TEST Creating log file...");

    // Dump logcat
    std::string logcatCmd("logcat -d -t 200 -f " + newLogPath);
    system(logcatCmd.c_str());

    //*** TEST
    DebugLog(_gameObject, "*** TEST Created");

    if(!_gameObject.empty())
    {
        pthread_t thread;
        pthread_create(&thread, NULL, callOnCrashDumpedThread,
            new CrashDumpedCallData{ _gameObject, newCrashPath });
    }
}

void SPUnityCrashReporter::dumpBreadcrumbs()
{
    _breadcrumbManager->dumpToFile();
}

//*** TEST
    void SPUnityCrashReporter::debug()
    {
        if(!_gameObject.empty())
        {
            UnityGameObject(_gameObject).SendMessage("DebugLog", "*** TEST DebugLog");
        }
    }
