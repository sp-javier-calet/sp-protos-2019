using Zenject;
using UnityEngine;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.AdminPanel;
using SocialPoint.Notifications;

public class NotificationInstaller : MonoInstaller
{
    [Inject]
    MonoBehaviour _behaviour;

    [Inject]
    IAppEvents _appEvents;

    [Inject]
    ICommandQueue _commandQueue;

    public override void InstallBindings()
    {
        if(Container.HasBinding<NotificationManager>())
        {
            return;
        }

        var mng = new NotificationManager(_behaviour, _appEvents, _commandQueue);
        Container.BindInstance(mng);
        Container.BindInstance(mng.Services);
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelNotifications>();
    }
}
