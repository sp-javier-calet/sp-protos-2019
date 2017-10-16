#if ADMIN_PANEL

using System;
using System.Collections;
using SocialPoint.AdminPanel;
using UnityEngine.UI;
using SocialPoint.Restart;

namespace SocialPoint.AppEvents
{
    public sealed class AdminPanelAppEvents : IAdminPanelGUI, IAdminPanelConfigurer, IDisposable
    {
        IAppEvents _appEvents;
        IRestarter _restarter;
        Text _textComponent;
        string _eventsLog;
        AdminPanelConsole _console;

        public AdminPanelAppEvents(IAppEvents appEvents, IRestarter restarter)
        {
            _eventsLog = string.Empty;
            _appEvents = appEvents;
            _restarter = restarter;

            _appEvents.OpenedFromSource += OnOpenedFromSource;
            _appEvents.WasCovered += OnWasCovered;
            _appEvents.WasOnBackground.Add(0, OnWasOnBackground);
            _appEvents.WillGoBackground.Add(0, OnWillGoBackground);
            _appEvents.GameWillRestart.Add(0, OnGameWillRestart);
            _appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
            _appEvents.AfterGameWasLoaded.Add(0, AfterGameWasLoaded);
            _appEvents.ApplicationQuit += OnApplicationQuit;
            _appEvents.ReceivedMemoryWarning += OnReceivedMemoryWarning;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("App Events", this));
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

        IEnumerator AfterGameWasLoaded()
        {
            AddEvent("AfterGameWasLoaded");
            yield break;
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
            _appEvents.WasOnBackground.Remove(OnWasOnBackground);
            _appEvents.WillGoBackground.Remove(OnWillGoBackground);
            _appEvents.GameWillRestart.Remove(OnGameWillRestart);
            _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            _appEvents.AfterGameWasLoaded.Remove(AfterGameWasLoaded);
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

            layout.CreateButton("Trigger Was On Background", () => {
                _appEvents.TriggerWasOnBackground();
                RefreshContent();
            });

            layout.CreateConfirmButton("Restart Game", () => _restarter.RestartGame());

            layout.CreateConfirmButton("Quit Game", () => {
                var moved = _appEvents.QuitGame();
                if(_console != null)
                {
                    _console.Print(string.Format("Moved to background: {0}", moved));
                }
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

#endif
