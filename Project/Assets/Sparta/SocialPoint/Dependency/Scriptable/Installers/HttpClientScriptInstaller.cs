using System;
using SocialPoint.Dependency;

public class HttpClientScriptInstaller : ScriptableInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string Config = "basegame";
        public bool EnableHttpStreamPinning = false;
    }

    public SettingsData Settings = new SettingsData();

    public HttpClientScriptInstaller() : base(ModuleType.Service)
    {
    }

    public override void InstallBindings()
    {

    }
}
