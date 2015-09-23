using System;
using SocialPoint.Locale;
using SocialPoint.AdminPanel;
using Zenject;

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
    };

    public SettingsData Settings;

    public override void InstallBindings()
    {	
        if(Container.HasBinding<Localization>())
        {
            return;
        }
        Container.Bind<Localization>().ToSingle();
        Container.BindInstance("locale_project_id", Settings.ProjectId);
        Container.BindInstance("locale_env_id", Settings.EnvironmentId.ToString());
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
        Container.BindInstance("locale_secret_key", secretKey);
        Container.BindInstance("locale_supported_langs", Settings.SupportedLanguages);
        Container.BindInstance("locale_timeout", Settings.Timeout);
        Container.BindInstance("locale_bundle_dir", Settings.BundleDir);
        Container.Bind<ILocalizationManager>().ToSingle<LocalizationManager>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLocale>();
    }
}
