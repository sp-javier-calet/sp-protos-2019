using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    public event Action<PointerEventData> OnBeginDrag;
    public event Action<PointerEventData> OnEndDrag;
    public event Action<PointerEventData> OnDrag;
    public event Action<PointerEventData> OnScroll;

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
        var handler = OnScroll;
        if(handler != null)
        {
            handler(eventData);
        }
    }
}
