using System;
using System.IO;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public struct NetworkServerSceneActionData
    {
        public bool Synced;
        public bool Unreliable;
    }

    public class NetworkServerSceneController : NetworkSceneController<NetworkGameObject<INetworkBehaviour>, INetworkBehaviour>, INetworkServerDelegate, INetworkMessageReceiver, IDeltaUpdateable, INetworkSceneController
    {
        public static bool SerializeAlways = false;
        public static bool IsEnableSceneDispose = false;
        public static float GlobalSyncInterval = -1f;

        struct ActionInfo
        {
            public NetworkServerSceneActionData Data;
            public object Action;
        }

        class ClientData
        {
            public int LastReceivedAction;
            public float LastAckTimestamp;
            public NetworkScene Scene;
        }

        INetworkServer _server;
        NetworkScene<INetworkSceneBehaviour> _scene;
        NetworkScene<INetworkSceneBehaviour> _prevScene;
        List<NetworkScene<INetworkSceneBehaviour>> _oldScenes;
        NetworkScene<INetworkSceneBehaviour> _emptyScene;
        INetworkMessageReceiver _receiver;
        NetworkSceneSerializer<INetworkSceneBehaviour> _serializer;

        public const float DefaultSyncInterval = 0.02f;
        public const int DefaultBufferSize = 10;

        public float SyncInterval = DefaultSyncInterval;
        public int BufferSize = DefaultBufferSize;

        float _timeSinceLastSync;
        float _timestamp;
        float _lastSentTimestamp;
        float _actionTimestampThreshold;

        Dictionary<byte, ClientData> _clientData;
        List<ActionInfo> _pendingActions;

        public NetworkScene<INetworkSceneBehaviour> Scene
        {
            get
            {
                return _scene;
            }
        }

        public IGameTime GameTime { get; private set; }

        GameTime _gameTime;
        ActionUpdater _sceneDisposer;

        public NetworkServerSceneController(INetworkServer server, IGameTime gameTime = null)
        {
            _server = server;
            _server.AddDelegate(this);
            _server.RegisterReceiver(this);

            GameTime = gameTime;
            if(GameTime == null)
            {
                _gameTime = new GameTime();
                GameTime = _gameTime;
            }

            _sceneDisposer = new ActionUpdater(DisposeScenes, 0.2f);
        }

        void OnObjectRemoved(NetworkGameObject go)
        {
            go.OnDestroy();
        }

        public void Restart(INetworkServer server)
        {
            _server = server;
            UnregisterAllBehaviours();

            _scene = new NetworkScene<INetworkSceneBehaviour>();
            _prevScene = (NetworkScene<INetworkSceneBehaviour>)_scene.DeepClone();
            _oldScenes = new List<NetworkScene<INetworkSceneBehaviour>>();
            _emptyScene = new NetworkScene<INetworkSceneBehaviour>();

            _clientData = new Dictionary<byte, ClientData>();
            _pendingActions = new List<ActionInfo>();
            _serializer = new NetworkSceneSerializer<INetworkSceneBehaviour>();

            _scene.OnObjectRemoved += OnObjectRemoved;
            Init(_scene);
        }

        public float LastUpdateTimestamp
        {
            get
            {
                return _timestamp - _timeSinceLastSync;
            }
        }

        public NetworkScene GetSceneForTimestamp(float ts)
        {
            var i = (int)(((LastUpdateTimestamp - ts) / SyncInterval) + 0.5f);
            if(i >= 0 && i < _oldScenes.Count)
            {
                return _oldScenes[i];
            }
            return null;
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
            _timeSinceLastSync = 0.0f;
            _lastSentTimestamp = -1.0f;
            _scene.Clear();
            _prevScene.Clear();
            _oldScenes.Clear();
            _clientData.Clear();
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            OnServerStopped();
        }

        virtual protected void OnServerStopped()
        {
            if(_server == null || Scene == null)
            {
                #warning CHECK: Receiving OnServerStopped on an undesired NetworkServerSceneController (Using standalone server but a local server is also instantiated)?
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
            if(GlobalSyncInterval < -0.1f)
            {
                GlobalSyncInterval = SyncInterval;
            }
            else
            {
                SyncInterval = GlobalSyncInterval;
            }
            SyncInterval = Math.Max(SyncInterval, 0f);

            //TODO: Remove this
            SyncInterval = 0.02f;

            if(!_server.Running)
            {
                return;
            }
            if(_gameTime != null)
            {
                _gameTime.Update(dt);
            }
            _timestamp += dt;
            _timeSinceLastSync += dt;
            UpdateScene(dt);
            if(_timeSinceLastSync >= SyncInterval || SerializeAlways)
            {
                SendScene();
                _timeSinceLastSync = 0f;
            }
            else
            {
                _sceneDisposer.Update(dt);
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

        List<byte> keyList = new List<byte>();

        void SendScene()
        {
            var timestamp = LastUpdateTimestamp;
            if(timestamp <= _lastSentTimestamp)
            {
                return;
            }
            var memStream = new MemoryStream();
            var binWriter = new SystemBinaryWriter(memStream);
            _serializer.Serialize(_scene, _prevScene, binWriter);
            var sceneBuffer = memStream.ToArray();
            // to avoid out of sync exception with GetEnumerator we will "make a copy" of the keys and iterate over them
            var clients = new Dictionary<byte,ClientData>.KeyCollection(_clientData);
            var itrKeys = _clientData.GetEnumerator();
            keyList.Clear();
            while(itrKeys.MoveNext())
            {
                keyList.Add(itrKeys.Current.Key);
            }
            itrKeys.Dispose();

            var itr = keyList.GetEnumerator();
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
                msg.Writer.Write(sceneBuffer, sceneBuffer.Length);
                msg.Writer.Write(new UpdateSceneEvent {
                    Timestamp = _timeSinceLastSync,
                    LastAction = _clientData[itr.Current].LastReceivedAction,
                });
                msg.Send();
            }
            itr.Dispose();
            _lastSentTimestamp = timestamp;

            //update old scenes
            if(IsEnableSceneDispose)
            {
                _prevScene.Dispose();
            }
            _prevScene.DeepCopy(_scene);
                
            if(BufferSize > 0)
            {
                _oldScenes.Insert(0, (NetworkScene<INetworkSceneBehaviour>)_scene.DeepClone());
            }
            //send pending actions
            for(var i = _pendingActions.Count - 1; i >= 0; i--)
            {
                var info = _pendingActions[i];
                if(info.Data.Synced)
                {
                    _actions.SendAction(info.Action, info.Data.Unreliable);
                    _pendingActions.RemoveAt(i);
                }
            }
        }

        void DisposeScenes(float dt)
        {
            var excess = _oldScenes.Count - BufferSize;
            if(excess <= 0)
            {
                return;
            }
            
            for(int i = 0; i < excess; ++i)
            {
                var scene = _oldScenes[BufferSize + i];
                scene.Dispose();
            }

            _oldScenes.RemoveRange(BufferSize, excess);
        }

        public NetworkGameObject InstantiateLocal(byte objType, Transform trans = null)
        {
            return Instantiate(objType, trans, true);
        }

        public NetworkGameObject Instantiate(byte objType, Transform trans = null, bool local = false)
        {
            var go = ObjectPool.Get<NetworkGameObject<INetworkBehaviour>>();
            go.Init(_scene.FreeObjectId, true, trans, objType, local);
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
                Synced = false,
                Unreliable = false
            }, evnt);
        }

        public void ApplyAction(NetworkServerSceneActionData data, object evnt)
        {
            if(data.Synced)
            {
                _actions.ApplyAction(evnt);
                _pendingActions.Add(new ActionInfo {
                    Data = data,
                    Action = evnt
                });
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
                #warning CHECK: Receiving OnClientConnected on an undesired NetworkServerSceneController (Using standalone server but a local server is also instantiated)?
                return;
            }

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
                ClientId = clientId,
                MessageType = SceneMsgType.UpdateSceneEvent
            });
            _serializer.Serialize(_scene, _emptyScene, msg.Writer);
            msg.Writer.Write(new UpdateSceneEvent {
                Timestamp = _timeSinceLastSync,
            });
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
                #warning CHECK: Receiving OnClientDisconnected on an undesired NetworkServerSceneController (Using standalone server but a local server is also instantiated)?
                return;
            }

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

        void INetworkServerDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(data.MessageType == SceneMsgType.UpdateSceneAckEvent)
            {
                var ev = reader.Read<UpdateSceneAckEvent>();
                float lastAckTimestamp = ev.Timestamp;
                ClientData clientData = null;
                if(_clientData.TryGetValue(data.ClientId, out clientData))
                {
                    clientData.LastAckTimestamp = lastAckTimestamp;
                    clientData.Scene = GetSceneForTimestamp(lastAckTimestamp);
                }
            }
            else
            {
                _actionTimestampThreshold = SyncInterval * BufferSize;
                ClientData clientData = null;
                NetworkScene mementoScene = null;
                float mementoDelta = 0f;
                if(_clientData.TryGetValue(data.ClientId, out clientData))
                {
                    mementoScene = clientData.Scene;
                    mementoDelta = _timestamp - clientData.LastAckTimestamp;
                }
                bool handled = _actions.ApplyActionReceived(data, mementoScene, mementoDelta, _actionTimestampThreshold, data.ClientId, reader);
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
