using System;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class NetworkServerSceneController : INetworkServerDelegate, IDisposable
    {
        NetworkGameScene _scene;
        NetworkGameScene _oldScene;
        INetworkServer _server;
        ISerializer<NetworkGameScene> _sceneSerializer;
        ISerializer<InstantiateEvent> _instSerializer;
        ISerializer<DestroyEvent> _destSerializer;
        Dictionary<int,List<INetworkGameBehaviour>> _behaviours;

        public NetworkGameScene Scene
        {
            get
            {
                return _scene;
            }
        }

        public NetworkServerSceneController(INetworkServer server)
        {
            _behaviours = new Dictionary<int,List<INetworkGameBehaviour>>();
            _sceneSerializer = new NetworkGameSceneSerializer();
            _instSerializer = new InstantiateEventSerializer();
            _destSerializer = new DestroyEventSerializer();
            _server = server;
            _server.AddDelegate(this);
        }

        public void Dispose()
        {
            _server.RemoveDelegate(this);
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
            var msg = _server.CreateMessage(new NetworkMessageInfo {
                MessageType = NetworkGameScene.MessageType
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
            
        public NetworkGameObject Instantiate(string prefabName, Transform trans)
        {
            var go = new NetworkGameObject(_scene.FreeObjectId, trans);
            _scene.AddObject(go);

            var msg = _server.CreateMessage(new NetworkMessageInfo {
                MessageType = InstantiateEvent.MessageType
            });
            _instSerializer.Serialize(new InstantiateEvent {
                ObjectId = go.Id,
                PrefabName = prefabName,
                Transform = trans
            }, msg.Writer);
            msg.Send();

            List<INetworkGameBehaviour> behaviours;
            if(_behaviours.TryGetValue(go.Id, out behaviours))
            {
                for(var i = 0; i < behaviours; i++)
                {
                    behaviours[i].OnStart(go);
                }
            }

            return go;
        }

        public NetworkGameObject Instantiate(string prefabName)
        {
            return Instantiate(prefabName, Transform.Identity);
        }

        public void Destroy(int id)
        {
            if(!_scene.RemoveObject(id))
            {
                return;
            }
            var msg = _server.CreateMessage(new NetworkMessageInfo {
                MessageType = DestroyEvent.MessageType
            });
            _destSerializer.Serialize(new DestroyEvent {
                ObjectId = id
            }, msg.Writer);
            msg.Send();

            List<INetworkGameBehaviour> behaviours;
            if(_behaviours.TryGetValue(id, out behaviours))
            {
                for(var i = 0; i < behaviours; i++)
                {
                    behaviours[i].OnDestroy();
                }
                _behaviours.Remove(id);
            }
        }

        public void AddBehaviours(int id, INetworkGameBehaviour[] newBehaviours)
        {
            var behaviours = GetOrCreateBehaviours(id);
            behaviours.AddRange(newBehaviours);
        }

        public void AddBehaviour(int id, INetworkGameBehaviour newBehaviour)
        {
            var behaviours = GetOrCreateBehaviours(id);
            behaviours.Add(newBehaviour);
        }

        List<INetworkGameBehaviour> GetOrCreateBehaviours(int id)
        {
            List<INetworkGameBehaviour> behaviours;
            if(!_behaviours.TryGetValue(id, out behaviours))
            {
                behaviours = new List<INetworkGameBehaviour>();
                _behaviours[id] = behaviours;
            }
            return behaviours;
        }


        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            var msg = _server.CreateMessage(new NetworkMessageInfo {
                ClientId = clientId,
                MessageType = NetworkGameScene.MessageType
            });
            _sceneSerializer.Serialize(_scene, msg.Writer);
            msg.Send();
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            
        }

        void INetworkServerDelegate.OnMessageReceived(byte clientId, ReceivedNetworkMessage msg)
        {
            
        }

        void INetworkServerDelegate.OnError(SocialPoint.Base.Error err)
        {
            
        }

    }
}