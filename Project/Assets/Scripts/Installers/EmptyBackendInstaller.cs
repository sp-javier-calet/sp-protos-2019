using System;
using Zenject;
using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Crash;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.ServerMessaging;
using System.Text;

public class EmptyBackendInstaller : MonoInstaller, IInitializable
{
    public override void InstallBindings()
    {
        if(!Container.HasBinding<IEventTracker>())
        {
            Container.Bind<IEventTracker>().ToSingle<EmptyEventTracker>();
            Container.Bind<IDisposable>().ToLookup<IEventTracker>();
        }
        if(!Container.HasBinding<ILogin>())
        {
            Container.Bind<IInitializable>().ToSingleInstance(this);
            Container.Bind<ILogin>().ToSingle<EmptyLogin>();
            Container.Bind<IDisposable>().ToLookup<ILogin>();
        }
        if(!Container.HasInstalled<LoginAdminPanelInstaller>())
        {
            Container.Install<LoginAdminPanelInstaller>();
        }
        if(!Container.HasBinding<ICommandQueue>())
        {
            Container.Bind<ICommandQueue>().ToSingle<EmptyCommandQueue>();
            Container.Bind<IDisposable>().ToLookup<ICommandQueue>();
        }
        if(!Container.HasBinding<BreadcrumbManager>())
        {
            Container.Bind<BreadcrumbManager>().ToSingle();
        }
        if(!Container.HasBinding<ICrashReporter>())
        {
            Container.Bind<ICrashReporter>().ToSingle<EmptyCrashReporter>();
            Container.Bind<IDisposable>().ToLookup<ICrashReporter>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelCrashReporter>();
        }
        if(!Container.HasBinding<IMessageCenter>())
        {
            Container.Bind<IMessageCenter>().ToSingle<EmptyMessageCenter>();
            Container.Bind<IDisposable>().ToLookup<IMessageCenter>();
            Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelMessageCenter>();
        }
        if(!Container.HasBinding<SocialPoint.Notifications.NotificationManager>())
        {
            Container.Install<NotificationInstaller>();
        }
        if(!Container.HasBinding<SocialPoint.CrossPromotion.CrossPromotionManager>())
        {
            Container.Install<CrossPromotionInstaller>();
        }
    }

    public void Initialize()
    {
        var loader = Container.TryResolve<IGameLoader>();
        if(loader != null)
        {
            loader.Load(null);
        }
    }

}
