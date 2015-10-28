using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using UnityEngine;
using Zenject;

public class AdminPanelGame : IAdminPanelConfigurer
{
    [Inject]
    IAppEvents _appEvents;

    [Inject]
    GameModel _model;

    public void OnConfigure(AdminPanel adminPanel)
    {
        adminPanel.RegisterGUI("Game", new AdminPanelGameControl(_appEvents));
        adminPanel.RegisterGUI("Game", new AdminPanelNestedGUI("Model", new AdminPanelGameModel(_model)));
    }

    private class AdminPanelGameControl : IAdminPanelGUI
    {
        IAppEvents _appEvents;

        public AdminPanelGameControl(IAppEvents appEvents)
        {
            _appEvents = appEvents;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Control");
            layout.CreateConfirmButton("Restart", () => {
                _appEvents.RestartGame();
            });
            layout.CreateMargin(2);
        }
    }

    private class AdminPanelGameModel : IAdminPanelGUI
    {
        [Inject]
        GameModel _model;

        public AdminPanelGameModel(GameModel model)
        {
            _model = model;
        }
        
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Model");

            if(_model != null)
            {
                layout.CreateLabel("Player");
                layout.CreateTextArea((_model.Player != null) ? _model.Player.ToString() : "No Player");

                layout.CreateLabel("Config");
                layout.CreateTextArea((_model.Config != null) ? _model.Config.ToString() : "No Config");
            }
        }
    }
}
