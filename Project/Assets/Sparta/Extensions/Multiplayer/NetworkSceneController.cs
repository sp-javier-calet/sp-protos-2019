using System;
using System.Collections.Generic;
using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public interface INetworkSceneController
    {
        NetworkScene Scene{ get; }

        NetworkGameObject InstantiateLocal(byte objType, Transform trans = null);

        void ApplyAction(object evnt);

        void ApplyActionSync(object evnt);

        void ApplyActionLocal(object evnt);

        void DestroyObject(int id);
    }

    public abstract class NetworkSceneController<GameObject> : INetworkMessageSender, IDisposable where GameObject : NetworkGameObject
    {
        protected NetworkActionHandler _actions;
        protected NetworkScene _activeScene;
        Dictionary<byte, KeyValuePair<List<INetworkBehaviour>, List<Type>>> _behaviourPrototypes = new Dictionary<byte, KeyValuePair<List<INetworkBehaviour>, List<Type>>>();
        List<INetworkBehaviour> _genericBehaviourPrototypes = new List<INetworkBehaviour>();
        List<Type> _genericBehaviourPrototypesTypes = new List<Type>();

        Action<float> _lateUpdateCallback;

        abstract public INetworkMessage CreateMessage(NetworkMessageData data);

        readonly NetworkSceneContext _context;
        public NetworkSceneContext Context
        {
            get
            {
                SocialPoint.Base.DebugUtils.Assert(_context != null, "NetworkSceneContext doesn't exists for this NetworkSceneController");
                return _context;
            }
        }

        protected NetworkSceneController(NetworkSceneContext context)
        {
            _context = context;
        }

        protected void Init(NetworkScene scene)
        {
            _activeScene = scene;
            _actions = new NetworkActionHandler(_activeScene, this);
        }

        public virtual void Dispose()
        {
            if(_activeScene != null)
            {
                var objectsEnum = _activeScene.GetObjectEnumerator();
                while(objectsEnum.MoveNext())
                {
                    if(objectsEnum.Current == null)
                    {
                        continue;
                    }
                    objectsEnum.Current.OnDestroy();
                }
                objectsEnum.Dispose();

                _activeScene = null;
            }
            
            UnregisterAllBehaviours();

            _actions = null;

            _lateUpdateCallback = null;
        }

        protected void UnregisterAllBehaviours()
        {
            var behaviourEnum = _behaviourPrototypes.GetEnumerator();
            while(behaviourEnum.MoveNext() != false)
            {
                var behaviourList = behaviourEnum.Current.Value;
                for(int i = 0; i < behaviourList.Key.Count; ++i)
                {
                    var behavior = behaviourList.Key[i];
                    if(behavior == null)
                    {
                        continue;
                    }
                    behavior.OnDestroy();
                }
            }

            behaviourEnum = _behaviourPrototypes.GetEnumerator();
            while(behaviourEnum.MoveNext() != false)
            {
                var behaviourList = behaviourEnum.Current.Value;
                for(int i = 0; i < behaviourList.Key.Count; ++i)
                {
                    if(behaviourList.Key[i] == null)
                    {
                        continue;
                    }
                    behaviourList.Key[i].Dispose();
                }
            }

            for(int i = 0; i < _genericBehaviourPrototypes.Count; ++i)
            {
                if(_genericBehaviourPrototypes[i] == null)
                {
                    continue;
                }
                _genericBehaviourPrototypes[i].OnDestroy();
            }

            for(int i = 0; i < _genericBehaviourPrototypes.Count; ++i)
            {
                if(_genericBehaviourPrototypes[i] == null)
                {
                    continue;
                }
                _genericBehaviourPrototypes[i].Dispose();
            }

            _behaviourPrototypes.Clear();
            _genericBehaviourPrototypes.Clear();
            _genericBehaviourPrototypesTypes.Clear();
        }

        protected void SetupObject(GameObject go)
        {
            go.TypedBehaviours.OnAdded += RegisterSpecialCallback;
            go.TypedBehaviours.OnRemoved += UnregisterSpecialCallback;

            KeyValuePair<List<INetworkBehaviour>, List<Type>> behaviourPrototypesAndTypes;
            go.AddClonedBehaviours(_genericBehaviourPrototypes, _genericBehaviourPrototypesTypes);
            if(_behaviourPrototypes.TryGetValue(go.Type, out behaviourPrototypesAndTypes))
            {
                go.AddClonedBehaviours(behaviourPrototypesAndTypes.Key, behaviourPrototypesAndTypes.Value);
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
                if(itr.Current == null)
                {
                    continue;
                }
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

        public void DestroyObject(int id)
        {
            SetupObjectToDestroy(id);
            var go = _activeScene.FindObject(id);
            if(go != null)
            {
                _activeScene.RemoveObject(id);
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

        public void RegisterBehaviour(NetworkGameObject gameObjectPrefab, INetworkBehaviour behaviour)
        {
            behaviour.GameObject = gameObjectPrefab;
            _genericBehaviourPrototypes.Add(behaviour);
            _genericBehaviourPrototypesTypes.Add(behaviour.GetType());
        }

        public void RegisterBehaviour(byte objType, NetworkGameObject gameObjectPrefab, INetworkBehaviour behaviour)
        {
            KeyValuePair<List<INetworkBehaviour>, List<Type>> behavioursAndTypes;
            if(!_behaviourPrototypes.TryGetValue(objType, out behavioursAndTypes))
            {
                var behaviours = new List<INetworkBehaviour>();
                var types = new List<Type>();
                behavioursAndTypes = new KeyValuePair<List<INetworkBehaviour>, List<Type>>(behaviours, types);

                _behaviourPrototypes[objType] = behavioursAndTypes;
            }
            behaviour.GameObject = gameObjectPrefab;
            behavioursAndTypes.Key.Add(behaviour);
            behavioursAndTypes.Value.Add(behaviour.GetType());
        }

        public void RegisterBehaviours(byte objType, NetworkGameObject gameObjectPrefab, INetworkBehaviour[] behaviours)
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

        void RegisterSpecialCallback(INetworkBehaviour behaviour)
        {
            ApplyIfCast<ILateUpdateable>(behaviour, (ILateUpdateable typedBehaviour) => {
                _lateUpdateCallback += typedBehaviour.LateUpdate;
            });
        }

        void UnregisterSpecialCallback(INetworkBehaviour behaviour)
        {
            ApplyIfCast<ILateUpdateable>(behaviour, (ILateUpdateable typedBehaviour) => {
                _lateUpdateCallback -= typedBehaviour.LateUpdate;
            });
        }

        void UnregisterSpecialCallbacks(GameObject go)
        {
            var tmp = Context.Pool.Get<List<INetworkBehaviour>>();
            var itr = go.TypedBehaviours.GetEnumerator(tmp);
            while(itr.MoveNext())
            {
                UnregisterSpecialCallback(itr.Current);
            }
            Context.Pool.Return(tmp);
            itr.Dispose();
        }

        static void ApplyIfCast<T>(INetworkBehaviour behaviour, Action<T> updateAction)  where T : class
        {
            var tBehaviour = behaviour as T;
            if(tBehaviour != null)
            {
                updateAction(tBehaviour);
            }
        }
    }
}
