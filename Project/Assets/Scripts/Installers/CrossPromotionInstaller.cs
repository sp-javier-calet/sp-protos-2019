
using System;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;

public class CrossPromotionInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<SocialPoint.CrossPromotion.CrossPromotionManager>().ToSingle<CrossPromotionManager>();
        Container.Bind<IDisposable>().ToLookup<CrossPromotionManager>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<SocialPoint.CrossPromotion.AdminPanelCrossPromotion>();
    }
}
