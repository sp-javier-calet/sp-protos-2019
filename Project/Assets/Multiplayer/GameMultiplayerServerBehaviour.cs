using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using System;
using System.Collections.Generic;

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
        _controller.RegisterActionDelegate<MovementAction>(new MovementActionDelegate());
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
                    RandomUtils.Range(-_movement.x, _movement.x),
                    0.0f,//RandomUtils.Range(-_movement.y, _movement.y),
                    RandomUtils.Range(-_movement.z, _movement.z));

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
}