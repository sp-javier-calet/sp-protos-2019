using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerSceneBehaviour
    {
        void Update(float dt, NetworkScene scene, NetworkScene oldScene);

        void OnClientConnected(byte clientId);

        void OnClientDisconnected(byte clientId);
    }

    public interface INetworkServerSceneReceiver : INetworkServerSceneBehaviour, INetworkMessageReceiver
    {
    }

    public class NetworkServerSceneController : INetworkServerDelegate, INetworkMessageReceiver, IDisposable
    {
        NetworkScene _scene;
        NetworkScene _oldScene;
        INetworkServer _server;
        List<INetworkServerSceneBehaviour> _sceneBehaviours;
        Dictionary<int, List<INetworkBehaviour>> _behaviours;
        Dictionary<string, List<INetworkBehaviour>> _behaviourPrototypes;
        INetworkServerSceneReceiver _receiver;

        Dictionary<byte, int> _lastReceivedAction;
        Dictionary<Type, List<INetworkActionDelegate>> _actionDelegates;

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
            _sceneBehaviours = new List<INetworkServerSceneBehaviour>();
            _behaviours = new Dictionary<int,List<INetworkBehaviour>>();
            _behaviourPrototypes = new Dictionary<string,List<INetworkBehaviour>>();
            _server = server;
            _server.AddDelegate(this);
            _server.RegisterReceiver(this);

            _lastReceivedAction = new Dictionary<byte, int>();
            _actionDelegates = new Dictionary<Type, List<INetworkActionDelegate>>();
        }

        public virtual void Dispose()
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

        public void AddBehaviour(int id, INetworkBehaviour behaviour)
        {
            var go = Scene.FindObject(id);
            if(go == null)
            {
                throw new InvalidOperationException("Could not find game object.");
            }
            List<INetworkBehaviour> behaviours;
            if(!_behaviours.TryGetValue(id, out behaviours))
            {
                behaviours = new List<INetworkBehaviour>();
                _behaviours[id] = behaviours;
            }
            behaviours.Add(behaviour);
            behaviour.OnStart(go);
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

        public void RegisterReceiver(INetworkServerSceneReceiver receiver)
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

        void INetworkServerDelegate.OnServerStarted()
        {
            OnServerStarted();
        }

        virtual protected void OnServerStarted()
        {
            _scene = new NetworkScene();
            _oldScene = new NetworkScene();
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            OnServerStopped();
        }

        virtual protected void OnServerStopped()
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
            // apply behaviours
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

            var memStream = new System.IO.MemoryStream();
            var binWriter = new SystemBinaryWriter(memStream);
            NetworkSceneSerializer.Instance.Serialize(_scene, _oldScene, binWriter);
            byte[] sceneBuffer = memStream.ToArray();

            var clientItr = _lastReceivedAction.GetEnumerator();
            while(clientItr.MoveNext())
            {
                byte clientId = clientItr.Current.Key;
                Int32 lastAction = (Int32)clientItr.Current.Value;

                var msg = _server.CreateMessage(new NetworkMessageData {
                    ClientId = clientId,
                    MessageType = SceneMsgType.UpdateSceneEvent
                });

                msg.Writer.Write(sceneBuffer, sceneBuffer.Length);
                msg.Writer.Write(lastAction);
                msg.Send();
            }
            clientItr.Dispose();

            _oldScene = new NetworkScene(_scene);
        }

        public NetworkGameObject Instantiate(string prefabName, Transform trans, INetworkBehaviour[] newBehaviours = null)
        {
            var go = new NetworkGameObject(_scene.FreeObjectId, trans);
            _scene.AddObject(go);

            _server.SendMessage(new NetworkMessageData {
                MessageType = SceneMsgType.InstantiateObjectEvent
            }, new InstantiateNetworkGameObjectEvent {
                ObjectId = go.Id,
                PrefabName = prefabName,
                Transform = trans
            });

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

        public NetworkGameObject Instantiate(string prefabName, INetworkBehaviour[] behaviours = null)
        {
            return Instantiate(prefabName, Transform.Identity, behaviours);
        }

        public void Destroy(int id)
        {
            if(!_scene.RemoveObject(id))
            {
                return;
            }
            _server.SendMessage(new NetworkMessageData {
                MessageType = SceneMsgType.DestroyObjectEvent
            }, new DestroyNetworkGameObjectEvent {
                ObjectId = id
            });

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
            _lastReceivedAction.Add(clientId, 0);
            for(var i = 0; i < _sceneBehaviours.Count; i++)
            {
                _sceneBehaviours[i].OnClientConnected(clientId);
            }
            var msg = _server.CreateMessage(new NetworkMessageData {
                ClientId = clientId,
                MessageType = SceneMsgType.UpdateSceneEvent
            });
            NetworkSceneSerializer.Instance.Serialize(_scene, msg.Writer);
            msg.Send();
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            _lastReceivedAction.Remove(clientId);
            for(var i = 0; i < _sceneBehaviours.Count; i++)
            {
                _sceneBehaviours[i].OnClientDisconnected(clientId);
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
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

        public void OnAction<T>(T action, byte clientId)
        {
            OnAction(typeof(T), action, clientId);
        }

        void OnAction(Type actionType, object action, byte clientId)
        {
            if(_lastReceivedAction.ContainsKey(clientId))
            {
                _lastReceivedAction[clientId]++;
            }
            ApplyActionToScene(actionType, action);
        }

        bool ApplyActionToScene(Type actionType, object action)
        {
            return NetworkActionUtils.ApplyAction(actionType, action, _actionDelegates, _scene);
        }

        public void RegisterActionDelegate<T>(Action<T, NetworkScene> callback)
        {
            NetworkActionUtils.RegisterActionDelegate<T>(callback, _actionDelegates);
        }

        public bool UnregisterActionDelegate<T>(Action<T, NetworkScene> callback)
        {
            return NetworkActionUtils.UnregisterActionDelegate<T>(callback, _actionDelegates);
        }
    }
}