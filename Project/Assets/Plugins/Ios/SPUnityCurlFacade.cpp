#include "SPUnityCurlFacade.h"
#include "SPUnityCurlManager.h"
#include <list>
#include <vector>
#include <string>
#include <cassert>
#include "pthread/pthread.h"

extern "C" {
#include "curl/curl.h"
}

pthread_mutex_t curlUpdateLock;
bool curlUpdateLockInitialized = false;

std::vector<std::string> split(const std::string& str, const std::string& sep, size_t max=std::string::npos)
{
    std::vector<std::string> tokens;
    if(max == std::string::npos)
    {
        max = str.size();
    }
    std::string::size_type spos = 0;
    while(spos <= max)
    {
        std::string::size_type epos = str.find(sep, spos);
        if(epos == std::string::npos)
        {
            tokens.push_back(str.substr(spos));
            break;
        }
        else
        {
            tokens.push_back(str.substr(spos, epos-spos));
        }
        spos = epos+sep.size();
    }
    return tokens;
}

SPUnityCurlGlobalInfo globalInfo;

int SPUnityCurlRunning()
{
    return globalInfo.still_running;
}

const int SPUnityCurlGetHttpResponseErrorCode(int code)
{
    switch(code)
    {
        case CURLE_URL_MALFORMAT:
        case CURLE_UNSUPPORTED_PROTOCOL:
        case CURLE_FAILED_INIT:
            return 400;
        case CURLE_COULDNT_RESOLVE_PROXY:
        case CURLE_COULDNT_RESOLVE_HOST:
        case CURLE_COULDNT_CONNECT:
        case CURLE_REMOTE_ACCESS_DENIED:
            return 475;
        case CURLE_HTTP_RETURNED_ERROR:
        case CURLE_TOO_MANY_REDIRECTS:
        case CURLE_OUT_OF_MEMORY:
        case CURLE_REMOTE_FILE_EXISTS:
        case CURLE_REMOTE_DISK_FULL:
        case CURLE_RECV_ERROR:
        case CURLE_GOT_NOTHING:
            return 474;
        case CURLE_SSL_ENGINE_NOTFOUND:
        case CURLE_SSL_CERTPROBLEM:
        case CURLE_SSL_CIPHER:
        case CURLE_SSL_CACERT:
        case CURLE_SSL_ENGINE_SETFAILED:
        case CURLE_USE_SSL_FAILED:
        case CURLE_SSL_ENGINE_INITFAILED:
            return 476;
        case CURLE_OPERATION_TIMEDOUT:
            return 408;
        case CURLE_ABORTED_BY_CALLBACK:
            return 409;
        default:
            return 500;
    }
}
size_t writeToString(void *contents, size_t size, size_t nmemb, void *userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}

CURL* SPUnityCurlCreate(SPUnityCurlRequestStruct req)
{
    CURL* curl = curl_easy_init();
    if(!curl)
    {
        return curl;
    }
    std::string url = (std::string(req.url)+"?"+ std::string(req.query)).c_str() ;

    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0L);
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0L);

    curl_easy_setopt(curl, CURLOPT_NOSIGNAL, 1);
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1);
    curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 10);

    if (strcmp(req.method , "GET") == 0) //method
    {
        curl_easy_setopt(curl, CURLOPT_HTTPGET, 1);
    }
    else if (strcmp(req.method , "POST") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_POST, 1);
    }
    else if (strcmp(req.method , "PUT") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "PUT");
    }
    else if (strcmp(req.method , "DELETE") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "DELETE");
    }
    else if (strcmp(req.method , "HEAD") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_NOBODY, 1);
    }
    else
    {
        assert(false);
        curl_easy_setopt(curl, CURLOPT_HTTPGET, 1);
    }

    if(req.timeout>0)
    {
        curl_easy_setopt(curl, CURLOPT_CONNECTTIMEOUT, req.activityTimeout);
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, req.timeout);
    }

    if(req.proxy != NULL && strlen(req.proxy)>0)
    {
        curl_easy_setopt(curl, CURLOPT_PROXY, req.proxy);
        curl_easy_setopt(curl, CURLOPT_PROXYTYPE, CURLPROXY_HTTP);
    }

    if (req.bodyLength > 0)
    {
        curl_easy_setopt(curl, CURLOPT_POSTFIELDS, req.body);
        curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, req.bodyLength);
    }

    if (req.headers != NULL)
    {
        std::vector<std::string> headersData = split(req.headers,"\n");
        curl_slist *headers = NULL;
        for( auto itr = headersData.begin(); itr != headersData.end(); ++itr )
        {
            headers = curl_slist_append(headers, itr->c_str());
        }
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
    }
    return curl;
}

#ifdef SP_UNITY_CURL_DEBUG
#include <fstream>
#endif

