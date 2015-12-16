using System;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Hardware;
using Zenject;

public class StorageInstaller : MonoInstaller
{
    [Inject]
    IDeviceInfo _deviceInfo;

    [Serializable]
    public class SettingsData
    {
        public string VolatilePrefix = string.Empty;
        public string PersistentPrefix = string.Empty;
    };
    
    public SettingsData Settings = new SettingsData();

	public override void InstallBindings()
	{		
        PathsManager.Init();

        var vol = new PlayerPrefsAttrStorage();
        vol.Prefix = Settings.VolatilePrefix;
        Container.Bind<IAttrStorage>("volatile").ToSingleInstance(vol);

        #if UNITY_IOS && !UNITY_EDITOR
        var persistent = new KeychainAttrStorage(Settings.PersistentPrefix);
        #else
        var persistent = new PersistentAttrStorage(_deviceInfo.Uid, Settings.PersistentPrefix);
        #endif

        var transition = new TransitionAttrStorage(vol, persistent);
        Container.Bind<IAttrStorage>("persistent").ToSingleInstance(transition);
	}
}
