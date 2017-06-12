using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Dependency;
using SocialPoint.Physics;

namespace SocialPoint.Multiplayer
{
    public class UnityNetworkDebugDrawBehaviour : MonoBehaviour, INetworkSceneBehaviour
    {
        NetworkClientSceneController _clientController;
        NetworkServerSceneController _serverController;
        GameObject _multiObject = null;

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

                var navMeshComponent = _multiObject.AddComponent<NavMeshDebugDrawer>();
                navMeshComponent.NavMesh = _clientController.Scene.GetBehaviour<GameMultiplayerClientBehaviour>().NavMesh;
            }
            return this;
        }

        void OnDestroy()
        {
            _clientController.Scene.RemoveBehaviour(this);
        }

        void INetworkSceneBehaviour.OnInstantiateObject(NetworkGameObject go)
        {
            var cgo = go as NetworkGameObject<INetworkBehaviour>;
            if(cgo != null && _serverController != null)
            {
                var serverGo = _serverController.FindObject(go.Id);
                if(serverGo != null)
                {
                    var rigid = serverGo.GetBehaviour<NetworkRigidBody>();
                    if(rigid != null)
                    {
                        cgo.AddBehaviour(new UnityDebugNetworkClientRigidBody().Init(_serverController, _clientController), UnityDebugNetworkClientRigidBodyType);
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

        void INetworkSceneBehaviour.Update(float dt)
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