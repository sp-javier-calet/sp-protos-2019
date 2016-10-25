using UnityEngine;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.Dependency;

public class TestMultiplayerBehaviour : MonoBehaviour
{
    //Disable if server will be created outside Unity
    public bool CreateServer = true;

    INetworkServer _server;
    INetworkClient _client;

    void Start()
    {
        if (CreateServer)
        {
            _server = ServiceLocator.Instance.Resolve<INetworkServer>();
        }
        _client = ServiceLocator.Instance.Resolve<INetworkClient>();

        if(_server != null)
        {
            _server.Start();
        }
        if(_client != null)
        {            
            _client.Connect();
        }
    }
}