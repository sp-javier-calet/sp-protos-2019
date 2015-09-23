using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using Zenject;

public class LocalizationManager : SocialPoint.Locale.LocalizationManager
{
    [Inject("locale_project_id")]
    string injectProjectId
    {
        set
        {
            Location.ProjectId = value;
        }
    }

    [Inject("locale_env_id")]
    string injectEnvironmentId
    {
        set
        {
            Location.EnvironmentId = value;
        }
    }

    [Inject("locale_secret_key")]
    string injectSecretKey
    {
        set
        {
            Location.SecretKey = value;
        }
    }

    [Inject("locale_supported_langs")]
    string[] injectSupportedLanguages
    {
        set
        {
            SupportedLanguages = value;
        }
    }

    [Inject("locale_timeout")]
    float injectTimeout
    {
        set
        {
            Timeout = value;
        }
    }

    
    [Inject("locale_bundle_dir")]
    string injectBundleDir
    {
        set
        {
            BundleDir = value;
        }
    }

    [Inject]
    Localization injectLocalization
    {
        set
        {
            Localization = value;
        }
    }

    public LocalizationManager(IHttpClient client, IAppInfo appInfo) :
        base(client, appInfo)
    {
    }

    [PostInject]
    void PostInject()
    {
        Start();
    }

}