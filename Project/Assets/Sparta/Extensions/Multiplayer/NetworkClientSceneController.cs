using System;
using System.Collections.Generic;
using SocialPoint.IO;

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
        IParser<NetworkScene> _sceneParser;
        IParser<InstantiateNetworkGameObjectEvent> _instParser;
        IParser<DestroyNetworkGameObjectEvent> _destParser;
        INetworkClientSceneReceiver _receiver;
        List<INetworkClientSceneBehaviour> _sceneBehaviours;

        public NetworkClientSceneController(INetworkClient client)
        {
            _client = client;
            _client.AddDelegate(this);
            _client.RegisterReceiver(this);
            _sceneParser = new NetworkGameSceneParser();
            _instParser = new InstantiateNetworkGameObjectEventParser();
            _destParser = new DestroyNetworkGameObjectEventParser();
            _sceneBehaviours = new List<INetworkClientSceneBehaviour>();
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

        void INetworkClientDelegate.OnConnected()
        {
            _scene = null;
        }

        void INetworkClientDelegate.OnDisconnected()
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
                    _scene = _sceneParser.Parse(reader);
                }
                else
                {
                    _scene = _sceneParser.Parse(_scene, reader);
                }
                var itr = _scene.GetObjectEnumerator();
                while(itr.MoveNext())
                {
                    var go = itr.Current;
                    UpdateObjectView(go.Id, go.Transform);
                }
                itr.Dispose();
            }
            else if(data.MessageType == SceneMsgType.InstantiateObjectEvent)
            {
                var ev = _instParser.Parse(reader);
                for(var i = 0; i < _sceneBehaviours.Count; i++)
                {
                    _sceneBehaviours[i].OnInstantiateObject(ev.ObjectId, ev.Transform);
                }
                InstantiateObjectView(ev);
            }
            else if(data.MessageType == SceneMsgType.DestroyObjectEvent)
            {
                var ev = _destParser.Parse(reader);
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

        virtual protected void UpdateObjectView(int objectId, Transform t)
        {
        }

        virtual protected void InstantiateObjectView(InstantiateNetworkGameObjectEvent ev)
        {
        }

        virtual protected void DestroyObjectView(DestroyNetworkGameObjectEvent ev)
        {
        }

        void INetworkClientDelegate.OnError(SocialPoint.Base.Error err)
        {
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

    }
}