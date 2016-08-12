using System;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public static class MsgType
    {
        public const byte UpdateSceneEvent = 1;
        public const byte InstantiateObjectEvent = 2;
        public const byte DestroyObjectEvent = 3;
        public const byte Highest = 2;
    }

    public interface INetworkServerSceneBehaviour
    {
        void Update(float dt, NetworkGameScene scene, NetworkGameScene oldScene);
        void OnClientConnected(byte clientId);
        void OnClientDisconnected(byte clientId);
        void OnMessageReceived(byte clientId, ReceivedNetworkMessage msg);
    }

    public class NetworkServerSceneController : INetworkServerDelegate, IDisposable
    {
        NetworkGameScene _scene;
        NetworkGameScene _oldScene;
        INetworkServer _server;
        ISerializer<NetworkGameScene> _sceneSerializer;
        ISerializer<InstantiateNetworkGameObjectEvent> _instSerializer;
        ISerializer<DestroyNetworkGameObjectEvent> _destSerializer;
        List<INetworkServerSceneBehaviour> _sceneBehaviours;
        Dictionary<int,List<INetworkBehaviour>> _behaviours;
        Dictionary<string,List<INetworkBehaviour>> _behaviourPrototypes;

        public NetworkGameScene Scene
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
        }

        public void Dispose()
        {
            _server.RemoveDelegate(this);
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

        void INetworkServerDelegate.OnStarted()
        {
            _scene = new NetworkGameScene();
            _oldScene = null;
        }

        void INetworkServerDelegate.OnStopped()
        {
            _scene = null;
            _oldScene = null;
        }

        public void Update(float dt)
        {
            if(_scene == null)
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
            var oldScene = new NetworkGameScene(_oldScene);
            for(var i = 0; i < _sceneBehaviours.Count; i++)
            {
                _sceneBehaviours[i].Update(dt, _scene, oldScene);
            }

            var msg = _server.CreateMessage(new NetworkMessageDest {
                MessageType = MsgType.UpdateSceneEvent
            });
            if(_oldScene == null)
            {
                _sceneSerializer.Serialize(_scene, msg.Writer);
            }
            else
            {
                _sceneSerializer.Serialize(_scene, _oldScene, msg.Writer);
            }
            msg.Send();
            _oldScene = new NetworkGameScene(_scene);
        }
            
        public NetworkGameObject Instantiate(string prefabName, Transform trans, INetworkBehaviour[] newBehaviours=null)
        {
            var go = new NetworkGameObject(_scene.FreeObjectId, trans);
            _scene.AddObject(go);

            var msg = _server.CreateMessage(new NetworkMessageDest {
                MessageType = MsgType.InstantiateObjectEvent
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
            var msg = _server.CreateMessage(new NetworkMessageDest {
                MessageType = MsgType.DestroyObjectEvent
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
            var msg = _server.CreateMessage(new NetworkMessageDest {
                ClientId = clientId,
                MessageType = MsgType.UpdateSceneEvent
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

        void INetworkServerDelegate.OnMessageReceived(byte clientId, ReceivedNetworkMessage msg)
        {
            for(var i = 0; i < _sceneBehaviours.Count; i++)
            {
                _sceneBehaviours[i].OnMessageReceived(clientId, msg);
            }
        }

        void INetworkServerDelegate.OnError(SocialPoint.Base.Error err)
        {
            
        }

    }
}