using System;
using SocialPoint.Attributes;
using SocialPoint.Dependency;
using SocialPoint.ScriptEvents;

namespace SocialPoint.Tutorial
{
    [Serializable]
    public class SimpleConditionTrigger : EventTriggerConditionBase, IScriptCondition
    {
        public string TriggerId;
        
        private IScriptEventProcessor _processor;
        
        public SimpleConditionTrigger()
        {
            TriggerId = "new_trigger_id";
        }
        
        public override void RegisterEvents()
        {
            if(_processor == null)
            {
                _processor = Services.Instance.Resolve<IScriptEventProcessor>();
            }
            
            _processor?.RegisterHandler(this, OnScriptEvent);
        }

        public override void UnregisterEvents()
        {
            _processor?.UnregisterHandler(OnScriptEvent);
        }
        
        public bool Matches(string name, Attr arguments)
        {
            return TriggerId.Equals(name);
        }
        
        void OnScriptEvent(string name, Attr arguments)
        {
            OnTriggerEvent();
        }

        public override void Update(float elapsed)
        {
        }
    }
}
