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
        TcpSocket,
        UdpSocket
    }

    [Serializable]
    public class SettingsData
    {
        public NetworkTech Tech = NetworkTech.Local;
        public LocalNetworkInstaller.SettingsData Local = new LocalNetworkInstaller.SettingsData();
        public UnetNetworkInstaller.SettingsData Unet = new UnetNetworkInstaller.SettingsData();
        public PhotonNetworkInstaller.SettingsData Photon = new PhotonNetworkInstaller.SettingsData();
        public TcpSocketNetworkInstaller.SettingsData TcpSocket = new TcpSocketNetworkInstaller.SettingsData();
        public UdpSocketNetworkInstaller.SettingsData UdpSocket = new UdpSocketNetworkInstaller.SettingsData();
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings(IBindingContainer container)
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
            case NetworkTech.UdpSocket:
                var udpSocket = new UdpSocketNetworkInstaller();
                udpSocket.Settings = Settings.UdpSocket;
                techInstaller = udpSocket;
                break;
            default:
                var local = new LocalNetworkInstaller();
                local.Settings = Settings.Local;
                techInstaller = local;
                break;
        }

        container.Install(techInstaller);
        container.Install(new NetworkInstaller());
    }
}
