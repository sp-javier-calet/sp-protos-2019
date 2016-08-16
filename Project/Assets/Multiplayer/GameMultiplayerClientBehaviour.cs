using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.Dependency;
using SocialPoint.Multiplayer;
using SocialPoint.IO;

public static class GameMsgType
{
    public const byte ClickAction = SceneMsgType.Highest + 1;
    public const byte ExplosionEvent = SceneMsgType.Highest + 2;
}

public class GameMultiplayerClientBehaviour : MonoBehaviour, INetworkClientSceneReceiver, IPointerClickHandler
{
    INetworkClient _client;
    NetworkClientSceneController _controller;

    ClickActionSerializer _clickSerializer = new ClickActionSerializer();
    ExplosionEventParser _explParser = new ExplosionEventParser();

    [SerializeField]
    GameObject _explosionPrefab;

    public void Start()
    {
        _client = ServiceLocator.Instance.Resolve<INetworkClient>();
        _controller = ServiceLocator.Instance.Resolve<NetworkClientSceneController>();
        _controller.RegisterReceiver(this);
    }

    public void OnDestroy()
    {
        if(_controller != null)
        {
            _controller.RegisterReceiver(null);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
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
        
    void INetworkClientSceneBehaviour.OnInstantiateObject(int id, SocialPoint.Multiplayer.Transform t)
    {
    }

    void INetworkClientSceneBehaviour.OnDestroyObject(int id)
    {
    }

    void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
    {
        if(data.MessageType == GameMsgType.ExplosionEvent)
        {
            var ev = _explParser.Parse(reader);
            SocialPoint.ObjectPool.ObjectPool.Spawn(_explosionPrefab, transform, ev.Position.ToUnity());
        }
    }
}
