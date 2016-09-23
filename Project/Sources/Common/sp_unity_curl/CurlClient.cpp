//
//  CurlClient.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//
//

#include "CurlClient.hpp"
#include "Mutex.hpp"
#include "Certificates.hpp"

#include <string>
#include <map>
#include <vector>
#include <cassert>

extern "C" {
    #include "curl.h"
}

namespace
{
    std::vector<std::string> split(const std::string& str, const std::string& sep, size_t max = std::string::npos)
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
                tokens.push_back(str.substr(spos, epos - spos));
            }
            spos = epos + sep.size();
        }
        return tokens;
    }
    
    static size_t writeToString(void* contents, size_t size, size_t nmemb, void* userp)
    {
        ((std::string*)userp)->append((char*)contents, size * nmemb);
        size_t s = size * nmemb;
        
        return s;
    }
    
    static int server_push_callback(CURL* parent, CURL* easy, size_t num_headers, struct curl_pushheaders* headers, void* userp)
    {
        /* TODO:
         This feature is not tested yet. We should define its possible uses and retrieve the pushed data accordingly.
         */
        
        // SPNativeCallsSender::SendMessage("OnHttp2Push", "");
        return CURL_PUSH_OK;
    }
}

CurlClient::CurlClient(bool enableHttp2)
: _multi(nullptr)
, _connections(enableHttp2)
, _verbose(false)
, _running(0)
, _pinnedPublicKey(nullptr)
, _pinnedPublicKeySize(0)
{
    _multi = curl_multi_init();
    
    if(enableHttp2) // TODO Move to a configurer class
    {
        curl_multi_setopt(_multi, CURLMOPT_PIPELINING, CURLPIPE_MULTIPLEX);
        curl_multi_setopt(_multi, CURLMOPT_PUSHFUNCTION, server_push_callback);
        static int dummy = 0;// TODO: Remove, define and use other pointer for desired struct needed in callback.
        curl_multi_setopt(_multi, CURLMOPT_PUSHDATA, &dummy);
        /* We do HTTP/2 so let's stick to one connection per host */
        curl_multi_setopt(_multi, CURLMOPT_MAX_HOST_CONNECTIONS, 1L);
    }
}

CurlClient::~CurlClient()
{
    if(_multi)
    {
        // Clean all remaining easy
        int msgs_left;
        while(auto res_msg = curl_multi_info_read(_multi, &msgs_left))
        {
            CURL* easy = res_msg->easy_handle;
            curl_multi_remove_handle(_multi, easy);
            curl_easy_cleanup(easy);
        }
        
        curl_multi_cleanup(_multi);
        _multi = nullptr;
    }
}

CURL* CurlClient::create(CurlRequest req)
{
    CURL* curl = curl_easy_init();
    if(!curl)
    {
        return curl;
    }
    
    std::string url = (std::string(req.url) + "?" + std::string(req.query)).c_str();
    
    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
    
    curl_easy_setopt(curl, CURLOPT_NOSIGNAL, 1);
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1);
    curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 10);
    
    if(strcmp(req.method, "GET") == 0)// method
    {
        curl_easy_setopt(curl, CURLOPT_HTTPGET, 1);
    }
    else if(strcmp(req.method, "POST") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_POST, 1);
    }
    else if(strcmp(req.method, "PUT") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "PUT");
    }
    else if(strcmp(req.method, "DELETE") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "DELETE");
    }
    else if(strcmp(req.method, "HEAD") == 0)
    {
        curl_easy_setopt(curl, CURLOPT_NOBODY, 1);
    }
    else
    {
        assert(false);
        curl_easy_setopt(curl, CURLOPT_HTTPGET, 1);
    }
    
    if(req.timeout > 0)
    {
        curl_easy_setopt(curl, CURLOPT_CONNECTTIMEOUT, req.activityTimeout);
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, req.timeout);
    }
    
    if(req.proxy != nullptr && strlen(req.proxy) > 0)
    {
        curl_easy_setopt(curl, CURLOPT_PROXY, req.proxy);
        curl_easy_setopt(curl, CURLOPT_PROXYTYPE, CURLPROXY_HTTP);
    }
    
    uint8_t* pinnedKey = nullptr;
    if(_certificate.getPinnedKey(&pinnedKey))
    {
        curl_easy_setopt(curl, CURLOPT_PINNEDPUBLICKEY, pinnedKey);
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 2);
        delete[] pinnedKey;
    }
    else
    {
        curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0);
    }
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0);
    
    if(req.bodyLength > 0)
    {
        curl_easy_setopt(curl, CURLOPT_POSTFIELDS, req.body);
        curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, req.bodyLength);
    }
    
    if(req.headers != nullptr)
    {
        std::vector<std::string> headersData = split(req.headers, "\n");
        curl_slist* headers = nullptr;
        for(auto itr = headersData.begin(); itr != headersData.end(); ++itr)
        {
            headers = curl_slist_append(headers, itr->c_str());
        }
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, headers);
    }
    
    // setting to print details about this
    if(_verbose)
    {
        curl_easy_setopt(curl, CURLOPT_VERBOSE, true);
    }
    
    // TODO HTTP2
    //*** TEST Borrar luego de descomentar lo anterior
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0);
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0);
    
    
    return curl;
}

