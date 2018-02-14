using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class UnityNetworkDebugDrawBehaviour : INetworkSceneBehaviour
    {
        NetworkClientSceneController _clientController;
        NetworkServerSceneController _serverController;
        GameObject _multiObject = null;
        NavMeshDebugDrawer _navMeshDebugDrawer = null;
        SharpNav.TiledNavMesh _navMesh;
        public bool IsServerEnabled{ get{ return _serverController != null && _serverController.Scene != null; } }

        private static readonly System.Type UnityDebugNetworkClientRigidBodyType = typeof(UnityDebugNetworkClientRigidBody);

        public void SetNavMesh(SharpNav.TiledNavMesh navMesh)
        {
            _navMesh = navMesh;
        }

        void INetworkSceneBehaviour.OnInstantiateObject(NetworkGameObject go)
        {
            if(IsServerEnabled && go != null && _serverController != null)
            {
                var serverGo = _serverController.FindObject(go.Id);
                if(serverGo != null)
                {
                    var rigid = serverGo.GetBehaviour<NetworkRigidBody>();
                    if(rigid != null)
                    {
                        go.AddBehaviour(new UnityDebugNetworkClientRigidBody().Init(_serverController, _clientController), UnityDebugNetworkClientRigidBodyType);
                    }
                }
            }
        }

        void INetworkSceneBehaviour.OnDestroyObject(int id)
        {
        }

        void INetworkSceneBehaviour.OnDestroy()
        {
        }

        void IDeltaUpdateable<float>.Update(float elapsed)
        {

        }

        public void Dispose()
        {
        }

        void INetworkSceneBehaviour.OnStart()
        {
            if(!IsServerEnabled)
            {
                return;
            }

            _clientController = Services.Instance.Resolve<NetworkClientSceneController>();
            NetworkGameObject gameObjectPrefab = new NetworkGameObject(_clientController.Context);
            _serverController = Services.Instance.Resolve<NetworkServerSceneController>();

            _multiObject = GameObject.Find("Multiplayer");
            if(_multiObject == null)
            {
                _multiObject = new GameObject("Multiplayer");
                GameObject.DontDestroyOnLoad(_multiObject);
            }

            if(_clientController.Scene.GetBehaviour<UnityNetworkDebugDrawBehaviour>(this) == null)
            {
                _clientController.Scene.AddBehaviour(this);
            }
            _clientController.RegisterBehaviour(gameObjectPrefab, new UnityNetworkClientDebugBehaviour().Init(_clientController, _serverController));
            _multiObject.AddComponent<UnityDebugMonoBehaviour>();

            var debugScene = _multiObject.AddComponent<UnityDebugSceneMonoBehaviour>();
            debugScene.Client = _clientController;
            debugScene.Server = _serverController;

            _navMeshDebugDrawer = _multiObject.AddComponent<NavMeshDebugDrawer>();
            _navMeshDebugDrawer.NavMesh = _navMesh;
        }

        NetworkScene INetworkSceneBehaviour.Scene
        {
            set
            {
            }
        }
    }
}