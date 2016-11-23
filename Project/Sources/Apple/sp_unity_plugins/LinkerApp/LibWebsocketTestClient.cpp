//
//  LibWebsocketTestClient.cpp
//  sp_unity_plugins
//
//  Created by Fernando Serra on 10/11/16.
//
//

#include "LibWebsocketTestClient.hpp"

#include "WebSocketConnection.hpp"
#include "WebSocketsManager.hpp"

#include <iostream>
#include <thread>

void LibWebsocketTestClient::run()
{
    std::atomic_flag running = ATOMIC_FLAG_INIT;
    std::atomic_int pendingMessages(0);
    running.test_and_set();

    WebSocketsManager::get().setProxySettings({"192.168.89.107", 8888});
    WebSocketsManager::get().setLogLevelMax();

    auto connection = std::unique_ptr<WebSocketConnection>(new WebSocketConnection());
    WebSocketConnectionInfo info;
    info.scheme = "wss";
    info.host = "echo.websocket.org";
    info.path = "";
    info.port = 443;
    connection->setAllowSelfSignedCertificates(true);
    connection->addUrl(info);

    auto newMessageReceivedCallback = [&pendingMessages](const std::string& message)
    {
        pendingMessages--;
        std::cout << "Received " << message << std::endl;
    };

    auto t = std::thread([&running, newMessageReceivedCallback, &connection]()
                         {
                             while(running.test_and_set())
                             {
                                 WebSocketsManager::get().update();
                                 if(connection->hasMessages())
                                 {
                                     newMessageReceivedCallback(connection->getMessage());
                                     connection->removeOldestMessage();
                                 }
                                 std::this_thread::sleep_for(std::chrono::milliseconds(50));
                             }
                         });


    std::cout << "Connecting Websocket to " << info << std::endl;

    connection->connect();

    while(connection->getState() == WebSocketConnection::State::Connecting)
    {
        std::this_thread::yield();
    }

    if(connection->getState() != WebSocketConnection::State::Open)
    {
        std::cout << "Websocket connection ERROR to " << info << std::endl;

        running.clear();
        t.join();
        return;
    }

    std::cout << "Websocket connected to " << info << std::endl;

    pendingMessages++;
    connection->send("This is a test message 1!");
    pendingMessages++;
    connection->send("This is a test message 2!");
    pendingMessages++;
    connection->send("This is a test message 3!");
    pendingMessages++;
    connection->send("This is a test message 4!");

    while(pendingMessages != 0)
    {
        std::this_thread::sleep_for(std::chrono::seconds(2));
        connection->sendPing();
    }

    running.clear();
    t.join();

    connection->closeSocket();
}