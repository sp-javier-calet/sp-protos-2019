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
        Dictionary<int,List<INetworkBehaviour>> _behaviours;
        Dictionary<string,List<INetworkBehaviour>> _behaviourPrototypes;
        INetworkServerSceneReceiver _receiver;

        int _lastReceivedAction;
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

            var msg = _server.CreateMessage(new NetworkMessageData {
                MessageType = SceneMsgType.UpdateSceneEvent
            });
            //TODO: Write action to avoid sending it through actionackevent (delete that class) msg.Writer.Write(_lastReceivedAction);
            NetworkSceneSerializer.Instance.Serialize(_scene, _oldScene, msg.Writer);
            msg.Send();
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

        public void OnAction(Type actionType, object action)
        {
            _lastReceivedAction++;
            ApplyActionToScene(new NetworkActionTuple(actionType, action), _scene);
            //TODO: Apply game object changes with _scene (if modified?)
            //TODO: Send message to client with last received action
        }

        void ApplyActionToScene(NetworkActionTuple actionTuple, NetworkScene scene)
        {
            NetworkActionUtils.ApplyAction(actionTuple, _actionDelegates, scene);
        }

        public void RegisterAction(Type actionType, INetworkActionDelegate callback)
        {
            List<INetworkActionDelegate> callbacksList;
            if(_actionDelegates.TryGetValue(actionType, out callbacksList))
            {
                callbacksList.Add(callback);
            }
            else
            {
                callbacksList = new List<INetworkActionDelegate>();
                callbacksList.Add(callback);
                _actionDelegates.Add(actionType, callbacksList);
            }
        }
    }
}