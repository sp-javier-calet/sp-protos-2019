using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Network.ServerEvents;

namespace SocialPoint.Multiplayer
{
    [Serializable]
    public class SyncGroupSettings
    {
        public float SyncInterval;
    }

    public class NetworkServerSceneActionData
    {
        public bool Synced = true;
        public bool Unreliable;
    }

    public class ClientData
    {
        public int LastReceivedAction;
        public float LastAckTimestamp;
        public NetworkScene Scene;
    }

    public struct ActionInfo
    {
        public NetworkServerSceneActionData Data;
        public object Action;
        public float Time;
    }

    [Serializable]
    public sealed class NetworkServerConfig
    {
        public const byte DefaultMaxPlayers = 6;
        public const int DefaultMetricSendInterval = 10000;
        public const bool DefaultUsePluginHttpClient = false;

        public byte MaxPlayers = DefaultMaxPlayers;
        public int MetricSendInterval = DefaultMetricSendInterval;
        public bool UsePluginHttpClient = DefaultUsePluginHttpClient;
        public Func<string> GetBackendUrlCallback;
        public string MetricEnvironment;
    }

    public class NetworkServerSceneController : NetworkSceneController<NetworkGameObject>, INetworkServerDelegate, INetworkMessageReceiver, IDeltaUpdateable, INetworkSceneController
    {
        public static bool SerializeAlways = false;

        INetworkServer _server;

        NetworkScene _scene;
        NetworkScene _prevScene;

        List<NetworkScene> _oldScenes;
        NetworkScene _emptyScene;
        INetworkMessageReceiver _receiver;
        NetworkSceneSerializer _serializer;

        public const int DefaultBufferSize = 0;
        public int BufferSize = DefaultBufferSize;

        float _timestamp;
        float _actionTimestampThreshold;

        Dictionary<byte, ClientData> _clientData;
        List<ActionInfo> _pendingActions;

        public NetworkServerConfig ServerConfig { get; set; }

        public NetworkServerSyncController SyncController{ get; private set; }

        public Func<bool> HasMatchFinished;

        public NetworkScene Scene
        {
            get
            {
                return _scene;
            }
        }

        public bool Paused { get; set; }

        public byte MaxPlayers
        {
            get
            {
                return ServerConfig.MaxPlayers;
            }
        }

        public bool Full
        {
            get
            {
                return PlayerCount >= ServerConfig.MaxPlayers;
            }
        }

        public int PlayerCount
        {
            get
            {
                return ClientCount;
            }
        }

        public int ClientCount { get; private set; }

        public bool Running
        {
            get
            {
                return _server.Running && !Paused;
            }
        }

        public IGameTime GameTime { get; private set; }

        GameTime _gameTime;

        public Action<Metric> SendMetric { get; set; }

        public Action<Network.ServerEvents.Log, bool> SendLog { get; set; }

        public Action<string, AttrDic, ErrorDelegate> SendTrack { get; set; }

        public NetworkServerSceneController(INetworkServer server, IGameTime gameTime = null)
        {
            GameTime = gameTime;
            if(GameTime == null)
            {
                _gameTime = new GameTime();
                GameTime = _gameTime;
            }

            Restart(server);
        }

        void OnObjectRemoved(NetworkGameObject go)
        {
            go.OnDestroy();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(_scene != null)
            {
                _scene.OnObjectRemoved -= OnObjectRemoved;
                
                _scene.Dispose();
                _scene = null;
            }

            if(_prevScene != null)
            {
                _prevScene.Dispose();
                _prevScene = null;
            }

            if(_emptyScene != null)
            {
                _emptyScene.Dispose();
                _emptyScene = null;
            }

            if(_oldScenes != null)
            {
                for(int i = 0; i < _oldScenes.Count; i++)
                {
                    _oldScenes[i].Dispose();
                }
                _oldScenes.Clear();
                _oldScenes = null;
            }

            if(SyncController != null)
            {
                SyncController.Dispose();
                SyncController = null;
            }

            if(_clientData != null)
            {
                _clientData.Clear();
                _clientData = null;
            }
            if(_pendingActions != null)
            {
                _pendingActions.Clear();
                _pendingActions = null;
            }

            _serializer = null;

            if(Context != null)
            {
                Context.Clear();
            }
        }

