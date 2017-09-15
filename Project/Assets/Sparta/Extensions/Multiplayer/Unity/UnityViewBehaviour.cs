using UnityEngine;
using SocialPoint.Pooling;
using SocialPoint.Physics;

namespace SocialPoint.Multiplayer
{
    public sealed class UnityViewBehaviour : NetworkBehaviour
    {
        class NetworkMonoBehaviour : MonoBehaviour
        {
            NetworkGameObject _go;

            public void Init(NetworkGameObject go)
            {
                _go = go;
            }

            void Start()
            {
                SyncTransform();
            }

            void Update()
            {
                SyncTransform();
            }

            void SyncTransform()
            {
                if(_go != null)
                {
                    transform.position = _go.Transform.Position.ToUnity();
                    transform.rotation = _go.Transform.Rotation.ToUnity();
                }
            }
        }

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
            _view = _viewPool.Spawn(prefabName,
                GameObject.Transform.Position.ToUnity(),
                GameObject.Transform.Rotation.ToUnity());
            _view.AddComponent<NetworkMonoBehaviour>().Init(GameObject);
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
            var vb = ObjectPool.Get<UnityViewBehaviour>().Init(_prefabName, _viewPool);
            vb._obtainPrefabNameCallback = _obtainPrefabNameCallback;
            return vb;
        }
    }
}
