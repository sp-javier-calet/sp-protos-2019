using System;
using SocialPoint.Dependency;
using SocialPoint.Network;

public class NetworkTechInstaller : Installer
{
    public enum NetworkTech
    {
        Local,
        Unet,
        Photon,
        Socket
    }

    [Serializable]
    public class SettingsData
    {
        public NetworkTech Tech = NetworkTech.Local;
        public LocalNetworkInstaller.SettingsData Local = new LocalNetworkInstaller.SettingsData();
        public UnetNetworkInstaller.SettingsData Unet = new UnetNetworkInstaller.SettingsData();
        public PhotonNetworkInstaller.SettingsData Photon = new PhotonNetworkInstaller.SettingsData();
        public SocketNetworkInstaller.SettingsData Socket = new SocketNetworkInstaller.SettingsData();
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        SubInstaller techInstaller;
        switch(Settings.Tech)
        {
        case NetworkTech.Unet:
            var unet = new UnetNetworkInstaller();
            unet.Settings = Settings.Unet;
            techInstaller = unet;
            break;
        case NetworkTech.Photon:
            var photon = new PhotonNetworkInstaller();
            photon.Settings = Settings.Photon;
            techInstaller = photon;
            break;
        case NetworkTech.Socket:
            var socket = new SocketNetworkInstaller();
            socket.Settings = Settings.Socket;
            techInstaller = socket;
            break;
        default:
            var local = new LocalNetworkInstaller();
            local.Settings = Settings.Local;
            techInstaller = local;
            break;
        }

        Container.Install(techInstaller);
        Container.Install(new NetworkInstaller());
    }
}
