using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SocialPoint.EventSystems
{
    public class ActionStandaloneInputModule : StandaloneInputModule,
    IPointerEventDispatcher, 
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
        [SerializeField]
        LayerMask _ignoreDispatcherMask;

        Dictionary<int, PointerEventData> _actionEventDispatcherPointerData = new Dictionary<int, PointerEventData>();
        Dictionary<int, PointerEventData> _defaultPointerData;
        ForcedGameObjectRaycaster _newActionRaycaster;
        bool _hasFocus = true;
        List<int> _pointerIds = new List<int>();

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

        bool ValidateLastEventMask(GameObject go)
        {
            while(go != null)
            {
                if((_ignoreDispatcherMask.value & 1 << go.layer) != 0)
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

        bool ValidateLastPointerEventData(PointerEventData p)
        {
            if(p != null)
            {
                if(ValidateLastEventMask(p.pointerPressRaycast.gameObject) ||
                   ValidateLastEventMask(p.pointerCurrentRaycast.gameObject))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Process()
        {
            _newActionRaycaster.RaycastResultGameObject = null;
            m_PointerData = _defaultPointerData;
            base.Process();
            if(ValidateLastPointerEventData(_newActionRaycaster.LastEventData))
            {
                _newActionRaycaster.RaycastResultGameObject = gameObject;
                m_PointerData = _actionEventDispatcherPointerData;
                base.Process();
            }
        }

        #region handlers

        public event Action<PointerEventData> OnBeginDrag;
        public event Action<PointerEventData> OnEndDrag;
        public event Action<PointerEventData> OnDrag;
        public event Action<PointerEventData> OnDragMain;
        public event Action<PointerEventData> OnScroll;
        public event Action<PointerEventData> OnPointerEnter;
        public event Action<PointerEventData> OnPointerExit;
        public event Action<PointerEventData> OnPointerDown;
        public event Action<PointerEventData> OnPointerUp;
        public event Action<PointerEventData> OnPointerClick;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _pointerIds.Add(eventData.pointerId);
            var handler = OnBeginDrag;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _pointerIds.Remove(eventData.pointerId);
            var handler = OnEndDrag;
            if(handler != null)
            {
                handler(eventData);
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if(_pointerIds.IndexOf(eventData.pointerId) == 0)
            {
                var mhandler = OnDragMain;
                if(mhandler != null)
                {
                    mhandler(eventData);
                }
            }
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