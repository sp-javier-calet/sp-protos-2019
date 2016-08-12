using UnityEngine;
using System.Collections.Generic;
using SocialPoint.ObjectPool;

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
        UnityEngine.Transform _parent;

        public UnityNetworkClientSceneController(INetworkClient client, UnityEngine.Transform parent):base(client)
        {
            _parent = parent;
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
            var go =  SocialPoint.ObjectPool.ObjectPool.Spawn(prefab, _parent,
                ev.Transform.Position.ToUnity(), ev.Transform.Rotation.ToUnity());
            _objects[ev.ObjectId] = go;
        }

        protected override void DestroyObjectView(DestroyNetworkGameObjectEvent ev)
        {
            GameObject go;
            if(_objects.TryGetValue(ev.ObjectId, out go))
            {
                SocialPoint.ObjectPool.ObjectPool.Recycle(go);
                _objects.Remove(ev.ObjectId);
            }
        }
    }
}