using UnityEngine;
using System.Collections;
using SocialPoint.Multiplayer;
using UnityEngine.EventSystems;


public static class GameMsgType
{
    public const byte ClickAction = MsgType.Highest + 1;
}

public class TestMultiplayerBehaviour : MonoBehaviour, IPointerClickHandler
{
    INetworkServer _server;
    INetworkClient _client;

    NetworkServerSceneController _serverCtrl;
    UnityNetworkClientSceneController _clientCtrl;

    ClickActionSerializer _clickSerializer = new ClickActionSerializer();

	void Start()
    {
        var localServer = new LocalNetworkServer();
        _client = new LocalNetworkClient(localServer);
        _server = localServer;

        _serverCtrl = new NetworkServerSceneController(_server);
        _serverCtrl.AddBehaviour(new TestMultiplayerServerBehaviour(_serverCtrl));



        _clientCtrl = new UnityNetworkClientSceneController(_client, transform);

        _server.Start();
        _client.Connect();
	}

    void OnDestroy()
    {
        _serverCtrl.Dispose();
        _clientCtrl.Dispose();
    }
	
	void Update()
    {
        _serverCtrl.Update(Time.deltaTime);
	}
        
    public void OnPointerClick(PointerEventData eventData)
    {
        if(_client.Connected)
        {
            var msg = _client.CreateMessage(new NetworkMessageDest {
                MessageType = GameMsgType.ClickAction
            });
            _clickSerializer.Serialize(new ClickAction {
                Position = eventData.pointerPressRaycast.worldPosition.ToMultiplayer()
            }, msg.Writer);
            msg.Send();
        }
    }
}
