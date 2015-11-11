using System;
using System.Collections.Generic;
using Zenject;
using SocialPoint.ScriptEvents;

public class ScriptEventsInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
    }

    public SettingsData Settings;

    PopupsController _popups;

    public override void InstallBindings()
    {
        Container.Rebind<IEventDispatcher>().ToSingleMethod<EventDispatcher>(CreateEventDispatcher);
        Container.Rebind<IScriptEventDispatcher>().ToSingleMethod<ScriptEventDispatcher>(CreateScriptEventDispatcher);
    }

    public EventDispatcher CreateEventDispatcher(InjectContext ctx)
    {
        var dispatcher = Container.Instantiate<EventDispatcher>();
        dispatcher.AddBridges(Container.Resolve<List<IEventsBridge>>());
        return dispatcher;
    }

    public ScriptEventDispatcher CreateScriptEventDispatcher(InjectContext ctx)
    {
        var dispatcher = Container.Instantiate<ScriptEventDispatcher>();
        dispatcher.AddBridges(Container.Resolve<List<IScriptEventsBridge>>());
        return dispatcher;
    }

}