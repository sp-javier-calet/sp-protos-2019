using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Locale
{
    public sealed class LocaleInstaller : ServiceInstaller, IInitializable
    {
        public enum LocalizationEnvironment
        {
            Development,
            Localization,
            Production
        }

        // Environment Ids mapping
        static readonly Dictionary<LocalizationEnvironment, string> EnvironmentIds = new Dictionary<LocalizationEnvironment, string> {
            { LocalizationEnvironment.Development,  "dev"  },
            { LocalizationEnvironment.Localization, "loc"  },
            { LocalizationEnvironment.Production,   "prod" }
        };

        [Serializable]
        public class SettingsData
        {
            public bool EnableViewLocalization = true;
            public LocalizationSettings Localization;
        }

        [Serializable]
        public class LocalizationSettings
        {
            public LocalizationEnvironment Environment = LocalizationEnvironment.Production;
            public string ProjectId = LocalizationManager.LocationData.DefaultProjectId;
            public string SecretKeyDev = LocalizationManager.LocationData.DefaultDevSecretKey;
            public string SecretKeyLoc = LocalizationManager.LocationData.DefaultDevSecretKey;
            public string SecretKeyProd = LocalizationManager.LocationData.DefaultProdSecretKey;
            public string BundleDir = LocalizationManager.DefaultBundleDir;
            public LocalizationManager.CsvMode CsvMode = LocalizationManager.CsvMode.NoCsv;
            public bool ShowKeysOnDevMode = true;
            public string[] SupportedLanguages = LocalizationManager.DefaultSupportedLanguages;
            public float Timeout = LocalizationManager.DefaultTimeout;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().ToInstance(this);
            Container.Bind<Localization>().ToGetter<ILocalizationManager>(mng => mng.Localization);

            Container.Rebind<LocalizeAttributeConfiguration>().ToMethod<LocalizeAttributeConfiguration>(CreateLocalizeAttributeConfiguration);

            Container.Rebind<UILocalizationUpdater>().ToMethod<UILocalizationUpdater>(CreateViewLocalizer);
            Container.Bind<IDisposable>().ToLookup<UILocalizationUpdater>();

            Container.Rebind<ILocalizationManager>().ToMethod<LocalizationManager>(CreateLocalizationManager, SetupLocalizationManager);
            Container.Bind<IDisposable>().ToLookup<ILocalizationManager>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelLocale>(CreateAdminPanel);
            #endif
        }

        public void Initialize()
        {
            if(Settings.EnableViewLocalization)
            {
                Container.Resolve<UILocalizationUpdater>();
            }
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

        UILocalizationUpdater CreateViewLocalizer()
        {
            return new UILocalizationUpdater(
                Container.Resolve<LocalizeAttributeConfiguration>(),
                Container.Resolve<IEventDispatcher>());
        }

        LocalizationManager CreateLocalizationManager()
        {
            LocalizationManager.CsvForNGUILoadedDelegate csvLoadedDelegate = null;

            #if NGUI
            csvLoadedDelegate = new LocalizationManager.CsvForNGUILoadedDelegate(LoadNGUICSV);
            #endif

            return new LocalizationManager(Settings.Localization.CsvMode, csvLoadedDelegate);
        }

        #if NGUI
        static void LoadNGUICSV(byte[] bytes)
        {
            var manager = Container.Resolve<ILocalizationManager>();

            NGUILocalization.LoadCSV(bytes);
            NGUILocalization.language = manager.CurrentLanguage;
        }
        #endif

        void SetupLocalizationManager(LocalizationManager mng)
        {
            mng.HttpClient = Container.Resolve<IHttpClient>();
            mng.AppInfo = Container.Resolve<IAppInfo>();
            mng.AppEvents = Container.Resolve<IAppEvents>();

            string secretKey;
            if(Settings.Localization.Environment == LocalizationEnvironment.Development)
            {
                secretKey = Settings.Localization.SecretKeyDev;
            }
            else if(Settings.Localization.Environment == LocalizationEnvironment.Localization)
            {
                secretKey = Settings.Localization.SecretKeyLoc;
            }
            else
            {
                secretKey = Settings.Localization.SecretKeyProd;
            }

            mng.Location.ProjectId = Settings.Localization.ProjectId;
            mng.Location.EnvironmentId = EnvironmentIds[Settings.Localization.Environment];
            mng.Location.SecretKey = secretKey;
            mng.Timeout = Settings.Localization.Timeout;
            mng.BundleDir = Settings.Localization.BundleDir;
            mng.SupportedLanguages = Settings.Localization.SupportedLanguages;
            mng.Localization.ShowKeysOnDevMode = Settings.Localization.ShowKeysOnDevMode;

            mng.UpdateDefaultLanguage();
        }
    }
}
