using System;
using SocialPoint.Locale;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Utils;
using SocialPoint.AppEvents;
using SocialPoint.ScriptEvents;

public class LocaleInstaller : MonoInstaller
{
    public enum EnvironmentID
    {
        dev,
        loc,
        prod
    }

    [Serializable]
    public class SettingsData
    {
        public string ProjectId = LocalizationManager.LocationData.DefaultProjectId;
        public EnvironmentID EnvironmentId = EnvironmentID.prod;
        public string SecretKeyDev = LocalizationManager.LocationData.DefaultSecretKey;
        public string SecretKeyLoc = LocalizationManager.LocationData.DefaultSecretKey;
        public string SecretKeyProd = LocalizationManager.LocationData.DefaultSecretKey;
        public string BundleDir = LocalizationManager.DefaultBundleDir;
        public string[] SupportedLanguages = LocalizationManager.DefaultSupportedLanguages;
        public float Timeout = LocalizationManager.DefaultTimeout;
        public bool EditorDebug = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<Localization>().ToMethod<Localization>(CreateLocalization);
        Container.Rebind<ILocalizationManager>().ToMethod<LocalizationManager>(CreateLocalizationManager, SetupLocalizationManager);
        Container.Bind<IDisposable>().ToLookup<ILocalizationManager>();
         
        Container.Rebind<LocalizeAttributeConfiguration>().ToMethod<LocalizeAttributeConfiguration>(CreateLocalizeAttributeConfiguration);

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelLocale>(CreateAdminPanel);
    }

    LocalizeAttributeConfiguration CreateLocalizeAttributeConfiguration()
    {
        return new LocalizeAttributeConfiguration(
            Container.Resolve<Localization>(),
            Container.ResolveList<IMemberAttributeObserver<LocalizeAttribute>>());
    }

    AdminPanelLocale CreateAdminPanel()
    {
        return new AdminPanelLocale(
            Container.Resolve<ILocalizationManager>());
    }

    LocalizationManager CreateLocalizationManager()
    {
        return new LocalizationManager(
            Container.Resolve<IHttpClient>(),
            Container.Resolve<IAppInfo>(),
            Container.Resolve<Localization>(),
            Container.Resolve<LocalizeAttributeConfiguration>(),
            Container.Resolve<IEventDispatcher>());
    }

    void SetupLocalizationManager(LocalizationManager mng)
    {
        string secretKey;
        if(Settings.EnvironmentId == EnvironmentID.dev)
        {
            secretKey = Settings.SecretKeyDev;
        }
        else if(Settings.EnvironmentId == EnvironmentID.loc)
        {
            secretKey = Settings.SecretKeyLoc;
        }
        else
        {
            secretKey = Settings.SecretKeyProd;
        }
        mng.Location.ProjectId = Settings.ProjectId;
        mng.Location.EnvironmentId = Settings.EnvironmentId.ToString();
        mng.Location.SecretKey = secretKey;
        mng.Timeout = Settings.Timeout;
        mng.BundleDir = Settings.BundleDir;
        mng.AppEvents = Container.Resolve<IAppEvents>();
    }

    Localization CreateLocalization()
    {
        var locale = new Localization();
#if UNITY_EDITOR
        locale.Debug = Settings.EditorDebug;
#endif
        return locale;
    }
}
