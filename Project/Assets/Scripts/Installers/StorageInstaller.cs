using System;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Hardware;
using SocialPoint.Dependency;

public class StorageInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public string VolatilePrefix = string.Empty;
        public string PersistentPrefix = string.Empty;
    }
    
    public SettingsData Settings = new SettingsData();

	public override void InstallBindings()
	{		
        Container.Bind<IAttrStorage>("volatile").ToMethod<PlayerPrefsAttrStorage>(CreateVolatileStorage);
        Container.Bind<IAttrStorage>("persistent").ToMethod<TransitionAttrStorage>(CreatePersistentStorage);

        // cannot move this into Initialize as creation of storages depends on it
        PathsManager.Init();
	}

    PlayerPrefsAttrStorage CreateVolatileStorage()
    {
        var vol = new PlayerPrefsAttrStorage();
        vol.Prefix = Settings.VolatilePrefix;
        return vol;
    }

    TransitionAttrStorage CreatePersistentStorage()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        var persistent = new KeychainAttrStorage(Settings.PersistentPrefix);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        var devInfo = Container.Resolve<IDeviceInfo>();
        var persistent = new PersistentAttrStorage(devInfo.Uid, Settings.PersistentPrefix);
        Container.Bind<IDisposable>().ToLookup<PersistentAttrStorage>();
        #else
        var persistent = new FileAttrStorage(PathsManager.AppPersistentDataPath); //TODO: doesnt work with prefixes
        #endif

        var vol = Container.Resolve<IAttrStorage>("volatile");
        return new TransitionAttrStorage(vol, persistent);
    }
}
