using SocialPoint.Attributes;
using SocialPoint.GUIControl;
using UnityEngine;
using SocialPoint.Lifecycle;

namespace SocialPoint.ScriptEvents
{
    public struct UIViewControllerAwakeEvent
    {
        public UIViewController Controller;
    }

    public struct UIViewControllerStateChangeEvent
    {
        public UIViewController Controller;
        public UIViewController.ViewState State;
    }

    public struct UIViewControllerInstantiateEvent
    {
        public UIViewController Controller;
        public GameObject Object;
    }

    public static class UIViewControllerScriptEventsExtension
    {
        public static string GetScriptEventId(this UIViewController ctrl)
        {
            return ctrl.GetType().FullName;
        }
    }

    public sealed class UIViewControllerAwakeEventSerializer : BaseScriptEventSerializer<UIViewControllerAwakeEvent>
    {
        public UIViewControllerAwakeEventSerializer() : base("event.gui.controller_awake")
        {
        }

        override protected Attr SerializeEvent(UIViewControllerAwakeEvent ev)
        {
            return new AttrString(ev.Controller.GetScriptEventId());
        }
    }

    public sealed class UIViewControllerStateChangeEventSerializer : BaseScriptEventSerializer<UIViewControllerStateChangeEvent>
    {
        public UIViewControllerStateChangeEventSerializer() : base("event.gui.controller_state_change")
        {
        }

        const string ControllerIdAttrKey = "controller";
        const string StateAttrKey = "state";

        override protected Attr SerializeEvent(UIViewControllerStateChangeEvent ev)
        {
            var data = new AttrDic();
            data.SetValue(ControllerIdAttrKey, ev.Controller.GetScriptEventId());
            data.SetValue(StateAttrKey, ev.State.ToString());
            return data;
        }
    }

    public sealed class UIViewControllerInstantiateEventSerializer : BaseScriptEventSerializer<UIViewControllerInstantiateEvent>
    {
        public UIViewControllerInstantiateEventSerializer() : base("event.gui.controller_instantiate")
        {
        }

        const string ControllerIdAttrKey = "controller";
        const string ObjectAttrKey = "object";

        override protected Attr SerializeEvent(UIViewControllerInstantiateEvent ev)
        {
            var data = new AttrDic();
            data.SetValue(ControllerIdAttrKey, ev.Controller.GetScriptEventId());
            data.SetValue(ObjectAttrKey, ev.Object.name);
            return data;
        }
    }

    public sealed class GUIControlBridge :
        IScriptEventsBridge
    {
        IEventProcessor _processor;

        public GUIControlBridge()
        {
            UIViewController.AwakeEvent += OnViewControllerAwake;
        }

        const string UIViewControllerInstantiateEvent = "event.gui.instantiate";

        public void Load(IScriptEventProcessor scriptProcessor, IEventProcessor processor)
        {
            _processor = processor;
            scriptProcessor.RegisterSerializer(new UIViewControllerAwakeEventSerializer());
            scriptProcessor.RegisterSerializer(new UIViewControllerStateChangeEventSerializer());
            scriptProcessor.RegisterSerializer(new UIViewControllerInstantiateEventSerializer());
        }

        public void Dispose()
        {
            UIViewController.AwakeEvent -= OnViewControllerAwake;
        }

        void OnViewControllerAwake(UIViewController ctrl)
        {
            ctrl.ViewEvent += OnViewControllerStateChange;
            ctrl.InstantiateEvent += OnViewControllerInstantiate;
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new UIViewControllerAwakeEvent {
                Controller = ctrl
            });
        }

        void OnViewControllerStateChange(UIViewController ctrl, UIViewController.ViewState state)
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new UIViewControllerStateChangeEvent { 
                Controller = ctrl,
                State = state
            });
        }

        void OnViewControllerInstantiate(UIViewController ctrl, GameObject go)
        {
            if(_processor == null)
            {
                return;
            }
            _processor.Process(new UIViewControllerInstantiateEvent { 
                Controller = ctrl,
                Object = go
            });
        }

    }

}