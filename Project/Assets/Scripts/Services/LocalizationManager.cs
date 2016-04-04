using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.ScriptEvents;
using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using Zenject;

public class LocalizationManager : SocialPoint.Locale.LocalizationManager
{
    IEventDispatcher _dispatcher;
    LocalizeAttributeConfiguration _localizeAttributeConfig;

    public LocalizationManager(IHttpClient client, IAppInfo appInfo, Localization locale) :
        base(client, appInfo, locale)
    {
        Location.ProjectId = ServiceLocator.Instance.Resolve<string>("locale_project_id");
        Location.EnvironmentId = ServiceLocator.Instance.Resolve<string>("locale_env_id");
        Location.SecretKey = ServiceLocator.Instance.Resolve<string>("locale_secret_key");
        Timeout = ServiceLocator.Instance.Resolve<float>("locale_timeout");
        BundleDir = ServiceLocator.Instance.Resolve<string>("locale_bundle_dir");
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
        _dispatcher = ServiceLocator.Instance.Resolve<IEventDispatcher>();
        _localizeAttributeConfig = ServiceLocator.Instance.Resolve<LocalizeAttributeConfiguration>();

        _dispatcher.AddListener<UIViewControllerStateChangeEvent>(OnViewControllerStateChangeEvent);
        _dispatcher.AddListener<UIViewControllerInstantiateEvent>(OnViewControllerInstantiateEvent);
    }

    void OnViewControllerStateChangeEvent(UIViewControllerStateChangeEvent ev)
    {
        if(ev.State == UIViewController.ViewState.Appearing || ev.State == UIViewController.ViewState.Shown)
        {
            _localizeAttributeConfig.Apply(ev.Controller);
        }
    }

    void OnViewControllerInstantiateEvent(UIViewControllerInstantiateEvent ev)
    {
        foreach(var component in ev.Object.GetComponents<UnityEngine.MonoBehaviour>())
        {
            _localizeAttributeConfig.Apply(component);
        }
    }

    override public void Dispose()
    {
        base.Dispose();
        _dispatcher.RemoveListener<UIViewControllerStateChangeEvent>(OnViewControllerStateChangeEvent);
        _dispatcher.RemoveListener<UIViewControllerInstantiateEvent>(OnViewControllerInstantiateEvent);
    }

}