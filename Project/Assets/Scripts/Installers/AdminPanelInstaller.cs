using Zenject;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

public class AdminPanelInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        if(Container.HasBinding<AdminPanel>())
        {
            return;
        }
        Container.Bind<AdminPanel>().ToSingle<AdminPanel>();       
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGame>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelLog>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelApplication>();
    }
}
