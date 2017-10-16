﻿using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Dependency;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class MultiplayerTestMonoBehaviour : MonoBehaviour, INetworkSceneBehaviour
    {
        //Disable if server will be created outside Unity
        public bool CreateServer = true;

        INetworkServer _server;
        INetworkClient _client;
        NetworkClientSceneController _clientController;
        NetworkServerSceneController _serverController;

        private static readonly System.Type UnityDebugNetworkClientRigidBodyType = typeof(UnityDebugNetworkClientRigidBody);

        void Start()
        {
            //#warning as we install 2 clients we should know which NetworkClient to use here
            _client = Services.Instance.Resolve<INetworkClient>();
            _clientController = Services.Instance.Resolve<NetworkClientSceneController>();
            NetworkGameObject gameObjectPrefab = new NetworkGameObject(_clientController.Context);

            if (CreateServer)
            {
                _server = Services.Instance.Resolve<INetworkServer>();
                _serverController = Services.Instance.Resolve<NetworkServerSceneController>();
            }

            _clientController.Scene.AddBehaviour(this);
            _clientController.RegisterBehaviour(gameObjectPrefab, new UnityNetworkClientDebugBehaviour().Init(_clientController, _serverController));

            var debugScene = gameObject.AddComponent<UnityDebugSceneMonoBehaviour>();
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

        void INetworkSceneBehaviour.OnStart()
        {
        }

        void IDeltaUpdateable.Update(float elapsed)
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