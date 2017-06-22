﻿using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Pooling;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{

    public class NetworkClientSceneController : NetworkSceneController<NetworkGameObject<INetworkBehaviour>, INetworkBehaviour>, INetworkClientDelegate, INetworkMessageReceiver, IDeltaUpdateable, INetworkSceneController
    {
        INetworkClient _client;
        NetworkScene<INetworkSceneBehaviour> _scene;
        NetworkScene<INetworkSceneBehaviour> _clientScene;
        INetworkMessageReceiver _receiver;
        NetworkSceneParser<INetworkSceneBehaviour> _parser;

        Dictionary<int, object> _pendingActions;
        int _lastAppliedAction;
        float _serverTimestamp;
        List<NetworkGameObject> _pendingGameObjectAdded;

        public event Action ServerUpdated;

        bool _stop = true;
        bool _willDestroyScene = false;

        public NetworkScene<INetworkSceneBehaviour> Scene
        {
            get
            {
                return _clientScene;
            }
        }

        public NetworkScene ServerScene
        {
            get
            {
                return _scene;
            }
        }

        public float ServerTimestamp
        {
            get
            {
                return _serverTimestamp;
            }
        }

        public INetworkClient Client
        {
            get
            {
                return _client;
            }
        }

        public NetworkClientSceneController(INetworkClient client)
        {
            _client = client;
        }

        public override INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return _client.CreateMessage(data);
        }

        void CopyNetworkGameObject(NetworkGameObject serverGo, NetworkGameObject clientGo)
        {   
            var interpolate = clientGo.GetBehaviour<INetworkInterpolate>();
            if(interpolate != null)
            {
                interpolate.OnServerTransform(serverGo.Transform, _serverTimestamp);
            }
            else
            {
                clientGo.Transform.Copy(serverGo.Transform);
            }
        }

        public void Restart(INetworkClient client)
        {
            _client = client;
            _stop = false;
            _willDestroyScene = false;

            UnregisterAllBehaviours();

            _clientScene = new NetworkScene<INetworkSceneBehaviour>();
            _scene = (NetworkScene<INetworkSceneBehaviour>)_clientScene.Clone();
            _parser = new NetworkSceneParser<INetworkSceneBehaviour>();
            _pendingActions = new Dictionary<int, object>();
            _pendingGameObjectAdded = new List<NetworkGameObject>();

            Init(_clientScene);
            _client.AddDelegate(this);
            _client.RegisterReceiver(this);
            _clientScene.OnObjectAdded += OnObjectAddedToScene;
            _clientScene.OnObjectRemoved += OnObjectRemovedFromScene;
        }

        public bool Equals(NetworkScene scene)
        {
            return _scene == scene;
        }

        public bool PredictionEquals(NetworkScene scene)
        {
            return _clientScene == scene;
        }

        void INetworkClientDelegate.OnClientConnected()
        {
            _scene.Clear();
            _clientScene.Clear();
            _clientScene.UpdatePendingLogic();
            _lastAppliedAction = 0;
            _serverTimestamp = 0.0f;
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(_willDestroyScene)
            {
                return;
            }

            if(data.MessageType == SceneMsgType.ConnectEvent)
            {
                var ev = reader.Read<ConnectEvent>();
                _serverTimestamp = ev.Timestamp;
            }
            else if(data.MessageType == SceneMsgType.UpdateSceneEvent)
            {
                _scene = _parser.Parse(_scene, reader);
                var ev = reader.Read<UpdateSceneEvent>();
                _serverTimestamp = ev.Timestamp;

                var msg = _client.CreateMessage(new NetworkMessageData {
                    MessageType = SceneMsgType.UpdateSceneAckEvent
                });
                msg.Writer.Write(new UpdateSceneAckEvent {
                    Timestamp = _serverTimestamp,
                });
                msg.Send();

                _clientScene.Copy(_scene, CopyNetworkGameObject);
                UpdatePendingLogic();
                OnActionFromServer(ev.LastAction);
                ServerUpdateObjectBehaviours();
            }
            else
            {
                bool handled = _actions.ApplyActionReceived(data, reader);
                if(!handled && _receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
            }
        }

        void OnObjectAddedToScene(NetworkGameObject go)
        {
            if(!go.Local)
            {
                if(go is NetworkGameObject<INetworkBehaviour>)
                {
                    SetupObject(go as NetworkGameObject<INetworkBehaviour>);
                }
            }
        }

        void OnObjectRemovedFromScene(NetworkGameObject go)
        {
            go.OnDestroy();
        }

        public void Pause()
        {
            _stop = true;
        }

        public void DestroyScene()
        {
            _stop = true;
            _willDestroyScene = true;

            var itr = GetObjectEnumerator();
            while(itr.MoveNext())
            {
                Destroy(itr.Current.Id);
            }
            itr.Dispose();
        }

        public void Resume()
        {
            _stop = false;
        }

        public void Update(float dt)
        {
            if(!_stop)
            {
                AddPendingGameObjects();
                UpdatePendingLogic();
                UpdateObjects(dt);
                LateUpdateObjects(dt);
                UpdatePendingLogic();
                _clientScene.Update(dt);
                UpdatePendingLogic();
            }
        }

        void ServerUpdateObjectBehaviours()
        {
            if(ServerUpdated != null)
            {
                ServerUpdated();
            }
        }

        protected override void UpdatePendingLogic()
        {
            _clientScene.UpdatePendingLogic();
            base.UpdatePendingLogic();
        }

        public void Destroy(int id)
        {
            SetupObjectToDestroy(id);
            var go = _clientScene.FindObject(id);
            if(go != null && (go.Local || _willDestroyScene))
            {
                _clientScene.RemoveObject(id);
            }
        }

        virtual protected void OnError(SocialPoint.Base.Error err)
        {
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            OnError(err);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void RegisterSceneParser<T>(byte type, IDiffReadParser<T> parser) where T : INetworkSceneBehaviour, ICopyable
        {
            SocialPoint.Base.Log.d("Registering parser of type " + typeof(T));
            _parser.RegisterSceneBehaviour<T>(type, parser);
        }

        public void RegisterObjectParser<T>(byte type, IDiffReadParser<T> parser) where T : INetworkBehaviour, ICopyable
        {
            _parser.RegisterObjectBehaviour<T>(type, parser);
        }

        public NetworkGameObject InstantiateLocal(byte objType, Transform trans = null)
        {
            return Instantiate(objType, trans);
        }

        public NetworkGameObject Instantiate(byte objType, Transform trans = null)
        {
            var go = ObjectPool.Get<NetworkGameObject<INetworkBehaviour>>();
            go.Init(_clientScene.ProvideObjectId(), false, trans, objType, true);
            SetupObject(go);
            _pendingGameObjectAdded.Add(go);
            return go;
        }

        protected void AddPendingGameObjects()
        {
            if(_pendingGameObjectAdded.Count == 0)
            {
                return;
            }
            var oldPendingGameObjectsAdded = new List<NetworkGameObject>(_pendingGameObjectAdded);
            _pendingGameObjectAdded.Clear();

            for(int i = 0; i < oldPendingGameObjectsAdded.Count; ++i)
            {
                var go = oldPendingGameObjectsAdded[i];
                _clientScene.AddObject(go);
            }
        }

        public void ApplyActionLocal(object action)
        {
            _actions.ApplyAction(action);
        }

        public void ApplyActionSync(object action)
        {
            ApplyActionLocal(action);
        }

        public void ApplyAction(object action)
        {
            _lastAppliedAction++;
            _pendingActions.Add(_lastAppliedAction, action);
            _actions.ApplyActionAndSend(action);
        }

        void OnActionFromServer(int lastServerAction)
        {
            //Remove pending actions with id or lower
            RemoveOldPendingActions(lastServerAction);

            //Reapply client prediction
            ApplyAllPendingActions();
        }

        void RemoveOldPendingActions(int fromAction)
        {
            bool olderActionsRemoved = false;
            while(!olderActionsRemoved)
            {
                if(_pendingActions.Remove(fromAction))
                {
                    fromAction--;
                }
                else
                {
                    olderActionsRemoved = true;
                }
            }
        }

        void ApplyAllPendingActions()
        {
            var itr = _pendingActions.GetEnumerator();
            while(itr.MoveNext())
            {
                _actions.ApplyAction(itr.Current.Value);
            }
            itr.Dispose();
        }
    }
}
