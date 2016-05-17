using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public class ActionEventSystem : EventSystem, IActionEventSystem, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField]
        EventSystem _preempt;

        [SerializeField]
        LayerMask _preemptMask;

        protected override void OnEnable()
        {
            // do not assign EventSystem.current
        }

        protected override void Update()
        {
            EventSystem originalCurrent = EventSystem.current;
            current = this; // in order to avoid reimplementing half of the EventSystem class, just temporarily assign this EventSystem to be the globally current one
            base.Update();
            current = originalCurrent;
        }

        bool _hasFocus = true;

        void OnApplicationFocus(bool focusStatus)
        {
            _hasFocus = focusStatus;
        }

        BaseRaycaster _raycaster;

        BaseRaycaster Raycaster
        {
            get
            {
                if(_raycaster == null)
                {
                    _raycaster = GetComponent<BaseRaycaster>();
                }
                if(_raycaster == null)
                {
                    _raycaster = gameObject.AddComponent<EmptyRaycaster>();
                }
                return _raycaster;
            }
        }

        bool PreemptRaycast(PointerEventData eventData)
        {
            var preempt = _preempt ?? this;
            var preemptResults = new List<RaycastResult>();
            preempt.RaycastAll(eventData, preemptResults);
            for(int i = 0, preemptResultsCount = preemptResults.Count; i < preemptResultsCount; i++)
            {
                var result = preemptResults[i];
                var go = result.gameObject;
                while(go != null)
                {
                    if((_preemptMask.value & 1 << go.layer) != 0)
                    {
                        return true;
                    }
                    var parent = go.transform.parent;
                    if(parent == null)
                    {
                        break;
                    }
                    go = parent.gameObject;
                }
            }
            return false;
        }

        public void RaycastCamera(PointerEventData eventData, List<RaycastResult> raycastResults)
        {
            if(PreemptRaycast(eventData))
            {
                return;
            }

            raycastResults.Add(new RaycastResult {
                gameObject = gameObject,
                module = Raycaster,
                distance = 0,
                index = raycastResults.Count
            });
        }

        #region handlers

        public event Action<PointerEventData> OnBeginDrag;
        public event Action<PointerEventData> OnEndDrag;
        public event Action<PointerEventData> OnDrag;
        public event Action<PointerEventData> OnScroll;
        public event Action<PointerEventData> OnPointerEnter;
        public event Action<PointerEventData> OnPointerExit;
        public event Action<PointerEventData> OnPointerDown;
        public event Action<PointerEventData> OnPointerUp;
        public event Action<PointerEventData> OnPointerClick;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            var handler = OnBeginDrag;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            var handler = OnEndDrag;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var handler = OnDrag;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if(!_hasFocus)
            {
                return;
            }

            var handler = OnScroll;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            var handler = OnPointerEnter;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            var handler = OnPointerExit;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            var handler = OnPointerDown;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            var handler = OnPointerUp;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            var handler = OnPointerClick;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        #endregion
    }
}
