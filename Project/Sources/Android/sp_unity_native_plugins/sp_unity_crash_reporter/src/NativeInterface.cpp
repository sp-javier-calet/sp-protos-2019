#include "SPUnityCrashReporter.hpp"

/* 
 * Exported interface 
 */
extern "C"{
    SPUnityCrashReporter* native_crashReporter_create(const char* path, const char* version,
                                                      const char* fileSeparator, const char* crashExtension,
                                                      const char* logExtension)
    {
        return new SPUnityCrashReporter(std::string(path), 
                                        std::string(version),
                                        std::string(fileSeparator), 
                                        std::string(crashExtension), 
                                        std::string(logExtension));
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