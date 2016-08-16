using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using System;
using System.Collections.Generic;

public class GameMultiplayerServerBehaviour : INetworkServerSceneReceiver, IDisposable
{
    INetworkServer _server;
    NetworkServerSceneController _controller;
    IParser<ClickAction> _clickParser;
    ISerializer<ExplosionEvent> _explSerializer;
    Dictionary<int,int> _updateTimes;

    public GameMultiplayerServerBehaviour(INetworkServer server, NetworkServerSceneController ctrl)
    {
        _server = server;
        _clickParser = new ClickActionParser();
        _explSerializer = new ExplosionEventSerializer();
        _controller = ctrl;
        _controller.RegisterReceiver(this);
        _updateTimes = new Dictionary<int,int>();
    }

    public void Dispose()
    {
        _controller.RegisterReceiver(null);
    }

    float _moveInterval = 1.0f;
    float _timeSinceLastMove = 0.0f;
    int _maxUpdateTimes = 3;

    void INetworkServerSceneBehaviour.Update(float dt, NetworkScene scene, NetworkScene oldScene)
    {
        _timeSinceLastMove += dt;

        if(_timeSinceLastMove > _moveInterval)
        {
            _timeSinceLastMove = 0.0f;
            var itr = _controller.Scene.GetObjectEnumerator();
            while(itr.MoveNext())
            {
                var t = itr.Current.Transform;
                var id = itr.Current.Id;
                t.Position.x += RandomUtils.Range(-0.5f, +0.5f);
                t.Position.z += RandomUtils.Range(-0.5f, +0.5f);
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
        var msg = _server.CreateMessage(new NetworkMessageData {
            MessageType = GameMsgType.ExplosionEvent
        });
        _explSerializer.Serialize(new ExplosionEvent {
            Position = t.Position
        }, msg.Writer);
        msg.Send();
    }

    void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
    {
        if(data.MessageType == GameMsgType.ClickAction)
        {
            var ac = _clickParser.Parse(reader);
            _controller.Instantiate("Cube", new Transform(
                ac.Position, Quaternion.Identity, Vector3.One));
        }
    }

    void INetworkServerSceneBehaviour.OnClientConnected(byte clientId)
    {
    }

    void INetworkServerSceneBehaviour.OnClientDisconnected(byte clientId)
    {
    }
}