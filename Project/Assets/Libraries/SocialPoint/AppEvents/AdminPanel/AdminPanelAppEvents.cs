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

            _appEvents.OpenedFromSource += (source) => { AddEvent("OpenedFromSource: " + source); };;
            _appEvents.WasCovered += () => { AddEvent("WasCovered"); };
            _appEvents.WasOnBackground += () => { AddEvent("WasOnBackground"); };
            _appEvents.WillGoBackground += () => { AddEvent("WillGoBackground"); };
            _appEvents.LevelWasLoaded += (value) => { AddEvent("LevelWasLoaded: " + value); };
            _appEvents.ReceivedMemoryWarning += () => { AddEvent("ReceivedMemoryWarning"); };
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("App Events");
            layout.CreateVerticalScrollLayout().CreateTextArea(_eventsLog, out _textComponent);
            layout.CreateButton("Refresh", () => {
                RefreshContent(); 
            });

            SocialPointAppEvents spAppEvents = _appEvents as SocialPointAppEvents;
            if(spAppEvents != null)
            {
                layout.CreateMargin();
                layout.CreateLabel(spAppEvents.GetType().Name);
                layout.CreateButton("Send Memory Warning", () => {

                    /* Invoke AppEventsBase OnReceivedMemoryWarning through reflection.
                     * This code is coupled to SocialPointAppEvents and AppEventsBase implementation */ 
                    var eventsBaseField = spAppEvents.GetType().GetField("_appEvents", BindingFlags.NonPublic | BindingFlags.Instance);
                    var eventsBase = eventsBaseField.GetValue(spAppEvents);

                    MethodInfo dynMethod = eventsBase.GetType().GetMethod("OnReceivedMemoryWarning", 
                                                                    BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(eventsBase, null);

                    RefreshContent();
                });
            }
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