        public void Restart(INetworkServer server)
        {
            if(_server != null)
            {
                _server.RemoveDelegate(this);
            }
            UnregisterAllBehaviours();
            _server = server;
            Paused = false;
            GameTime.Scale = 1f;
            _server.AddDelegate(this);
            _server.RegisterReceiver(this);

            _scene = new NetworkScene(Context);
            _prevScene = (NetworkScene)_scene.DeepClone();

            _oldScenes = new List<NetworkScene>();
            _emptyScene = new NetworkScene(Context);

            _clientData = new Dictionary<byte, ClientData>();
            _pendingActions = new List<ActionInfo>();
            _serializer = new NetworkSceneSerializer(Context);

            _scene.OnObjectRemoved += OnObjectRemoved;

            Init(_scene);

            if(SyncController != null)
            {
                SyncController.Dispose();
            }
            SyncController = new NetworkServerSyncController(_server, _clientData, _serializer, _scene, _prevScene, _actions, _pendingActions);

            _server.RemoveDelegate(this);
            _server.AddDelegate(this);
            _server.RegisterReceiver(this);
        }

        public NetworkScene GetSceneForTimestamp(float ts)
        {
            if(BufferSize > 0)
            {
                var i = (int)(((SyncController.LastUpdateTimestamp - ts) / SyncController.SyncInterval) + 0.5f);
                if(i >= 0 && i < _oldScenes.Count)
                {
                    return _oldScenes[i];
                }
                return null;
            }
            else
            {
                return _activeScene;
            }
        }

        public override INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return _server.CreateMessage(data);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void RegisterObjectSerializer<T>(byte type, IDiffWriteSerializer<T> serializer) where T : INetworkBehaviour, ICopyable
        {
            _serializer.RegisterObjectBehaviour<T>(type, serializer);
        }

        public void RegisterSceneSerializer<T>(byte type, IDiffWriteSerializer<T> serializer) where T : INetworkSceneBehaviour
        {
            _serializer.RegisterSceneBehaviour<T>(type, serializer);
        }

        void INetworkServerDelegate.OnServerStarted()
        {
            OnServerStarted();
        }

        virtual protected void OnServerStarted()
        {
            _timestamp = 0.0f;

            _scene.Clear();
            _prevScene.Clear();

            _oldScenes.Clear();
            _clientData.Clear();

            SyncController.Reset();
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            OnServerStopped();
        }

        virtual protected void OnServerStopped()
        {
            if(_server == null || Scene == null)
            {
                //#warning CHECK: Receiving OnServerStopped on an undesired NetworkServerSceneController (Using standalone server but a local server is also instantiated)?
                return;
            }

            var itr = GetObjectEnumerator();
            while(itr.MoveNext())
            {
                Destroy(itr.Current.Id);
            }
            itr.Dispose();
        }

        public void Update(float dt)
        {
            if(_scene == null || !Running)
            {
                return;
            }

            if(_gameTime != null)
            {
                _gameTime.Update(dt);
            }

            _timestamp += dt;
            UpdateScene(dt);

            var synced = SyncController.Update(_timestamp, dt);

            if(synced && BufferSize > 0)
            {
                _oldScenes.Insert(0, (NetworkScene)_scene.DeepClone());

                var excess = _oldScenes.Count - BufferSize;
                if(excess > 0)
                {
                    _oldScenes.RemoveRange(BufferSize, excess);
                }
            }

            if(HasMatchFinished != null)
            {
                Paused = HasMatchFinished();
            }
        }

        void UpdateScene(float dt)
        {
            //Add or remove logic changed before update
            UpdatePendingLogic();
            //Update behaviours
            UpdateObjects(dt);
            //LateUpdate behaviours
            LateUpdateObjects(dt);
            //Add or remove logic changed during behaviour update
            UpdatePendingLogic();
            //Update scene behaviours
            _scene.Update(dt);
            //Add or remove logic changed during scene behaviour update
            UpdatePendingLogic();
        }

        public NetworkGameObject InstantiateLocal(byte objType, Transform trans = null)
        {
            return Instantiate(objType, trans, true);
        }

        public NetworkGameObject Instantiate(byte objType, Transform trans = null, bool local = false, int syncGroup = 0)
        {
            var go = Context.Pool.Get<NetworkGameObject>();
            go.Init(Context, _scene.FreeObjectId, true, trans, objType, local, syncGroup);
            SetupObject(go);
            _scene.AddObject(go);
            return go;
        }

        public void Destroy(int id)
        {
            SetupObjectToDestroy(id);
            _scene.RemoveObject(id);
        }

