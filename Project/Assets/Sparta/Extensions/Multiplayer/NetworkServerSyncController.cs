using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Multiplayer;

public class NetworkServerSyncController
{
    const float DefaultSyncInterval = 0.07f;

    float _timestamp;
    readonly List<byte> _keyList;

    NetworkSceneSerializer _serializer;
    NetworkScene _scene;
    NetworkScene _prevScene;

    Dictionary<byte, ClientData> _clientData;
    List<ActionInfo> _pendingActions;
    INetworkServer _server;
    NetworkActionHandler _actions;

    public bool BroadcastSyncMessageManually; // If false, Photon will send message to all clients. Otherwise, we'll have to do manually.
    public float SyncInterval = DefaultSyncInterval;
    public float TimeSinceLastSync;

    public float LastUpdateTimestamp
    {
        get
        {
            return _timestamp - TimeSinceLastSync;
        }
    }

    MemoryStream _memStream = new MemoryStream(64 * 1024);

    public NetworkServerSyncController(INetworkServer server, Dictionary<byte, ClientData> clientData, NetworkSceneSerializer serializer, NetworkScene scene, NetworkScene prevScene, NetworkActionHandler actions, List<ActionInfo> pendingActions)
    {
        _keyList = new List<byte>();
        _server = server;
        _clientData = clientData;
        _serializer = serializer;
        _actions = actions;
        _scene = scene;
        _prevScene = prevScene;
        _pendingActions = pendingActions;
        BroadcastSyncMessageManually = false;
    }

    public void Reset()
    {
        _timestamp = 0f;
        TimeSinceLastSync = 0f;
    }

    public bool Update(float serverTimestamp, float dt)
    {
        _timestamp = serverTimestamp;
        TimeSinceLastSync += dt;

        if(TimeSinceLastSync >= SyncInterval)
        {
            SendScene();
            TimeSinceLastSync = 0f;
            return true;
        }

        return false;
    }

    void SendScene()
    {
        _memStream.SetLength(0);
        _memStream.Seek(0, SeekOrigin.Begin);
        var binWriter = new SystemBinaryWriter(_memStream);
        _serializer.Serialize(_scene, _prevScene, binWriter);

        var itrKeys = _clientData.GetEnumerator();
        _keyList.Clear();
        while(itrKeys.MoveNext())
        {
            _keyList.Add(itrKeys.Current.Key);
        }
        itrKeys.Dispose();

        if(BroadcastSyncMessageManually)
        {
            var itr = _keyList.GetEnumerator();
            while(itr.MoveNext())
            {
                if(!_clientData.ContainsKey(itr.Current))
                {
                    continue;
                }
                var msg = _server.CreateMessage(new NetworkMessageData
                {
                    ClientIds = new List<byte>(){ itr.Current },
                    MessageType = SceneMsgType.UpdateSceneEvent
                });

                // Write Scene Diff
                msg.Writer.Write(_memStream.GetBuffer(), (int)_memStream.Length);

                // Write Update Scene Event content
                msg.Writer.Write(new UpdateSceneEvent
                {
                    Timestamp = TimeSinceLastSync,
                    LastAction = _clientData[itr.Current].LastReceivedAction,
                });

                // Write Events
                msg.Writer.Write((int)_pendingActions.Count);
                for(var i = 0; i < _pendingActions.Count; ++i)
                {
                    var info = _pendingActions[i];
                    _actions.SerializeAction(info.Action, msg.Writer);
                }

                msg.Send();
            }
            itr.Dispose();
        }
        else
        {
            var msg = _server.CreateMessage(new NetworkMessageData
            {
                ClientIds = _keyList,
                MessageType = SceneMsgType.UpdateSceneEvent
            });

            // Write Scene Diff
            msg.Writer.Write(_memStream.GetBuffer(), (int)_memStream.Length);

            // Write Update Scene Event content
            msg.Writer.Write(new UpdateSceneEvent
            {
                Timestamp = TimeSinceLastSync,
                LastAction = -1,
            });

            // Write Events
            msg.Writer.Write((int)_pendingActions.Count);
            for(var i = 0; i < _pendingActions.Count; ++i)
            {
                var info = _pendingActions[i];
                _actions.SerializeAction(info.Action, msg.Writer);
            }

            msg.Send();
        }

        _pendingActions.Clear();

        _prevScene.DeepCopy(_scene);
    }

    public void Dispose()
    {
        _keyList.Clear();

        _server = null;
        _prevScene = null;

        _clientData = null;
        _serializer = null;

        _actions= null;
        _pendingActions = null;
    }
}
