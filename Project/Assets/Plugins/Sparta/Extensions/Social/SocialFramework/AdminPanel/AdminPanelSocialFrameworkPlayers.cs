#if ADMIN_PANEL 

using System;
using SocialPoint.AdminPanel;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class AdminPanelSocialFrameworkPlayers : IAdminPanelGUI
    {
        readonly AdminPanelPlayersRanking _rankingPanel;
        readonly AdminPanelLocalPlayerInfo _localPlayerPanel;

        public AdminPanelSocialFrameworkPlayers(PlayersManager playersManager, SocialManager socialManager, AdminPanelConsole console)
        {
            _rankingPanel = new AdminPanelPlayersRanking(playersManager, console);
            _localPlayerPanel = new AdminPanelLocalPlayerInfo(playersManager, socialManager, console);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Players");
            layout.CreateMargin();
            layout.CreateOpenPanelButton("Local Player Info", _localPlayerPanel);
            layout.CreateOpenPanelButton("Ranking", _rankingPanel);
        }

        #region Base Panels

        abstract class BasePlayersPanel : AdminPanelSocialFramework.BaseRequestPanel
        {
            protected readonly PlayersManager _playersManager;
            protected readonly AdminPanelConsole _console;

            protected BasePlayersPanel(PlayersManager players, AdminPanelConsole console)
            {
                _playersManager = players;
                _console = console;
            }
        }

        #endregion

        class AdminPanelLocalPlayerInfo : BasePlayersPanel
        {
            readonly SocialManager _socialManager;

            public AdminPanelLocalPlayerInfo(PlayersManager players, SocialManager socialManager, AdminPanelConsole console) : base(players, console)
            {
                _socialManager = socialManager;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Local Player Info");
                layout.CreateMargin();
                layout.CreateTextArea(_socialManager.LocalPlayer.ToString());
            }
        }

        class AdminPanelPlayersRanking : BasePlayersPanel
        {
            PlayersRanking _ranking;

            readonly AdminPanelSocialFramework.BaseUserInfoPanel _infoPanel;

            public AdminPanelPlayersRanking(PlayersManager players, AdminPanelConsole console) : base(players, console)
            {
                _infoPanel = new AdminPanelSocialFramework.BaseUserInfoPanel(players, console);
            }

            public override void OnOpened()
            {
                base.OnOpened();
                _ranking = null;
            }

            public override void OnCreateGUI(AdminPanelLayout layout)
            {
                layout.CreateLabel("Ranking");
                layout.CreateMargin();

                if(_ranking != null)
                {
                    var itr = _ranking.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var player = itr.Current;
                        var playerLabel = string.Format("[{0}({1})]: {2}", player.Name, player.Uid, player.Score);
                        layout.CreateOpenPanelButton(playerLabel, _infoPanel);
                    }
                    itr.Dispose();
                    layout.CreateMargin();
                }
                else
                {
                    if(_wampRequest == null)
                    {
                        _wampRequest = _playersManager.LoadPlayersRanking(null,
                            (err, ranking) => {
                                if(Error.IsNullOrEmpty(err))
                                {
                                    _ranking = ranking;
                                    _console.Print("Ranking loaded successfully");
                                    Cancel();
                                    layout.Refresh();
                                }
                                else
                                {
                                    _console.Print(string.Format("Error loading ranking. {0} ", err));
                                    _wampRequestError = err;
                                    layout.Refresh();
                                }
                            });
                    }  
                    if(Error.IsNullOrEmpty(_wampRequestError))
                    {
                        layout.CreateLabel("Loading ranking...");
                    }
                    else
                    {
                        layout.CreateLabel("Error loading ranking.");
                        layout.CreateTextArea(_wampRequestError.ToString());
                        layout.CreateButton("Retry", () => {
                            Cancel();
                            layout.Refresh();
                        });
                    }
                }
            }
        }
    }
}

#endif
