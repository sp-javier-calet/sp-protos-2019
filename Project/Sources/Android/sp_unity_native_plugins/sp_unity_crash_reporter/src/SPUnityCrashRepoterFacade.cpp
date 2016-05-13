#include "SPUnityCrashReporter.hpp"

/*
 * Exported interface
 */
extern "C"{
    SPUnityCrashReporter* SPUnityCrashReporterCreate(const char* crashPath, 
                                                      const char* version,
                                                      const char* fileSeparator, 
                                                      const char* crashExtension,
                                                      const char* logExtension, 
                                                      const char* breadcrumbPath, 
                                                      const char* breadcrumbFile, 
                                                      const char* gameObject)
    {
        return new SPUnityCrashReporter(std::string(crashPath),
                                        std::string(version),
                                        std::string(fileSeparator),
                                        std::string(crashExtension),
                                        std::string(logExtension),
                                        std::string(breadcrumbPath),
                                        std::string(breadcrumbFile),
                                        std::string(gameObject));
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

    //*** TEST
    void SPUnityCrashReporterDebug(SPUnityCrashReporter* crashReporter)
    {
        crashReporter->debug();
    }
}