        protected override void UpdatePendingLogic()
        {
            _scene.UpdatePendingLogic();
            base.UpdatePendingLogic();
        }

        public void ApplyActionLocal(object evnt)
        {
            _actions.ApplyAction(evnt);
        }

        public void ApplyActionSync(object evnt)
        {
            ApplyAction(new NetworkServerSceneActionData {
                Synced = true,
                Unreliable = false
            }, evnt);
        }

        public void ApplyAction(object evnt)
        {
            ApplyAction(new NetworkServerSceneActionData {
                Synced = true,
                Unreliable = false
            }, evnt);
        }

        public void ApplyAction(NetworkServerSceneActionData data, object evnt)
        {
            if(data.Synced)
            {
                _pendingActions.Add(new ActionInfo {
                    Data = data,
                    Action = evnt,
                    Time = GameTime.Time
                });

                _actions.ApplyAction(evnt);
            }
            else
            {
                _actions.ApplyActionAndSend(evnt, data.Unreliable);
            }
        }

        public event Action<byte> ClientConnected;
        public event Action<byte> ClientDisconnected;

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            if(_server == null || Scene == null)
            {
                //#warning CHECK: Receiving OnClientConnected on an undesired NetworkServerSceneController (Using standalone server but a local server is also instantiated)?
                return;
            }

            ++ClientCount;

            _clientData.Add(clientId, new ClientData {
                LastReceivedAction = 0,
                LastAckTimestamp = 0f,
            });

            _server.SendMessage(new NetworkMessageData {
                MessageType = SceneMsgType.ConnectEvent
            }, new ConnectEvent {
                Timestamp = _timestamp
            });
            //Send scene
            var msg = _server.CreateMessage(new NetworkMessageData {
                ClientIds = new List<byte>() { clientId },
                MessageType = SceneMsgType.UpdateSceneEvent
            });

            _serializer.Serialize(_scene, _emptyScene, msg.Writer);

            msg.Writer.Write(new UpdateSceneEvent {
                Timestamp = SyncController.TimeSinceLastSync,
            });

            // Write Events
            msg.Writer.Write(0);

            msg.Send();

            //Run behaviours logic as last step
            UpdatePendingLogic();
            if(ClientConnected != null)
            {
                ClientConnected(clientId);
            }
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            if(_server == null || Scene == null)
            {
                //#warning CHECK: Receiving OnClientDisconnected on an undesired NetworkServerSceneController (Using standalone server but a local server is also instantiated)?
                return;
            }

            --ClientCount;

            _clientData.Remove(clientId);
            UpdatePendingLogic();
            if(ClientDisconnected != null)
            {
                ClientDisconnected(clientId);
            }
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkServerDelegate.OnNetworkError(Error err)
        {
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            DebugUtils.Assert(data.ClientIds.Count == 1);
            var clientId = data.ClientIds[0];

            if(data.MessageType == SceneMsgType.UpdateSceneAckEvent)
            {
                var ev = reader.Read<UpdateSceneAckEvent>();
                float lastAckTimestamp = ev.Timestamp;
                ClientData clientData = null;
                if(_clientData.TryGetValue(clientId, out clientData))
                {
                    clientData.LastAckTimestamp = lastAckTimestamp;
                    clientData.Scene = GetSceneForTimestamp(lastAckTimestamp);
                }
            }
            else
            {
                _actionTimestampThreshold = SyncController.SyncInterval * BufferSize;
                ClientData clientData = null;
                NetworkScene mementoScene = null;
                float mementoDelta = 0f;
                if(_clientData.TryGetValue(clientId, out clientData))
                {
                    mementoScene = _scene; //clientData.Scene;
                    mementoDelta = _timestamp - clientData.LastAckTimestamp;
                }
                bool handled = _actions.ApplyActionReceived(data, mementoScene, mementoDelta, _actionTimestampThreshold, clientId, reader);
                if(handled)
                {
                    if(clientData != null)
                    {
                        clientData.LastReceivedAction++;
                    }
                }
                else if(_receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
            }
        }

        public NetworkScene FindSceneMemento(byte clientId)
        {
            ClientData clientData = null;
            NetworkScene mementoScene = null;
            if(_clientData.TryGetValue(clientId, out clientData))
            {
                mementoScene = clientData.Scene;
            }

            return mementoScene;
        }

        public event Action GameStarted;

        public void OnGameStarted()
        {
            if(GameStarted != null)
            {
                GameStarted();
            }
        }
    }
}
