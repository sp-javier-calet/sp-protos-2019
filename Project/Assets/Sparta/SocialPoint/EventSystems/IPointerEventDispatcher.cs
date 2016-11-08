using System;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public interface IPointerEventDispatcher
    {
        event Action<PointerEventData> OnBeginDrag;
        event Action<PointerEventData> OnEndDrag;
        event Action<PointerEventData> OnDrag;
        event Action<PointerEventData> OnDragMain;
        event Action<PointerEventData> OnScroll;
        event Action<PointerEventData> OnPointerEnter;
        event Action<PointerEventData> OnPointerExit;
        event Action<PointerEventData> OnPointerDown;
        event Action<PointerEventData> OnPointerUp;
        event Action<PointerEventData> OnPointerClick;
    }
}
