
using System.Diagnostics.SymbolStore;
using DG.Tweening;
using SocialPoint.Utils;
using UnityEngine;

public class CP_Ball : MonoBehaviour
{
    enum BallState
    {
        E_NONE,
        E_APPEAR,
        E_BOUNCING
    }

    const float kBallHorSpeed = 0.05f;

    public float BallHeight = 0.0f;
    public Renderer BallRenderer = null;
    public Rigidbody BallRigidBody = null;
    public Collider BallCollider = null;

    Vector3 _vectorTemp = Vector3.zero;
    RaycastHit _hitDown;
    RaycastHit _hitForward;
    BallState _fishState;
    long _stateStartTime = 0;
    long _stateRandomTime = 0;
    bool _firstBounce = true;
    Vector3 _initialPosition = Vector3.zero;

    bool GetHitDistance(out float distance, out RaycastHit hit, Vector3 initPosition, Vector3 direction, float maxDistance = 0.0001f, int layerMask = 1 << 10)
    {
        distance = 0f;

        Ray downRay = new Ray(initPosition, direction);
        if (Physics.Raycast(downRay, out hit, maxDistance, layerMask))
        {
            distance = hit.distance;
            return true;
        }

        return false;
    }

    void ProceedState(BallState state)
    {
        switch(state)
        {
            case BallState.E_APPEAR:
            {
                transform.localScale = Vector3.one;
                transform.localPosition = new Vector3(7f, 10f, -1.35f);
                _firstBounce = true;

                _initialPosition = transform.position;

                if(BallRenderer != null)
                {
                    BallRenderer.enabled = false;
                }

                if(BallRigidBody != null)
                {
                    BallRigidBody.useGravity = false;
                }

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1000;

                break;
            }

            case BallState.E_BOUNCING:
            {
                transform.localPosition = new Vector3(7f, 10f, -1.35f);

                if(BallRenderer != null)
                {
                    BallRenderer.enabled = true;
                }

                if(BallRigidBody != null)
                {
                    BallRigidBody.useGravity = true;
                }

                break;
            }
        }

        _fishState = state;
    }

    void MoveForward()
    {
        transform.position += (Vector3.left * kBallHorSpeed);

        float finishXPoint = _initialPosition.x - 12.0f;
        float distance = transform.position.x - finishXPoint;
        if(distance < 3.0f)
        {
            float delta = (3.0f - distance) / 3.0f;
            delta = 1.0f - delta;

            transform.localScale = Vector3.one * delta;
        }

    }

    void Update()
    {
        if(_fishState == BallState.E_NONE)
        {
            ProceedState(BallState.E_APPEAR);
        }

        var passedStateTime = (TimeUtils.TimestampMilliseconds > _stateStartTime + _stateRandomTime);

        switch(_fishState)
        {
            case BallState.E_APPEAR:
            {
                if(passedStateTime)
                {
                    ProceedState(BallState.E_BOUNCING);
                }

                break;
            }
            case BallState.E_BOUNCING:
            {
                var dist = 0f;
                if (GetHitDistance(out dist, out _hitDown, transform.position, -Vector3.up, 0.5f))
                {
                    _vectorTemp = transform.localPosition;
                    _vectorTemp.y = 1.6f;

                    transform.localPosition = _vectorTemp;

                    BallRigidBody.velocity = Vector3.zero;
                    BallRigidBody.AddForce(Vector3.up * BallHeight, ForceMode.Impulse);

                    _firstBounce = false;
                }

                if(!_firstBounce)
                {
                    MoveForward();
                }

                if (GetHitDistance(out dist, out _hitForward, transform.position, Vector3.left, 0.5f, 1 << 16))
                {
                    ProceedState(BallState.E_APPEAR);
                }

                break;
            }
        }
    }
}
