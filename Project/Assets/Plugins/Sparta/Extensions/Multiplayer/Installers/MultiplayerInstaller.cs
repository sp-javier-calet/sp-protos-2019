using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Crash;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class MultiplayerInstaller : ServiceInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public string MultiplayerParentTag = "MultiplayerParent";
            public int ServerBufferSize = NetworkServerSceneController.DefaultBufferSize;
            public List<SyncGroupSettings> SyncGroupsSettings = new List<SyncGroupSettings>() {
                new SyncGroupSettings{ SyncInterval = 0.3f },
                new SyncGroupSettings{ SyncInterval = 0.1f },
                new SyncGroupSettings{ SyncInterval = 0.05f },
            };
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<NetworkServerSceneController>()
                .ToMethod<NetworkServerSceneController>(CreateServerSceneController, SetupServerSceneController);        
            Container.Bind<IDeltaUpdateable>().ToLookup<NetworkServerSceneController>();
            Container.Rebind<UnityNetworkClientSceneController>()
                .ToMethod<UnityNetworkClientSceneController>(CreateClientSceneController, SetupClientSceneController);
            Container.Rebind<NetworkClientSceneController>().ToLookup<UnityNetworkClientSceneController>();
            Container.Bind<IDeltaUpdateable>().ToLookup<UnityNetworkClientSceneController>();
            Container.Bind<IInitializable>().ToInstance(this);

            Container.BindDefault<NetworkSceneContext>().ToMethod<NetworkSceneContext>(CreateContext);
        }

        //TODO FIX FOR ORDERS FIXME
        public void Initialize()
        {
            var scheduler = Container.Resolve<IUpdateScheduler>();
            var updateables = Container.ResolveList<IUpdateable>();
            if(updateables != null)
            {
                for(var i = 0; i < updateables.Count; i++)
                {
                    scheduler.Add(updateables[i]);
                }
            }
            var deltaUpdateables = Container.ResolveList<IDeltaUpdateable>();
            if(deltaUpdateables != null)
            {
                for(var i = 0; i < deltaUpdateables.Count; i++)
                {
                    scheduler.Add(deltaUpdateables[i]);
                }
            }
        }

        NetworkServerSceneController CreateServerSceneController()
        {
            var server = new NetworkServerSceneController(Container.Resolve<INetworkServer>(), Container.Resolve<NetworkSceneContext>(), Container.Resolve<IGameTime>());

            server.BufferSize = Settings.ServerBufferSize;

            return server;
        }

        UnityNetworkClientSceneController CreateClientSceneController()
        {
            UnityNetworkClientSceneController networkClient = new UnityNetworkClientSceneController(
                                                                  Container.Resolve<INetworkClient>(),
                                                                  Container.Resolve<NetworkSceneContext>());
            networkClient.HandleException += Container.Resolve<ICrashReporter>().ReportHandledException;
            return networkClient;
        }

        void SetupServerSceneController(NetworkServerSceneController ctrl)
        {
            var behaviours = Container.ResolveList<INetworkSceneBehaviour>("Server");
            for(var i = 0; i < behaviours.Count; i++)
            {
                ctrl.Scene.AddBehaviour(behaviours[i]);
            }
        }

        void SetupClientSceneController(NetworkClientSceneController ctrl)
        {
            var behaviours = Container.ResolveList<INetworkSceneBehaviour>("Client");
            for(var i = 0; i < behaviours.Count; i++)
            {
                ctrl.Scene.AddBehaviour(behaviours[i]);
            }
        }

        NetworkSceneContext CreateContext()
        {
            return new NetworkSceneContext();
        }
    }
}
