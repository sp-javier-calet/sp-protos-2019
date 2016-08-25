using System;
using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

public class CrossPromotionInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<CrossPromotionManager>().ToMethod<CrossPromotionManager>(CreateManager, SetupManager);
        Container.Bind<IDisposable>().ToLookup<CrossPromotionManager>();
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCrossPromotion>(CreateAdminPanel);
    }

    AdminPanelCrossPromotion CreateAdminPanel()
    {
        return new AdminPanelCrossPromotion(Container.Resolve<CrossPromotionManager>());
    }

    CrossPromotionManager CreateManager()
    {
        return new CrossPromotionManager(Container.Resolve<ICoroutineRunner>(), Container.Resolve<PopupsController>());
    }

    void SetupManager(CrossPromotionManager mng)
    {
        var eventTracker = Container.Resolve<IEventTracker>();
        mng.TrackSystemEvent = eventTracker.TrackSystemEvent;
        mng.TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
        mng.AppEvents = Container.Resolve<IAppEvents>();
    }
}
