using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Multiplayer;
using System.Diagnostics;

public class NetworkServerSyncController
{
    float _timestamp;
    List<byte> _keyList = new List<byte>();

    NetworkSceneSerializer<INetworkSceneBehaviour> _serializer;
    NetworkScene<INetworkSceneBehaviour> _scene;
    NetworkScene<INetworkSceneBehaviour> _prevScene;

    Dictionary<byte, ClientData> _clientData;
    List<ActionInfo> _pendingActions;
    INetworkServer _server;
    NetworkActionHandler _actions;

    public float MaxSyncInterval;
    public float TimeSinceLastSync;

    public float LastUpdateTimestamp
    {
        get
        {
            return _timestamp - TimeSinceLastSync;
        }
    }

    List<float> _groupSyncTimestamps = new List<float>();
    IGameTime _gameTime;

    public void Init(IGameTime gameTime, INetworkServer server, Dictionary<byte, ClientData> clientData, NetworkSceneSerializer<INetworkSceneBehaviour> serializer, NetworkScene<INetworkSceneBehaviour> scene, NetworkScene<INetworkSceneBehaviour> prevScene, NetworkActionHandler actions, List<ActionInfo> pendingActions)
    {
        _gameTime = gameTime;
        _server = server;
        _clientData = clientData;
        _serializer = serializer;
        _actions = actions;
        _scene = scene;
        _prevScene = prevScene;
        _pendingActions = pendingActions;

        for(int i = 0; i < _scene.SyncGroupSettings.Count; ++i)
        {
            _groupSyncTimestamps.Add(-1f);
            MaxSyncInterval = Math.Max(MaxSyncInterval, _scene.SyncGroupSettings[i].SyncInterval);
        }
    }

    public void Reset()
    {
        _timestamp = 0f;
        TimeSinceLastSync = -1f;
    }

    public bool Update(float serverTimestamp, float dt)
    {
        _timestamp = serverTimestamp;

        var synced = false;
        for(int i = 0; i < _scene.SyncGroups.Count; ++i)
        {
            if(_scene.SyncGroups[i].CanSync/* || _scene.InmediateUpdate*/)
            {
                _scene.InmediateUpdate = false;
                //Debug.WriteLine("Sync Group: "+ i + " time since last sync: "+ _scene.SyncGroups[i].TimeSinceLastSync);
                _scene.SelectedSyncGroupId = i;
                _prevScene.SelectedSyncGroupId = i;

                SendScene(i);
                _groupSyncTimestamps[i] = _gameTime.Time;
                CalculateLastSyncTimestamp();

                _scene.SelectedSyncGroupId = -1;
                _prevScene.SelectedSyncGroupId = -1;

                synced = true;
            }
        }

        return synced;
    }

    void CalculateLastSyncTimestamp()
    {
        TimeSinceLastSync = _groupSyncTimestamps[0];
        for(int i = 1; i < _groupSyncTimestamps.Count; ++i)
        {
            TimeSinceLastSync = Math.Min(TimeSinceLastSync, _groupSyncTimestamps[i]); 
        }
    }

    void SendScene(int syncGroupId)
    {
        var syncGroup = _scene.SyncGroups[syncGroupId];
        var syncGroupTimeSinceLastSync = syncGroup.TimeSinceLastSync;

        var memStream = new MemoryStream();
        var binWriter = new SystemBinaryWriter(memStream);
        _serializer.Serialize(_scene, _prevScene, binWriter);
        var sceneBuffer = memStream.ToArray();
        // to avoid out of sync exception with GetEnumerator we will "make a copy" of the keys and iterate over them
        var clients = new Dictionary<byte,ClientData>.KeyCollection(_clientData);
        var itrKeys = _clientData.GetEnumerator();
        _keyList.Clear();
        while(itrKeys.MoveNext())
        {
            _keyList.Add(itrKeys.Current.Key);
        }
        itrKeys.Dispose();

        var itr = _keyList.GetEnumerator();
        int clientIndex = 0;
        while(itr.MoveNext())
        {
            if(!_clientData.ContainsKey(itr.Current))
            {
                continue;
            }
            var msg = _server.CreateMessage(new NetworkMessageData {
                ClientId = itr.Current,
                MessageType = SceneMsgType.UpdateSceneEvent
            });

            // Write Scene Diff
            msg.Writer.Write(sceneBuffer, sceneBuffer.Length);

            // Write Update Scene Event contant
            msg.Writer.Write(new UpdateSceneEvent {
                Timestamp = syncGroupTimeSinceLastSync,
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

        _pendingActions.Clear();

        _prevScene.DeepCopy(_scene);
    }
}
