using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Utils
{
    public sealed class AdminPanelLog : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly List<LogEntry> _entries;
        Dictionary<LogType, bool> _activeTypes;
        Text _textComponent;
        bool _autoRefresh;
        readonly int _maxEntriesToDisplay = 100;

        public AdminPanelLog()
        {
            _autoRefresh = true;
            _entries = new List<LogEntry>();
            _activeTypes = new Dictionary<LogType, bool>();

            LogCallbackHandler.RegisterLogCallback(HandleLog);

            var array = Enum.GetValues(typeof(LogType));
            for(int i = 0, arrayCount = array.Length; i < arrayCount; i++)
            {
                var type = (LogType)array.GetValue(i);
                _activeTypes[type] = true;
            }
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
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
                hLayout.CreateToggleButton("Auto", _autoRefresh, value => {
                    _autoRefresh = value;
                });

                hLayout.CreateButton("Refresh", RefreshContent);

                hLayout.CreateButton("Clear", () => {
                    _entries.Clear();
                    RefreshContent();
                });
            }

            layout.CreateMargin();
            layout.CreateLabel("Log level");

            var array = Enum.GetValues(typeof(LogType));
            for(int i = 0, arrayCount = array.Length; i < arrayCount; i++)
            {
                // Each lambda must capture a diferent reference, so it has to be a local variable
                var type = (LogType)array.GetValue(i);
                LogType aType = type;
                layout.CreateToggleButton(aType.ToString(), _activeTypes[aType], value => ActivateLogType(aType, value));
            }

            RefreshContent();
        }

        void ActivateLogType(LogType type, bool active)
        {
            _activeTypes[type] = active;
            if(_autoRefresh)
            {
                RefreshContent();
            }
        }

        void RefreshContent()
        {
            if(_textComponent != null)
            {
                int numEntriesToDisplay = 0;
                var logContent = StringUtils.StartBuilder();
                for(int i = _entries.Count - 1; i > -1 && numEntriesToDisplay < _maxEntriesToDisplay; i--)
                {
                    LogEntry entry = _entries[i];
                    if(_activeTypes[entry.Type])
                    {
                        logContent.Append(entry.Content);
                        numEntriesToDisplay++;
                    }
                }

                _textComponent.text = StringUtils.FinishBuilder(logContent);
            }
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            _entries.Add(new LogEntry(type, message, stackTrace));
            RefreshContent();
        }

        class LogEntry
        {
            static readonly Dictionary<LogType, string> LogColors = new Dictionary<LogType, string> {
                { LogType.Log, "#EEE" },
                { LogType.Warning, "#FFA" },
                { LogType.Error, "#F88" },
                { LogType.Exception, "#F66" },
                { LogType.Assert, "#FA0" }
            };

            public LogType Type { get; private set; }

            public string Content { get; private set; }

            public LogEntry(LogType type, string message, string stackTrace)
            {
                Type = type;
                string color;
                LogColors.TryGetValue(type, out color);

                var contentBuilder = StringUtils.StartBuilder();
                contentBuilder.Append("<color=").Append(color).Append("><b> ").Append(type.ToString()).Append("</b>: ")
                              .AppendLine(message)
                              .Append(((type == LogType.Exception) ? "<b>Stack:</b>" + stackTrace : ""))
                              .AppendLine("</color>");
                Content = StringUtils.FinishBuilder(contentBuilder);
            }
        }
    }
}
