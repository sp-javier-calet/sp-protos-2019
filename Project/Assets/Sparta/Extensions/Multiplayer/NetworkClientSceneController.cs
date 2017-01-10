﻿using System;
using System.Collections.Generic;
using SocialPoint.Geometry;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public interface INetworkClientSceneBehaviour
    {
        void OnInstantiateObject(int id, Transform t);

        void OnDestroyObject(int id);
    }

    public interface INetworkClientSceneReceiver : INetworkClientSceneBehaviour, INetworkMessageReceiver
    {
    }

    public class NetworkClientSceneController : INetworkClientDelegate, INetworkMessageReceiver, IDisposable
    {
        INetworkClient _client;
        NetworkScene _scene;
        INetworkClientSceneReceiver _receiver;
        List<INetworkClientSceneBehaviour> _sceneBehaviours;

        NetworkScene _clientScene;
        int _lastAppliedAction;
        Dictionary<int, object> _pendingActions;

        NetworkSceneActionHandler _actionHandler;
        TypedWriteSerializer _actionSerializer;

        public NetworkClientSceneController(INetworkClient client)
        {
            _sceneBehaviours = new List<INetworkClientSceneBehaviour>();
            _pendingActions = new Dictionary<int, object>();
            _actionHandler = new NetworkSceneActionHandler();
            _actionSerializer = new TypedWriteSerializer();

            _client = client;
            _client.AddDelegate(this);
            _client.RegisterReceiver(this);
        }

        public virtual void Dispose()
        {
            _client.RemoveDelegate(this);
            _client.RegisterReceiver(null);
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
            _scene = null;
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            _scene = null;
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(data.MessageType == SceneMsgType.UpdateSceneEvent)
            {
                if(_scene == null)
                {
                    _scene = NetworkSceneParser.Instance.Parse(reader);
                    _clientScene = new NetworkScene(_scene);
                }
                else
                {
                    _scene = NetworkSceneParser.Instance.Parse(_scene, reader);
                    _clientScene = new NetworkScene(_scene);
                    int lastServerAction = reader.ReadInt32();
                    OnActionFromServer(lastServerAction);
                }

                UpdateSceneView();
            }
            else if(data.MessageType == SceneMsgType.InstantiateObjectEvent)
            {
                var ev = reader.Read<InstantiateNetworkGameObjectEvent>();
                for(var i = 0; i < _sceneBehaviours.Count; i++)
                {
                    _sceneBehaviours[i].OnInstantiateObject(ev.ObjectId, ev.Transform);
                }
                InstantiateObjectView(ev);
            }
            else if(data.MessageType == SceneMsgType.DestroyObjectEvent)
            {
                var ev = reader.Read<DestroyNetworkGameObjectEvent>();
                for(var i = 0; i < _sceneBehaviours.Count; i++)
                {
                    _sceneBehaviours[i].OnDestroyObject(ev.ObjectId);
                }
                DestroyObjectView(ev);
            }
            else
            {
                if(_receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
            }
        }

        void UpdateSceneView()
        {
            var itr = _clientScene.GetObjectEnumerator();
            while(itr.MoveNext())
            {
                var go = itr.Current;
                UpdateObjectView(go.Id, go.Transform);
            }
            itr.Dispose();
        }

        virtual protected void UpdateObjectView(int objectId, Transform t)
        {
        }

        virtual protected void InstantiateObjectView(InstantiateNetworkGameObjectEvent ev)
        {
        }

        virtual protected void DestroyObjectView(DestroyNetworkGameObjectEvent ev)
        {
        }

        virtual protected void OnError(SocialPoint.Base.Error err)
        {
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
            OnError(err);
        }

        public void RegisterReceiver(INetworkClientSceneReceiver receiver)
        {
            if(receiver == null)
            {
                _sceneBehaviours.Remove(_receiver);
            }
            else
            {
                if(!_sceneBehaviours.Contains(receiver))
                {
                    _sceneBehaviours.Add(receiver);
                }
            }
            _receiver = receiver;
        }

        public void AddBehaviour(INetworkClientSceneBehaviour behaviour)
        {
            _sceneBehaviours.Add(behaviour);
        }

        public void RemoveBehaviour(INetworkClientSceneBehaviour behaviour)
        {
            _sceneBehaviours.Remove(behaviour);
        }
            
        public void ApplyAction(object action)
        {
            _lastAppliedAction++;
            _pendingActions.Add(_lastAppliedAction, action);
            if(ApplyActionToScene(action))
            {
                UpdateSceneView();
            }

            byte msgType;
            if(_actionSerializer.FindCode(action, out msgType))
            {
                var msg = _client.CreateMessage(new NetworkMessageData {
                    MessageType = msgType
                });
                _actionSerializer.Serialize(action, msg.Writer);
                msg.Send();
            }
        }

        bool ApplyActionToScene(object action)
        {
            return _actionHandler.HandleAction(_clientScene, action);
        }

        public void RegisterAction<T>(byte msgType, Action<NetworkScene, T> callback=null) where T : INetworkShareable
        {
            if(callback != null)
            {
                _actionHandler.Register(callback);
            }
            _actionSerializer.Register<T>(msgType);
        }

        public void RegisterAction<T>(byte msgType, Action<NetworkScene, T> callback, IWriteSerializer<T> serializer)
        {
            _actionHandler.Register(callback);
            _actionSerializer.Register<T>(msgType, serializer);
        }

        public void RegisterAction<T>(byte msgType, IActionHandler<NetworkScene, T> handler) where T : INetworkShareable
        {
            _actionHandler.Register(handler);
            _actionSerializer.Register<T>(msgType);
        }

        public void RegisterAction<T>(byte msgType, IActionHandler<NetworkScene, T> handler, IWriteSerializer<T> serializer)
        {
            _actionHandler.Register(handler);
            _actionSerializer.Register<T>(msgType, serializer);
        }

        public void UnregisterAction<T>()
        {
            _actionSerializer.Unregister<T>();
            _actionHandler.Unregister<T>();
        }

        public void OnActionFromServer(int lastServerAction)
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
                ApplyActionToScene(itr.Current.Value);
            }
            itr.Dispose();
        }
    }
}