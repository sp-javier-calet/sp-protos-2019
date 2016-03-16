using System;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Hardware;
using Zenject;

public class StorageInstaller : MonoInstaller
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
        Container.Bind<IAttrStorage>("volatile").ToSingleMethod<PlayerPrefsAttrStorage>(CreateVolatileStorage);
        Container.Bind<IAttrStorage>("persistent").ToSingleMethod<TransitionAttrStorage>(CreatePersistentStorage);

        // cannot move this into Initialize as creation of storages depends on it
        PathsManager.Init();
	}

    PlayerPrefsAttrStorage CreateVolatileStorage(InjectContext ctx)
    {
        var vol = new PlayerPrefsAttrStorage();
        vol.Prefix = Settings.VolatilePrefix;
        return vol;
    }

    TransitionAttrStorage CreatePersistentStorage(InjectContext ctx)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        var persistent = new KeychainAttrStorage(Settings.PersistentPrefix);
        #elif UNITY_ANDROID && !UNITY_EDITOR
        var devInfo = ctx.Container.Resolve<IDeviceInfo>();
        var persistent = new PersistentAttrStorage(devInfo.Uid, Settings.PersistentPrefix);
        #else
        var persistent = new FileAttrStorage(PathsManager.AppPersistentDataPath); //TODO: doesnt work with prefixes
        #endif

        var vol = ctx.Container.Resolve<IAttrStorage>("volatile");
        return new TransitionAttrStorage(vol, persistent);
    }
}
