#ifndef __SPUnityCurlManager__
#define __SPUnityCurlManager__

#include <map>
#include <string>

extern "C" {
#include "curl/curl.h"
}

/* Global information, common to all connections */
struct SPUnityCurlGlobalInfo
{
    CURLM* multi;
    int still_running;

    SPUnityCurlGlobalInfo();
};

/* Information associated with a specific easy handle */
struct SPUnityCurlConnInfo
{
    intptr_t id;
    CURL* easy;
    intptr_t responseCode;
    std::string bodyBuffer;
    std::string headersBuffer;
    std::string errorBuffer;
    double downloadSize;
    double downloadSpeed;

    SPUnityCurlConnInfo(intptr_t id);
};

class SPUnityCurlManager
{
    typedef SPUnityCurlConnInfo ConnInfo;
    typedef SPUnityCurlGlobalInfo GlobalInfo;
    typedef std::map<intptr_t, ConnInfo*> MClients;

    MClients _clients;

private:

    SPUnityCurlManager();

public:

    static SPUnityCurlManager& getInstance();
    void addConn(intptr_t id);
    ConnInfo* getConnById(intptr_t id);
    bool removeConn(intptr_t id);
};

#endif
