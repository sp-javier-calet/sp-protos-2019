using SocialPoint.Attributes;
using SocialPoint.Alert;
using System;
using System.Collections.Generic;

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

    public class AlertActionParser : BaseScriptEventParser<AlertAction>
    {
        const string AttrKeyId = "id";
        const string AttrKeyTitle = "title";
        const string AttrKeyMessage = "message";
        const string AttrKeySignature = "signature";
        const string AttrKeyButtons = "buttons";
        const string AttrKeyActions = "actions";
        const string AttrKeyInput = "input";


        IScriptEventDispatcher _dispatcher;
        
        public AlertActionParser(IScriptEventDispatcher dispatcher): base("action.alert")
        {
            _dispatcher = dispatcher;
        }
        
        override protected AlertAction ParseEvent(Attr data)
        {
            var actions = new List<object>();
            foreach(var act in data.AsDic[AttrKeyActions].AsList)
            {
                actions.Add(_dispatcher.Parse(act));
            }
            return new AlertAction{
                Id = data.AsDic[AttrKeyId].AsValue.ToString(),
                Title = data.AsDic[AttrKeyTitle].AsValue.ToString(),
                Message = data.AsDic[AttrKeyMessage].AsValue.ToString(),
                Signature = data.AsDic[AttrKeySignature].AsValue.ToString(),
                Buttons = data.AsDic[AttrKeyButtons].AsList.ToList<string>().ToArray(),
                Actions = actions.ToArray(),
                Input = data.AsDic[AttrKeyInput].AsValue.ToBool()
            };
        }
    }
    
    public class AlertBridge :
        IEventsBridge, 
        IScriptEventsBridge
    {
        
        IEventDispatcher _dispatcher;
        IAlertView _prototype;
        
        public AlertBridge(IAlertView prototype)
        {
            _prototype = prototype;
        }
        
        public void Load(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddListener<AlertAction>(OnAlertAction);
        }
        
        public void Load(IScriptEventDispatcher dispatcher)
        {
            dispatcher.AddParser(new AlertActionParser(dispatcher));
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
            alert.Show((int result) => {
                if(result >= 0 && action.Actions.Length > result)
                {
                    _dispatcher.Raise(action.Actions[result]);
                }
                alert.Dispose();
            });
        }

    }
}