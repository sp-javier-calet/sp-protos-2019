
using System;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.CrossPromotion;

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
        return new CrossPromotionManager(Container.Resolve<ICoroutineRunner>());
    }
}
