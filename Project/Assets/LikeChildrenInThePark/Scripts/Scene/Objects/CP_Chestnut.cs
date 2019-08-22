
using System.Diagnostics.SymbolStore;
using DG.Tweening;
using SocialPoint.Utils;
using UnityEngine;

public class CP_Chestnut : MonoBehaviour
{
    public enum BallState
    {
        E_NONE,
        E_WAITING,
        E_APPEAR,
        E_MOVING,
        E_FALLING
    }

    const int kChestnutWaiting = 500;
    const int kChestnutAppear = 500;
    const int kChestnutMoving = 500;

    public long ChestNutOffsetTime = 0;
    public Renderer ChestNutRenderer = null;
    public Rigidbody ChestNutRigidBody = null;
    public Collider ChestNutCollider = null;

    Vector3 _vectorTemp = Vector3.zero;
    RaycastHit _hitDown;
    public BallState _fishState;
    long _stateStartTime = 0;
    long _stateRandomTime = 0;
    Vector3 _initialPosition = Vector3.zero;
    bool _firstFalling = true;

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
            case BallState.E_WAITING:
            {
                if(ChestNutRigidBody != null)
                {
                    ChestNutRigidBody.velocity = Vector3.zero;
                    ChestNutRigidBody.useGravity = false;
                }

                if(ChestNutRenderer != null)
                {
                    ChestNutRenderer.enabled = false;
                }

                transform.position = _initialPosition;

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                if(_firstFalling)
                {
                    _stateRandomTime = ChestNutOffsetTime;
                }
                else
                {
                    _stateRandomTime = kChestnutWaiting;
                }

                _firstFalling = false;

                break;
            }

            case BallState.E_APPEAR:
            {
                if(ChestNutRigidBody != null)
                {
                    ChestNutRigidBody.velocity = Vector3.zero;
                    ChestNutRigidBody.useGravity = false;
                }

                if(ChestNutRenderer != null)
                {
                    ChestNutRenderer.enabled = true;
                }

                transform.position = _initialPosition;

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = kChestnutAppear;

                break;
            }

            case BallState.E_MOVING:
            {
                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = kChestnutMoving;

                Sequence seq = DOTween.Sequence();
                seq.Append(ChestNutRenderer.transform.DOLocalMove(new Vector3(0.05f, 0f, 0f), kChestnutMoving / 1000.0f / 10f).SetLoops(10, LoopType.Yoyo));
                seq.Play();

                break;
            }

            case BallState.E_FALLING:
            {
                if(ChestNutRigidBody != null)
                {
                    ChestNutRigidBody.useGravity = true;
                }

                break;
            }
        }

        _fishState = state;
    }

    void Update()
    {
        if(_fishState == BallState.E_NONE)
        {
            _initialPosition = transform.position;

            ProceedState(BallState.E_WAITING);
        }

        var passedStateTime = (TimeUtils.TimestampMilliseconds > _stateStartTime + _stateRandomTime);

        switch(_fishState)
        {
            case BallState.E_WAITING:
            {
                if(passedStateTime)
                {
                    ProceedState(BallState.E_APPEAR);
                }

                break;
            }
            case BallState.E_APPEAR:
            {
                if(passedStateTime)
                {
                    ProceedState(BallState.E_MOVING);
                }

                break;
            }
            case BallState.E_MOVING:
            {
                if(passedStateTime)
                {
                    ProceedState(BallState.E_FALLING);
                }

                break;
            }
            case BallState.E_FALLING:
            {
                var dist = 0f;
                if (GetHitDistance(out dist, out _hitDown, transform.position, -Vector3.up, 0.75f))
                {
                    ProceedState(BallState.E_WAITING);
                }

                break;
            }
        }
    }
}
