using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.ServerEvents;

public class CrossPromotionInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Listen<CrossPromotionManager>().WhenResolved(SetupManager);

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCrossPromotion>(CreateAdminPanel);
    }

    void SetupManager(CrossPromotionManager mng)
    {
        var eventTracker = Container.Resolve<IEventTracker>();
        mng.TrackSystemEvent = eventTracker.TrackSystemEvent;
        mng.TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
        mng.AppEvents = Container.Resolve<IAppEvents>();
    }

    AdminPanelCrossPromotion CreateAdminPanel()
    {
        return new AdminPanelCrossPromotion(Container.Resolve<CrossPromotionManager>());
    }
}
