using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.AdminPanel;

namespace SocialPoint.Utils
{
    public class AdminPanelLogGUI : IAdminPanelConfigurer, IAdminPanelGUI
    {
        private List<LogEntry> _entries;
        private Dictionary<LogType, bool> _activeTypes;
        private Text _textComponent;
        private bool _autoRefresh;
        
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _autoRefresh = true;
            _entries = new List<LogEntry>();
            _activeTypes = new Dictionary<LogType, bool>();

            foreach(LogType type in Enum.GetValues(typeof(LogType)))
            {
                _activeTypes[type] = true;
            }

            LogCallbackHandler.RegisterLogCallback(HandleLog);

            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Log", this));
        }
        
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("System Log");
            _textComponent = layout.CreateVerticalScrollLayout().CreateTextArea();

            // Configure text component for rich text
            _textComponent.supportRichText = true;

            layout.CreateMargin();

            using(var hLayout = layout.CreateHorizontalLayout())
            {
                hLayout.CreateToggleButton("Auto", _autoRefresh, (value) => {
                    _autoRefresh = value;
                });

                hLayout.CreateButton("Refresh", () => {
                    RefreshContent();
                });

                hLayout.CreateButton("Clear", () => {
                    _entries.Clear();
                    RefreshContent();
                });
            }

            layout.CreateMargin();
            layout.CreateLabel("Log level");

            foreach(LogType type in Enum.GetValues(typeof(LogType)))
            {
                // Each lambda must capture a diferent reference, so it has to be a local variable
                LogType aType = type; 
                layout.CreateToggleButton(aType.ToString(), _activeTypes[aType], (value) => {
                    ActivateLogType(aType, value); 
                });
            }

            RefreshContent();
        }

        private void ActivateLogType(LogType type, bool active)
        {
            _activeTypes[type] = active;
            if(_autoRefresh)
            {
                RefreshContent();
            }
        }

        private void RefreshContent()
        {
            if(_textComponent != null)
            {
                StringBuilder logContent = new StringBuilder();

                foreach(LogEntry entry in _entries)
                {
                    if(_activeTypes[entry.Type] == true)
                    {
                        logContent.Append(entry.Content);
                    }
                }

                _textComponent.text = logContent.ToString();
            }
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            _entries.Add(new LogEntry(type, message, stackTrace));
            RefreshContent();
        }

        protected class LogEntry
        {
            private static readonly Dictionary<LogType, string> LogColors = new Dictionary<LogType, string> {
                { LogType.Log, "#EEE" },
                { LogType.Warning, "#FFA" },
                { LogType.Error, "#F88" },
                { LogType.Exception, "#F66" },
                { LogType.Assert, "#FA0" }
            };

            public LogType Type { get; private set; }
            public string Content { get; private set; }

            public LogEntry (LogType type, string message, string stackTrace)
            {
                Type = type;
                string color = null;
                LogColors.TryGetValue(type, out color);

                StringBuilder contentBuilder = new StringBuilder();
                contentBuilder.Append("<color=").Append(color).Append("><b> ").Append(type.ToString()).Append("</b>: ")
                              .AppendLine(message)
                              .Append(((type == LogType.Exception)? "<b>Stack:</b>"+stackTrace : ""))
                              .AppendLine("</color>");
                Content = contentBuilder.ToString();
            }
        }
    }
}
