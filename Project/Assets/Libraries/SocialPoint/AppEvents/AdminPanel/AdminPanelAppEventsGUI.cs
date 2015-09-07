using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using SocialPoint.AdminPanel;

namespace SocialPoint.AppEvents
{
    public class AdminPanelAppEventsGUI : IAdminPanelGUI, IAdminPanelConfigurer
    {
        public IAppEvents AppEvents;

        private Text _textComponent;
        private string _eventsLog;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _eventsLog = string.Empty;

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("App Events", this));

            AppEvents.OpenedFromSource += (source) => { AddEvent("OpenedFromSource: " + source); };;
            AppEvents.WasCovered += () => { AddEvent("WasCovered"); };
            AppEvents.WasOnBackground += () => { AddEvent("WasOnBackground"); };
            AppEvents.WillGoBackground += () => { AddEvent("WillGoBackground"); };
            AppEvents.LevelWasLoaded += (value) => { AddEvent("LevelWasLoaded: " + value); };
            AppEvents.ReceivedMemoryWarning += () => { AddEvent("ReceivedMemoryWarning"); };
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("App Events");
            layout.CreateVerticalScrollLayout().CreateTextArea(_eventsLog, out _textComponent);
            layout.CreateButton("Refresh", () => {
                RefreshContent(); 
            });

            SocialPointAppEvents spAppEvents = AppEvents as SocialPointAppEvents;
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