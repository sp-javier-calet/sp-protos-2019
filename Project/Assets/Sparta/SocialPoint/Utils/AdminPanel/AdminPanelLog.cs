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
        readonly HashSet<string> _availableTags;
        readonly HashSet<string> _selectedTags;
        bool _showLogLevels;
        bool _showTags;
        Text _textComponent;
        LogConfig _config;

        public AdminPanelLog()
        {
            _entries = new List<LogEntry>();
            _availableTags = new HashSet<string>();
            _selectedTags = new HashSet<string>();
            _config = new LogConfig();

            LogCallbackHandler.RegisterLogCallback(HandleLog);

            var array = Enum.GetValues(typeof(LogType));
            for(int i = 0, arrayCount = array.Length; i < arrayCount; i++)
            {
                var type = (LogType)array.GetValue(i);
                _config.ActiveTypes[type] = true;
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

            var hLayout = layout.CreateHorizontalLayout();
            hLayout.CreateToggleButton("Auto", _config.AutoRefresh, value => {
                _config.AutoRefresh = value;
            });

            hLayout.CreateButton("Refresh", RefreshContent);

            hLayout.CreateButton("Clear", () => {
                _entries.Clear();
                _availableTags.Clear();
                RefreshContent();
            });

            layout.CreateToggleButton("Log Levels", _showLogLevels, status => {
                _showLogLevels = status;
                layout.Refresh();
            });
            if(_showLogLevels)
            {
                LogLevelsFoldoutGUI(layout);
            }

            layout.CreateToggleButton("Tags", _showTags, status => {
                _showTags = status;
                layout.Refresh();
            });
            if(_showTags)
            {
                TagsFoldoutGUI(layout);
            }

            layout.CreateMargin(2);

            layout.CreateTextInput("Filter", 
                value => {
                    _config.Filter = string.IsNullOrEmpty(value) ? null : value.ToLower();
                    RefreshContent();
                },
                status => {
                    _config.Filter = string.IsNullOrEmpty(status.Content) ? null : status.Content.ToLower();
                    RefreshContent();
                });

            RefreshContent();
        }

        public void LogLevelsFoldoutGUI(AdminPanelLayout layout)
        {
            var vlayout = layout.CreateVerticalLayout();

            var array = Enum.GetValues(typeof(LogType));
            for(int i = 0, arrayCount = array.Length; i < arrayCount; i++)
            {
                // Each lambda must capture a diferent reference, so it has to be a local variable
                var type = (LogType)array.GetValue(i);
                LogType aType = type;
                vlayout.CreateToggleButton(aType.ToString(), _config.ActiveTypes[aType], value => ActivateLogType(aType, value));
            }
        }

        public void TagsFoldoutGUI(AdminPanelLayout layout)
        {
            var vlayout = layout.CreateVerticalLayout().CreateVerticalScrollLayout();
            if(_availableTags.Count > 0)
            {
                foreach(var tagFilter in _availableTags)
                {
                    vlayout.CreateToggleButton(tagFilter, _selectedTags.Contains(tagFilter), value => {
                        if(value)
                        {
                            _selectedTags.Add(tagFilter);
                        }
                        else
                        {
                            _selectedTags.Remove(tagFilter);
                        }

                        if(_config.AutoRefresh)
                        {
                            RefreshContent();
                        }
                    });
                }
            }
            else
            {
                vlayout.CreateLabel("No Tags available");
            }
        }

        void ActivateLogType(LogType type, bool active)
        {
            _config.ActiveTypes[type] = active;
            if(_config.AutoRefresh)
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
                for(int i = _entries.Count - 1; i > -1 && numEntriesToDisplay < _config.MaxEntriesToDisplay; i--)
                {
                    LogEntry entry = _entries[i];
                    if(_config.ActiveTypes[entry.Type])
                    {
                        if(_config.Filter != null && !entry.LowerContent.Contains(_config.Filter))
                        {
                            continue;
                        }
                        if(_selectedTags.Count > 0 && !_selectedTags.Contains(entry.Tag))
                        {
                            continue;
                        }

                        logContent.Append(entry.Content);
                        numEntriesToDisplay++;
                    }
                }

                _textComponent.text = StringUtils.FinishBuilder(logContent);
            }
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            string tag = null;
            if(!string.IsNullOrEmpty(message))
            {
                if(message[0] == '[')
                {
                    var idx = message.IndexOf(']');
                    if(idx > 0)
                    {
                        tag = message.Substring(1, idx - 1);
                        _availableTags.Add(tag);
                    }
                }
            }
            _entries.Add(new LogEntry(type, message, stackTrace, tag));
            RefreshContent();
        }

        sealed class LogConfig
        {
            public readonly Dictionary<LogType, bool> ActiveTypes = new Dictionary<LogType, bool>();
            public bool AutoRefresh = true;
            public int MaxEntriesToDisplay = 100;
            public string Filter;
        }

        sealed class LogEntry
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

            public string LowerContent { get; private set; }

            public string Tag { get; private set; }

            public LogEntry(LogType type, string message, string stackTrace, string tag = null)
            {
                Type = type;
                Tag = tag;
                string color;
                LogColors.TryGetValue(type, out color);

                var contentBuilder = StringUtils.StartBuilder();
                contentBuilder.Append("<color=").Append(color).Append("><b> ").Append(type.ToString()).Append("</b>: ")
                              .AppendLine(message)
                              .Append(((type == LogType.Exception) ? "<b>Stack:</b>" + stackTrace : ""))
                              .AppendLine("</color>");
                Content = StringUtils.FinishBuilder(contentBuilder);
                LowerContent = Content.ToLower();
            }
        }
    }
}
