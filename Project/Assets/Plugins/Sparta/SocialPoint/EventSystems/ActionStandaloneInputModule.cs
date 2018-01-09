using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public class ActionStandaloneInputModule : StandaloneInputModule, 
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler, 
    IScrollHandler,
    IPointerEnterHandler, 
    IPointerExitHandler, 
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
    {
        readonly Dictionary<int, PointerEventData> _actionEventDispatcherPointerData = new Dictionary<int, PointerEventData>();
        readonly Dictionary<EventTriggerType, Dictionary<GameObject, LayerMask>> _registeredEvents = new Dictionary<EventTriggerType, Dictionary<GameObject, LayerMask>>();
        readonly List<int> _pointerIds = new List<int>();

        Dictionary<int, PointerEventData> _defaultPointerData;
        ForcedGameObjectRaycaster _newActionRaycaster;
        GameObject _currentOverGo;
        bool _hasFocus = true;

        void OnApplicationFocus(bool focusStatus)
        {
            _hasFocus = focusStatus;
        }

        public override void ActivateModule()
        {
            base.ActivateModule();
            _defaultPointerData = m_PointerData;
            _newActionRaycaster = gameObject.AddComponent<ForcedGameObjectRaycaster>();
        }

        public override void Process()
        {
            // First process the current tick with all received events
            _newActionRaycaster.RaycastResultGameObject = null;
            m_PointerData = _defaultPointerData;
            base.Process();

            // We need to save the current touched GameObject because we want to check later if it's layer need to be  ignored for the forced event
            _currentOverGo = GetCurrentFocusedGameObject();

            // Second, with the previous events received we want to check if someone has been registered and in this case, for every registered GameObjects
            // we will check if the events layer need to be ignored. If not, we will redirect the event calls to every of the desired registered GameObjects
            _newActionRaycaster.RaycastResultGameObject = gameObject;
            m_PointerData = _actionEventDispatcherPointerData;
            base.Process();
        }
            
        public void RegisterEventReceiver(EventTriggerType eventTriggerType, GameObject sender, LayerMask ignoreDispatcherMask)
        {
            if(_registeredEvents.ContainsKey(eventTriggerType))
            {
                _registeredEvents[eventTriggerType].Add(sender, ignoreDispatcherMask);
            }
            else
            {
                _registeredEvents.Add(eventTriggerType, new Dictionary<GameObject, LayerMask> {{sender, ignoreDispatcherMask}});
            }
        }

        public void UnRegisterEventReceiver(EventTriggerType eventTriggerType, GameObject sender)
        {
            if(_registeredEvents.ContainsKey(eventTriggerType))
            {
                _registeredEvents[eventTriggerType].Remove(sender);
            }
        }

        Dictionary<GameObject, LayerMask> GetRegisteredEventHandler(EventTriggerType eventTriggerType)
        {
            Dictionary<GameObject, LayerMask> objects;
            return _registeredEvents.TryGetValue(eventTriggerType, out objects) ? objects : null;
        }
            
        static bool ValidateLastEventMask(GameObject sender, LayerMask ignoreDispatcherMask)
        {
            while(sender != null)
            {
                if((ignoreDispatcherMask.value & 1 << sender.layer) != 0)
                {
                    return false;
                }

                var parent = sender.transform.parent;
                if(parent == null)
                {
                    break;
                }

                sender = parent.gameObject;
            }

            return true;
        }

        void ExecuteForcedPointerEvents<T>(BaseEventData eventData, EventTriggerType eventTriggerType, ExecuteEvents.EventFunction<T> eventFunction) where T : IEventSystemHandler
        {
            var objects = GetRegisteredEventHandler(eventTriggerType);
            if(objects != null && objects.Count > 0)
            {
                var iter = objects.GetEnumerator();
                while(iter.MoveNext())
                {
                    var sender = iter.Current.Key;
                    var ignoreDispatcherMask = iter.Current.Value;

                    // If we are pointing over a GameObject and the ignored mask filter is correct we propagate the event system event received to all the needed registered GameObjects
                    if(_currentOverGo != null && ValidateLastEventMask(_currentOverGo, ignoreDispatcherMask))
                    {
                        eventData.selectedObject = sender;
                        ExecuteEvents.Execute(sender, eventData, eventFunction);
                    }
                }
                iter.Dispose();
            }
        }
            
        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("ActionStandaloneInputModule event {0} executed", msg));
        }

        #region handlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            _pointerIds.Add(eventData.pointerId);

            DebugLog("OnBeginDrag");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.BeginDrag, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DebugLog("OnDrag");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.Drag, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _pointerIds.Remove(eventData.pointerId);

            DebugLog("OnEndDrag");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.EndDrag, ExecuteEvents.endDragHandler);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if(!_hasFocus)
            {
                return;
            }

            DebugLog("OnScroll");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.Scroll, ExecuteEvents.scrollHandler);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DebugLog("OnPointerEnter");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerEnter, ExecuteEvents.pointerEnterHandler);
        }
            
        public void OnPointerExit(PointerEventData eventData)
        {
            DebugLog("OnPointerExit");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerExit, ExecuteEvents.pointerExitHandler);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            DebugLog("OnPointerDown");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerDown, ExecuteEvents.pointerDownHandler);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DebugLog("OnPointerUp");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerUp, ExecuteEvents.pointerUpHandler);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            DebugLog("OnPointerClick");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerClick, ExecuteEvents.pointerClickHandler);
        }

        #endregion
    }
}