using System.Collections.Generic;
using SocialPoint.Alert;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{

    public struct AlertAction
    {
        public string Id;
        public string Title;
        public string Message;
        public string Signature;
        public string[] Buttons;
        public object[] Actions;
        public bool Input;
    }

    public sealed class AlertActionParser : BaseScriptEventParser<AlertAction>
    {
        const string AttrKeyId = "id";
        const string AttrKeyTitle = "title";
        const string AttrKeyMessage = "message";
        const string AttrKeySignature = "signature";
        const string AttrKeyButtons = "buttons";
        const string AttrKeyActions = "actions";
        const string AttrKeyInput = "input";


        readonly IScriptEventProcessor _dispatcher;

        public AlertActionParser(IScriptEventProcessor dispatcher) : base("action.alert")
        {
            _dispatcher = dispatcher;
        }

        override protected AlertAction ParseEvent(Attr data)
        {
            var actions = new List<object>();
            var itr = data.AsDic[AttrKeyActions].AsList.GetEnumerator();
            while(itr.MoveNext())
            {
                var act = itr.Current;
                actions.Add(_dispatcher.Parse(act));
            }
            itr.Dispose();

            return new AlertAction {
                Id = data.AsDic[AttrKeyId].AsValue.ToString(),
                Title = data.AsDic[AttrKeyTitle].AsValue.ToString(),
                Message = data.AsDic[AttrKeyMessage].AsValue.ToString(),
                Signature = data.AsDic[AttrKeySignature].AsValue.ToString(),
                Buttons = data.AsDic[AttrKeyButtons].AsList.ToArray<string>(),
                Actions = actions.ToArray(),
                Input = data.AsDic[AttrKeyInput].AsValue.ToBool()
            };
        }
    }

    public sealed class AlertBridge :
        IEventsBridge, 
        IScriptEventsBridge
    {
        
        IEventDispatcher _dispatcher;
        readonly IAlertView _prototype;

        public AlertBridge(IAlertView prototype)
        {
            _prototype = prototype;
        }

        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddListener<AlertAction>(OnAlertAction);
        }

        public void Load(IScriptEventProcessor dispatcher)
        {
            dispatcher.RegisterParser(new AlertActionParser(dispatcher));
        }

        public void Dispose()
        {
            if(_dispatcher != null)
            {
                _dispatcher.RemoveListener<AlertAction>(OnAlertAction);
            }
        }

        void OnAlertAction(AlertAction action)
        {
            var alert = (IAlertView)_prototype.Clone();
            alert.Title = action.Title;
            alert.Message = action.Message;
            alert.Buttons = action.Buttons;
            alert.Signature = action.Signature;
            alert.Input = action.Input;
            alert.Show(result => {
                if(result >= 0 && action.Actions.Length > result)
                {
                    _dispatcher.Raise(action.Actions[result]);
                }
                alert.Dispose();
            });
        }

    }
}