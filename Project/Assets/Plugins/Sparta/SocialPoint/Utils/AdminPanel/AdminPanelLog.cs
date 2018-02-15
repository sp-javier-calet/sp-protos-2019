#if ADMIN_PANEL 

using System;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Console;
using UnityEngine;
using UnityEngine.UI;

namespace SocialPoint.Utils
{
    public sealed class AdminPanelLog : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly List<LogEntry> _entries;
        readonly HashSet<string> _availableTags;
        readonly HashSet<string> _selectedTags;
        Text _textComponent;
        LogConfig _config;

        event Action<string> ContentUpdated;

        public AdminPanelLog()
        {
            _entries = new List<LogEntry>();
            _availableTags = new HashSet<string>();
            _selectedTags = new HashSet<string>();
            _config = new LogConfig();

            Application.logMessageReceived += HandleLog;

            var array = Enum.GetValues(typeof(LogType));
            for(int i = 0, arrayCount = array.Length; i < arrayCount; i++)
            {
                var type = (LogType)array.GetValue(i);
                _config.ActiveTypes[type] = true;
            }

            ContentUpdated += OnContentUpdated;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Log", this));

            // Bind Log commands. Use anonymous delegates to avoid issues with conditional methods during compilation
            adminPanel.RegisterCommand("log", CreateLogCommand("default (info)", msg => Log.i(msg), (tag, msg) => Log.i(tag, msg)));
            adminPanel.RegisterCommand("logv", CreateLogCommand("verbose", msg => Log.v(msg), (tag, msg) => Log.v(tag, msg)));
            adminPanel.RegisterCommand("logd", CreateLogCommand("debug", msg => Log.d(msg), (tag, msg) => Log.d(tag, msg)));
            adminPanel.RegisterCommand("logi", CreateLogCommand("info", msg => Log.i(msg), (tag, msg) => Log.i(tag, msg)));
            adminPanel.RegisterCommand("logw", CreateLogCommand("warning", msg => Log.w(msg), (tag, msg) => Log.w(tag, msg)));
            adminPanel.RegisterCommand("loge", CreateLogCommand("error", msg => Log.e(msg), (tag, msg) => Log.e(tag, msg)));

            adminPanel.RegisterCommand("logx", new ConsoleCommand()
                .WithDescription("log a exception")
                .WithDelegate(command => LogExceptionCommand(exception => Log.x(exception), command)));
            
            adminPanel.RegisterCommand("logb", new ConsoleCommand()
                .WithDescription("leave a breadcrumb")
                .WithDelegate(command => LogBreadcrumbCommand(msg => Log.b(msg), command)));
        }

        static ConsoleCommand CreateLogCommand(string level, Action<string> dlg, Action<string, string> taggedDlg)
        {
            return new ConsoleCommand()
                .WithDescription("log a message as " + level)
                .WithOption(new ConsoleCommandOption("t|tag")
                    .WithDescription("log tag"))
                .WithDelegate(command => LogCommand(dlg, taggedDlg, command));
        }

        static void LogCommand(Action<string> dlg, Action<string, string> taggedDlg, ConsoleCommand cmd)
        {
            var content = GetContent(cmd);
            var tag = cmd["tag"];
            if(tag != null && tag.Value != null)
            {
                taggedDlg(tag.Value, content);
            }
            else
            {
                dlg(content);
            } 
        }

        static void LogExceptionCommand(Action<Exception> dlg, ConsoleCommand cmd)
        {
            dlg(new Exception(GetContent(cmd)));
        }

        static void LogBreadcrumbCommand(Action<string> dlg, ConsoleCommand cmd)
        {
            dlg(GetContent(cmd));
        }

        static string GetContent(ConsoleCommand cmd)
        {
            var list = new List<string>(cmd.Arguments);
            if(list.Count == 0)
            {
                throw new ConsoleException("Need at least a content argument");
            }

            var content = string.Empty;
            for(var i = 0; i < list.Count; ++i)
            {
                var arg = list[i];
                if(!arg.StartsWith("-"))
                {
                    content += arg;
                }
            }
            return content;
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

            var foldoutLayout = layout.CreateFoldoutLayout("LogLevels");
            LogLevelsFoldoutGUI(foldoutLayout);
            layout.CreateMargin();

            foldoutLayout = layout.CreateFoldoutLayout("Tags");
            TagsFoldoutGUI(foldoutLayout);
            layout.CreateMargin();

            layout.CreateTextInput("Filter", 
                value => {
                    _config.Filter = string.IsNullOrEmpty(value) ? null : value.ToLower();
                    RefreshContent();
                },
                status => {
                    _config.Filter = string.IsNullOrEmpty(status.Content) ? null : status.Content.ToLower();
                    RefreshContent();
                });

            layout.CreateButton("Show log", () => {
                FloatingPanelController.Create(new LogFloatingGUI(this)).Show();
                RefreshContent();
            });

            RefreshContent();
        }

