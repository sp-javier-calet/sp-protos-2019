//
//  WebSocketConnection.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 2/11/16.
//

#ifndef __sparta__WebSocketConnection__
#define __sparta__WebSocketConnection__

#include <string>
#include <functional>
#include <vector>
#include <queue>
#include <libwebsockets.h>

class WebSocketConnection
{
    // TODO FROM inetworkconnection
public:
    enum class State
    {
        CLOSING,
        CLOSED,
        CONNECTING,
        OPEN
    };
    
    typedef std::function<void(State)> ConnectionStateChangedCallback;
    typedef std::function<void()> ConnectionErrorCallback;
    
    typedef std::function<void()> ReceivePongCallback;
    
private:
    //INetworkConnection
    State _state;
    ConnectionStateChangedCallback _connChangedCallback;

    // IWebsocketConnection
    std::vector<std::string> _vecSupportedProtocols;
    std::string _origin;
    
    
    bool _allowSelfSignedCertificates;
    /**
     * URL to connect
     */
    std::string _url;
    
    /**
     * Vector of candidate urls to try the connection
     */
    std::vector<std::string> _vecUrls;
    
    size_t _currentUrlIndex;
    
    libwebsocket* _websocket;
    
    
    std::queue<std::string> _pendingQueue;
    int _pendingPings;
    
    /**
     * Accumulated message received in multiple frames
     */
    std::string _accumulatedMessage;
    
public:
        void setState(State pNewState);
    
    /**
     * Called when new data is received form _socket
     */
    void receivedData(const std::string& message, bool isFinalFrame);
    
    void receivedPong();
    
    void connectionError();
    
    bool hasDataToSend();
    
    /**
     * Returns next data to send, but NOT remove it from the queue
     */
    const std::string& getNextDataToSend();
    /**
     * Remove and delete the oldest data in the queue
     */
    void removeOldestData();
    
    /**
     * Checks for pending Pings and if any, decrements by 1 and returns true, false otherwise
     */
    bool checkAndDecrementPingCounter();
    
    /**
     * Performs all necessary actions when the socket is connected
     */
    void connectionEstablished();
    /**
     * Performs all necessary actions when the socket is closed remotelly or by error
     */
    void closeSocket();
    
    void setWebsocket(libwebsocket* wsi);
    libwebsocket* getWebsocket();
    
public:
    /**
     * Constructor
     * @param pUrl url of the Websocket Server
     */
    WebSocketConnection(const std::string& pUrl);
    WebSocketConnection(const std::vector<std::string>& pVecUrls);
    ~WebSocketConnection();
    
    /**
     * Start the connecting process asynchronously to the URL given to the constructor. The connectionStateChanged will be called when the
     * operation will finish
     */
    void connect();
    
    bool isConnected();
    
    bool isConnecting();
    
    void disconnect();

    virtual void send(const std::string& message);
    
    /**
     * Sends a websocket PING frame
     */
    void sendPing();
    
    const std::string& getUrl() const;
    void setUrl(const std::string& pNewUrl);
    
    const std::vector<std::string>& getVecUrls() const;
    
    size_t getCurrentUrlIndex() const;
    void setCurrentUrlIndex(size_t newIndex);
    
    bool getAllowSelfSignedCertificates() const;
    void setAllowSelfSignedCertificates(bool pNewValue);
    
    /**
     * Setter for the callback to be called when the connection state has changed
     */
    virtual void setConnectionStateChangedCallback(ConnectionStateChangedCallback pNewCallback);
    
    void setSupportedProtocols(const std::vector<std::string>& pNewValue);
    const std::vector<std::string>& getSuportedProtocols() const;
    
    const std::string getSuportedProtocolsString() const;
    
    void setOrigin(const std::string& pNewOrigin);
    const std::string& getOrigin() const;
};


#endif /* defined(__sparta__WebSocketConnection__) */
