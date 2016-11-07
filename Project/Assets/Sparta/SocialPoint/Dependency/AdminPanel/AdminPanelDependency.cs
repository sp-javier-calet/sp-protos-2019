using SocialPoint.AdminPanel;

namespace SocialPoint.Dependency
{
    public class AdminPanelDependency : IAdminPanelConfigurer, IAdminPanelGUI
    {
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("Dependency", this);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
           // var graph = DependencyGraphBuilder.Graph;


        }
    }
}
