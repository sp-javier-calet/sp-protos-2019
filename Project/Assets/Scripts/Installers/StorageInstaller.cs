using System;
using SocialPoint.Attributes;
using SocialPoint.IO;
using Zenject;

public class StorageInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string VolatilePrefix = string.Empty;
        public string PersistentPrefix = string.Empty;
    };
    
    public SettingsData Settings;

	public override void InstallBindings()
	{		
        PathsManager.Init(this, true);

        var vol = new PlayerPrefsAttrStorage();
        vol.Prefix = Settings.VolatilePrefix;
        Container.Bind<IAttrStorage>("volatile").ToSingleInstance(vol);

#if UNITY_IOS && !UNITY_EDITOR
        var persistent = new KeychainAttrStorage(Settings.PersistentPrefix);
#else
        var persistent = new PersistentAttrStorage(FileUtils.Combine(PathsManager.PersistentDataPath, Settings.PersistentPrefix));
#endif
        Container.Bind<IAttrStorage>("persistent").ToSingleInstance(persistent);
	}
}
