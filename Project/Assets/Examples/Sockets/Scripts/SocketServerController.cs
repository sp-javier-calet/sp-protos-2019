using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Dependency;

public class SocketServerController : MonoBehaviour, INetworkServerDelegate, INetworkMessageReceiver
{
    TcpSocketNetworkServer _netServer;

    void Start()
    {
        //SERVER
        _netServer = Services.Instance.Resolve<TcpSocketNetworkServer>();
        _netServer.Start();
    }

    void OnApplicationQuit()
    {
        _netServer.Dispose();
    }

    #region INetworkServerDelegate implementation

    public void OnServerStarted()
    {
        throw new System.NotImplementedException();
    }

    public void OnServerStopped()
    {
        throw new System.NotImplementedException();
    }

    public void OnClientConnected(byte clientId)
    {
        throw new System.NotImplementedException();
    }

    public void OnClientDisconnected(byte clientId)
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
