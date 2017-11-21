using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Dependency;
using SocialPoint.Physics;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Multiplayer
{
    public class UnityNetworkDebugDrawBehaviour : MonoBehaviour, INetworkSceneBehaviour
    {
        NetworkClientSceneController _clientController;
        NetworkServerSceneController _serverController;
        GameObject _multiObject = null;
		NavMeshDebugDrawer _navMeshDebugDrawer = null;

        private static readonly System.Type UnityDebugNetworkClientRigidBodyType = typeof(UnityDebugNetworkClientRigidBody);

        public UnityNetworkDebugDrawBehaviour Init()
        {
            _clientController = Services.Instance.Resolve<NetworkClientSceneController>();
            NetworkGameObject gameObjectPrefab = new NetworkGameObject(_clientController.Context);
            _serverController = Services.Instance.Resolve<NetworkServerSceneController>();

            _multiObject = UnityEngine.GameObject.Find("Multiplayer");
            if(_multiObject )
            {
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
            }
            return this;
        }

		void SetNavMeshForDebugDrawer(SharpNav.TiledNavMesh navMesh)
		{
			_navMeshDebugDrawer.NavMesh = navMesh;
		}

        void OnDestroy()
        {
            _clientController.Scene.RemoveBehaviour(this);
        }

        void INetworkSceneBehaviour.OnInstantiateObject(NetworkGameObject go)
        {
            if(go != null && _serverController != null)
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

        public void Update(float elapsed)
        {

        }

        public void Dispose()
        {

        }

        void INetworkSceneBehaviour.OnStart()
        {
        }

        NetworkScene INetworkSceneBehaviour.Scene
        {
            set
            {
            }
        }
    }
}