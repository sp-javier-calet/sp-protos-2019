using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;
using System.Diagnostics;

namespace SocialPoint.Multiplayer
{
    [Serializable]
    public class SyncGroupSettings
    {
        public float SyncInterval;
    }

//    public struct NetworkServerSceneActionData
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

    public class NetworkServerSceneController : NetworkSceneController<NetworkGameObject<INetworkBehaviour>, INetworkBehaviour>, INetworkServerDelegate, INetworkMessageReceiver, IDeltaUpdateable, INetworkSceneController
    {
        public static bool SerializeAlways = false;

        INetworkServer _server;

        NetworkScene<INetworkSceneBehaviour> _scene;
        NetworkScene<INetworkSceneBehaviour> _prevScene;

        List<NetworkScene<INetworkSceneBehaviour>> _oldScenes;
        NetworkScene<INetworkSceneBehaviour> _emptyScene;
        INetworkMessageReceiver _receiver;
        NetworkSceneSerializer<INetworkSceneBehaviour> _serializer;

        public const int DefaultBufferSize = 0;
        public int BufferSize = DefaultBufferSize;
        public List<SyncGroupSettings> SyncGroupsSettings = new List<SyncGroupSettings> {
            new SyncGroupSettings { SyncInterval = 0.05f },
        };

        float _timestamp;
        float _actionTimestampThreshold;

        bool _paused = false;

        Dictionary<byte, ClientData> _clientData;
        List<ActionInfo> _pendingActions;

        public NetworkServerSyncController SyncController = new NetworkServerSyncController();

        public NetworkScene<INetworkSceneBehaviour> Scene
        {
            get
            {
                return _scene;
            }
        }

        public bool Paused
        {
            get{ return _paused;} set { _paused = value;}
        }

        public IGameTime GameTime { get; private set; }

        GameTime _gameTime;

        public NetworkServerSceneController(INetworkServer server, IGameTime gameTime = null)
        {
            _server = server;
            Paused = false;
            GameTime = gameTime;
            if(GameTime == null)
            {
                _gameTime = new GameTime();
                GameTime = _gameTime;
            }
        }

        void OnObjectRemoved(NetworkGameObject go)
        {
            go.OnDestroy();
        }

        public void Restart(INetworkServer server)
        {
            _server = server;
            Paused = false;
            UnregisterAllBehaviours();

            _scene = new NetworkScene<INetworkSceneBehaviour>(Context);
            _prevScene = (NetworkScene<INetworkSceneBehaviour>)_scene.DeepClone();

            _oldScenes = new List<NetworkScene<INetworkSceneBehaviour>>();
            _emptyScene = new NetworkScene<INetworkSceneBehaviour>(Context);

            _clientData = new Dictionary<byte, ClientData>();
            _pendingActions = new List<ActionInfo>();
            _serializer = new NetworkSceneSerializer<INetworkSceneBehaviour>(Context);

            _scene.OnObjectRemoved += OnObjectRemoved;

            Init(_scene);

            _scene.SetSyncGroupSettings(SyncGroupsSettings);
            _prevScene.SetSyncGroupSettings(SyncGroupsSettings);

            SyncController = new NetworkServerSyncController();
            SyncController.Init(GameTime, _server, _clientData, _serializer, _scene, _prevScene, _actions, _pendingActions);

            _server.RemoveDelegate(this);
            _server.AddDelegate(this);
            _server.RegisterReceiver(this);

        }

        public NetworkScene GetSceneForTimestamp(float ts)
        {
            if(BufferSize > 0)
            {
                var i = (int)(((SyncController.LastUpdateTimestamp - ts) / SyncController.MaxSyncInterval) + 0.5f);
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

        public void SetSyncInterval(int groupId, float syncInterval)
        {
            if(SyncGroupsSettings.Count > groupId)
            {
                SyncGroupsSettings[groupId].SyncInterval = syncInterval;

                if(_scene != null)
                {
                    _scene.SyncGroups[groupId].Settings.SyncInterval = syncInterval;
                }

                if(_prevScene != null)
                {
                    _prevScene.SyncGroups[groupId].Settings.SyncInterval = syncInterval;
                }
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

        protected override void OnObjectSyncGroupChanged(NetworkGameObject obj)
        {
            base.OnObjectSyncGroupChanged(obj);

            var oldObject = _prevScene.FindObject(obj.Id);
            if(oldObject != null)
            {
                oldObject.SyncGroup = obj.SyncGroup;
                _prevScene.AddObjectInSyncGroup(oldObject);
            }
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
            if(!_server.Running || Paused)
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
                _oldScenes.Insert(0, (NetworkScene<INetworkSceneBehaviour>)_scene.DeepClone());

                var excess = _oldScenes.Count - BufferSize;
                if(excess > 0)
                {
                    _oldScenes.RemoveRange(BufferSize, excess);
                }
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
            var go = Context.Pool.Get<NetworkGameObject<INetworkBehaviour>>();
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
                _actions.ApplyAction(evnt);
                _pendingActions.Add(new ActionInfo {
                    Data = data,
                    Action = evnt,
                    Time = GameTime.Time
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
                _actionTimestampThreshold = SyncController.MaxSyncInterval * BufferSize;
                ClientData clientData = null;
                NetworkScene mementoScene = null;
                float mementoDelta = 0f;
                if(_clientData.TryGetValue(data.ClientId, out clientData))
                {
                    mementoScene = _scene; //clientData.Scene;
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
