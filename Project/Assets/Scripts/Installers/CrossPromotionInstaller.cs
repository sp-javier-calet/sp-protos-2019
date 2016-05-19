
using System;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.CrossPromotion;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;

public class CrossPromotionInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<SocialPoint.CrossPromotion.CrossPromotionManager>().ToMethod<CrossPromotionManager>(CreateManager, SetupManager);
        Container.Bind<IDisposable>().ToLookup<CrossPromotionManager>();
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCrossPromotion>(CreateAdminPanel);
    }

    AdminPanelCrossPromotion CreateAdminPanel()
    {
        return new AdminPanelCrossPromotion(Container.Resolve<SocialPoint.CrossPromotion.CrossPromotionManager>());
    }

    CrossPromotionManager CreateManager()
    {
        return new CrossPromotionManager(Container.Resolve<ICoroutineRunner>());
    }

    void SetupManager(CrossPromotionManager mng)
    {
        var eventTracker = Container.Resolve<IEventTracker>();
        mng.TrackSystemEvent = eventTracker.TrackSystemEvent;
        mng.TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
        mng.AppEvents = Container.Resolve<IAppEvents>();
    }
}