bool CurlClient::send(CurlRequest req)
{
    printf("\n\n###  ======================  SEND %d ###\n\n", req.id);
    
    auto* conn = _connections.get(req.id);
    
    if(conn == nullptr)
    {
        return false;
    }
    
    conn->easy = create(req);
    if(!conn->easy)
    {
        return false;
    }
    
    curl_easy_setopt(conn->easy, CURLOPT_HEADERFUNCTION, writeToString);
    curl_easy_setopt(conn->easy, CURLOPT_HEADERDATA, &conn->headersBuffer);
    curl_easy_setopt(conn->easy, CURLOPT_WRITEFUNCTION, writeToString);
    curl_easy_setopt(conn->easy, CURLOPT_WRITEDATA, &conn->bodyBuffer);
    curl_easy_setopt(conn->easy, CURLOPT_PRIVATE, conn);
    
    /* HTTP/2 please */
    curl_easy_setopt(conn->easy, CURLOPT_HTTP_VERSION, CURL_HTTP_VERSION_2_0);
    
    /* Wait for pipe connection to confirm. Works together with CURLMOPT_PIPELINING option in the multi handle */
    //curl_easy_setopt(conn->easy, CURLOPT_PIPEWAIT, 1L);
    
    //*** TEST
    curl_easy_setopt(conn->easy, CURLOPT_VERBOSE, 1L);
    
    
    conn->bodyBuffer.clear();
    conn->headersBuffer.clear();
    CURLMcode rc = curl_multi_add_handle(_multi, conn->easy);
    
    if(rc != CURLM_OK)
    {
        conn->errorBuffer = curl_multi_strerror(rc);
        return false;
    }
    
    _running++;
    
    return true;
}

