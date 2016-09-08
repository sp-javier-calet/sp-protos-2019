using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using System;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;

public class GameMultiplayerServerBehaviour : INetworkServerSceneReceiver, IDisposable
{
    INetworkServer _server;
    NetworkServerSceneController _controller;

    Dictionary<int,int> _updateTimes;
    float _moveInterval = 1.0f;
    float _timeSinceLastMove = 0.0f;
    int _maxUpdateTimes = 3;
    Vector3 _movement;

    public GameMultiplayerServerBehaviour(INetworkServer server, NetworkServerSceneController ctrl)
    {
        _server = server;
        _controller = ctrl;
        _controller.RegisterReceiver(this);
        _controller.RegisterActionDelegate<MovementAction>(MovementAction.Apply);
        _updateTimes = new Dictionary<int,int>();
        _movement = new Vector3(2.0f, 0.0f, 2.0f);
    }

    public void Dispose()
    {
        _controller.RegisterReceiver(null);
    }

    void INetworkServerSceneBehaviour.Update(float dt, NetworkScene scene, NetworkScene oldScene)
    {
        _timeSinceLastMove += dt;

        if(_timeSinceLastMove > _moveInterval)
        {
            _timeSinceLastMove = 0.0f;
            var itr = _controller.Scene.GetObjectEnumerator();
            while(itr.MoveNext())
            {
                var id = itr.Current.Id;
                if(id == 1)
                {
                    //Using first cube as MovementAction target
                    continue;
                }

                var p = itr.Current.Transform.Position;

                p += new Vector3(
                    RandomUtils.Range(-_movement.X, _movement.X),
                    0.0f,//RandomUtils.Range(-_movement.Y, _movement.Y),
                    RandomUtils.Range(-_movement.Z, _movement.Z));

                _controller.Tween(id, p, _moveInterval);
                int times;

                if(!_updateTimes.TryGetValue(id, out times))
                {
                    times = 0;
                }
                if(times > _maxUpdateTimes)
                {
                    var go = _controller.Scene.FindObject(id);
                    if(go != null)
                    {
                        SendExplosionEvent(go.Transform);
                    }
                    _controller.Destroy(id);
                    _updateTimes.Remove(id);
                }
                else
                {
                    _updateTimes[id] = times + 1;
                }
            }
            itr.Dispose();
        }

        CheckPlayerCollision(dt);
    }

    void SendExplosionEvent(Transform t)
    {
        _server.SendMessage(new NetworkMessageData {
            MessageType = GameMsgType.ExplosionEvent
        }, new ExplosionEvent {
            Position = t.Position
        });
    }

    void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
    {
        if(data.MessageType == GameMsgType.ClickAction)
        {
            var ac = reader.Read<ClickAction>();
            _controller.Instantiate("Cube", new Transform(
                ac.Position, Quaternion.Identity, Vector3.One));
        }
        else if(data.MessageType == GameMsgType.MovementAction)
        {
            var ac = reader.Read<MovementAction>();
            _controller.OnAction<MovementAction>(ac, data.ClientId);
        }
    }

    void INetworkServerSceneBehaviour.OnClientConnected(byte clientId)
    {
    }

    void INetworkServerSceneBehaviour.OnClientDisconnected(byte clientId)
    {
    }

    //TODO: Improve logic... super inefficient method
    void CheckPlayerCollision(float dt)
    {
        UpdatePhysicsPositions();

        //_controller.PhysicsLateHelper.Update(dt);
        _controller.PhysicsLateHelper.FixedUpdate();
        _controller.PhysicsWorld.OnDrawGizmos();
        //CollisionEventHandler.OnPhysicsStep(_controller.CollisionWorld);

        var itr1 = _controller.Scene.GetObjectEnumerator();
        while(itr1.MoveNext())
        {
            var itr2 = _controller.Scene.GetObjectEnumerator();
            while(itr2.MoveNext())
            {
                if(itr1.Current.Id == itr2.Current.Id)
                {
                    continue;
                }

                NetworkGameObject obj1 = itr1.Current;
                NetworkGameObject obj2 = itr2.Current;
                obj1.CollisionObject.WorldTransform = obj1.Transform.WorldToLocalMatrix();
                obj2.CollisionObject.WorldTransform = obj2.Transform.WorldToLocalMatrix();

                //BoxBoxDetector detector = new BoxBoxDetector((BoxShape)obj1.CollisionObject.CollisionShape, (BoxShape)obj2.CollisionObject.CollisionShape); 
                //bool collision = detector.
                /*bool collision = obj1.CollisionObject.CheckCollideWith(obj2.CollisionObject);
                if(collision)
                {
                    UnityEngine.Debug.Log("*** TEST Collision!");
                }*/
            }
            itr2.Dispose();
        }
        itr1.Dispose();
    }

    void UpdatePhysicsPositions()
    {
        var itr = _controller.Scene.GetObjectEnumerator();
        while(itr.MoveNext())
        {
            NetworkGameObject obj = itr.Current;
            obj.CollisionObject.WorldTransform = obj.Transform.WorldToLocalMatrix();
        }
        itr.Dispose();
    }
}