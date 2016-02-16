using System;
using System.Collections.Generic;
using Zenject;
using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;
using SocialPoint.AdminPanel;

public class ScriptEventsPostInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToSingleInstance(this);
    }

    public void Initialize()
    {
        {
            var dispatcher = Container.Resolve<IEventDispatcher>();
            var bridges = Container.Resolve<List<IEventsBridge>>();
            foreach(var bridge in bridges)
            {
                dispatcher.AddBridge(bridge);
            }
        }
        {
            var dispatcher = Container.Resolve<IScriptEventDispatcher>();
            var bridges = Container.Resolve<List<IScriptEventsBridge>>();
            foreach(var bridge in bridges)
            {
                dispatcher.AddBridge(bridge);
            }
        }
    }
}