void CurlClient::update()
{
    printf("\n\n###  ======================  UPDATE ###\n\n");
    
    //do {
    struct timeval timeout;
    int rc; /* select() return code */
    CURLMcode mc; /* curl_multi_fdset() return code */
    
    fd_set fdread;
    fd_set fdwrite;
    fd_set fdexcep;
    int maxfd = -1;
    
    long curl_timeo = -1;
    
    FD_ZERO(&fdread);
    FD_ZERO(&fdwrite);
    FD_ZERO(&fdexcep);
    
    /* set a suitable timeout to play around with */
    timeout.tv_sec = 1;
    timeout.tv_usec = 0;
    
    curl_multi_timeout(_multi, &curl_timeo);
    if(curl_timeo >= 0) {
        timeout.tv_sec = curl_timeo / 1000;
        if(timeout.tv_sec > 1)
            timeout.tv_sec = 1;
        else
            timeout.tv_usec = (curl_timeo % 1000) * 1000;
    }
    
    /* get file descriptors from the transfers */
    mc = curl_multi_fdset(_multi, &fdread, &fdwrite, &fdexcep, &maxfd);
    
    if(mc != CURLM_OK) {
        fprintf(stderr, "curl_multi_fdset() failed, code %d.\n", mc);
        //break;
    }
    
    /* On success the value of maxfd is guaranteed to be >= -1. We call
     select(maxfd + 1, ...); specially in case of (maxfd == -1) there are
     no fds ready yet so we call select(0, ...) --or Sleep() on Windows--
     to sleep 100ms, which is the minimum suggested value in the
     curl_multi_fdset() doc. */
    
    if(maxfd == -1) {
        /* Portable sleep for platforms other than Windows. */
        struct timeval wait = { 0, 100 * 1000 }; /* 100ms */
        rc = select(0, nullptr, nullptr, nullptr, &wait);
    }
    else {
        /* Note that on some platforms 'timeout' may be modified by select().
         If you need access to the original value save a copy beforehand. */
        rc = select(maxfd+1, &fdread, &fdwrite, &fdexcep, &timeout);
    }
    
    switch(rc) {
        case -1:
            /* select error */
            return;
            //break;
        case 0: /* timeout */
        default: /* action */
            //curl_multi_perform(globalInfo.multi, &globalInfo.still_running);
            break;
    }
    //} while(globalInfo.still_running);
    
    
    
    
    // Do multi_perform and read buffers
    CurlConnection* conn = nullptr;
    //CURLMcode mc;
    int numfds = 0;
    
    curlUpdateLock.lock();
    mc = curl_multi_perform(_multi, &_running);
    
    if(mc == CURLM_OK ) {
        mc = curl_multi_wait(_multi, nullptr, 0, 1000, &numfds);
    }
    
    if(mc != CURLM_OK || !numfds)
    {
        // TODO
        curlUpdateLock.unlock();
        return;
    }
    
    int msgs_left;
    while(auto res_msg = curl_multi_info_read(_multi, &msgs_left))
    {
        CURL* easy = res_msg->easy_handle;
        CURLMsg* msg = (CURLMsg*)res_msg;
        
        if(msg->msg == CURLMSG_DONE)
        {
            curl_easy_getinfo(easy, CURLINFO_PRIVATE, &conn);
            if(msg->data.result != CURLE_OK)
            {
                conn->errorBuffer = curl_easy_strerror(msg->data.result);
                conn->errorCode = msg->data.result;
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
            
            curl_multi_remove_handle(_multi, easy);
            curl_easy_cleanup(easy);
        }
    }
    
    curlUpdateLock.unlock();
}

bool CurlClient::update(int id)
{
    update();
    return isFinished(id);
}

bool CurlClient::isFinished(int id)
{
    if(id == CurlConnection::kInvalidId)
    {
        return false;
    }
    
    CurlConnection* conn = _connections.get(id);
    if(conn == nullptr || conn->responseCode != 0 || conn->errorCode != 0)
    {
        return true;
    }
    
    return false;
}

int CurlClient::createConnection()
{
    printf("\n\n###  ======================  CONNECTION ####\n\n");
    return _connections.create();
}

bool CurlClient::destroyConnection(int id)
{
    printf("\n\n###  ======================  DESSTROY CONN %d ###\n\n", id);
    return _connections.remove(id);
}

double CurlClient::getTime(int id)
{
    
    auto conn = _connections.get(id);
    if(conn)
    {
        return conn->connectTime;
    }
    return 0;
}

double CurlClient::getTotalTime(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return conn->totalTime;
    }
    return 0;
}

double CurlClient::getDownloadSize(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return conn->downloadSize;
    }
    return 0;
}


double CurlClient::getDownloadSpeed(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return conn->downloadSpeed;
    }
    return 0;
}

int CurlClient::getResponseCode(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return conn->responseCode;
    }
    return 0;
}

int CurlClient::getErrorCode(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return conn->errorCode;
    }
    return 0;
}

bool CurlClient::getError(int id, char* data)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        memcpy(data, conn->errorBuffer.c_str(), conn->errorBuffer.length() * sizeof(char));
        return true;
    }
    return false;
}

bool CurlClient::getBody(int id, char* data)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        memcpy(data, conn->bodyBuffer.c_str(), conn->bodyBuffer.length() * sizeof(char));
        return true;
    }
    return false;
}

bool CurlClient::getHeaders(int id, char* data)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        memcpy(data, conn->headersBuffer.c_str(), conn->headersBuffer.length() * sizeof(char));
        return true;
    }
    return false;
}

int CurlClient::getErrorLength(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return (int)conn->errorBuffer.length();
    }
    return 0;
}

int CurlClient::getBodyLength(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return (int)conn->bodyBuffer.length();
    }
    return 0;
}

int CurlClient::getHeadersLength(int id)
{
    auto conn = _connections.get(id);
    if(conn)
    {
        return (int)conn->headersBuffer.length();
    }
    return 0;
}

void CurlClient::setConfig(const std::string &name)
{
    _certificate = Certificate(name);
}
