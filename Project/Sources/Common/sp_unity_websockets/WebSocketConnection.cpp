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


WebSocketConnection::WebSocketConnection(const std::string& pUrl)
: _allowSelfSignedCertificates(false)
, _vecUrls(1, pUrl)
, _currentUrlIndex(0)
, _websocket(nullptr)
, _pendingPings(0)
, _state(State::CLOSED)
, _connChangedCallback(nullptr)
{
}

WebSocketConnection::WebSocketConnection(const std::vector<std::string>& pVecUrls)
: _allowSelfSignedCertificates(false)
, _vecUrls(pVecUrls)
, _currentUrlIndex(0)
, _websocket(nullptr)
, _pendingPings(0)
, _state(State::CLOSED)
, _connChangedCallback(nullptr)
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
        auto received = _accumulatedMessage;
        _accumulatedMessage = "";
        // TODO Receive_receiveCallback(received);
    }
}

void WebSocketConnection::receivedPong()
{
    // TODO
}

void WebSocketConnection::connectionError()
{
    _accumulatedMessage = "";
    
    // TODO Notify error
}

bool WebSocketConnection::hasDataToSend()
{
    return !_pendingQueue.empty();
}

const std::string& WebSocketConnection::getNextDataToSend()
{
    return _pendingQueue.front();
}

void WebSocketConnection::removeOldestData()
{
    if(!_pendingQueue.empty())
    {
        _pendingQueue.pop();
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
    setState(State::OPEN);
}

void WebSocketConnection::closeSocket()
{
    if(_state != State::CLOSED)
    {
        assert(_websocket);
        WebSocketsManager::get().remove(this);
        _websocket = nullptr;
        setState(State::CLOSED);
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
    if(_state != State::CONNECTING)
    {
        WebSocketsManager::get().connect(this);
        setState(State::CONNECTING);
    }
}

void WebSocketConnection::disconnect()
{
    if(_state == State::OPEN && _websocket)
    {
        setState(State::CLOSING);
        WebSocketsManager::get().markSocketToClose(_websocket);
    }
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
    
    _pendingQueue.push(message);
    
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
    if(_state != pNewState)
    {
        _state = pNewState;
        // TODO Notify connection state change
    }
}

void WebSocketConnection::setConnectionStateChangedCallback(ConnectionStateChangedCallback pNewCallback)
{
    _connChangedCallback = pNewCallback;
}

void WebSocketConnection::setSupportedProtocols(const std::vector<std::string>& pNewValue)
{
    _vecSupportedProtocols = pNewValue;
};

const std::vector<std::string>& WebSocketConnection::getSuportedProtocols() const
{
    return _vecSupportedProtocols;
};

const std::string WebSocketConnection::getSuportedProtocolsString() const
{
    // TODO return StringUtils::join(getSuportedProtocols(), ",");
    return "";
};

void WebSocketConnection::setOrigin(const std::string& pNewOrigin)
{
    _origin = pNewOrigin;
};

const std::string& WebSocketConnection::getOrigin() const
{
    return _origin;
};
