
using System.Diagnostics.SymbolStore;
using DG.Tweening;
using SocialPoint.Utils;
using UnityEngine;

public class CP_Fish : MonoBehaviour
{
    enum FishState
    {
        E_NONE,
        E_INVISIBLE,
        E_SHOWING,
        E_JUMPING,
        E_JUMPING_FALL
    }

    public Renderer FishRenderer = null;
    public Rigidbody FishRigidBody = null;
    public Collider FishCollider = null;

    RaycastHit _hitDown;
    FishState _fishState;
    long _stateStartTime = 0;
    long _stateRandomTime = 0;

    bool GetHitDistance(out float distance, out RaycastHit hit, Vector3 initPosition, Vector3 direction, float maxDistance = 0.0001f)
    {
        distance = 0f;

        int layerMask = 1 << 10;

        Ray downRay = new Ray(initPosition, direction);
        if (Physics.Raycast(downRay, out hit, maxDistance, layerMask))
        {
            distance = hit.distance;
            return true;
        }

        return false;
    }

    void ProceedState(FishState state)
    {
        switch(state)
        {
            case FishState.E_INVISIBLE:
            {
                if(FishRenderer != null)
                {
                    FishRenderer.enabled = false;
                }

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = RandomUtils.Range(1000, 2000);

                break;
            }

            case FishState.E_SHOWING:
            {
                if(FishRenderer != null)
                {
                    FishRenderer.enabled = true;
                }

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1000;

                break;
            }

            case FishState.E_JUMPING:
            {
                if(FishRigidBody != null)
                {
                    FishRigidBody.AddForce(Vector3.up * 18.0f, ForceMode.Impulse);
                }

                if(FishCollider != null)
                {
                    FishCollider.isTrigger = true;
                }

                break;
            }
        }

        _fishState = state;
    }

    void Update()
    {
        if(_fishState == FishState.E_NONE)
        {
            ProceedState(FishState.E_INVISIBLE);
        }

        var passedStateTime = (TimeUtils.TimestampMilliseconds > _stateStartTime + _stateRandomTime);

        switch(_fishState)
        {
            case FishState.E_INVISIBLE:
            {
                if(passedStateTime)
                {
                    ProceedState(FishState.E_SHOWING);
                }

                break;
            }
            case FishState.E_SHOWING:
            {
                if(passedStateTime)
                {
                    ProceedState(FishState.E_JUMPING);
                }

                break;
            }
            case FishState.E_JUMPING:
            {
                if(FishRigidBody != null)
                {
                    if(FishRigidBody.velocity.y < 0.0f)
                    {
                        ProceedState(FishState.E_JUMPING_FALL);
                    }
                }

                break;
            }
            case FishState.E_JUMPING_FALL:
            {
                var dist = 0f;
                if (GetHitDistance(out dist, out _hitDown, transform.position, -Vector3.up, 0.5f))
                {
                    if(FishCollider != null)
                    {
                        FishCollider.isTrigger = false;
                    }

                    ProceedState(FishState.E_INVISIBLE);
                }

                break;
            }
        }
    }
}
