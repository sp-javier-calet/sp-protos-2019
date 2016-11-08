//
//  WebSocketConnection.cpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 2/11/16.
//

#include "WebSocketConnection.hpp"
#include "WebSocketsManager.hpp"
#include <libwebsockets.h>
#include <cassert>
#include <string>
#include <sstream>

WebSocketConnection::WebSocketConnection()
: _allowSelfSignedCertificates(false)
, _currentUrlIndex(0)
, _websocket(nullptr)
, _pendingPings(0)
, _state(State::Closed)
{
}

WebSocketConnection::~WebSocketConnection()
{
    if(_websocket)
    {
        disconnect();
        closeSocket();
    }
}

void WebSocketConnection::receivedData(const std::string& message, bool isFinalFrame)
{
    _accumulatedMessage += message;
    
    if(isFinalFrame)
    {
        _incomingQueue.push(_accumulatedMessage);
        _accumulatedMessage = "";
    }
}

void WebSocketConnection::connectionError(int code, const std::string& message)
{
    _accumulatedMessage = "";
    _errorCode = code;
    _errorMessage = message;
}

bool WebSocketConnection::hasError()
{
    return _errorCode || !_errorMessage.empty();
}

int WebSocketConnection::getErrorCode()
{
    return _errorCode;
}

const std::string& WebSocketConnection::getError()
{
    return _errorMessage;
}

void WebSocketConnection::clearError()
{
    _errorCode = 0;
    _errorMessage = "";
}

bool WebSocketConnection::hasDataToSend()
{
    return !_outcomingQueue.empty();
}

const std::string& WebSocketConnection::getNextDataToSend()
{
    return _outcomingQueue.front();
}

void WebSocketConnection::removeOldestData()
{
    if(!_outcomingQueue.empty())
    {
        _outcomingQueue.pop();
    }
}

bool WebSocketConnection::hasMessages()
{
    return !_incomingQueue.empty();
}

const std::string& WebSocketConnection::getMessage()
{
    return _incomingQueue.front();
}

void WebSocketConnection::removeOldestMessage()
{
    if(!_incomingQueue.empty())
    {
        _incomingQueue.pop();
    }
}

bool WebSocketConnection::checkAndDecrementPingCounter()
{
    if(_pendingPings > 0)
    {
        _pendingPings--;
        return true;
    }
    return false;
}

void WebSocketConnection::connectionEstablished()
{
    assert(_websocket);
    setState(State::Open);
}

void WebSocketConnection::closeSocket()
{
    if(_state != State::Closed)
    {
        assert(_websocket);
        WebSocketsManager::get().remove(this);
        _websocket = nullptr;
        setState(State::Closed);
    }
}

void WebSocketConnection::setWebsocket(libwebsocket* wsi)
{
    assert(!_websocket);
    _websocket = wsi;
}

libwebsocket* WebSocketConnection::getWebsocket()
{
    return _websocket;
}

void WebSocketConnection::connect()
{
    if(_state != State::Connecting)
    {
        WebSocketsManager::get().connect(this);
        setState(State::Connecting);
    }
}

void WebSocketConnection::disconnect()
{
    if(_state == State::Open && _websocket)
    {
        setState(State::Closing);
        WebSocketsManager::get().markSocketToClose(_websocket);
    }
}

void WebSocketConnection::addUrl(const std::string &url)
{
    _vecUrls.push_back(url);
}

void WebSocketConnection::send(const std::string& message)
{
    assert(_websocket);
    /* TODO padding
    unsigned char* buffer =
    (unsigned char*)calloc((LWS_SEND_BUFFER_PRE_PADDING + message.size() + LWS_SEND_BUFFER_POST_PADDING), sizeof(unsigned char));
    memcpy(&buffer[LWS_SEND_BUFFER_PRE_PADDING], data.getBytes(), message.size());
    Data* dataPadded =
    new Data(buffer, (LWS_SEND_BUFFER_PRE_PADDING + data.getSize() + LWS_SEND_BUFFER_POST_PADDING) * sizeof(unsigned char), true); 
     */
    
    _outcomingQueue.push(message);
    
    WebSocketsManager::get().dataReadyToSendOnConnection(this);
}

void WebSocketConnection::sendPing()
{
    _pendingPings++;
    
    WebSocketsManager::get().dataReadyToSendOnConnection(this);
}

bool WebSocketConnection::getAllowSelfSignedCertificates() const
{
    return _allowSelfSignedCertificates;
}

void WebSocketConnection::setAllowSelfSignedCertificates(bool pNewValue)
{
    _allowSelfSignedCertificates = pNewValue;
}

const std::string& WebSocketConnection::getUrl() const
{
    return _url;
}

void WebSocketConnection::setUrl(const std::string& newUrl)
{
    _url = newUrl;
}

const std::vector<std::string>& WebSocketConnection::getVecUrls() const
{
    return _vecUrls;
}

size_t WebSocketConnection::getCurrentUrlIndex() const
{
    return _currentUrlIndex;
}

void WebSocketConnection::setCurrentUrlIndex(size_t newIndex)
{
    _currentUrlIndex = newIndex;
}

void WebSocketConnection::setState(State pNewState)
{
    _state = pNewState;
}


WebSocketConnection::State WebSocketConnection::getState()
{
    return _state;
}

void WebSocketConnection::addSupportedProtocol(const std::string& protocol)
{
    _vecSupportedProtocols.push_back(protocol);
}

const std::string WebSocketConnection::getSuportedProtocolsString() const
{
    std::string protocols;
    bool first = true;
    for(auto& protocol : _vecSupportedProtocols)
    {
        if(!first)
        {
            protocols += ",";
        }
        protocols += protocol;
        first = false;
    }
    return protocols;
}

void WebSocketConnection::setOrigin(const std::string& pNewOrigin)
{
    _origin = pNewOrigin;
}

const std::string& WebSocketConnection::getOrigin() const
{
    return _origin;
}
