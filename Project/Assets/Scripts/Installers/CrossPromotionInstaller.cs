
using System;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.CrossPromotion;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;

public class CrossPromotionInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<SocialPoint.CrossPromotion.CrossPromotionManager>().ToSingleMethod<CrossPromotionManager>(CreateCrossPromotionManager);
        Container.Bind<IDisposable>().ToLookup<CrossPromotionManager>();
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelCrossPromotion>(CreateAdminPanel);
    }

    AdminPanelCrossPromotion CreateAdminPanel()
    {
        return new AdminPanelCrossPromotion(Container.Resolve<SocialPoint.CrossPromotion.CrossPromotionManager>());
    }

    CrossPromotionManager CreateCrossPromotionManager()
    {
        var mng = new CrossPromotionManager(Container.Resolve<ICoroutineRunner>());
        var eventTracker = Container.Resolve<IEventTracker>();
        mng.TrackSystemEvent = eventTracker.TrackSystemEvent;
        mng.TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
        mng.AppEvents = Container.Resolve<IAppEvents>();
        return mng;
    }
}
