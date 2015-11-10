using System;
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
        Container.Bind<IEventsBridge>().ToSingle<AppEventsBridge>();
        Container.Bind<IEventsBridge>().ToSingle<ServerEventsBridge>();
        Container.Rebind<IEventDispatcher>().ToSingle<EventDispatcher>();
    }

}