﻿using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerDelegate
    {
        void OnStarted();
        void OnStopped();
        void OnClientConnected(byte clientId);
        void OnClientDisconnected(byte clientId);
        void OnMessageReceived(byte clientId, ReceivedNetworkMessage msg);
        void OnError(Error err);
    }

    public interface INetworkServer
    {
        bool Running{ get; }
        void Start();
        void Stop();
        INetworkMessage CreateMessage(NetworkMessageDest info);
        void AddDelegate(INetworkServerDelegate dlg);
        void RemoveDelegate(INetworkServerDelegate dlg);
    }
}
