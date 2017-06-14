//
//  CurlClient.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 23/09/16.
//
//

#include "CurlClient.hpp"
#include "Mutex.hpp"

#include <string>
#include <map>
#include <vector>
#include <cassert>
#include <sstream>

extern "C" {
#include <curl/curl.h>
}

/*
 * CurlClient Global initialization as a static instance
 */

CurlClient::CurlHttpClientGlobal CurlClient::global;

CurlClient::CurlHttpClientGlobal::CurlHttpClientGlobal()
{
    curl_global_init(CURL_GLOBAL_ALL);
}

CurlClient::CurlHttpClientGlobal::~CurlHttpClientGlobal()
{
    curl_global_cleanup();
}

/*
 * CurlClient implementation
 */

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

    static size_t write_to_string(void* contents, size_t size, size_t nmemb, void* userp)
    {
        size_t totalBytes = size * nmemb;
        ((std::string*)userp)->append((char*)contents, totalBytes);

        return totalBytes;
    }

    static size_t message_receive_callback(char* ptr, size_t size, size_t nmemb, void* userdata)
    {
        CurlConnection* conn = (CurlConnection*)userdata;

        size_t totalBytes = size * nmemb;
        std::string data(ptr, totalBytes);

        conn->messages.incoming.append(ptr, size * nmemb);

        return totalBytes;
    }

    static size_t message_send_callback(char* buffer, size_t size, size_t nmemb, void* userdata)
    {
        CurlConnection* conn = (CurlConnection*)userdata;
        size_t bufferSize = size * nmemb;
        size_t msgLength = conn->messages.outcoming.length();
        size_t msgSize = msgLength * sizeof(char);
        size_t bytesWritten = 0;

        if(msgSize < bufferSize)
        {
            bytesWritten = msgSize;
        }
        else
        {
            bytesWritten = bufferSize;
        }

        if(!msgLength)
        {
            return CURL_READFUNC_PAUSE;
        }

        memcpy(buffer, &conn->messages.outcoming, bytesWritten);
        conn->messages.outcoming = conn->messages.outcoming.substr(bytesWritten / sizeof(char));

        return CURLE_OK;
    }

    static int server_push_callback(CURL* parent, CURL* easy, size_t num_headers, struct curl_pushheaders* headers, void* userp)
    {
        /* TODO:
         This feature is not tested yet. We should define its possible uses and retrieve the pushed data accordingly.
         */
        return CURL_PUSH_OK;
    }
}

CurlClient::CurlClient(bool enableHttp2)
: _multi(nullptr)
, _connections(enableHttp2)
, _supportsHttp2(enableHttp2)
, _verbose(false)
, _running(0)
, _pinnedPublicKey(nullptr)
, _pinnedPublicKeySize(0)
{
    _multi = curl_multi_init();

    if(_supportsHttp2)
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

bool CurlClient::isRunning()
{
    return _running > 0;
}

void CurlClient::setVerbose(bool verbose)
{
    _verbose = verbose;
}

CURL* CurlClient::create(CurlRequestInfo& req)
{
    assert(req);
    CURL* curl = curl_easy_init();
    if(!curl)
    {
        return curl;
    }

    std::string url = (req.url + "?" + req.query).c_str();

    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());

    curl_easy_setopt(curl, CURLOPT_NOSIGNAL, 1);
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1);
    curl_easy_setopt(curl, CURLOPT_MAXREDIRS, 10);

    if(req.method == "GET")// method
    {
        curl_easy_setopt(curl, CURLOPT_HTTPGET, 1);
    }
    else if(req.method == "POST")
    {
        curl_easy_setopt(curl, CURLOPT_POST, 1);
    }
    else if(req.method == "PUT")
    {
        curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "PUT");
    }
    else if(req.method == "DELETE")
    {
        curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "DELETE");
    }
    else if(req.method == "HEAD")
    {
        curl_easy_setopt(curl, CURLOPT_NOBODY, 1);
    }
    else
    {
        assert(false);
        curl_easy_setopt(curl, CURLOPT_HTTPGET, 1);
    }

    if(req->timeout > 0)
    {
        curl_easy_setopt(curl, CURLOPT_CONNECTTIMEOUT, req->activityTimeout);
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, req->timeout);
    }

    if(!req.proxy.empty())
    {
        curl_easy_setopt(curl, CURLOPT_PROXY, req.proxy.c_str());
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

    if(!req.body.empty())
    {
        curl_easy_setopt(curl, CURLOPT_POSTFIELDS, req.body.c_str());
        curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE, req.body.length());
    }

    if(!req.headers.empty())
    {
        std::vector<std::string> headersData = split(req->headers, "\n");
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
    return curl;
}

