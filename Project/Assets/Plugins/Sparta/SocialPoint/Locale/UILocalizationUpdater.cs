using System;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;

namespace SocialPoint.Locale
{
    public class UILocalizationUpdater : IDisposable
    {
        readonly IEventDispatcher _dispatcher;
        readonly LocalizeAttributeConfiguration _localizeAttributeConfig;

        public UILocalizationUpdater(LocalizeAttributeConfiguration attrConfig, IEventDispatcher dispatcher)
        {
            _localizeAttributeConfig = attrConfig;
            _dispatcher = dispatcher;
            DebugUtils.Assert(_dispatcher != null, "UILocalizationUpdater requires a valid IEventDispatcher instance");
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
            var objs = ev.Object.GetComponents<UnityEngine.MonoBehaviour>();
            for(var i = 0; i < objs.Length; ++i)
            {
                var component = objs[i];
                _localizeAttributeConfig.Apply(component);
            }
        }

        public void Dispose()
        {
            _dispatcher.RemoveListener<UIViewControllerStateChangeEvent>(OnViewControllerStateChangeEvent);
            _dispatcher.RemoveListener<UIViewControllerInstantiateEvent>(OnViewControllerInstantiateEvent);
        }
    }
}
