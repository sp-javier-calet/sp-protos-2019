using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public class ActionEventSystem : EventSystem, IActionEventSystem, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
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

        public bool Enabled = true;

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

        public void RaycastCamera(PointerEventData eventData, List<RaycastResult> raycastResults)
        {
            if(Enabled)
            {
                raycastResults.Add(new RaycastResult {
                    gameObject = gameObject,
                    module = Raycaster,
                    distance = 0,
                    index = raycastResults.Count
                });
            }
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
