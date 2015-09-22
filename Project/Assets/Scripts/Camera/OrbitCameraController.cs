using SocialPoint.Base;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitCameraController : BaseCameraController
{
    protected override void OnDragTwoTouches(Touch touch0, Touch touch1)
    {
        UpdateZoom(touch0, touch1, false);
    }

    protected override void OnDragOneTouch(Touch touch0)
    {
        DoRotate(_pivot, touch0.NormalizedDeltaPosition().x);
    }

    protected override void OnDragLeftMouse(PointerEventData eventData)
    {
        DoRotate(_camera.WorldPosFromScreenNormalized(Vector2.zero), eventData.delta.x);
    }

    protected override void OnDragRightMouse(PointerEventData eventData)
    {
    }

    protected override void OnScrollWheel(PointerEventData eventData)
    {
        var zoom = 1.0f - eventData.scrollDelta.y * 0.1f;
        DoZoom(zoom, eventData.position, false);
    }
}