using UnityEditor;
using UnityEngine;

public class SocketsAppBuilder: MonoBehaviour
{
    [MenuItem("Sockets/Client MAC")]
    public static void PerformClientMacBuild ()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Examples/Sockets/Scenes/SocketClientExample.unity" };
        buildPlayerOptions.locationPathName = "Builds/SocketClient";
        buildPlayerOptions.target = BuildTarget.StandaloneOSXIntel64;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Sockets/Server MAC")]
    public static void PerformServerMacBuild ()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Examples/Sockets/Scenes/SocketServerExample.unity" };
        buildPlayerOptions.locationPathName = "Builds/SocketServer";
        buildPlayerOptions.target = BuildTarget.StandaloneOSXIntel64;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Sockets/Server Linux")]
    public static void PerformServerLinuxBuild ()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] { "Assets/Examples/Sockets/Scenes/SocketServerExample.unity" };
        buildPlayerOptions.locationPathName = "Builds/SocketServer";
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.EnableHeadlessMode;
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
