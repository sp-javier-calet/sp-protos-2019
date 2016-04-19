#include "SPUnityCrashReporter.hpp"

/*
 * Exported interface
 */
extern "C"{
    SPUnityCrashReporter* SPUnityCrashReporterCreate(const char* path, const char* version,
                                                      const char* fileSeparator, const char* crashExtension,
                                                      const char* logExtension)
    {
        return new SPUnityCrashReporter(std::string(path),
                                        std::string(version),
                                        std::string(fileSeparator),
                                        std::string(crashExtension),
                                        std::string(logExtension));
    }

    void SPUnityCrashReporterEnable(SPUnityCrashReporter* crashReporter)
    {
        crashReporter->enable();
    }

    void SPUnityCrashReporterDisable(SPUnityCrashReporter* crashReporter)
    {
        crashReporter->disable();
    }

    void SPUnityCrashReporterDestroy(SPUnityCrashReporter* crashReporter)
    {
        delete crashReporter;
    }

    void SPUnityCrashReporterForceCrash()
    {
        *((unsigned int*)0) = 0xDEAD;
    }

    const char* SPUnityCrashReporterGetCrashPaths(SPUnityCrashReporter* crashReporter)
    {
        return crashReporter->getCrashPaths().c_str();
    }

    void SPUnityCrashReporterClearCrashPaths(SPUnityCrashReporter* crashReporter)
    {
        crashReporter->clearCrashPaths();
    }
}
