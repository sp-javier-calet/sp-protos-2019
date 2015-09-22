using System;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseCameraController : MonoBehaviour
{
    List<int> _pointerIds = new List<int>();

    protected Camera _camera;
    protected Vector3 _pivot;

    const int LeftMouseId = -1;
    const int RightMouseId = -2;

    public UIEventHandler UIEventHandler;

    void OnEnable()
    {
        if(UIEventHandler == null)
        {
            throw new ArgumentNullException("UIEventHandler", "UIEventHandler cannot be null or empty!");
        }
        else
        {
            UIEventHandler.OnBeginDrag += OnBeginDrag;
            UIEventHandler.OnEndDrag += OnEndDrag;
            UIEventHandler.OnDrag += OnDrag;
            UIEventHandler.OnScroll += OnScroll;
        }
    }

    void OnDisable()
    {
        if(UIEventHandler != null)
        {
            UIEventHandler.OnBeginDrag -= OnBeginDrag;
            UIEventHandler.OnEndDrag -= OnEndDrag;
            UIEventHandler.OnDrag -= OnDrag;
            UIEventHandler.OnScroll -= OnScroll;
        }
    }

    void OnBeginDrag(PointerEventData eventData)
    {
        _pivot = _camera.WorldPosFromScreenPos(eventData.position);
        _pointerIds.Add(eventData.pointerId);

        _usingInertia = false;
    }

    void OnEndDrag(PointerEventData eventData)
    {
        _pointerIds.Remove(eventData.pointerId);

        _usingInertia = true;
    }

    void OnDrag(PointerEventData eventData)
    {
        if(_pointerIds.IndexOf(eventData.pointerId) == 0)
        {
            var numTouches = Input.touchCount;
            if(numTouches >= 2)
            {
                var touch0 = Input.touches[0];
                var touch1 = Input.touches[1];

                OnDragTwoTouches(touch0, touch1);
            }
            else if(numTouches == 1)
            {
                var touch0 = Input.touches[0];
                OnDragOneTouch(touch0);
            }
            else
            {
                if(eventData.pointerId.Equals(LeftMouseId))
                {
                    OnDragLeftMouse(eventData);
                }
                else if(eventData.pointerId.Equals(RightMouseId))
                {
                    OnDragRightMouse(eventData);
                }
            }
        }
    }

    void OnScroll(PointerEventData eventData)
    {
        OnScrollWheel(eventData);
    }

    protected virtual void OnDragTwoTouches(Touch touch0, Touch touch1){}

    protected virtual void OnDragOneTouch(Touch touch0){}

    protected virtual void OnDragLeftMouse(PointerEventData eventData){}

    protected virtual void OnDragRightMouse(PointerEventData eventData){}

    protected virtual void OnScrollWheel(PointerEventData eventData){}

    float _camSinAngle;
    float _originalOrthoSize;
    float _originalFieldOfView;

    // Zoom
    [SerializeField]
    [Range(0.1f, 1.0f)]
    float MinStaticZoom = 0.10f;

    [SerializeField]
    [Range(1.0f, 2.0f)]
    float MaxStaticZoom = 1.10f;

    // Rotation
    [SerializeField]
    [Range(1.0f, 10.0f)]
    float RotationFactor = 1.0f;

    // Inertia
    Vector3 _velocity;
    bool _usingInertia;

    [SerializeField]
    [Range(0.0f, 0.9f)]
    float IntertiaRatio = 0.75f;

    float AccumZoom
    {
        get
        {
            if(_camera.orthographic)
            {
                return (_camera.orthographicSize) / _originalOrthoSize;
            }
            else
            {
                return (_camera.fieldOfView) / _originalFieldOfView;
            }
        }
    }

    float MinZoom
    {
        get { return MinStaticZoom; }
    }

    float MaxZoom
    {
        get { return MaxStaticZoom; }
    }


    void Awake()
    {
        _camera = GetComponent<Camera>();
        if(_camera == null)
        {
            throw new ArgumentNullException("_camera", "_camera cannot be null or empty!");
        }

        float xAngle = _camera.transform.rotation.eulerAngles.x;
        _camSinAngle = Mathf.Sin(Mathf.Deg2Rad * xAngle);
        _originalOrthoSize = _camera.orthographicSize;
        _originalFieldOfView = _camera.fieldOfView;
    }

    void Update()
    {
        if(_usingInertia)
        {
            MovePosition(_velocity * Time.deltaTime);
            _velocity *= IntertiaRatio;

            _usingInertia &= _velocity.magnitude > 0.0f;
        }
    }

    protected void UpdatePan2Touches(Touch touch1, Touch touch2)
    {
        Vector2 touchMiddlePos = (touch1.position + touch2.position) * 0.5f;
        Vector2 prevTouchMiddlePos = ((touch1.position - touch1.NormalizedDeltaPosition()) + (touch2.position - touch2.NormalizedDeltaPosition())) * 0.5f;
        Vector2 deltaTouchPosition = touchMiddlePos - prevTouchMiddlePos;

        DoPan(deltaTouchPosition);
    }

    protected void DoPan(Vector2 deltaScreen)
    {
        Vector3 deltaCameraPos = ScreenDelta2CameraDelta(deltaScreen);
        MovePosition(deltaCameraPos);
    }

    Vector3 ScreenDelta2CameraDelta(Vector2 deltaScreen)
    {
        Vector2 cameraDeltaPerScreenDelta = GetWorldUnitsPerScreenUnit();

        // move horizontal
        float xDeltaScaled = deltaScreen.x * cameraDeltaPerScreenDelta.x;
        Vector2 right = new Vector2(_camera.transform.right.x, _camera.transform.right.z).normalized;
        var rightMove = new Vector3(-right.x * xDeltaScaled, 0, -right.y * xDeltaScaled);

        // move vertical
        float yDeltaScaled = deltaScreen.y * cameraDeltaPerScreenDelta.y;
        Vector2 forward = new Vector2(_camera.transform.forward.x, _camera.transform.forward.z).normalized;
        var forwardMove = new Vector3(-forward.x * yDeltaScaled, 0, -forward.y * yDeltaScaled);

        Vector3 totalMove = rightMove + forwardMove;

        return totalMove;
    }

    void MovePosition(Vector3 cameraDeltaPos)
    {
        Vector3 newPosition = _camera.transform.position;
        newPosition += cameraDeltaPos;

        _camera.transform.position = newPosition;

        _velocity = cameraDeltaPos / Time.deltaTime;
    }

    Vector2 GetWorldUnitsPerScreenUnit()
    {
        float cameraHeight;
        float cameraWitdh;
        CalculateCurrentCameraWidthAndHeight(out cameraWitdh, out cameraHeight);

        var ratio = new Vector2(cameraWitdh / Screen.width, cameraHeight / Screen.height);
        return ratio;
    }

    void CalculateCurrentCameraWidthAndHeight(out float oCameraWitdh, out float oCameraHeight)
    {
        if(_camera.orthographic)
        {
            CalculateCurrentCameraWidhAndHeightOrtho(out oCameraWitdh, out oCameraHeight);
        }
        else
        {
            CalculateCurrentCameraWidthAndHeightPerpective(out oCameraWitdh, out oCameraHeight);
        }
    }

    void CalculateCurrentCameraWidhAndHeightOrtho(out float oCameraWitdh, out float oCameraHeight)
    {
        float ar = (float)Screen.width / (float)Screen.height;

        // orthographic size is half vertical size
        float cameraHeight = _camera.orthographicSize * 2.0f;
        float cameraWitdh = cameraHeight * ar;
        float camFrontDistance = cameraHeight / _camSinAngle;

        oCameraWitdh = cameraWitdh;
        oCameraHeight = camFrontDistance;
    }

    void CalculateCurrentCameraWidthAndHeightPerpective(out float oCameraWitdh, out float oCameraHeight)
    {
        Vector3 middleBottomWP = _camera.WorldPosFromScreenNormalized(new Vector2(0f, -1f));
        Vector3 middleTopWP = _camera.WorldPosFromScreenNormalized(new Vector2(0f, 1f));

        Vector3 middleLeftWP = _camera.WorldPosFromScreenNormalized(new Vector2(-1f, 0f));
        Vector3 middleRightWP = _camera.WorldPosFromScreenNormalized(new Vector2(1f, 0f));

        Vector3 HorizontalDir = middleRightWP - middleLeftWP;
        Vector3 VerticalDir = middleTopWP - middleBottomWP;

        float cameraWidth = HorizontalDir.magnitude;
        oCameraWitdh = cameraWidth;

        float cameraHeight = VerticalDir.magnitude;
        oCameraHeight = cameraHeight;
    }

    #region Zoom

    protected void UpdateZoom(Touch touch1, Touch touch2, bool translate = true)
    {
        float zoomCurDist = Vector2.Distance(touch1.position, touch2.position);
        float zoomPrevDistance = Vector2.Distance(touch1.position - touch1.NormalizedDeltaPosition(), touch2.position - touch2.NormalizedDeltaPosition());

        float zoom = zoomPrevDistance / zoomCurDist;

        Vector2 touchMiddlePos = (touch1.position + touch2.position) * 0.5f;
        DoZoom(zoom, touchMiddlePos, translate);
    }

    protected void DoZoom(float zoom, Vector2 deltaMiddlePos, bool translate = true)
    {
        zoom = SetZoomBetweenLimits(zoom, MinZoom, MaxZoom);

        // screen delta pos
        deltaMiddlePos.x = deltaMiddlePos.x - ((float)Screen.width * 0.5f);
        deltaMiddlePos.y = deltaMiddlePos.y - ((float)Screen.height * 0.5f);

        // camera delta pos
        Vector2 cameraUnitsPerPixel = GetWorldUnitsPerScreenUnit();
        var deltaMiddlePosCameraUnits = new Vector2(deltaMiddlePos.x * cameraUnitsPerPixel.x, deltaMiddlePos.y * cameraUnitsPerPixel.y);
        deltaMiddlePosCameraUnits = deltaMiddlePosCameraUnits * (zoom - 1.0f);

        // apply camera movement horizontal
        var zoomCameraDeltaMove = new Vector3(0, 0, 0);
        var right = new Vector2(_camera.transform.right.x, _camera.transform.right.z);
        right.Normalize();
        zoomCameraDeltaMove -= new Vector3(right.x, 0, right.y) * deltaMiddlePosCameraUnits.x;

        var forward = new Vector2(_camera.transform.forward.x, _camera.transform.forward.z);
        forward.Normalize();
        zoomCameraDeltaMove -= new Vector3(forward.x, 0, forward.y) * deltaMiddlePosCameraUnits.y;

//         apply the scale
        _camera.orthographicSize *= zoom;
        _camera.fieldOfView *= zoom;

        if(translate)
        {
            // apply the new position
            var newPosition = _camera.transform.position + zoomCameraDeltaMove;
            _camera.transform.position = newPosition;
        }
    }

    float SetZoomBetweenLimits(float zoom, float minZoom, float maxZoom)
    {
        float nextAccumulatedZoom = AccumZoom * zoom;

        if(nextAccumulatedZoom > (maxZoom + Mathf.Epsilon) && zoom > 1.0f)
        {
            zoom = maxZoom / AccumZoom;
        }
        else if(nextAccumulatedZoom < (minZoom - Mathf.Epsilon) && zoom < 1.0f)
        {
            zoom = minZoom / AccumZoom;
        }

        return zoom;
    }

    #endregion

    #region Rotation

    protected void UpdateRotation(Touch touch1, Touch touch2)
    {
        float angle = GetRotationAngle(touch1, touch2);

        Vector3 pivotPos = GetPivotPos(touch1, touch2);

        DoRotate(pivotPos, angle);
    }

    float GetRotationAngle(Touch touch1, Touch touch2)
    {
        Vector2 front = (touch1.position - touch1.NormalizedDeltaPosition()) - (touch2.position - touch2.NormalizedDeltaPosition());
        front.Normalize();

        var right = new Vector2(-front.y, front.x);
        right.Normalize();

        Vector2 newFront = (touch1.position - touch2.position);
        newFront.Normalize();

        //project newFront to front and right
        float frontProj = Vector2.Dot(newFront, front); // x axis
        float rightProj = Vector2.Dot(newFront, right); // y axis

        float angle = Mathf.Atan2(rightProj, frontProj);

        angle *= Mathf.Rad2Deg;
        angle *= RotationFactor;

        return angle;
    }

    Vector3 GetPivotPos(Touch touch1, Touch touch2)
    {
        Vector2 touchMiddlePos = (touch1.position + touch2.position) * 0.5f;
        Vector3 pivot = _camera.WorldPosFromScreenPos(touchMiddlePos);
        return pivot;
    }

    protected void DoRotate(Vector3 pivotPos, float angle)
    {
        // Rotate according to the reference pos
        Vector3 postLocalToPivot = _camera.transform.position - pivotPos;

        // rotate the pivot pos
        Quaternion pivotRot = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 rotatedPivot = pivotRot * postLocalToPivot;

        // reposition ourself
        var newPosition = new Vector3(pivotPos.x + rotatedPivot.x, _camera.transform.position.y, pivotPos.z + rotatedPivot.z);
        _camera.transform.position = newPosition;

        Quaternion newQuat = pivotRot * _camera.transform.rotation;
        _camera.transform.rotation = newQuat;
    }

    #endregion

}
