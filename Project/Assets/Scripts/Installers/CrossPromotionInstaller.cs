using Zenject;
using SocialPoint.AdminPanel;

public class CrossPromotionInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<CrossPromotionManager>().ToSingle<CrossPromotionManager>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<SocialPoint.CrossPromotion.AdminPanelCrossPromotion>();
    }
}
