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
        Dictionary<byte, KeyValuePair<List<ObjectBehaviour>, List<Type>>> _behaviourPrototypes = new Dictionary<byte, KeyValuePair<List<ObjectBehaviour>, List<Type>>>();
        List<ObjectBehaviour> _genericBehaviourPrototypes = new List<ObjectBehaviour>();
        List<Type> _genericBehaviourPrototypesTypes = new List<Type>();

        Action<float> _lateUpdateCallback;

        abstract public INetworkMessage CreateMessage(NetworkMessageData data);

        readonly NetworkSceneContext _context = new NetworkSceneContext();
        public NetworkSceneContext Context
        {
            get
            {
                SocialPoint.Base.DebugUtils.Assert(_context != null);
                return _context;
            }
        }

        protected void Init(NetworkScene scene)
        {
            _activeScene = scene;
            _actions = new NetworkActionHandler(_activeScene, this);
        }

        protected void UnregisterAllBehaviours()
        {
            //#warning we should destroy properly old behaviours
            var behaviourEnum = _behaviourPrototypes.GetEnumerator();
            while(behaviourEnum.MoveNext() != false)
            {
                var behaviourList = behaviourEnum.Current.Value;
                for(int i = 0; i < behaviourList.Key.Count; ++i)
                {
                    behaviourList.Key[i].OnDestroy();
                }
            }
            for(int i = 0; i < _genericBehaviourPrototypes.Count; ++i)
            {
                _genericBehaviourPrototypes[i].OnDestroy();
            }
            _behaviourPrototypes = new Dictionary<byte, KeyValuePair<List<ObjectBehaviour>, List<Type>>>();
            _genericBehaviourPrototypes = new List<ObjectBehaviour>();
            _genericBehaviourPrototypesTypes = new List<Type>();
        }

        protected void SetupObject(GameObject go)
        {
            go.TypedBehaviours.OnAdded += RegisterSpecialCallback;
            go.TypedBehaviours.OnRemoved += UnregisterSpecialCallback;
            go.SyncGroupChanged += OnObjectSyncGroupChanged;

            KeyValuePair<List<ObjectBehaviour>, List<Type>> behaviourPrototypesAndTypes;
            go.AddClonedBehaviours(_genericBehaviourPrototypes, _genericBehaviourPrototypesTypes);
            if(_behaviourPrototypes.TryGetValue(go.Type, out behaviourPrototypesAndTypes))
            {
                go.AddClonedBehaviours(behaviourPrototypesAndTypes.Key, behaviourPrototypesAndTypes.Value);
            }
        }

        protected virtual void OnObjectSyncGroupChanged(NetworkGameObject obj)
        {
            _activeScene.AddObjectInSyncGroup(obj);
        }

        protected void SetupObjectToDestroy(int id)
        {
            SetupObjectToDestroy(FindObject(id));
        }

        protected void SetupObjectToDestroy(GameObject go)
        {
            go.TypedBehaviours.OnAdded -= RegisterSpecialCallback;
            go.TypedBehaviours.OnRemoved -= UnregisterSpecialCallback;
            go.SyncGroupChanged -= OnObjectSyncGroupChanged;

            UnregisterSpecialCallbacks(go);
        }

        public GameObject FindObject(int id)
        {
            return _activeScene.FindObject(id) as GameObject;
        }

        public IEnumerator<GameObject> GetObjectEnumerator()
        {
            var tmp = Context.Pool.Get<List<NetworkGameObject>>();
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
            Context.Pool.Return(tmp);
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

        public void RegisterBehaviour(NetworkGameObject gameObjectPrefab, ObjectBehaviour behaviour)
        {
            behaviour.GameObject = gameObjectPrefab;
            _genericBehaviourPrototypes.Add(behaviour);
            _genericBehaviourPrototypesTypes.Add(behaviour.GetType());
        }

        public void RegisterBehaviour(byte objType, NetworkGameObject gameObjectPrefab, ObjectBehaviour behaviour)
        {
            KeyValuePair<List<ObjectBehaviour>, List<Type>> behavioursAndTypes;
            if(!_behaviourPrototypes.TryGetValue(objType, out behavioursAndTypes))
            {
                var behaviours = new List<ObjectBehaviour>();
                var types = new List<Type>();
                behavioursAndTypes = new KeyValuePair<List<ObjectBehaviour>, List<Type>>(behaviours, types);

                _behaviourPrototypes[objType] = behavioursAndTypes;
            }
            behaviour.GameObject = gameObjectPrefab;
            behavioursAndTypes.Key.Add(behaviour);
            behavioursAndTypes.Value.Add(behaviour.GetType());
        }

        public void RegisterBehaviours(byte objType, NetworkGameObject gameObjectPrefab, ObjectBehaviour[] behaviours)
        {
            for(var i = 0; i < behaviours.Length; i++)
            {
                RegisterBehaviour(objType, gameObjectPrefab, behaviours[i]);
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
            var tmp = Context.Pool.Get<List<ObjectBehaviour>>();
            var itr = go.TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                UnregisterSpecialCallback(itr.Current);
            }
            Context.Pool.Return(tmp);
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
