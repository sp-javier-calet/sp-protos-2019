using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using SocialPoint.Base;
using UnityEngine;
using SocialPoint.Attributes;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Locale
{
    public sealed class LocaleInstaller : ServiceInstaller
    {
        [Serializable]
        public struct EnvironmentData
        {
            public EnvironmentType EnvironmentType;
            public string Id;
            public string SecretKey;
        }

        [Serializable]
        public class SettingsData
        {
            public bool UseAlwaysDeviceLanguage = true;
            public LocalizationSettings Localization;
        }

        [Serializable]
        public class LocalizationSettings
        {
            public string ProjectId = LocalizationManager.LocationData.DefaultProjectId;
            public List<EnvironmentData> EnvironmentsData = new List<EnvironmentData>();
            public string SecretKeyDev = string.Empty;
            public string SecretKeyLoc = string.Empty;
            public string SecretKeyProd = string.Empty;
            public string BundleDir = LocalizationManager.DefaultBundleDir;
            public LocalizationManager.CsvMode CsvMode = LocalizationManager.CsvMode.NoCsv;
            public bool ShowKeysOnDevMode = true;
            public string[] SupportedLanguages = LocalizationManager.DefaultSupportedLanguages;
            public float Timeout = LocalizationManager.DefaultTimeout;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<bool>("use_always_device_language").ToInstance(Settings.UseAlwaysDeviceLanguage);

            Container.Bind<Localization>().ToGetter<ILocalizationManager>(mng => mng.Localization);
            Container.Rebind<LocalizeAttributeConfiguration>().ToMethod<LocalizeAttributeConfiguration>(CreateLocalizeAttributeConfiguration);
            Container.Rebind<ILocalizationManager>().ToMethod<LocalizationManager>(CreateLocalizationManager, SetupLocalizationManager);
            Container.Bind<IDisposable>().ToLookup<ILocalizationManager>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelLocale>(CreateAdminPanel);
            #endif
        }
            
        LocalizeAttributeConfiguration CreateLocalizeAttributeConfiguration()
        {
            return new LocalizeAttributeConfiguration(
                Container.Resolve<Localization>(),
                Container.ResolveList<IMemberAttributeObserver<LocalizeAttribute>>());
        }

        #if ADMIN_PANEL
        AdminPanelLocale CreateAdminPanel()
        {
            return new AdminPanelLocale(
                Container.Resolve<ILocalizationManager>());
        }
        #endif

        LocalizationManager CreateLocalizationManager()
        {
            #if NGUI
            LocalizationManager.CsvForNGUILoadedDelegate csvLoadedDelegate = null;
            csvLoadedDelegate = new LocalizationManager.CsvForNGUILoadedDelegate(LoadNGUICSV);
            #endif

            IAttrStorage storage = Container.Resolve<IAttrStorage>(kPersistentTag);

            #if NGUI
                LocalizationManager localizationManager = new LocalizationManager(Settings.Localization.CsvMode, csvLoadedDelegate);
            #else
                LocalizationManager localizationManager = new LocalizationManager(storage);
            #endif
            localizationManager.UseAlwaysDeviceLanguage = Settings.UseAlwaysDeviceLanguage;

            return localizationManager;
        }

        #if NGUI
        void LoadNGUICSV(byte[] bytes)
        {
            var manager = Container.Resolve<ILocalizationManager>();

            // Add localizations to NGUI and Update current language
            NGUILocalization.LoadCSV(bytes);
            NGUILocalization.language = manager.CurrentLanguage;

            UILocalize[] localizadElements = GameObject.FindObjectsOfType<UILocalize>();
            for(int i = 0; i < localizadElements.Length; i++)
            {
                localizadElements[i].OnLocalize();
            }
        }
        #endif


        void SetupLocalizationManager(LocalizationManager mng)
        {
            mng.HttpClient = Container.Resolve<IHttpClient>();
            mng.AppInfo = Container.Resolve<IAppInfo>();
            mng.BackendEnvironments = Container.Resolve<IBackendEnvironment>();
            mng.AppEvents = Container.Resolve<IAppEvents>();
            mng.Location.ProjectId = Settings.Localization.ProjectId;
            mng.Location.EnvironmentsData = Settings.Localization.EnvironmentsData;
            mng.Timeout = Settings.Localization.Timeout;
            mng.BundleDir = Settings.Localization.BundleDir;
            mng.SupportedLanguages = Settings.Localization.SupportedLanguages;
            mng.Localization.ShowKeysOnDevMode = Settings.Localization.ShowKeysOnDevMode;
            mng.UpdateDefaultLanguage();
        }
    }
}