bool CurlClient::send(CurlRequest* req)
{
    assert(req);
    CurlConnection& conn = _connections.get(req->id);
    if(!conn.isValid)
    {
        return false;
    }
    
    // Copy request data to c++ managed memory
    conn.request.url = req.url;
    conn.request.query = req.query;
    conn.request.method = req.method;
    conn.request.proxy = req.proxy;
    conn.request.headers = req.headers;
    conn.request.body = std::string((const char*)req.body, req.bodyLength);
    conn.request.timeout = req.timeout;
    conn.request.activityTimeout = req.activityTimeout;
    
    conn.easy = create(conn.request);
    if(!conn.easy)
    {
        return false;
    }
    
    curl_easy_setopt(conn.easy, CURLOPT_HEADERFUNCTION, write_to_string);
    curl_easy_setopt(conn.easy, CURLOPT_HEADERDATA, &conn.response.headers);
    curl_easy_setopt(conn.easy, CURLOPT_PRIVATE, &conn);

    if(_supportsHttp2)
    {
        curl_easy_setopt(conn.easy, CURLOPT_WRITEFUNCTION, message_receive_callback);
        curl_easy_setopt(conn.easy, CURLOPT_WRITEDATA, &conn);
        curl_easy_setopt(conn.easy, CURLOPT_READFUNCTION, message_send_callback);
        curl_easy_setopt(conn.easy, CURLOPT_READDATA, &conn);

        /* Upgrade requests to http2 if possible */
        curl_easy_setopt(conn.easy, CURLOPT_HTTP_VERSION, CURL_HTTP_VERSION_2_0);

        /* Wait for pipe connection to confirm. Works together with CURLMOPT_PIPELINING option in the multi handle */
        curl_easy_setopt(conn.easy, CURLOPT_PIPEWAIT, 1L);
    }
    else
    {
        curl_easy_setopt(conn.easy, CURLOPT_WRITEFUNCTION, write_to_string);

        curl_easy_setopt(conn.easy, CURLOPT_WRITEDATA, &conn.response.body);
    }

    conn.response.body.clear();
    conn.response.headers.clear();
    CURLMcode rc = curl_multi_add_handle(_multi, conn.easy);

    if(rc != CURLM_OK)
    {
        conn.response.error = curl_multi_strerror(rc);
        return false;
    }

    _running++;

    return true;
}

void CurlClient::update()
{
    CURLMcode mc;
    CurlConnection* conn = nullptr;

    curlUpdateLock.lock();
    mc = curl_multi_perform(_multi, &_running);

    int msgs_left = 0;
    while(auto res_msg = curl_multi_info_read(_multi, &msgs_left))
    {
        CURL* easy = res_msg->easy_handle;
        CURLMsg* msg = (CURLMsg*)res_msg;

        if(msg->msg == CURLMSG_DONE)
        {
            curl_easy_getinfo(easy, CURLINFO_PRIVATE, &conn);
            assert(conn && "Could not access to curl connection data");

            if(msg->data.result != CURLE_OK)
            {
                conn->response.error = curl_easy_strerror(msg->data.result);
                conn->response.errorCode = msg->data.result;
            }
            else
            {
                long code = 0;
                curl_easy_getinfo(easy, CURLINFO_RESPONSE_CODE, &code);
                conn->response.code = (int)code;
            }

            curl_easy_getinfo(easy, CURLINFO_CONNECT_TIME, &conn->response.connectTime);
            curl_easy_getinfo(easy, CURLINFO_TOTAL_TIME, &conn->response.totalTime);
            curl_easy_getinfo(easy, CURLINFO_SIZE_DOWNLOAD, &conn->response.downloadSize);
            curl_easy_getinfo(easy, CURLINFO_SPEED_DOWNLOAD, &conn->response.downloadSpeed);

            curl_multi_remove_handle(_multi, easy);
            curl_easy_cleanup(easy);
            conn->isActive = false;
        }
    }

    curlUpdateLock.unlock();
}

