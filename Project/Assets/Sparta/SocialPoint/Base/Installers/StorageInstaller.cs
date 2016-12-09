using System;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Dependency;

namespace SocialPoint.Base
{
    public class StorageInstaller : ServiceInstaller
    {
        const string VolatileTag = "volatile";
        const string PersistentTag = "persistent";

        [Serializable]
        public class SettingsData
        {
            public string VolatilePrefix = string.Empty;
            public string PersistentPrefix = string.Empty;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {		
            Container.Bind<IAttrStorage>(VolatileTag).ToMethod<PlayerPrefsAttrStorage>(CreateVolatileStorage);
            Container.Bind<IAttrStorage>(PersistentTag).ToMethod<TransitionAttrStorage>(CreatePersistentStorage);

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAttrStorage>(() => CreateAdminPanel(VolatileTag));
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAttrStorage>(() => CreateAdminPanel(PersistentTag));

            // cannot move this into Initialize as creation of storages depends on it
            PathsManager.Init();
        }

        PlayerPrefsAttrStorage CreateVolatileStorage()
        {
            var vol = new PlayerPrefsAttrStorage();
            vol.Prefix = Settings.VolatilePrefix;
            #if UNITY_STANDALONE
            // avoid editor and standalone overwriting
            vol.Prefix += UnityEngine.Application.platform.ToString();
            #endif
            return vol;
        }

        TransitionAttrStorage CreatePersistentStorage()
        {
            #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            var persistent = new KeychainAttrStorage(Settings.PersistentPrefix);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            var devInfo = Container.Resolve<SocialPoint.Hardware.IDeviceInfo>();
            var persistent = new PersistentAttrStorage(devInfo.Uid, Settings.PersistentPrefix);
            Container.Bind<IDisposable>().ToLookup<PersistentAttrStorage>();
            #else
            var path = PathsManager.AppPersistentDataPath;
            #if UNITY_STANDALONE
            // avoid editor and standalone overwriting
            path = System.IO.Path.Combine(path, UnityEngine.Application.platform.ToString());
            #endif
            var persistent = new FileAttrStorage(path); //TODO: doesnt work with prefixes
            #endif

            var vol = Container.Resolve<IAttrStorage>(VolatileTag);
            return new TransitionAttrStorage(vol, persistent);
        }

        AdminPanelAttrStorage CreateAdminPanel(string tag)
        {
            return new AdminPanelAttrStorage(tag, Container.Resolve<IAttrStorage>(tag));
        }
    }
}