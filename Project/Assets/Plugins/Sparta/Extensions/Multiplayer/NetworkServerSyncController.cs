using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Multiplayer;

public class NetworkServerSyncController
{
    public class ServerControllerData
    {
        public INetworkServer Server;
        public Dictionary<byte, ClientData> ClientData;
        public NetworkSceneSerializer Serializer;
        public NetworkScene Scene;
        public NetworkScene PrevScene;
        public NetworkActionProcessor Actions;
        public List<ActionInfo> PendingActions;
        public bool EnablePrediction;
    }
    
    const float DefaultSyncInterval = 0.07f;

    float _timestamp;
    readonly List<byte> _keyList;

    ServerControllerData _serverControllerData;

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

    public NetworkServerSyncController(ServerControllerData serverControllerData)
    {
        _keyList = new List<byte>();
        _serverControllerData = serverControllerData;
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
        _serverControllerData.Serializer.Serialize(_serverControllerData.Scene, _serverControllerData.PrevScene, binWriter);

        var itrKeys = _serverControllerData.ClientData.GetEnumerator();
        _keyList.Clear();
        while(itrKeys.MoveNext())
        {
            _keyList.Add(itrKeys.Current.Key);
        }
        itrKeys.Dispose();

        if(_serverControllerData.EnablePrediction)
        {
            var itr = _keyList.GetEnumerator();
            while(itr.MoveNext())
            {
                if(!_serverControllerData.ClientData.ContainsKey(itr.Current))
                {
                    continue;
                }
                var msg = _serverControllerData.Server.CreateMessage(new NetworkMessageData
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
                    LastAction = _serverControllerData.ClientData[itr.Current].LastReceivedAction,
                });

                // Write Events
                msg.Writer.Write((int)_serverControllerData.PendingActions.Count);
                for(var i = 0; i < _serverControllerData.PendingActions.Count; ++i)
                {
                    var info = _serverControllerData.PendingActions[i];
                    _serverControllerData.Actions.SerializeAction(info.Action, msg.Writer);
                }

                msg.Send();
            }
            itr.Dispose();
        }
        else
        {
            var msg = _serverControllerData.Server.CreateMessage(new NetworkMessageData
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
            msg.Writer.Write((int)_serverControllerData.PendingActions.Count);
            for(var i = 0; i < _serverControllerData.PendingActions.Count; ++i)
            {
                var info = _serverControllerData.PendingActions[i];
                _serverControllerData.Actions.SerializeAction(info.Action, msg.Writer);
            }

            msg.Send();
        }

        _serverControllerData.PendingActions.Clear();

        _serverControllerData.PrevScene.DeepCopy(_serverControllerData.Scene);
    }

    public void Dispose()
    {
        _keyList.Clear();
        _serverControllerData = null;
    }
}
