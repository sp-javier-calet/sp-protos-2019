using System;
using System.Collections;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.ScriptEvents
{
    public struct RunScriptAction
    {
        public ScriptModel Script;
    }

    public struct LogAction
    {
        public LogType Type;
        public string Message;
        public UnityEngine.Object Object;
    }

    public struct WaitAction
    {
        public float Seconds;
        public object Action;
    }

    public sealed class RunScriptActionParser : BaseScriptEventParser<RunScriptAction>
    {
        readonly IAttrObjParser<ScriptModel> _parser;

        public RunScriptActionParser(IAttrObjParser<ScriptModel> parser) : base("action.base.run_script")
        {
            _parser = parser;
        }

        override protected RunScriptAction ParseEvent(Attr data)
        {
            return new RunScriptAction {
                Script = _parser.Parse(data)
            };
        }
    }

    public sealed class LogActionParser : BaseScriptEventParser<LogAction>
    {
        const string AttrKeyMessage = "msg";
        const string AttrKeyLogType = "type";
        const string AttrKeyLogTypeInfo = "info";
        const string AttrKeyLogTypeWarn = "warn";
        const string AttrKeyLogTypeError = "error";

        public LogActionParser() : base("action.log")
        {
        }

        override protected LogAction ParseEvent(Attr data)
        {
            if(data.AttrType == AttrType.VALUE)
            {
                return new LogAction {
                    Message = data.AsValue.ToString(),
                    Type = LogType.Log
                };
            }
            else
            {
                var typeStr = data.AsDic[AttrKeyLogType].AsValue.ToString();
                LogType type = LogType.Log;
                if(typeStr == AttrKeyLogTypeError)
                {
                    type = LogType.Error;
                }
                else if(typeStr == AttrKeyLogTypeWarn)
                {
                    type = LogType.Warning;
                }
                return new LogAction {
                    Message = data.AsDic[AttrKeyMessage].AsValue.ToString(),
                    Type = type
                };
            }
        }
    }

    public sealed class WaitActionParser : BaseScriptEventParser<WaitAction>
    {
        const string AttrKeySeconds = "secs";
        const string AttrKeyAction = "action";

        readonly IScriptEventDispatcher _dispatcher;

        public WaitActionParser(IScriptEventDispatcher dispatcher) : base("action.wait")
        {
            _dispatcher = dispatcher;
        }

        override protected WaitAction ParseEvent(Attr data)
        {
            var action = data.AsDic[AttrKeyAction].AsDic;

            return new WaitAction {
                Seconds = data.AsDic[AttrKeySeconds].AsValue.ToFloat(),
                Action = _dispatcher.Parse(action)
            };
        }
    }

    public sealed class ScriptBridge :
        IEventsBridge,
        IScriptEventsBridge
    {
        IEventDispatcher _dispatcher;
        IScriptEventDispatcher _scriptDispatcher;
        IAttrObjParser<ScriptModel> _scriptParser;
        ICoroutineRunner _runner;

        public ScriptBridge(IAttrObjParser<ScriptModel> scriptParser, ICoroutineRunner runner)
        {
            _scriptParser = scriptParser;
            _runner = runner;
        }

        public void Load(IScriptEventDispatcher dispatcher)
        {
            _scriptDispatcher = dispatcher;
            _scriptDispatcher.AddParser(new RunScriptActionParser(_scriptParser));
            _scriptDispatcher.AddParser(new LogActionParser());
            _scriptDispatcher.AddParser(new WaitActionParser(dispatcher));
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddListener<RunScriptAction>(OnRunScriptAction);
            _dispatcher.AddListener<LogAction>(OnLogAction);
            _dispatcher.AddListener<WaitAction>(OnWaitAction);
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

        static void OnLogAction(LogAction action)
        {
            if(action.Type == LogType.Error)
            {
                Log.e(string.Format("{0} - {1}", action.Message, action.Object));
            }
            else if(action.Type == LogType.Warning)
            {
                Log.w(string.Format("{0} - {1}", action.Message, action.Object));
            }
            else
            {
                Log.i(string.Format("{0} - {1}", action.Message, action.Object));
            }
        }

        void OnWaitAction(WaitAction action)
        {
            _runner.StartCoroutine(WaitCoroutine(action));
        }

        IEnumerator WaitCoroutine(WaitAction action)
        {
            yield return new WaitForSeconds(action.Seconds);
            _dispatcher.Raise(action.Action);
        }

    }

}