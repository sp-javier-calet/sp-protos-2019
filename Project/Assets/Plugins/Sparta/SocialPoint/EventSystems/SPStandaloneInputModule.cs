using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public class SPStandaloneInputModule : UnityStandaloneInputModule, 
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
        readonly Dictionary<EventTriggerType, Dictionary<GameObject, LayerMask>> _registeredEvents = new Dictionary<EventTriggerType, Dictionary<GameObject, LayerMask>>();
        readonly List<int> _pointerIds = new List<int>();

        GameObject _currentOverGo;
        bool _hasFocus = true;

        void OnApplicationFocus(bool focusStatus)
        {
            _hasFocus = focusStatus;
        }

        public override void Process()
        {
            DebugShowObjectIsHit();

            // We will process the received events for the current tick. This call will force to send events to the current selected GameObject and
            // another event to this class that will redirect the called event (if needed) to the desired registered class
            base.Process();

            // We need to save the current touched GameObject because we want to check later if it's layer need to be  ignored for the forced event
            #if UNITY_2017_1_OR_NEWER
            _currentOverGo = GetCurrentFocusedGameObject();
            #else
            _currentOverGo = eventSystem.currentSelectedGameObject;
            #endif
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
        static void DebugShowObjectIsHit()
        {
            var pe = new PointerEventData(EventSystem.current);
            pe.position = Input.mousePosition;

            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll( pe, hits );

            string msg = string.Empty;
            foreach(RaycastResult rr in hits)
            {

                GameObject go = rr.gameObject;
                if(go != null)
                {
                    msg += " - " + go;
                }
            }

            DebugLog("ObjectsHit " + msg);
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugGUIControlFlag)]
        static void DebugLog(string msg)
        {
            Log.i(string.Format("ActionStandaloneInputModule msg - {0}", msg));
        }

        #region handlers

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _pointerIds.Add(eventData.pointerId);

            DebugLog("Executed OnBeginDrag");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.BeginDrag, ExecuteEvents.beginDragHandler);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            DebugLog("Executed OnDrag");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.Drag, ExecuteEvents.dragHandler);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _pointerIds.Remove(eventData.pointerId);

            DebugLog("Executed OnEndDrag");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.EndDrag, ExecuteEvents.endDragHandler);
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if(!_hasFocus)
            {
                DebugLog("Executed OnScroll cancelled");
                return;
            }

            DebugLog("Executed OnScroll");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.Scroll, ExecuteEvents.scrollHandler);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            DebugLog("Executed OnPointerEnter");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerEnter, ExecuteEvents.pointerEnterHandler);
        }
            
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            DebugLog("Executed OnPointerExit");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerExit, ExecuteEvents.pointerExitHandler);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            DebugLog("Executed OnPointerDown");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerDown, ExecuteEvents.pointerDownHandler);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            DebugLog("Executed OnPointerUp");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerUp, ExecuteEvents.pointerUpHandler);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            DebugLog("Executed OnPointerClick");
            ExecuteForcedPointerEvents(eventData, EventTriggerType.PointerClick, ExecuteEvents.pointerClickHandler);
        }

        #endregion
    }
}