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

        NetworkScene _clientScene;
        int _lastAppliedAction;
        Dictionary<int, NetworkActionTuple> _pendingActions;
        Dictionary<Type, List<INetworkActionDelegate>> _actionDelegates;

        public NetworkClientSceneController(INetworkClient client)
        {
            _client = client;
            _client.AddDelegate(this);
            _client.RegisterReceiver(this);
            _sceneBehaviours = new List<INetworkClientSceneBehaviour>();

            _pendingActions = new Dictionary<int, NetworkActionTuple>();
            _actionDelegates = new Dictionary<Type, List<INetworkActionDelegate>>();
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
                    _clientScene = new NetworkScene(_scene);
                }
                else
                {
                    int lastServerAction = reader.ReadInt32();//Read last action before reading scene
                    _scene = NetworkSceneParser.Instance.Parse(_scene, reader);
                    _clientScene = new NetworkScene(_scene);
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
            var itr = _clientScene.GetObjectEnumerator();//TODO: Previously was with _scene, now with _clientScene
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

        public void ApplyActionToScene<T>(T action)
        {
            ApplyActionToScene(typeof(T), action);
        }

        void ApplyActionToScene(Type actionType, object action)
        {
            _lastAppliedAction++;
            _pendingActions.Add(_lastAppliedAction, new NetworkActionTuple(actionType, action));
            if(ApplyActionToScene(new NetworkActionTuple(actionType, action), _clientScene))
            {
                //TODO: Apply game object changes with _clientScene (only if modified). NEEDED? or use UpdateSceneView in Update
                UpdateSceneView();
            }

            //TODO: Send activation to server
        }

        bool ApplyActionToScene(NetworkActionTuple actionTuple, NetworkScene scene)
        {
            return NetworkActionUtils.ApplyAction(actionTuple, _actionDelegates, scene);
        }

        public void RegisterAction(Type actionType, INetworkActionDelegate callback)
        {
            NetworkActionUtils.RegisterAction(actionType, callback, _actionDelegates);
        }

        public void OnActionFromServer(int lastServerAction)
        {
            /*NetworkActionTuple actionTuple;
            if(_pendingActions.TryGetValue(lastServerAction, out actionTuple))
            {
                //TODO: NEEDED? In Update we are parsing the whole scene. We should do it before calling this function
                //ApplyActionToScene(actionTuple, _scene);
                //Copy _scene into _clientScene
                //_clientScene = new NetworkScene(_scene);
            }*/

            //Remove pending actions with id or lower
            RemoveOldPendingActions(lastServerAction);

            //Reapply client prediction
            ApplyAllPendingActions();
        }

        void RemoveOldPendingActions(int fromAction)
        {
            bool olderActionsRemoved = false;
            while(olderActionsRemoved)
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
                NetworkActionTuple actionTuple = itr.Current.Value;
                ApplyActionToScene(actionTuple, _clientScene);
            }
            itr.Dispose();
        }
    }
}