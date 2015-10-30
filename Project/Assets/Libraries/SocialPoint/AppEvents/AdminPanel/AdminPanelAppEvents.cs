using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using SocialPoint.AdminPanel;

namespace SocialPoint.AppEvents
{
    public class AdminPanelAppEvents : IAdminPanelGUI, IAdminPanelConfigurer
    {
        private IAppEvents _appEvents;
        private Text _textComponent;
        private string _eventsLog;

        public AdminPanelAppEvents(IAppEvents appEvents)
        {
            _appEvents = appEvents;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _eventsLog = string.Empty;

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("App Events", this));

            _appEvents.OpenedFromSource += (source) => { AddEvent("OpenedFromSource: " + source); };
            _appEvents.WasCovered += () => { AddEvent("WasCovered"); };
            _appEvents.WasOnBackground += () => { AddEvent("WasOnBackground"); };
            _appEvents.RegisterWillGoBackground(0, () => { AddEvent("WillGoBackground"); });
            _appEvents.RegisterGameWillRestart(0, () => { AddEvent("GameWillRestart"); });
            _appEvents.RegisterGameWasLoaded(0, () => { AddEvent("GameWasLoaded"); });
            _appEvents.LevelWasLoaded += (value) => { AddEvent("LevelWasLoaded: " + value); };
            _appEvents.ApplicationQuit += () =>  { AddEvent("ApplicationQuit"); };
            _appEvents.ReceivedMemoryWarning += () => { AddEvent("ReceivedMemoryWarning"); };
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("App Events");
            _textComponent = layout.CreateVerticalScrollLayout().CreateTextArea(_eventsLog);
            layout.CreateButton("Refresh", () => {
                RefreshContent(); 
            });

            layout.CreateMargin();
            layout.CreateButton("Trigger Memory Warning", () => {
                _appEvents.TriggerMemoryWarning();
                RefreshContent();
            });

            layout.CreateButton("Trigger Go Background", () => {
                _appEvents.TriggerWillGoBackground();
                RefreshContent();
            });

            layout.CreateConfirmButton("Restart Game", () => {
                _appEvents.RestartGame();
            });
        }

        private void RefreshContent()
        {
            if(_textComponent)
            {
                _textComponent.text = _eventsLog;
            }
        }

        private void AddEvent(string newEvent)
        {
            _eventsLog += newEvent + "\n";
            RefreshContent();
        }
    }
}
