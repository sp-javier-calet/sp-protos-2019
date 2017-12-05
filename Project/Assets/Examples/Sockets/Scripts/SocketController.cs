﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Dependency;

public class SocketController : MonoBehaviour , INetworkClientDelegate, INetworkMessageReceiver
{


    SocketNetworkClient _netClient;
    SocketNetworkServer _netServer;


    void Start()
    {
        //SERVER
        _netServer = Services.Instance.Resolve<SocketNetworkServer>();
        _netServer.Start();


//        //CLIENT
//        _netClient = Services.Instance.Resolve<SocketNetworkClient>();
//        _netClient.RemoveDelegate(this);
//        _netClient.AddDelegate(this);
//        _netClient.RegisterReceiver(this);
//        _netClient.Connect();
    }


    #region INetworkClientDelegate implementation
    public void OnClientConnected()
    {
        throw new System.NotImplementedException();
    }
    public void OnClientDisconnected()
    {
        throw new System.NotImplementedException();
    }
    public void OnMessageReceived(NetworkMessageData data)
    {
        throw new System.NotImplementedException();
    }
    public void OnNetworkError(SocialPoint.Base.Error err)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region INetworkMessageReceiver implementation

    public void OnMessageReceived(NetworkMessageData data, SocialPoint.IO.IReader reader)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}
