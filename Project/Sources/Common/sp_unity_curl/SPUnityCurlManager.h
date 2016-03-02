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
    int id;
    CURL* easy;
    int responseCode;
    int errorCode;
    std::string bodyBuffer;
    std::string headersBuffer;
    std::string errorBuffer;
    double downloadSize;
    double downloadSpeed;
    double connectTime;
    double totalTime;

    SPUnityCurlConnInfo(int id);
};

class SPUnityCurlManager
{
    typedef SPUnityCurlConnInfo ConnInfo;
    typedef SPUnityCurlGlobalInfo GlobalInfo;
    typedef std::map<int, ConnInfo*> MClients;

    MClients _clients;

private:

    SPUnityCurlManager();

public:

    static SPUnityCurlManager& getInstance();
    void addConn(int id);
    ConnInfo* getConnById(int id);
    bool removeConn(int id);
};

#endif
