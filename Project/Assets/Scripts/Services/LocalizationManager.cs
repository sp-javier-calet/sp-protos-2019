using Zenject;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Locale;

class LocalizationManager : SocialPoint.Locale.LocalizationManager
{
    [Inject("locale_project_id")]
    public string InjectProjectId
    {
        set
        {
            Location.ProjectId = value;
        }
    }

    [Inject("locale_env_id")]
    public string InjectEnvironmentId
    {
        set
        {
            Location.EnvironmentId = value;
        }
    }

    [Inject("locale_secret_key")]
    public string InjectSecretKey
    {
        set
        {
            Location.SecretKey = value;
        }
    }
    
    [Inject("locale_supported_langs")]
    public string[] InjectSupportedLanguages
    {
        set
        {
            SupportedLanguages = value;
        }
    }

    [Inject("locale_timeout")]
    public float InjectTimeout
    {
        set
        {
            Timeout = value;
        }
    }

    
    [Inject("locale_bundle_dir")]
    public string InjectBundleDir
    {
        set
        {
            BundleDir = value;
        }
    }

    [Inject]
    public Localization InjectLocalization
    {
        set
        {
            Localization = value;
        }
    }

    public LocalizationManager(IHttpClient client, IAppInfo appInfo, MonoBehaviour behaviour):
        base(client, appInfo, behaviour)
    {
    }

    [PostInject]
    public void PostInject()
    {
        Start();
    }

}