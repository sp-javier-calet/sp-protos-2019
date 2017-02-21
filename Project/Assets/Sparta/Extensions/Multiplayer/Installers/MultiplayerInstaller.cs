using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public class MultiplayerInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public string MultiplayerParentTag = "MultiplayerParent";
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Rebind<NetworkServerSceneController>()
            .ToMethod<NetworkServerSceneController>(CreateServerSceneController, SetupServerSceneController);
            Container.Rebind<NetworkClientSceneController>()
            .ToMethod<NetworkClientSceneController>(CreateClientSceneController, SetupClientSceneController);
        }

        NetworkServerSceneController CreateServerSceneController()
        {
            return new UnityNetworkServerSceneController(
                Container.Resolve<INetworkServer>(),
                Container.Resolve<IUpdateScheduler>());
        }

        NetworkClientSceneController CreateClientSceneController()
        {
            return new UnityNetworkClientSceneController(
                Container.Resolve<INetworkClient>(),
                Settings.MultiplayerParentTag);
        }

        void SetupServerSceneController(NetworkServerSceneController ctrl)
        {
            var behaviours = Container.ResolveList<INetworkServerSceneBehaviour>();
            for(var i = 0; i < behaviours.Count; i++)
            {
                ctrl.AddBehaviour(behaviours[i]);
            }
        }

        void SetupClientSceneController(NetworkClientSceneController ctrl)
        {
            var behaviours = Container.ResolveList<INetworkClientSceneBehaviour>();
            for(var i = 0; i < behaviours.Count; i++)
            {
                ctrl.AddBehaviour(behaviours[i]);
            }
        }
    }
}