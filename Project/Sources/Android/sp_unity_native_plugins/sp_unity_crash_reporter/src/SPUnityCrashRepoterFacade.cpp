#include "SPUnityCrashReporter.hpp"

/*
 * Exported interface
 */
extern "C"{
    SPUnityCrashReporter* SPUnityCrashReporterCreate(const char* crashPath,
                                                      const char* version,
                                                      const char* fileSeparator,
                                                      const char* crashExtension,
                                                      const char* logExtension)
    {
        return new SPUnityCrashReporter(std::string(crashPath),
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
        unsigned int* invalidAddress = 0;
        *(invalidAddress) = 0xDEAD;
    }
}
