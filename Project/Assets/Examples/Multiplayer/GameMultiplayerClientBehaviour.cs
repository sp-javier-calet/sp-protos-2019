using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.Dependency;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Pooling;
using Jitter.LinearMath;

public static class GameMsgType
{
    public const byte ClickAction = SceneMsgType.Highest + 1;
    public const byte ExplosionEvent = SceneMsgType.Highest + 2;
    public const byte MovementAction = SceneMsgType.Highest + 3;
}

public class GameMultiplayerClientBehaviour : MonoBehaviour, INetworkClientSceneReceiver, IPointerClickHandler
{
    INetworkClient _client;
    NetworkClientSceneController _controller;
    GameMultiplayerServerBehaviour _gameServer;

    [SerializeField]
    GameObject _explosionPrefab;

    public void Start()
    {
        _client = ServiceLocator.Instance.Resolve<INetworkClient>();
        _controller = ServiceLocator.Instance.Resolve<NetworkClientSceneController>();
        _controller.RegisterReceiver(this);
        _controller.RegisterActionDelegate<MovementAction>(MovementAction.Apply);
    }

    public void OnDestroy()
    {
        if(_controller != null)
        {
            _controller.RegisterReceiver(null);
        }
    }

    public void Update()
    {
        KeyInputHandler();
    }

    void KeyInputHandler()
    {
        float delta = 0.1f;
        var movement = new JVector(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * delta;
        bool input = (movement != JVector.Zero);

        if(input && _client.Connected)
        {
            var movementAction = new MovementAction {
                Movement = movement
            };
            NetworkMessageData msgData = new NetworkMessageData {
                MessageType = GameMsgType.MovementAction
            };

            _controller.ApplyActionAndSend(movementAction, msgData);
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if(_client.Connected)
        {
            UnityEngine.Ray clickRay = eventData.pressEventCamera.ScreenPointToRay(eventData.pressPosition);
            _client.SendMessage(new NetworkMessageData {
                MessageType = GameMsgType.ClickAction
            }, new ClickAction {
                Position = eventData.pointerPressRaycast.worldPosition.ToMultiplayer(),
                Ray = new SocialPoint.Multiplayer.Ray(clickRay.origin.ToMultiplayer(), clickRay.direction.ToMultiplayer())
            });
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
            var ev = reader.Read<ExplosionEvent>();
            ObjectPool.Spawn(_explosionPrefab, transform, ev.Position.ToUnity());
        }
    }
}
