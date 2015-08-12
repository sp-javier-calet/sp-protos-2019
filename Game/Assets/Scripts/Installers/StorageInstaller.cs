using System;
using SocialPoint.Attributes;
using Zenject;

public class StorageInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string PersistentPrefix = string.Empty;
    };
    
    public SettingsData Settings;

	public override void InstallBindings()
	{		
        Container.Bind<IAttrStorage>().ToSingle<PlayerPrefsAttrStorage>();

        var persistent = new PersistentAttrStorage(Settings.PersistentPrefix);
        Container.Bind<IAttrStorage>().ToSingleInstance("persistent", persistent);
	}
}
