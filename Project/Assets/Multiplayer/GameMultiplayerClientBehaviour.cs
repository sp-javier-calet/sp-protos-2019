﻿using UnityEngine;
using UnityEngine.EventSystems;
using SocialPoint.Dependency;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.IO;

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

    [SerializeField]
    GameObject _explosionPrefab;

    public void Start()
    {
        _client = ServiceLocator.Instance.Resolve<INetworkClient>();
        _controller = ServiceLocator.Instance.Resolve<NetworkClientSceneController>();
        _controller.RegisterReceiver(this);
        _controller.RegisterActionDelegate<MovementAction>(new MovementActionDelegate());
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
        SocialPoint.Multiplayer.Vector3 movement = SocialPoint.Multiplayer.Vector3.Zero;
        if(Input.GetKey(KeyCode.UpArrow))
        {
            movement += new SocialPoint.Multiplayer.Vector3(0, 0, delta);
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            movement += new SocialPoint.Multiplayer.Vector3(0, 0, -delta);
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            movement += new SocialPoint.Multiplayer.Vector3(delta, 0, 0);
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            movement += new SocialPoint.Multiplayer.Vector3(-delta, 0, 0);
        }

        bool input = (movement != SocialPoint.Multiplayer.Vector3.Zero);

        if(input && _client.Connected)
        {
            var ac = new MovementAction {
                Movement = movement
            };

            _controller.ApplyAction<MovementAction>(ac);
            //*** TEST - Use this instead of previous line to avoid prediction:
            /*
            _client.SendMessage(new NetworkMessageData {
                MessageType = GameMsgType.MovementAction
            }, ac);
            //*/
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if(_client.Connected)
        {
            _client.SendMessage(new NetworkMessageData {
                MessageType = GameMsgType.ClickAction
            }, new ClickAction {
                Position = eventData.pointerPressRaycast.worldPosition.ToMultiplayer()
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
            SocialPoint.ObjectPool.ObjectPool.Spawn(_explosionPrefab, transform, ev.Position.ToUnity());
        }
    }
}
