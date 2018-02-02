using UnityEditor;
using UnityEngine;

public class BuildSocketsTest : MonoBehaviour
{
    [MenuItem ("Sparta/Build/Examples/Sockets/Build Example for MAC")]
    public static void ClientMacBuild ()
    {
        BuildPlayerOptions buildPlayerOptionsClient = new BuildPlayerOptions ();
        buildPlayerOptionsClient.scenes = new [] { "Assets/Examples/Sockets/Scenes/SocketClient.unity" };
        buildPlayerOptionsClient.locationPathName = "Builds/ClientMac";
        buildPlayerOptionsClient.target = BuildTarget.StandaloneOSXUniversal;
        buildPlayerOptionsClient.options = BuildOptions.None;
        BuildPipeline.BuildPlayer (buildPlayerOptionsClient);

        BuildPlayerOptions buildPlayerOptionsServer = new BuildPlayerOptions ();
        buildPlayerOptionsServer.scenes = new [] { "Assets/Examples/Sockets/Scenes/SocketServer.unity" };
        buildPlayerOptionsServer.locationPathName = "Builds/ServerMac";
        buildPlayerOptionsServer.target = BuildTarget.StandaloneOSXUniversal;
        buildPlayerOptionsServer.options = BuildOptions.None;
        BuildPipeline.BuildPlayer (buildPlayerOptionsServer);
    }

    [MenuItem ("Sparta/Build/Examples/Sockets/Build Example for LINUX")]
    public static void ServerLinuxBuild ()
    {
        BuildPlayerOptions buildPlayerOptionsClient = new BuildPlayerOptions ();
        buildPlayerOptionsClient.scenes = new [] { "Assets/Examples/Sockets/Scenes/SocketClient.unity" };
        buildPlayerOptionsClient.locationPathName = "Builds/ClientMac";
        buildPlayerOptionsClient.target = BuildTarget.StandaloneOSXUniversal;
        buildPlayerOptionsClient.options = BuildOptions.None;
        BuildPipeline.BuildPlayer (buildPlayerOptionsClient);

        BuildPlayerOptions buildPlayerOptionsServer = new BuildPlayerOptions ();
        buildPlayerOptionsServer.scenes = new [] { "Assets/Examples/Sockets/Scenes/SocketServer.unity" };
        buildPlayerOptionsServer.locationPathName = "Builds/ServerLlinux";
        buildPlayerOptionsServer.target = BuildTarget.StandaloneLinuxUniversal;
        buildPlayerOptionsServer.options = BuildOptions.EnableHeadlessMode;
        BuildPipeline.BuildPlayer (buildPlayerOptionsServer);
    }
}
