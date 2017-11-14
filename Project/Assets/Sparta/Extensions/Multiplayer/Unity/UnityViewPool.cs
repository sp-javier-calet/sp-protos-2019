using UnityEngine;
using SocialPoint.Pooling;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class UnityViewPool
    {
        Dictionary<string, GameObject> _prefabs;

        readonly string _parentTag;

        UnityEngine.Transform _parentTransform;
        public UnityEngine.Transform ParentTransform
        {
            get
            {                
                if(_parentTransform == null && !string.IsNullOrEmpty(_parentTag))
                {
                    var parentGo = UnityEngine.GameObject.FindGameObjectWithTag(_parentTag);
                    if(parentGo != null)
                    {
                        _parentTransform = parentGo.transform;
                    }
                }
                return _parentTransform;
            }
        }

        public UnityViewPool(string parentTag = "MultiplayerParent")
        {
            _prefabs = new Dictionary<string, GameObject>();
            _parentTag = parentTag;
        }

        public void Clear()
        {
            _prefabs.Clear();
        }

        public GameObject Spawn(string prefabName, Vector3 position, Quaternion rotation)
        {
            GameObject prefab;
            if (!_prefabs.TryGetValue(prefabName, out prefab))
            {
                prefab = Resources.Load(prefabName) as GameObject;
                _prefabs.Add(prefabName, prefab);
            }
            return ObjectPool.Spawn(prefab, ParentTransform, position, rotation);
        }

        public void Recycle(GameObject view)
        {
            ObjectPool.Recycle(view);
        }
    }
}
