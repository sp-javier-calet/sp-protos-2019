using System;
using SocialPoint.AdminPanel;
using UnityEngine.UI;

namespace SocialPoint.AppEvents
{
    public class AdminPanelAppEvents : IAdminPanelGUI, IAdminPanelConfigurer, IDisposable
    {
        IAppEvents _appEvents;
        Text _textComponent;
        string _eventsLog;

        public AdminPanelAppEvents(IAppEvents appEvents)
        {
            _appEvents = appEvents;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _eventsLog = string.Empty;

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("App Events", this));

            _appEvents.OpenedFromSource += OnOpenedFromSource;
            _appEvents.WasCovered += OnWasCovered;
            _appEvents.WasOnBackground += OnWasOnBackground;
            _appEvents.WillGoBackground.Add(0, OnWillGoBackground);
            _appEvents.GameWillRestart.Add(0, OnGameWillRestart);
            _appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
            _appEvents.LevelWasLoaded += OnLevelWasLoaded;
            _appEvents.ApplicationQuit += OnApplicationQuit;
            _appEvents.ReceivedMemoryWarning += OnReceivedMemoryWarning;
        }

        void OnOpenedFromSource(object source)
        {
            AddEvent("OpenedFromSource: " + source);
        }

        void OnWasCovered()
        {
            AddEvent("WasCovered");
        }

        void OnWasOnBackground()
        {
            AddEvent("WasOnBackground");
        }

        void OnWillGoBackground()
        {
            AddEvent("WillGoBackground");
        }

        void OnGameWillRestart()
        {
            AddEvent("GameWillRestart");
        }

        void OnGameWasLoaded()
        {
            AddEvent("GameWasLoaded");
        }

        void OnLevelWasLoaded(int level)
        {
            AddEvent("LevelWasLoaded: " + level); 
        }

        void OnApplicationQuit()
        {
            AddEvent("ApplicationQuit");
        }

        void OnReceivedMemoryWarning()
        {
            AddEvent("ReceivedMemoryWarning");
        }

        public void Dispose()
        {
            _appEvents.OpenedFromSource -= OnOpenedFromSource;
            _appEvents.WasCovered -= OnWasCovered;
            _appEvents.WasOnBackground -= OnWasOnBackground;
            _appEvents.WillGoBackground.Remove(OnWillGoBackground);
            _appEvents.GameWillRestart.Remove(OnGameWillRestart);
            _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            _appEvents.LevelWasLoaded -= OnLevelWasLoaded;
            _appEvents.ApplicationQuit -= OnApplicationQuit;
            _appEvents.ReceivedMemoryWarning -= OnReceivedMemoryWarning;
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("App Events");
            _textComponent = layout.CreateVerticalScrollLayout().CreateTextArea(_eventsLog);
            layout.CreateButton("Refresh", RefreshContent);

            layout.CreateMargin();
            layout.CreateButton("Trigger Memory Warning", () => {
                _appEvents.TriggerMemoryWarning();
                RefreshContent();
            });

            layout.CreateButton("Trigger Go Background", () => {
                _appEvents.TriggerWillGoBackground();
                RefreshContent();
            });

            layout.CreateConfirmButton("Restart Game", () => _appEvents.RestartGame());

            layout.CreateConfirmButton("Quit Game", () => {
                var moved = _appEvents.QuitGame();
                layout.AdminPanel.Console.Print(string.Format("Moved to background: {0}", moved));
            });

            layout.CreateConfirmButton("Kill Game", () => _appEvents.KillGame());
        }

        void RefreshContent()
        {
            if(_textComponent)
            {
                _textComponent.text = _eventsLog;
            }
        }

        void AddEvent(string newEvent)
        {
            _eventsLog += newEvent + "\n";
            RefreshContent();
        }
    }
}
