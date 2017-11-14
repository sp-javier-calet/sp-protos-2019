using System;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Dependency;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Base
{
    public class StorageInstaller : ServiceInstaller
    {
        public const string VolatileTag = "volatile";
        public const string PersistentTag = "persistent";

        [Serializable]
        public class SettingsData
        {
            public string VolatilePrefix = string.Empty;
            public string PersistentPrefix = string.Empty;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<IFileManager>().ToMethod<UnityFileManager>(CreateFileManager);
            Container.Bind<IAttrStorage>(VolatileTag).ToMethod<PlayerPrefsAttrStorage>(CreateVolatileStorage);
            Container.Bind<IAttrStorage>(PersistentTag).ToMethod<TransitionAttrStorage>(CreatePersistentStorage);

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAttrStorage>(() => CreateAdminPanel(VolatileTag));
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAttrStorage>(() => CreateAdminPanel(PersistentTag));
            #endif

            // cannot move this into Initialize as creation of storages depends on it
            PathsManager.Init();
        }

        UnityFileManager CreateFileManager()
        {
            return new UnityFileManager();
        }

        PlayerPrefsAttrStorage CreateVolatileStorage()
        {
            var vol = new PlayerPrefsAttrStorage();
            vol.Prefix = VolatilePrefix;
            #if UNITY_STANDALONE
            // avoid editor and standalone overwriting
            vol.Prefix += UnityEngine.Application.platform.ToString();
            #endif
            return vol;
        }

        string PersistentPrefix
        {
            get
            {
                return Settings.PersistentPrefix;
            }
        }

        string VolatilePrefix
        {
            get
            {
                return Settings.VolatilePrefix;
            }
        }

        TransitionAttrStorage CreatePersistentStorage()
        {
            #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            var persistent = new KeychainAttrStorage(PersistentPrefix);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            var devInfo = Container.Resolve<SocialPoint.Hardware.IDeviceInfo>();
            var persistent = new PersistentAttrStorage(devInfo.Uid, PersistentPrefix);
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

        #if ADMIN_PANEL
        AdminPanelAttrStorage CreateAdminPanel(string tag)
        {
            return new AdminPanelAttrStorage(tag, Container.Resolve<IAttrStorage>(tag));
        }
        #endif
    }
}
