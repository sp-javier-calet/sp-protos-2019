using SocialPoint.GUIControl;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.ScriptEvents;

public class LocalizationManager : SocialPoint.Locale.LocalizationManager
{
    readonly EventDispatcher _dispatcher;
    LocalizeAttributeConfiguration _localizeAttributeConfig;

    public LocalizationManager(IHttpClient client, IAppInfo appInfo, Localization locale, LocalizeAttributeConfiguration attrConfig, IEventDispatcher dispatcher) :
        base(client, appInfo, locale)
    {
        _localizeAttributeConfig = attrConfig;
        _dispatcher = dispatcher as EventDispatcher;
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