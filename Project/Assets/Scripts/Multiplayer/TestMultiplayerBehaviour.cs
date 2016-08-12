using UnityEngine;
using System.Collections;
using SocialPoint.Multiplayer;
using SocialPoint.Utils;
using UnityEngine.EventSystems;


public static class GameMsgType
{
    public const byte ClickAction = SceneMsgType.Highest + 1;
}

public enum TestMultiplayerMode
{
    Local,
    Unet
}

public class TestMultiplayerBehaviour : MonoBehaviour, IPointerClickHandler
{
    INetworkServer _server;
    INetworkClient _client;

    NetworkServerSceneController _serverCtrl;
    UnityNetworkClientSceneController _clientCtrl;

    ClickActionSerializer _clickSerializer = new ClickActionSerializer();

    [SerializeField]
    TestMultiplayerMode _mode = TestMultiplayerMode.Local;

	void Start()
    {
        if(_mode == TestMultiplayerMode.Unet)
        {
            var scheduler = gameObject.AddComponent<UnityUpdateRunner>();
            _server = new UnetNetworkServer(scheduler);
            _client = new UnetNetworkClient();
        }
        else
        {
            var localServer = new LocalNetworkServer();
            _client = new LocalNetworkClient(localServer);
            _server = localServer;
        }

        _serverCtrl = new NetworkServerSceneController(_server);
        _serverCtrl.AddBehaviour(new TestMultiplayerServerBehaviour(_serverCtrl));
        _serverCtrl.UpdateInterval = 2.0f;

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
            var msg = _client.CreateMessage(new NetworkMessageData {
                MessageType = GameMsgType.ClickAction
            });
            _clickSerializer.Serialize(new ClickAction {
                Position = eventData.pointerPressRaycast.worldPosition.ToMultiplayer()
            }, msg.Writer);
            msg.Send();
        }
    }
}
