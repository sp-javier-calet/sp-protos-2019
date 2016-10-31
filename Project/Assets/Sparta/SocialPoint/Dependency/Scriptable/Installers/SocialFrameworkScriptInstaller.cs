using System;
using SocialPoint.Dependency;

public class SocialFrameworkScriptInstaller : ScriptableInstaller
{
    const string DefaultWAMPProtocol = "wamp.2.json";
    const string DefaultEndpoint = "ws://sprocket-00.int.lod.laicosp.net:8001/ws";

    [Serializable]
    public class SettingsData
    {
        public string Endpoint = DefaultEndpoint;
        public string[] Protocols = new string[] { DefaultWAMPProtocol };
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {

    }
}