using System;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Locale;
using Zenject;

public class LocaleInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string ProjectId = LocalizationManager.LocationData.DefaultProjectId;
        public string EnvironmentId = LocalizationManager.LocationData.DefaultEnvironmentId;
        public string SecretKey = LocalizationManager.LocationData.DefaultSecretKey;
        public string BundleDir = LocalizationManager.DefaultBundleDir;
        public string[] SupportedLanguages = LocalizationManager.DefaultSupportedLanguages;
        public float Timeout = LocalizationManager.DefaultTimeout;
    };
    
    public SettingsData Settings;

	public override void InstallBindings()
	{	
        Container.Bind<Localization>().ToSingle();
        Container.BindInstance("locale_project_id", Settings.ProjectId);
        Container.BindInstance("locale_env_id", Settings.EnvironmentId);
        Container.BindInstance("locale_secret_key", Settings.SecretKey);
        Container.BindInstance("locale_supported_langs", Settings.SupportedLanguages);
        Container.BindInstance("locale_timeout", Settings.Timeout);
        Container.BindInstance("locale_bundle_dir", Settings.BundleDir);

        var mng = Container.Instantiate<LocalizationManager>();
        Container.BindInstance(mng);
	}
}
