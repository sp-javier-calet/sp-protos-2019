using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

public class AdminPanelInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<AdminPanel>().ToSingle<AdminPanel>();       
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGame>();
    }
}
