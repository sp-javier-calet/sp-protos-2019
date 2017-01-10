﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Pooling;
using SocialPoint.Geometry;
using Jitter.LinearMath;

public class GameMultiplayerClientBehaviour : MonoBehaviour, INetworkClientSceneReceiver, IPointerClickHandler
{
    INetworkClient _client;
    NetworkClientSceneController _controller;
    GameMultiplayerServerBehaviour _gameServer;

    List<GameObject> _visualPathNodes = new List<GameObject>();

    [SerializeField]
    GameObject _explosionPrefab;

    [SerializeField]
    GameObject _pathNodePrefab;

    [SerializeField]
    GameObject _pathEdgePrefab;

    public void Start()
    {
        _client = ServiceLocator.Instance.Resolve<INetworkClient>();
        _controller = ServiceLocator.Instance.Resolve<NetworkClientSceneController>();
        _controller.RegisterReceiver(this);

        _controller.RegisterAction<ClickAction>(GameMsgType.ClickAction);
        _controller.RegisterAction<MovementAction>(GameMsgType.MovementAction, MovementAction.Apply);
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
            _controller.ApplyAction(movementAction);
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
                Position = Vector.Convert(eventData.pointerPressRaycast.worldPosition),
                Ray = new SocialPoint.Physics.Ray(Vector.Convert(clickRay.origin), Vector.Convert(clickRay.direction))
            });
        }
    }

    void INetworkClientSceneBehaviour.OnInstantiateObject(int id, SocialPoint.Geometry.Transform t)
    {
    }

    void INetworkClientSceneBehaviour.OnDestroyObject(int id)
    {
    }

    void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
    {
        switch(data.MessageType)
        {
        case GameMsgType.ExplosionEvent:
            ReadExplosionEvent(reader);
            break;
        case GameMsgType.PathEvent:
            ReadPathEvent(reader);
            break;
        default:
            break;
        }
    }

    void ReadExplosionEvent(IReader reader)
    {
        var ev = reader.Read<ExplosionEvent>();
        ObjectPool.Spawn(_explosionPrefab, transform, Vector.Convert(ev.Position));
    }

    void ReadPathEvent(IReader reader)
    {
        //Clear previous objects
        for(int i = 0; i < _visualPathNodes.Count; i++)
        {
            Destroy(_visualPathNodes[i]);
        }

        //Create new path nodes
        var ev = reader.Read<PathEvent>();
        for(int i = 0; i < ev.Points.Length; i++)
        {
            var nodeObj = Instantiate(_pathNodePrefab, ev.Points[i], Quaternion.identity) as GameObject;
            nodeObj.transform.SetParent(transform);
            _visualPathNodes.Add(nodeObj);
        }

        //Create path edges
        for(int i = 0; i < ev.Points.Length - 1; i++)
        {
            var point1 = ev.Points[i];
            var point2 = ev.Points[i + 1];
            var edgeObj = Instantiate(_pathEdgePrefab, point1, Quaternion.identity) as GameObject;
            edgeObj.transform.LookAt(point2);
            edgeObj.transform.localScale = new Vector3(1, 1, Vector3.Distance(point1, point2) * 0.5f);
            _visualPathNodes.Add(edgeObj);
        }
    }
}