        void OnContentUpdated(string content)
        {
            if(_textComponent != null)
            {
                _textComponent.text = content;
            }
        }

        public void LogLevelsFoldoutGUI(AdminPanelLayout layout)
        {
            var array = Enum.GetValues(typeof(LogType));
            for(int i = 0, arrayCount = array.Length; i < arrayCount; i++)
            {
                // Each lambda must capture a diferent reference, so it has to be a local variable
                var type = (LogType)array.GetValue(i);
                LogType aType = type;
                layout.CreateToggleButton(aType.ToString(), _config.ActiveTypes[aType], value => ActivateLogType(aType, value));
            }
        }

        public void TagsFoldoutGUI(AdminPanelLayout layout)
        {
            var vlayout = layout.CreateVerticalLayout().CreateVerticalScrollLayout();
            if(_availableTags.Count > 0)
            {
                var itr = _availableTags.GetEnumerator();
                while(itr.MoveNext())
                {
                    var tagFilter = itr.Current;
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
                itr.Dispose();
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
            if(ContentUpdated != null)
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

                var content = StringUtils.FinishBuilder(logContent);
                ContentUpdated(content);
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

        sealed class LogFloatingGUI : IFloatingPanelGUI, IAdminPanelManagedGUI
        {
            Text _textComponent;
            string _content;
            AdminPanelLog _logPanel;

            public LogFloatingGUI(AdminPanelLog panel)
            {
                _logPanel = panel;
            }

            public void OnCreateFloatingPanel(FloatingPanelController panel)
            {
                panel.Title = "Log";
                panel.Size = new Vector2(200, 120);
            }

            public void OnCreateGUI(AdminPanelLayout layout)
            {
                _textComponent = layout.CreateTextArea(_content);
            }

            public void OnOpened()
            {
                _logPanel.ContentUpdated += OnContentUpdated;
            }

            public void OnClosed()
            {
                _logPanel.ContentUpdated -= OnContentUpdated;
            }

            void OnContentUpdated(string content)
            {
                if(_textComponent != null)
                {
                    _content = content;
                    _textComponent.text = _content;
                }
            }
        }

        sealed class LogTypeComparer : IEqualityComparer<LogType>
        {
            public bool Equals(LogType x, LogType y)
            {
                return x == y;
            }

            public int GetHashCode(LogType obj)
            {
                return (int)obj;
            }
        }

        static string GetLogTypeString(LogType type)
        {
            switch(type)
            {
            case LogType.Error: 
                return "Error";
            case LogType.Assert:
                return "Assert";
            case LogType.Warning:
                return "Warning";
            case LogType.Log:
                return "Log";
            case LogType.Exception:
                return "Exception";
            default:
                DebugUtils.Assert(false, "Type '" + type + "' is unknown");
                return "";
            }
        }

        sealed class LogConfig
        {
			public readonly Dictionary<LogType, bool> ActiveTypes = new Dictionary<LogType, bool>(new LogTypeComparer());
            public bool AutoRefresh = true;
            public int MaxEntriesToDisplay = 100;
            public string Filter;
        }

        sealed class LogEntry
        {
			static readonly Dictionary<LogType, string> LogColors = new Dictionary<LogType, string>(new LogTypeComparer()) {
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
                contentBuilder.Append("<color=").Append(color).Append("><b> ").Append(GetLogTypeString(type)).Append("</b>: ")
                              .AppendLine(message)
                              .Append(((type == LogType.Exception) ? "<b>Stack:</b>" + stackTrace : ""))
                              .AppendLine("</color>");
                Content = StringUtils.FinishBuilder(contentBuilder);
                LowerContent = Content.ToLower();
            }
        }
    }
}

#endif
