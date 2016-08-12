﻿using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerSceneBehaviour
    {
        void Update(float dt, NetworkScene scene, NetworkScene oldScene);
        void OnClientConnected(byte clientId);
        void OnClientDisconnected(byte clientId);
    }

    public class NetworkServerSceneController : INetworkServerDelegate, INetworkMessageReceiver, IDisposable
    {
        NetworkScene _scene;
        NetworkScene _oldScene;
        INetworkServer _server;
        ISerializer<NetworkScene> _sceneSerializer;
        ISerializer<InstantiateNetworkGameObjectEvent> _instSerializer;
        ISerializer<DestroyNetworkGameObjectEvent> _destSerializer;
        List<INetworkServerSceneBehaviour> _sceneBehaviours;
        Dictionary<int,List<INetworkBehaviour>> _behaviours;
        Dictionary<string,List<INetworkBehaviour>> _behaviourPrototypes;
        INetworkMessageReceiver _receiver;

        public NetworkScene Scene
        {
            get
            {
                return _scene;
            }
        }

        public INetworkServer Server
        {
            get
            {
                return _server;
            }
        }

        public float UpdateInterval = 0.0f;
        float _timeSinceListUpdate = 0.0f;

        public NetworkServerSceneController(INetworkServer server)
        {
            _behaviours = new Dictionary<int,List<INetworkBehaviour>>();
            _sceneSerializer = new NetworkGameSceneSerializer();
            _instSerializer = new InstantiateNetworkGameObjectEventSerializer();
            _destSerializer = new DestroyNetworkGameObjectEventSerializer();
            _sceneBehaviours = new List<INetworkServerSceneBehaviour>();
            _behaviours = new Dictionary<int,List<INetworkBehaviour>>();
            _behaviourPrototypes = new Dictionary<string,List<INetworkBehaviour>>();
            _server = server;
            _server.AddDelegate(this);
            _server.RegisterReceiver(this);
        }

        public void Dispose()
        {
            _server.RemoveDelegate(this);
            _server.RegisterReceiver(null);
        }

        public void AddBehaviour(INetworkServerSceneBehaviour behaviour)
        {
            _sceneBehaviours.Add(behaviour);
        }

        public void RemoveBehaviour(INetworkServerSceneBehaviour behaviour)
        {
            _sceneBehaviours.Remove(behaviour);
        }

        public void AddBehaviour(string prefabName, INetworkBehaviour behaviour)
        {
            List<INetworkBehaviour> behaviours;
            if(!_behaviourPrototypes.TryGetValue(prefabName, out behaviours))
            {
                behaviours = new List<INetworkBehaviour>();
                _behaviourPrototypes[prefabName] = behaviours;
            }
            behaviours.Add(behaviour);
        }

        public void RemoveBehaviour(INetworkBehaviour behaviour)
        {
            {
                var itr = _behaviourPrototypes.GetEnumerator();
                while(itr.MoveNext())
                {
                    var behaviours = itr.Current.Value;
                    behaviours.Remove(behaviour);
                }
                itr.Dispose();
            }
            {
                var itr = _behaviours.GetEnumerator();
                while(itr.MoveNext())
                {
                    var behaviours = itr.Current.Value;
                    behaviours.Remove(behaviour);
                }
                itr.Dispose();
            }
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        void INetworkServerDelegate.OnStarted()
        {
            _scene = new NetworkScene();
            _oldScene = new NetworkScene();
        }

        void INetworkServerDelegate.OnStopped()
        {
            _scene = null;
            _oldScene = null;
        }

        public void Update(float dt)
        {
            if(!_server.Running)
            {
                return;
            }
            if(UpdateInterval <= 0.0f)
            {
                UpdateScene(dt);
            }
            else
            {
                _timeSinceListUpdate += dt;
                while(_timeSinceListUpdate > UpdateInterval)
                {
                    UpdateScene(UpdateInterval);
                    _timeSinceListUpdate -= UpdateInterval;
                }
            }
        }

        void UpdateScene(float dt)
        {
            var itr = _behaviours.GetEnumerator();
            while(itr.MoveNext())
            {
                var behaviours = itr.Current.Value;
                for(var i = 0; i < behaviours.Count; i++)
                {
                    behaviours[i].Update(dt);
                }
            }
            itr.Dispose();

            // copy old scene so that the behaviours cannot change it
            var oldScene = new NetworkScene(_oldScene);
            for(var i = 0; i < _sceneBehaviours.Count; i++)
            {
                _sceneBehaviours[i].Update(dt, _scene, oldScene);
            }

            var msg = _server.CreateMessage(new NetworkMessageData {
                MessageType = SceneMsgType.UpdateSceneEvent
            });
            _sceneSerializer.Serialize(_scene, _oldScene, msg.Writer);
            msg.Send();
            _oldScene = new NetworkScene(_scene);
        }            
            
        public NetworkGameObject Instantiate(string prefabName, Transform trans, INetworkBehaviour[] newBehaviours=null)
        {
            var go = new NetworkGameObject(_scene.FreeObjectId, trans);
            _scene.AddObject(go);

            var msg = _server.CreateMessage(new NetworkMessageData {
                MessageType = SceneMsgType.InstantiateObjectEvent
            });
            _instSerializer.Serialize(new InstantiateNetworkGameObjectEvent {
                ObjectId = go.Id,
                PrefabName = prefabName,
                Transform = trans
            }, msg.Writer);
            msg.Send();

            var behaviours = new List<INetworkBehaviour>();
            if(newBehaviours != null)
            {
                behaviours.AddRange(newBehaviours);
            }
            _behaviours[go.Id] = behaviours;
            List<INetworkBehaviour> behaviourPrototypes;
            if(_behaviourPrototypes.TryGetValue(prefabName, out behaviourPrototypes))
            {
                for(var i = 0; i < behaviourPrototypes.Count; i++)
                {
                    behaviours.Add((INetworkBehaviour)behaviourPrototypes[i].Clone());
                }
            }
            for(var i = 0; i < behaviours.Count; i++)
            {
                behaviours[i].OnStart(go);
            }
            return go;
        }

        public NetworkGameObject Instantiate(string prefabName, INetworkBehaviour[] behaviours=null)
        {
            return Instantiate(prefabName, Transform.Identity, behaviours);
        }

        public void Destroy(int id)
        {
            if(!_scene.RemoveObject(id))
            {
                return;
            }
            var msg = _server.CreateMessage(new NetworkMessageData {
                MessageType = SceneMsgType.DestroyObjectEvent
            });
            _destSerializer.Serialize(new DestroyNetworkGameObjectEvent {
                ObjectId = id
            }, msg.Writer);
            msg.Send();

            List<INetworkBehaviour> behaviours;
            if(_behaviours.TryGetValue(id, out behaviours))
            {
                for(var i = 0; i < behaviours.Count; i++)
                {
                    behaviours[i].OnDestroy();
                }
                _behaviours.Remove(id);
            }
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            for(var i = 0; i < _sceneBehaviours.Count; i++)
            {
                _sceneBehaviours[i].OnClientConnected(clientId);
            }
            var msg = _server.CreateMessage(new NetworkMessageData {
                ClientId = clientId,
                MessageType = SceneMsgType.UpdateSceneEvent
            });
            _sceneSerializer.Serialize(_scene, msg.Writer);
            msg.Send();
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            for(var i = 0; i < _sceneBehaviours.Count; i++)
            {
                _sceneBehaviours[i].OnClientDisconnected(clientId);
            }
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkServerDelegate.OnError(SocialPoint.Base.Error err)
        {
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }
    }
}