#if ADMIN_PANEL 

using System;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using UnityEngine.UI;

namespace SocialPoint.ScriptEvents
{
    public sealed class AdminPanelScriptEvents : IAdminPanelGUI, IAdminPanelConfigurer
    {
        IScriptEventDispatcher _dispatcher;
        Text _eventsArea;
        string _eventsLog;
        IAttrSerializer _attrSerializer;
        IAttrParser _attrParser;
        IAttrObjParser<ScriptModel> _scriptParser;

        public AdminPanelScriptEvents(IScriptEventDispatcher dispatcher, IAttrObjParser<ScriptModel> scriptParser)
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
                argstr = _attrSerializer.SerializeString(args);
            }
            _eventsLog += string.Format("{0}\n{1}\n----\n", name, argstr);
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Events");
            _eventsArea = layout.CreateVerticalScrollLayout().CreateTextArea();
            UpdateEventsContent();
            layout.CreateButton("Refresh", UpdateEventsContent);
            layout.CreateButton("Clear", ClearEventsContent);
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
                var scriptModel = _scriptParser.Parse(attr);
                Log(string.Format("loaded script '{0}'\n{1}", value, scriptModel));
                var script = new Script(_dispatcher, scriptModel);
                script.StepStarted += () => Log(string.Format("script '{0}' step {1} started", value, script.CurrentStepNum));
                script.StepFinished += (decision, evName, evArgs) => Log(string.Format("script '{0}' step {1} finished with decision {2}", value, script.CurrentStepNum, decision));
                script.Run(() => Log(string.Format("script '{0}' finished running", value)));
                UpdateEventsContent();
            }
            catch(Exception e)
            {
                Log(string.Format("{0} loading script '{1}':\n{2}", value, e.GetType().FullName, e.Message));
            }
        }

        void ClearEventsContent()
        {
            _eventsLog = string.Empty;
            UpdateEventsContent();
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

#endif
