using UnityEngine;
using SocialPoint.Physics;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public sealed class UnityViewBehaviour : NetworkBehaviour
    {
        GameObject _view;

        public GameObject View
        {
            get
            {
                if(_view == null)
                {
                    InstantiateGameObject();
                }
                return _view;
            }
        }

        const string _prefabsBasePath = "Prefabs/";
        string _prefabName;

        UnityViewPool _viewPool;

        System.Func<NetworkGameObject, string> _obtainPrefabNameCallback;

        void InstantiateGameObject()
        {
            var prefabName = _obtainPrefabNameCallback == null ? _prefabName : _obtainPrefabNameCallback(GameObject);
            InstantiateGameObject(prefabName);
        }

        void InstantiateGameObject(string prefabName)
        {
            _view = SpawnPrefab(prefabName);

            var networkMonoBehaviour = _view.GetComponent<NetworkMonoBehaviour>();
            if(networkMonoBehaviour == null)
            {
                Log.w("NetworkMonoBehaviour is not added to '" + prefabName + "' prefab");
                networkMonoBehaviour = _view.AddComponent<NetworkMonoBehaviour>();
            }
            networkMonoBehaviour.Init(GameObject);
        }

        GameObject SpawnPrefab(string prefabName)
        {
            return _viewPool.Spawn(prefabName,
                GameObject.Transform.Position.ToUnity(),
                GameObject.Transform.Rotation.ToUnity());
        }

        public UnityViewBehaviour Init(System.Func<NetworkGameObject, string> obtainPrefabNameCallback, UnityViewPool viewPool)
        {
            _viewPool = viewPool;
            _obtainPrefabNameCallback = obtainPrefabNameCallback;
            return this;
        }

        public UnityViewBehaviour Init(string prefabName, UnityViewPool viewPool)
        {
            _prefabName = prefabName;
            _viewPool = viewPool;
            return this;
        }

        protected override void OnStart()
        {
            if(_view == null)
            {
                InstantiateGameObject();
            }
        }

        protected override void OnDestroy()
        {
            DestroyView();
        }

        protected override void Dispose()
        {
            DestroyView();

            _viewPool = null;
            _obtainPrefabNameCallback = null;

            base.Dispose();
        }

        void DestroyView()
        {
            if(_view != null)
            {
                _viewPool.Recycle(_view);
                _view = null;
            }
        }

        public void SetHeroViewType(string heroType)
        {
            string heroPrefabName = heroType.ToLower();
            _prefabName = _prefabsBasePath + heroPrefabName;
        }

        public override object Clone()
        {
            var vb = GameObject.Context.Pool.Get<UnityViewBehaviour>();
            vb.Init(_prefabName, _viewPool);
            vb._obtainPrefabNameCallback = _obtainPrefabNameCallback;
            return vb;
        }
    }
}
