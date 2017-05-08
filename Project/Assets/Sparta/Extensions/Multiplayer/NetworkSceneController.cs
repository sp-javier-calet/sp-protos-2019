using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public interface INetworkSceneController
    {
        NetworkScene<INetworkSceneBehaviour> Scene{ get; }

        NetworkGameObject InstantiateLocal(byte objType, Transform trans = null);

        void ApplyAction(object evnt);

        void ApplyActionSync(object evnt);

        void ApplyActionLocal(object evnt);

        void Destroy(int id);
    }

    public abstract class NetworkSceneController<GameObject, ObjectBehaviour> : INetworkMessageSender where GameObject : NetworkGameObject<ObjectBehaviour> where ObjectBehaviour : class, INetworkBehaviour
    {
        protected NetworkActionHandler _actions;
        protected NetworkScene _activeScene;
        Dictionary<byte, List<ObjectBehaviour>> _behaviourPrototypes = new Dictionary<byte, List<ObjectBehaviour>>();
        List<ObjectBehaviour> _genericBehaviourPrototypes = new List<ObjectBehaviour>();

        Action<float> _lateUpdateCallback;

        abstract public INetworkMessage CreateMessage(NetworkMessageData data);

        protected void Init(NetworkScene scene)
        {
            _activeScene = scene;
            _actions = new NetworkActionHandler(_activeScene, this);
        }

        protected void UnregisterAllBehaviours()
        {
            var behaviourEnum = _behaviourPrototypes.GetEnumerator();
            while(behaviourEnum.MoveNext() != false)
            {
                var behaviourList = behaviourEnum.Current.Value;
                for(int i = 0; i < behaviourList.Count; ++i)
                {
                    behaviourList[i].OnDestroy();
                }
            }
            for(int i = 0; i < _genericBehaviourPrototypes.Count; ++i)
            {
                _genericBehaviourPrototypes[i].OnDestroy();
            }
            _behaviourPrototypes = new Dictionary<byte, List<ObjectBehaviour>>();
            _genericBehaviourPrototypes = new List<ObjectBehaviour>();
        }

        protected void SetupObject(GameObject go)
        {
            go.TypedBehaviours.OnAdded += RegisterSpecialCallback;
            go.TypedBehaviours.OnRemoved += UnregisterSpecialCallback;

            List<ObjectBehaviour> behaviourPrototypes;
            go.AddClonedBehaviours(_genericBehaviourPrototypes);
            if(_behaviourPrototypes.TryGetValue(go.Type, out behaviourPrototypes))
            {
                go.AddClonedBehaviours(behaviourPrototypes);
            }
        }

        protected void SetupObjectToDestroy(int id)
        {
            SetupObjectToDestroy(FindObject(id));
        }

        protected void SetupObjectToDestroy(GameObject go)
        {
            go.TypedBehaviours.OnAdded -= RegisterSpecialCallback;
            go.TypedBehaviours.OnRemoved -= UnregisterSpecialCallback;

            UnregisterSpecialCallbacks(go);
        }

        public GameObject FindObject(int id)
        {
            return _activeScene.FindObject(id) as GameObject;
        }

        public IEnumerator<GameObject> GetObjectEnumerator()
        {
            var tmp = ObjectPool.Get<List<NetworkGameObject>>();
            var itr = _activeScene.GetObjectEnumerator(tmp);
            while(itr.MoveNext())
            {
                var go = itr.Current as GameObject;
                if(go != null)
                {
                    yield return go;
                }
            }
            itr.Dispose();
            ObjectPool.Return(tmp);
        }

        protected void UpdateObjects(float dt)
        {
            var itr = GetObjectEnumerator();
            while(itr.MoveNext())
            {
                itr.Current.Update(dt);
            }
            itr.Dispose();
        }

        protected void LateUpdateObjects(float dt)
        {
            if(_lateUpdateCallback != null)
            {
                _lateUpdateCallback(dt);
            }
        }

        protected virtual void UpdatePendingLogic()
        {
            var itr = GetObjectEnumerator();
            while(itr.MoveNext())
            {
                itr.Current.UpdatePendingLogic();
            }
            itr.Dispose();
        }

        public void RegisterBehaviour(ObjectBehaviour behaviour)
        {
            _genericBehaviourPrototypes.Add(behaviour);
        }

        public void RegisterBehaviour(byte objType, ObjectBehaviour behaviour)
        {
            List<ObjectBehaviour> behaviours;
            if(!_behaviourPrototypes.TryGetValue(objType, out behaviours))
            {
                behaviours = new List<ObjectBehaviour>();
                _behaviourPrototypes[objType] = behaviours;
            }
            behaviours.Add(behaviour);
        }

        public void RegisterBehaviours(byte objType, ObjectBehaviour[] behaviours)
        {
            for(var i = 0; i < behaviours.Length; i++)
            {
                RegisterBehaviour(objType, behaviours[i]);
            }
        }

        public void RegisterAction<T>(byte msgType, Action<NetworkSceneMemento, T> callback = null) where T : INetworkShareable, new()
        {
            _actions.RegisterAction<T>(msgType, callback);
        }

        public void RegisterAction<T>(byte msgType, IActionHandler<NetworkSceneMemento, T> handler) where T : INetworkShareable, new()
        {
            _actions.RegisterAction<T>(msgType, handler);
        }

        public void UnregisterAction<T>()
        {
            _actions.UnregisterAction<T>();
        }

        void RegisterSpecialCallback(ObjectBehaviour behaviour)
        {
            ApplyIfCast<ILateUpdateable>(behaviour, (ILateUpdateable typedBehaviour) => {
                _lateUpdateCallback += typedBehaviour.LateUpdate;
            });
        }

        void UnregisterSpecialCallback(ObjectBehaviour behaviour)
        {
            ApplyIfCast<ILateUpdateable>(behaviour, (ILateUpdateable typedBehaviour) => {
                _lateUpdateCallback -= typedBehaviour.LateUpdate;
            });
        }

        void UnregisterSpecialCallbacks(GameObject go)
        {
            var tmp = ObjectPool.Get<List<ObjectBehaviour>>();
            var itr = go.TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                UnregisterSpecialCallback(itr.Current);
            }
            ObjectPool.Return(tmp);
            itr.Dispose();
        }

        static void ApplyIfCast<T>(ObjectBehaviour behaviour, Action<T> updateAction)  where T : class
        {
            var tBehaviour = behaviour as T;
            if(tBehaviour != null)
            {
                updateAction(tBehaviour);
            }
        }
    }
}
