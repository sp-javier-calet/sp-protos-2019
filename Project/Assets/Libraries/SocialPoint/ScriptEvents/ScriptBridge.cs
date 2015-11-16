using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using System;

namespace SocialPoint.ScriptEvents
{
    public struct RunScriptAction
    {
        public ScriptModel Script;
    }

    public class RunScriptActionParser : BaseScriptEventParser<RunScriptAction>
    {
        const string AttrKeyUri = "uri";
        const string AttrKeyScheme = "scheme";
        const string AttrKeyParameters = "params";

        IParser<ScriptModel> _parser;

        public RunScriptActionParser(IParser<ScriptModel> parser): base("action.base.run_script")
        {
            _parser = parser;
        }
        
        override protected RunScriptAction ParseEvent(Attr data)
        {
            return new RunScriptAction{
                Script = _parser.Parse(data)
            };
        }
    }

    public class ScriptBridge :
        IEventsBridge,
        IScriptEventsBridge
    {
        IEventDispatcher _dispatcher;
        IScriptEventDispatcher _scriptDispatcher;
        IParser<ScriptModel> _scriptParser;

        public ScriptBridge(IParser<ScriptModel> scriptParser)
        {
            _scriptParser = scriptParser;
        }

        public void Load(IScriptEventDispatcher dispatcher)
        {
            _scriptDispatcher = dispatcher;
            _scriptDispatcher.AddParser(new RunScriptActionParser(_scriptParser));
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddListener<RunScriptAction>(OnRunScriptAction);
        }

        public void Dispose()
        {
            if(_dispatcher != null)
            {
                _dispatcher.RemoveListener<RunScriptAction>(OnRunScriptAction);
            }
        }

        void OnRunScriptAction(RunScriptAction action)
        {
            var script = new Script(_scriptDispatcher, action.Script);
            script.Run();
        }

    }

}