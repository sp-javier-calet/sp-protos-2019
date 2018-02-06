using System;
using SocialPoint.Dependency;
using SocialPoint.Network;

public class NetworkTechInstaller : Installer
{
    public enum NetworkTech
    {
        Local,
        LocalBridge,
        Unet,
        Photon,
        TcpSocket
    }

    [Serializable]
    public class SettingsData
    {
        public NetworkTech Tech = NetworkTech.Local;
        public LocalNetworkInstaller.SettingsData Local = new LocalNetworkInstaller.SettingsData();
        public PhotonNetworkInstaller.SettingsData LocalBridge = new PhotonNetworkInstaller.SettingsData();
        public UnetNetworkInstaller.SettingsData Unet = new UnetNetworkInstaller.SettingsData();
        public PhotonNetworkInstaller.SettingsData Photon = new PhotonNetworkInstaller.SettingsData();
        public TcpSocketNetworkInstaller.SettingsData TcpSocket = new TcpSocketNetworkInstaller.SettingsData();
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
        case NetworkTech.TcpSocket:
            var tcpSocket = new TcpSocketNetworkInstaller();
            tcpSocket.Settings = Settings.TcpSocket;
            techInstaller = tcpSocket;
            break;
        case NetworkTech.LocalBridge:
            var localBridge = new LocalBridgeNetworkInstaller();
            localBridge.Settings = Settings.LocalBridge;
            techInstaller = localBridge;
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