EXPORT_API int SPUnityCurlSend(SPUnityCurlRequestStruct req)
{
#ifdef SP_UNITY_CURL_DEBUG
    std::ofstream ss;
    ss.open("/tmp/sp_unity_curl.log");
    ss << "request" << std::endl;
    ss << "id " << req.id << std::endl;
    ss << "url " << req.url << std::endl;
    ss << "query  " << req.query << std::endl;
    ss << "method " << req.method << std::endl;
    ss << "timeout " << req.timeout << std::endl;
    ss << "activityTimeout " << req.activityTimeout << std::endl;
    ss << "proxy " << req.proxy << std::endl;
    ss << "headers " << req.headers << std::endl;
    ss << "bodyLength " << req.bodyLength << std::endl;
    ss.close();
#endif
    
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(req.id);
    if (conn == NULL)
    {
        return 0;
    }
    
    conn->easy = SPUnityCurlCreate(req);
    curl_easy_setopt(conn->easy, CURLOPT_HEADERFUNCTION, writeToString);
    curl_easy_setopt(conn->easy, CURLOPT_HEADERDATA, &conn->headersBuffer);
    curl_easy_setopt(conn->easy, CURLOPT_WRITEFUNCTION, writeToString);
    curl_easy_setopt(conn->easy, CURLOPT_WRITEDATA, &conn->bodyBuffer);
    curl_easy_setopt(conn->easy, CURLOPT_PRIVATE, conn);
    if(!conn->easy)
    {
        return 0;
    }
    conn->bodyBuffer.clear();
    conn->headersBuffer.clear();
    CURLMcode rc = curl_multi_add_handle(globalInfo.multi, conn->easy);

    if (rc != CURLM_OK)
    {
        conn->errorBuffer = curl_multi_strerror(rc);
        return 0;
    }
    globalInfo.still_running++;
    return 1;
}

EXPORT_API int SPUnityCurlUpdate(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if(id != 0 && (conn == NULL || conn->responseCode != 0))
    {
        return 1;
    }

    if (!curlUpdateLockInitialized)
    {
        pthread_mutex_init(&curlUpdateLock, NULL);
        curlUpdateLockInitialized = true;
    }

    pthread_mutex_lock(&curlUpdateLock);
    curl_multi_perform(globalInfo.multi, &globalInfo.still_running);

    int msgs_left;
    int finished = 0;

    while(auto res_msg = curl_multi_info_read(globalInfo.multi, &msgs_left))
    {
        CURL* easy = res_msg->easy_handle;
        CURLMsg *msg = (CURLMsg*)res_msg;

        if(msg->msg == CURLMSG_DONE)
        {
            curl_easy_getinfo(easy, CURLINFO_PRIVATE, &conn);
            if(msg->data.result != CURLE_OK)
            {
                conn->errorBuffer = curl_easy_strerror(msg->data.result);
                conn->responseCode = SPUnityCurlGetHttpResponseErrorCode(msg->data.result);
            }
            else
            {
                long code = 0;
                curl_easy_getinfo(easy, CURLINFO_RESPONSE_CODE, &code);
                conn->responseCode = (int)code;
            }

            curl_easy_getinfo(easy, CURLINFO_SIZE_DOWNLOAD, &conn->downloadSize);
            curl_easy_getinfo(easy, CURLINFO_SPEED_DOWNLOAD, &conn->downloadSpeed);

            curl_multi_remove_handle(globalInfo.multi, easy);
            curl_easy_cleanup(easy);
            if(conn->id == id)
            {
                finished = 1;
            }
        }
    }

    pthread_mutex_unlock(&curlUpdateLock);
    return finished;
}

static int counterConns = 1;
EXPORT_API int SPUnityCurlCreateConn()
{
    int id = counterConns; // generate new Id
    counterConns = (counterConns +1) % LONG_MAX;
    SPUnityCurlManager::getInstance().addConn(id);
    return id;
}

EXPORT_API void SPUnityCurlDestroyConn(int id)
{
    SPUnityCurlManager::getInstance().removeConn(id); // remove conn to manager
}

EXPORT_API int SPUnityCurlGetCode(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return conn->responseCode;
    }
    return 0;
}

EXPORT_API void SPUnityCurlGetError(int id, char* data)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        memcpy(data, conn->errorBuffer.c_str(), conn->errorBuffer.length()*sizeof(char));
    }
}

EXPORT_API void SPUnityCurlGetBody(int id, char* data)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        memcpy(data, conn->bodyBuffer.c_str(), conn->bodyBuffer.length()*sizeof(char));
    }
}

EXPORT_API void SPUnityCurlGetHeaders(int id, char* data)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        memcpy(data, conn->headersBuffer.c_str(), conn->headersBuffer.length()*sizeof(char));
    }
}

EXPORT_API int SPUnityCurlGetErrorLength(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return (int)conn->errorBuffer.length();
    }
    return 0;
}

EXPORT_API int SPUnityCurlGetBodyLength(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return (int)conn->bodyBuffer.length();
    }
    return 0;
}

EXPORT_API int SPUnityCurlGetHeadersLength(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return (int)conn->headersBuffer.length();
    }
    return 0;
}

EXPORT_API int SPUnityCurlGetDownloadSize(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return conn->downloadSize;
    }
    return 0;
}

EXPORT_API int SPUnityCurlGetDownloadSpeed(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return conn->downloadSpeed;
    }
    return 0;
}

EXPORT_API void SPUnityCurlInit()
{
    memset(&globalInfo, 0, sizeof(SPUnityCurlGlobalInfo));
    globalInfo.multi =  curl_multi_init();
}

EXPORT_API void SPUnityCurlDestroy()
{
    if(globalInfo.multi)
    {
        curl_multi_cleanup(globalInfo.multi);
        globalInfo.multi = NULL;
    }
}
