//
//  WebSocketConnection.hpp
//  sp_unity_plugins
//
//  Created by Manuel Álvarez de Toledo on 2/11/16.
//

#ifndef __sparta__WebSocketConnection__
#define __sparta__WebSocketConnection__

#include <ctime>
#include <string>
#include <functional>
#include <vector>
#include <queue>
#include "libwebsockets.h"
#include "WebSocketConnection.fwd.hpp"

class WebSocketConnection
{
  public:
    enum class State
    {
        Closed = 0,
        Closing,
        Connecting,
        Open
    };

    enum class Error : int
    {
        None = 0,
        WriteError,
        StreamError,
        ConnectionError,
        MaxPings
    };


  private:
    State _state;
    std::vector<std::string> _vecSupportedProtocols;
    std::string _origin;

    bool _allowSelfSignedCertificates;

    /**
     * Vector of candidate urls to try the connection
     */
    std::vector<WebSocketConnectionInfo> _vecUrls;

    size_t _currentUrlIndex;

    lws* _websocket;

    std::queue<std::string> _incomingQueue;
    std::queue<std::string> _outcomingQueue;
    int _pendingPings;
    int _missingPong;

    int _errorCode;
    std::string _errorMessage;

    /**
     * Accumulated message received in multiple frames
     */
    std::string _accumulatedMessage;
    
    bool _standby;
    std::time_t _standbyStartTime;
    std::time_t _standbyTimeout;
    
    void startStandby();
    void endStandby();
    void startStandbyCountdown();
    void checkStandbyTimeout();

  public:
    State getState();
    void setState(State pNewState);
    
    /**
     * Called when new data is received form _socket
     */
    void receivedData(const std::string& message, bool isFinalFrame);

    void connectionError(int code, const std::string& message);

    bool hasError();
    int getErrorCode();
    const std::string& getError();
    void clearError();

    bool hasDataToSend();

    /**
     * Returns next data to send, but NOT remove it from the queue
     */
    const std::string& getNextDataToSend();
    /**
     * Remove and delete the oldest data in the queue
     */
    void removeOldestData();

    bool hasMessages();
    const std::string& getMessage();
    void removeOldestMessage();

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

    void setWebsocket(lws* wsi);
    lws* getWebsocket();

    bool onPingSent();
    void onPongReceived();
    void resetPing();

  public:
    /**
     * Constructor
     * @param pUrl url of the Websocket Server
     */
    WebSocketConnection();
    ~WebSocketConnection();

    /**
     * Start the connecting process asynchronously to the URL given to the constructor. The connectionStateChanged will be called when the
     * operation will finish
     */
    void connect();

    void disconnect();

    void addUrl(WebSocketConnectionInfo url);
    void send(const std::string& message);

    /**
     * Sends a websocket PING frame
     */
    void sendPing();

    const std::vector<WebSocketConnectionInfo>& getVecUrls() const;

    size_t getCurrentUrlIndex() const;
    void setCurrentUrlIndex(size_t newIndex);

    bool getAllowSelfSignedCertificates() const;
    void setAllowSelfSignedCertificates(bool pNewValue);

    void addSupportedProtocol(const std::string& protocol);
    const std::string getSuportedProtocolsString() const;

    void setOrigin(const std::string& pNewOrigin);
    const std::string& getOrigin() const;

    void onWillGoBackground();
    void onWasOnBackground();
    
    bool inStandby();
    void setStandbyTimeout(std::time_t timeout);
    
    void update();
};


#endif /* defined(__sparta__WebSocketConnection__) */
