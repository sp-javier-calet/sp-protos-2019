﻿using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using UnityEngine;

public class AdminPanelGame : IAdminPanelConfigurer
{
    IAppEvents _appEvents;
    IGameLoader _gameLoader;
    GameModel _model;

    public AdminPanelGame(IAppEvents appEvents, IGameLoader gameLoader, GameModel model)
    {
        _appEvents = appEvents;
        _gameLoader = gameLoader;
        _model = model;
    }

    public void OnConfigure(AdminPanel adminPanel)
    {
        adminPanel.RegisterGUI("Game", new AdminPanelGameControl(_appEvents, _gameLoader));
        adminPanel.RegisterGUI("Game", new AdminPanelNestedGUI("Model", new AdminPanelGameModel(_model)));
    }

    private class AdminPanelGameControl : IAdminPanelGUI
    {
        IAppEvents _appEvents;
        IGameLoader _gameLoader;

        public AdminPanelGameControl(IAppEvents appEvents, IGameLoader gameLoader)
        {
            _appEvents = appEvents;
            _gameLoader = gameLoader;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Game Control");
            layout.CreateConfirmButton("Restart", () => {
                _appEvents.RestartGame();
            });
            layout.CreateButton("Delete local game", () => {
                _gameLoader.DeleteLocalGame();
            });
            layout.CreateMargin(2);
        }
    }

    private class AdminPanelGameModel : IAdminPanelGUI
    {
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
