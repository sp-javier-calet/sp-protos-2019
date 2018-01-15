#ifndef __SPUnityCrashReporter__
#define __SPUnityCrashReporter__

#include <string>

// Fordward declarations
namespace google_breakpad
{
    class ExceptionHandler;
}

class SPUnityBreadcrumbManager;

class SPUnityCrashReporter
{
private:
    std::string _crashDirectory;
    std::string _version;
    std::string _error;
    std::string _fileSeparator;
    std::string _crashExtension;
    std::string _logExtension;

    google_breakpad::ExceptionHandler* _exceptionHandler;

    SPUnityBreadcrumbManager& _breadcrumbManager;

public:
    SPUnityCrashReporter(const std::string& crashPath,
                    	 const std::string& version,
                     	 const std::string& fileSeparator,
                     	 const std::string& crashExtension,
                     	 const std::string& logExtension);

    bool enable();
    bool disable();
    void dumpCrash(const std::string& crashPath);
    const std::string& getCrashPaths() const;
    void clearCrashPaths();

    void dumpBreadcrumbs();
};

#endif
