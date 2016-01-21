#include "SPUnityCurlFacade.h"
#include "SPUnityCurlManager.h"
#include <list>
#include <vector>
#include <string>
#include <cassert>

extern "C" {
#include "curl/curl.h"
}

#if defined(WIN32) || defined(WIN64)
    #include <mutex>
    typedef std::Mutex Mutex;
#else
    #include "pthread.h"
    class Mutex
    {
    private:
        pthread_mutex_t _mutex;
        bool _initialized;

    public:
        Mutex()
        : _initialized(false)
        {
        }

        void lock()
        {
            if(!_initialized)
            {
                _initialized = true;
                pthread_mutex_init(&_mutex, NULL);
            }
            pthread_mutex_lock(&_mutex);
        }

        void unlock()
        {
            pthread_mutex_unlock(&_mutex);
        }
    };
#endif

Mutex curlUpdateLock;
const uint8_t* curlPinnedPublicKey = nullptr;
size_t curlPinnedPublicKeySize = 0;
const char* curlConfig = nullptr;

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
        case CURLE_RECV_ERROR:
        case CURLE_SEND_ERROR:
        case CURLE_HTTP_RETURNED_ERROR:
        case CURLE_TOO_MANY_REDIRECTS:
        case CURLE_REMOTE_FILE_EXISTS:
        case CURLE_REMOTE_DISK_FULL:
        case CURLE_GOT_NOTHING:
            return 475;
        case CURLE_SSL_ENGINE_NOTFOUND:
        case CURLE_SSL_CERTPROBLEM:
        case CURLE_SSL_CIPHER:
        case CURLE_SSL_CACERT:
        case CURLE_SSL_ENGINE_SETFAILED:
        case CURLE_USE_SSL_FAILED:
        case CURLE_SSL_ENGINE_INITFAILED:
        case CURLE_SSL_PINNEDPUBKEYNOTMATCH:
            return 476;
        case CURLE_OPERATION_TIMEDOUT:
            return 408;
        case CURLE_ABORTED_BY_CALLBACK:
            return 409;
        default:
            return 470;
    }
}

size_t writeToString(void *contents, size_t size, size_t nmemb, void *userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}

void obfuscate(const uint8_t* in, uint8_t** out, size_t size)
{
    static const int secretLength = 8;
    static const uint8_t secret[secretLength] = {55, 11, 44, 71, 66, 177, 253, 122};

    (*out) = new uint8_t[size + 1];// size + null terminated char

    for(size_t i = 0; i < size; ++i)
    {
        (*out)[i] = in[i] ^ secret[i % secretLength];
    }

    (*out)[size] = 0;
}


static const uint8_t pinnedCertBaseGame[] = {
        0x44, 0x63, 0x4D, 0x75, 0x77, 0x87, 0xD2, 0x55, 0x65, 0x49,
        0x5E, 0x04, 0x2B, 0xF3, 0xBB, 0x1B, 0x75, 0x5E, 0x07, 0x73,
        0x70, 0xFD, 0x99, 0x23, 0x47, 0x6E, 0x4F, 0x33, 0x25, 0xFB,
        0x99, 0x23, 0x55, 0x53, 0x62, 0x71, 0x35, 0xF0, 0xD6, 0x0D,
        0x66, 0x20, 0x45, 0x71, 0x07, 0xF3, 0xBF, 0x51, 0x46, 0x7C,
        0x6D, 0x7A
    };

// sha256///sdIuK4VrbGBsCFHmQJFCTnWERJZe8gNXphH3gcS7v8=
static const uint8_t pinnedCertDragonLand[] = {
        0x44, 0x63, 0x4D, 0x75, 0x77, 0x87, 0xD2, 0x55, 0x18, 0x78,
        0x48, 0x0E, 0x37, 0xFA, 0xC9, 0x2C, 0x45, 0x69, 0x6B, 0x05,
        0x31, 0xF2, 0xBB, 0x32, 0x5A, 0x5A, 0x66, 0x01, 0x01, 0xE5,
        0x93, 0x2D, 0x72, 0x59, 0x66, 0x1D, 0x27, 0x89, 0x9A, 0x34,
        0x6F, 0x7B, 0x44, 0x0F, 0x71, 0xD6, 0x9E, 0x29, 0x00, 0x7D,
        0x14, 0x7A
    };

// sha256//syRv+CQwrcHdmCVoxIwgl0RIipmnOw6oxPnYVFD1iUw=
static const uint8_t pinnedCertDragonStadium[] = {
        0x44, 0x63, 0x4D, 0x75, 0x77, 0x87, 0xD2, 0x55, 0x44, 0x72,
        0x7E, 0x31, 0x69, 0xF2, 0xAC, 0x0D, 0x45, 0x68, 0x64, 0x23,
        0x2F, 0xF2, 0xAB, 0x15, 0x4F, 0x42, 0x5B, 0x20, 0x2E, 0x81,
        0xAF, 0x33, 0x5E, 0x7B, 0x41, 0x29, 0x0D, 0xC6, 0xCB, 0x15,
        0x4F, 0x5B, 0x42, 0x1E, 0x14, 0xF7, 0xB9, 0x4B, 0x5E, 0x5E,
        0x5B, 0x7A
    };

void SPUnityCurlSetupConfig(CURL* curl)
{
    if(curlConfig == "dragonland")
    {
        curlPinnedPublicKey = pinnedCertDragonLand;
        curlPinnedPublicKeySize = sizeof(pinnedCertDragonLand);
    }
    else if(curlConfig == "dragonstadium")
    {
        curlPinnedPublicKey = pinnedCertDragonStadium;
        curlPinnedPublicKeySize = sizeof(pinnedCertDragonStadium);
    }
    else
    {
        curlPinnedPublicKey = pinnedCertBaseGame;
        curlPinnedPublicKeySize = sizeof(pinnedCertBaseGame);
    }
}

CURL* SPUnityCurlCreate(SPUnityCurlRequestStruct req)
{
    CURL* curl = curl_easy_init();
    if(!curl)
    {
        return curl;
    }

    SPUnityCurlSetupConfig(curl);

    std::string url = (std::string(req.url)+"?"+ std::string(req.query)).c_str() ;

    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());

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

    if(curlPinnedPublicKey)
    {
        uint8_t* out = nullptr;
        obfuscate(curlPinnedPublicKey, &out, curlPinnedPublicKeySize);
        curl_easy_setopt(curl, CURLOPT_PINNEDPUBLICKEY, out);
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 2);
        delete[] out;
    }
    else
    {
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0);
    }
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0);

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

    // setting to print details about this
    // curl_easy_setopt(curl, CURLOPT_VERBOSE, true);

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
    if(req.proxy != nullptr)
    {
        ss << "proxy " << req.proxy << std::endl;
    }
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

    curlUpdateLock.lock();

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

            curl_easy_getinfo(easy, CURLINFO_CONNECT_TIME, &conn->connectTime);
            curl_easy_getinfo(easy, CURLINFO_TOTAL_TIME, &conn->totalTime);
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

    curlUpdateLock.unlock();
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

EXPORT_API double SPUnityCurlGetConnectTime(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return conn->connectTime;
    }
    return 0;
}

EXPORT_API double SPUnityCurlGetTotalTime(int id)
{
    SPUnityCurlConnInfo* conn = SPUnityCurlManager::getInstance().getConnById(id);
    if (conn)
    {
        return conn->totalTime;
    }
    return 0;
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

EXPORT_API void SPUnityCurlSetConfig(const char* name)
{
    curlConfig = name;
}
