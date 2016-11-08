using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Pooling;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public interface IUnityNetworkBehaviour
    {
        void OnStart(NetworkGameObject ngo, UnityEngine.GameObject go);

        void Update(float dt);

        void OnDestroy();
    }

    public class UnityNetworkClientSceneController : NetworkClientSceneController
    {
        Dictionary<string,GameObject> _prefabs = new Dictionary<string,GameObject>();
        Dictionary<int,GameObject> _objects = new Dictionary<int,GameObject>();
        string _parentTag;

        public UnityNetworkClientSceneController(INetworkClient client, string parentTag = null) : base(client)
        {
            _parentTag = parentTag;
        }

        protected override void UpdateObjectView(int objectId, Transform t)
        {
            GameObject go;
            if(_objects.TryGetValue(objectId, out go))
            {
                var ut = go.transform;
                ut.position = t.Position.ToUnity();
                ut.rotation = t.Rotation.ToUnity();
            }
        }

        protected override void InstantiateObjectView(InstantiateNetworkGameObjectEvent ev)
        {
            GameObject prefab;
            if(!_prefabs.TryGetValue(ev.PrefabName, out prefab))
            {
                prefab = Resources.Load(ev.PrefabName) as GameObject;
                _prefabs[ev.PrefabName] = prefab;
            }

            UnityEngine.Transform parent = null;
            if(!string.IsNullOrEmpty(_parentTag))
            {
                // should be cached
                var parentGo = GameObject.FindGameObjectWithTag(_parentTag);
                if(parentGo != null)
                {
                    parent = parentGo.transform;
                }
            }

            var go = ObjectPool.Spawn(prefab, parent,
                         ev.Transform.Position.ToUnity(), ev.Transform.Rotation.ToUnity());
            _objects[ev.ObjectId] = go;
        }

        protected override void DestroyObjectView(DestroyNetworkGameObjectEvent ev)
        {
            GameObject go;
            if(_objects.TryGetValue(ev.ObjectId, out go))
            {
                ObjectPool.Recycle(go);
                _objects.Remove(ev.ObjectId);
            }
        }

        protected override void OnError(SocialPoint.Base.Error err)
        {
            Debug.LogError(err.Msg);
        }
    }
}