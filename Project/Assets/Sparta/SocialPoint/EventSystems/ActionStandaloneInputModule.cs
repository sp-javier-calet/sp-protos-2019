using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public class ActionStandaloneInputModule : StandaloneInputModule
    {
        Dictionary<int, PointerEventData> _actionEventDispatcherPointerData = new Dictionary<int, PointerEventData>();
        Dictionary<int, PointerEventData> _defaultPointerData;
        Dictionary<GameObject, LayerMask> _registeredHandlers = new Dictionary<GameObject, LayerMask>();
        ForcedGameObjectRaycaster _newActionRaycaster;

        public override void ActivateModule()
        {
            base.ActivateModule();
            _defaultPointerData = m_PointerData;
            _newActionRaycaster = gameObject.AddComponent<ForcedGameObjectRaycaster>();
        }

        public void AddEventListener(GameObject sender, LayerMask ignoreDispatcherMask)
        {
            if(_registeredHandlers.ContainsKey(sender))
            {
                return;
            }
                
            _registeredHandlers.Add(sender, ignoreDispatcherMask);
        }

        public void RemoveEventListener(GameObject sender)
        {
            if(_registeredHandlers.Count == 0)
            {
                return;
            }
                
            if(!_registeredHandlers.ContainsKey(sender))
            {
                return;
            }
                
            _registeredHandlers.Remove(sender);
        }

        public void ClearEventListeners()
        {
            _registeredHandlers.Clear();
        }

        protected override void OnDestroy()
        {
            ClearEventListeners();

            base.OnDestroy();
        }

        bool ValidateLastEventMask(LayerMask ignoreDispatcherMask, GameObject go)
        {
            while(go != null)
            {
                if((ignoreDispatcherMask.value & 1 << go.layer) != 0)
                {
                    return false;
                }
                var parent = go.transform.parent;
                if(parent == null)
                {
                    break;
                }
                go = parent.gameObject;
            }
            return true;
        }

        bool ValidateLastPointerEventData(PointerEventData p, LayerMask ignoreDispatcherMask)
        {
            if(p != null)
            {
                if(ValidateLastEventMask(ignoreDispatcherMask, p.pointerPressRaycast.gameObject) || ValidateLastEventMask(ignoreDispatcherMask, p.pointerCurrentRaycast.gameObject))
                {
                    return true;
                }
            }
            return false;
        }
            
        public override void Process()
        {
            // First process the current tick with all received events
            _newActionRaycaster.RaycastResultGameObject = null;
            m_PointerData = _defaultPointerData;
            base.Process();

            // Second, with the previous events received we want to check if someone has been registered 
            // and in this case, for every registered GameObjects we will check if the events layer need to be ignored.
            // If not, we will redirect the event calls to every of the desired registered GameObjects
            ProcessRegisteredHandlers();
        }

        void ProcessRegisteredHandlers()
        {
            if(_registeredHandlers.Count == 0)
            {
                return;
            }

            var itr = _registeredHandlers.GetEnumerator();
            while(itr.MoveNext())
            {
                var sender = itr.Current.Key;
                var layers = itr.Current.Value;
                if(ValidateLastPointerEventData(_newActionRaycaster.LastEventData, layers))
                {
                    _newActionRaycaster.RaycastResultGameObject = sender;
                    m_PointerData = _actionEventDispatcherPointerData;
                    base.Process();
                }
            }
        }
    }
}