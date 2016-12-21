using SocialPoint.GUIControl;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Network;
using SocialPoint.ScriptEvents;

public class GameLocalizationManager : LocalizationManager
{
    readonly IEventDispatcher _dispatcher;
    LocalizeAttributeConfiguration _localizeAttributeConfig;

    public GameLocalizationManager(LocalizeAttributeConfiguration attrConfig, IEventDispatcher dispatcher)
    {
        _localizeAttributeConfig = attrConfig;
        _dispatcher = dispatcher;
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