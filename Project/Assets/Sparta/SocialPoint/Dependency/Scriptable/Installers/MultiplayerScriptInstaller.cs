using System;
using SocialPoint.Utils;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Multiplayer;
using SocialPoint.AdminPanel;


public class MultiplayerScriptInstaller : ScriptableInstaller {

    [Serializable]
    public class SettingsData
    {
        public string MultiplayerParentTag = "MultiplayerParent";
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
    }
}
