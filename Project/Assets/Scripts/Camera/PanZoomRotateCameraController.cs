using SocialPoint.Base;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanZoomRotateCameraController : BaseCameraController
{
    protected override void OnDragTwoTouches(Touch touch0, Touch touch1)
    {
        UpdatePan2Touches(touch0, touch1);
        UpdateZoom(touch0, touch1);
        UpdateRotation(touch0, touch1);
    }

    protected override void OnDragOneTouch(Touch touch0)
    {
        DoPan(touch0.NormalizedDeltaPosition());
    }

    protected override void OnDragLeftMouse(PointerEventData eventData)
    {
        DoPan(eventData.delta);
    }

    protected override void OnDragRightMouse(PointerEventData eventData)
    {
        DoRotate(_pivot, eventData.delta.x);
    }

    protected override void OnScrollWheel(PointerEventData eventData)
    {
        var zoom = 1.0f - eventData.scrollDelta.y * 0.1f;
        DoZoom(zoom, eventData.position);
    }
}
