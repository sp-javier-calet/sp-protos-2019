using System;
using SocialPoint.Locale;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Utils;
using SocialPoint.AppEvents;
using SocialPoint.ScriptEvents;

public class LocaleInstaller : Installer
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
       // public string ProjectId = GameLocalizationManager.LocationData.DefaultProjectId;
        public EnvironmentID EnvironmentId = EnvironmentID.prod;
        //public string SecretKeyDev = GameLocalizationManager.LocationData.DefaultSecretKey;
        //public string SecretKeyLoc = GameLocalizationManager.LocationData.DefaultSecretKey;
        //public string SecretKeyProd = GameLocalizationManager.LocationData.DefaultSecretKey;
        //public string BundleDir = GameLocalizationManager.DefaultBundleDir;
        //public string[] SupportedLanguages = GameLocalizationManager.DefaultSupportedLanguages;
        //public float Timeout = GameLocalizationManager.DefaultTimeout;
        public bool EditorDebug = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        // TODO Game code
        Container.Rebind<Localization>().ToMethod<Localization>(CreateLocalization);
        //Container.Rebind<ILocalizationManager>().ToMethod<GameLocalizationManager>(CreateLocalizationManager, SetupLocalizationManager);
        //Container.Bind<IDisposable>().ToLookup<ILocalizationManager>();
         
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

   /* GameLocalizationManager CreateLocalizationManager()
    {
        return new GameLocalizationManager(
            Container.Resolve<IHttpClient>(),
            Container.Resolve<IAppInfo>(),
            Container.Resolve<Localization>(),
            Container.Resolve<LocalizeAttributeConfiguration>(),
            Container.Resolve<IEventDispatcher>());
    }

    void SetupLocalizationManager(GameLocalizationManager mng)
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
    */

    Localization CreateLocalization()
    {
        var locale = new Localization();
#if UNITY_EDITOR
        locale.Debug = Settings.EditorDebug;
#endif
        return locale;
    }
}
