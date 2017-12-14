using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Dependency;

public class SocketClientController : MonoBehaviour, INetworkClientDelegate, INetworkMessageReceiver
{

    SocketNetworkClient _netClient;

    void Start()
    {
        //CLIENT
        _netClient = Services.Instance.Resolve<SocketNetworkClient>();
        _netClient.Connect();
    }

    void OnDestroy()
    {
        _netClient.Dispose();
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
