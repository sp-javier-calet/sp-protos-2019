using Zenject;
using SocialPoint.AdminPanel;

public class AdminPanelGame : IAdminPanelConfigurer {
    
    public void OnConfigure(AdminPanel adminPanel)
    {
        adminPanel.RegisterGUI("Game", new AdminPanelNestedGUI("Model", new AdminPanelGameModel()));
        adminPanel.RegisterGUI("Game", new AdminPanelNestedGUI("Control", new AdminPanelGameControl()));
        adminPanel.RegisterGUI("Game", new AdminPanelNestedGUI("Zenject", new AdminPanelZenject()));
    }

    private class AdminPanelGameControl : IAdminPanelGUI
    {


        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Control");

        }
    }

    private class AdminPanelGameModel : IAdminPanelGUI
    {
        [Inject]
        GameModel GameModel;
        
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Model");

            if(GameModel != null)
            {
                layout.CreateLabel("Player");
                layout.CreateTextArea((GameModel.Player != null) ? GameModel.Player.ToString() : "No Player");

                layout.CreateLabel("Config");
                layout.CreateTextArea((GameModel.Config != null) ? GameModel.Config.ToString() : "No Config");
            }
        }
    }

    private class AdminPanelZenject : IAdminPanelGUI
    {
        [Inject]
        DiContainer Container;

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Zenject");
            if(Container != null)
            {
                layout.CreateLabel(Container.ToString());
            }
        }
    }
}
