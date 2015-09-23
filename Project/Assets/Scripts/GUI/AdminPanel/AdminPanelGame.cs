using SocialPoint.AdminPanel;
using UnityEngine;
using Zenject;

public class AdminPanelGame : IAdminPanelConfigurer {
    
    public void OnConfigure(AdminPanel adminPanel)
    {
        adminPanel.RegisterGUI("Game", new AdminPanelGameControl());
        adminPanel.RegisterGUI("Game", new AdminPanelNestedGUI("Model", new AdminPanelGameModel()));
    }

    private class AdminPanelGameControl : IAdminPanelGUI
    {
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Control");
            layout.CreateConfirmButton("Restart", () => {
                // FIXME
                ZenUtil.LoadScene(Application.loadedLevelName);
            });

            layout.CreateMargin(2);
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
}
