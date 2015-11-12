using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace SocialPoint.ScriptEvents
{
    public class AdminPanelScriptEvents : IAdminPanelGUI, IAdminPanelConfigurer
    {
        IScriptEventDispatcher _dispatcher;
        Text _eventsArea;
        string _eventsLog;
        IAttrSerializer _attrSerializer;
        IAttrParser _attrParser;
        IParser<Script> _scriptParser;

        public AdminPanelScriptEvents(IScriptEventDispatcher dispatcher, IParser<Script> scriptParser)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddListener(OnScriptEvent);
            var serializer = new JsonAttrSerializer();
            serializer.PrettyPrint = true;
            _attrSerializer = serializer;
            _attrParser = new JsonAttrParser();
            _scriptParser = scriptParser;
        }
        
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Script Events", this));
        }

        public void OnScriptEvent(string name, Attr args)
        {
            string argstr = null;
            if(!Attr.IsNullOrEmpty(args))
            {
                argstr =  _attrSerializer.SerializeString(args);
            }
            _eventsLog += string.Format("{0}\n{1}\n----\n", name, argstr);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Events");
            _eventsArea = layout.CreateVerticalScrollLayout().CreateTextArea();
            UpdateEventsContent();
            layout.CreateButton("Refresh", UpdateEventsContent);
            layout.CreateLabel("Load Script");
            layout.CreateTextInput(OnScriptSubmitted);
        }

        void Log(string msg)
        {
            _eventsLog += string.Format("{0}\n----\n", msg);
        }

        const string ScriptPathFormat = "EventScripts/{0}";

        void OnScriptSubmitted(string value)
        {
            try
            {
                var path = string.Format(ScriptPathFormat, value);
                Log(string.Format("loading script '{0}' from resource path '{1}'...", value, path));
                var defaultGame = (UnityEngine.Resources.Load(path) as UnityEngine.TextAsset).text;
                var attr = _attrParser.ParseString(defaultGame);
                var script = _scriptParser.Parse(attr);
                script.Run(() => {
                    Log(string.Format("script '{0}' finished running", value));
                });
            }
            catch(Exception e)
            {
                Log(string.Format("{0} loading script '{1}':\n{2}", value, e.GetType().FullName, e.Message));
            }
        }

        void UpdateEventsContent()
        {
            if(_eventsArea != null)
            {
                _eventsArea.text = _eventsLog;
            }
        }
    }
}