bool CurlClient::update(int id)
{
    if(!isFinished(id))
    {
        update();
    }
    return isFinished(id);
}

bool CurlClient::isFinished(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(!conn.isValid || conn.response.code != 0 || conn.response.errorCode != 0)
    {
        return true;
    }

    return false;
}

int CurlClient::createConnection()
{
    return _connections.create();
}

bool CurlClient::destroyConnection(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid && conn.isActive)
    {
        curl_multi_remove_handle(_multi, conn.easy);
        curl_easy_cleanup(conn.easy);
        conn.isActive = false;
    }

    return _connections.remove(id);
}

bool CurlClient::sendStreamMessage(int id, CurlMessage* data)
{
    assert(data);
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        if(conn.messages.outcoming.empty())
        {
            curl_easy_pause(conn.easy, CURLPAUSE_CONT);
        }

        conn.messages.outcoming.append((char*)data->message, data->messageLength * sizeof(char));
        return true;
    }
    return false;
}

int CurlClient::getStreamMessageLenght(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return (int)conn.messages.incoming.length();
    }
    return 0;
}

void CurlClient::getStreamMessage(int id, char* data)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        memcpy(data, conn.messages.incoming.c_str(), conn.messages.incoming.length() * sizeof(char));

        // Clear message after retrieve it
        conn.messages.incoming.clear();
    }
}

double CurlClient::getTime(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return conn.response.connectTime;
    }
    return 0;
}

double CurlClient::getTotalTime(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return conn.response.totalTime;
    }
    return 0;
}

double CurlClient::getDownloadSize(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return conn.response.downloadSize;
    }
    return 0;
}


double CurlClient::getDownloadSpeed(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return conn.response.downloadSpeed;
    }
    return 0;
}

int CurlClient::getResponseCode(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return conn.response.code;
    }
    return 0;
}

int CurlClient::getErrorCode(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return conn.response.errorCode;
    }
    return 0;
}

bool CurlClient::getError(int id, char* data)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        memcpy(data, conn.response.error.c_str(), conn.response.error.length() * sizeof(char));
        return true;
    }
    return false;
}

bool CurlClient::getBody(int id, char* data)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        memcpy(data, conn.response.body.c_str(), conn.response.body.length() * sizeof(char));
        return true;
    }
    return false;
}

bool CurlClient::getHeaders(int id, char* data)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        memcpy(data, conn.response.headers.c_str(), conn.response.headers.length() * sizeof(char));
        return true;
    }
    return false;
}

bool CurlClient::getVersionInfo(char* data)
{
    std::string versionInfo(curl_version());
    memcpy(data, versionInfo.c_str(), versionInfo.length() * sizeof(char));
    return true;
}

int CurlClient::getErrorLength(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return (int)conn.response.error.length();
    }
    return 0;
}

int CurlClient::getBodyLength(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return (int)conn.response.body.length();
    }
    return 0;
}

int CurlClient::getHeadersLength(int id)
{
    CurlConnection& conn = _connections.get(id);
    if(conn.isValid)
    {
        return (int)conn.response.headers.length();
    }
    return 0;
}

int CurlClient::getVersionInfoLength()
{
    std::string versionInfo(curl_version());
    return (int)versionInfo.length();
}

void CurlClient::setConfig(const std::string& name)
{
    _certificate = Certificate(name);
}
