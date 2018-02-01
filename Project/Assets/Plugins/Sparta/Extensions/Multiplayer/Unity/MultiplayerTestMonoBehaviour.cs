using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Dependency;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class MultiplayerTestMonoBehaviour : INetworkSceneBehaviour
    {
        //Disable if server will be created outside Unity
        public bool CreateServer = true;

        INetworkServer _server;
        INetworkClient _client;
        NetworkClientSceneController _clientController;
        NetworkServerSceneController _serverController;

        private static readonly System.Type UnityDebugNetworkClientRigidBodyType = typeof(UnityDebugNetworkClientRigidBody);

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
            _clientController.Scene.RemoveBehaviour(this);
        }

        void INetworkSceneBehaviour.OnStart()
        {
            //#warning as we install 2 clients we should know which NetworkClient to use here
            var clientFactory = Services.Instance.Resolve<INetworkClientFactory>();
            _client = clientFactory.Create();
            _clientController = Services.Instance.Resolve<NetworkClientSceneController>();
            NetworkGameObject gameObjectPrefab = new NetworkGameObject(_clientController.Context);

            if (CreateServer)
            {
                var factory = Services.Instance.Resolve<INetworkServerFactory>();
                _server = factory.Create();
                _serverController = Services.Instance.Resolve<NetworkServerSceneController>();
            }

            _clientController.RegisterBehaviour(gameObjectPrefab, new UnityNetworkClientDebugBehaviour().Init(_clientController, _serverController));

            var debugScene = GameObject.FindObjectOfType<UnityDebugSceneMonoBehaviour>();
            if(debugScene == null)
            {
                debugScene = new GameObject("UnityDebugSceneMonoBehaviour").AddComponent<UnityDebugSceneMonoBehaviour>();
            }
            debugScene.Client = _clientController;
            debugScene.Server = _serverController;

            if(_server != null)
            {
                _server.Start();
            }
            if(_client != null)
            {
                _client.Connect();
            }
        }

        void IDeltaUpdateable<float>.Update(float elapsed)
        {
            
        }

        void IDisposable.Dispose()
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