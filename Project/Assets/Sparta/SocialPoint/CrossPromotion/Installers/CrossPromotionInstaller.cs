using SocialPoint.AdminPanel;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;

public class CrossPromotionInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCrossPromotion>(CreateAdminPanel);
    }

    AdminPanelCrossPromotion CreateAdminPanel()
    {
        return new AdminPanelCrossPromotion(Container.Resolve<CrossPromotionManager>());
    }
}
