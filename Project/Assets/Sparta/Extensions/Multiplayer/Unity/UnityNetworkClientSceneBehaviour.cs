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

        protected override void InstantiateObjectView(string prefabName, NetworkGameObject ngo)
        {
            GameObject prefab;
            if(!_prefabs.TryGetValue(prefabName, out prefab))
            {
                prefab = Resources.Load(prefabName) as GameObject;
                _prefabs[prefabName] = prefab;
            }           
            var go =  SocialPoint.ObjectPool.ObjectPool.Spawn(prefab, _parent,
                ngo.Transform.Position.ToUnity(), ngo.Transform.Rotation.ToUnity());
            _objects[ngo.Id] = go;
        }

        protected override void DestroyObjectView(int objectId)
        {
            GameObject go;
            if(_objects.TryGetValue(objectId, out go))
            {
                SocialPoint.ObjectPool.ObjectPool.Recycle(go);
                _objects.Remove(objectId);
            }
        }
    }
}