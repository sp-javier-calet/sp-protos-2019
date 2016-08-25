using System;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;

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

        /*NetworkScene _clientScene;
        Dictionary<int, object> _pendingActions;
        int _lastAppliedAction;*/

        public NetworkClientSceneController(INetworkClient client)
        {
            _client = client;
            _client.AddDelegate(this);
            _client.RegisterReceiver(this);
            _sceneBehaviours = new List<INetworkClientSceneBehaviour>();

            //_pendingActions = new Dictionary<int, object>();
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
                }
                else
                {
                    _scene = NetworkSceneParser.Instance.Parse(_scene, reader);
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

        virtual protected void UpdateObjectView(int objectId, Transform t)
        {
        }

        virtual protected void InstantiateObjectView(InstantiateNetworkGameObjectEvent ev)
        {
        }

        virtual protected void DestroyObjectView(DestroyNetworkGameObjectEvent ev)
        {
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
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