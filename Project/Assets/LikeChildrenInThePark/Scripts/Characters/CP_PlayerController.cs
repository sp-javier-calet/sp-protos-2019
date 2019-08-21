
using SocialPoint.Utils;
using UnityEngine;

public class CP_PlayerController : MonoBehaviour
{
    enum PlayerState
    {
        E_NONE,
        E_WALKING,
        E_JUMPING,
        E_JUMPING_FALL,
        E_STOPPED
    }

    const int kHoldingJumpMaxMillis = 150;
    const float kMoveForce = 0.05f;
    const float kMoveJumpingForce = 0.03f;
    const float kJumpForce = 11.0f;
    const float kFallingThreshold = 1f;
    const float kMaxFallingThreshold = 20f;

    Vector3 _vectTemp = new Vector3();

    bool _holding = false;
    long _holdingStartTime = 0;
    PlayerState _playerState = PlayerState.E_NONE;

    Rigidbody _rigidBody = null;
    Animation _animator = null;
    Camera _gameCamera = null;
    CP_SceneManager _sceneManager = null;
    RaycastHit _hit;
    float _initialDistance = 0f;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animation>();
        if(_animator != null)
        {
            _animator["walk"].speed = 2.0f;
            _animator["jump"].speed = 0.5f;
        }

        _gameCamera = GameObject.Find("GameCamera").GetComponent<Camera>();

        transform.position = new Vector3(CP_SceneManager.kScenePieceSize * 1.5f, 1.0f, -1.2f);

        var dist = 0f;
        GetHitDistance(out dist);
        _initialDistance = dist;

        Walk();
    }

    bool GetHitDistance(out float distance)
    {
        distance = 0f;

        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        Ray downRay = new Ray(transform.position, -Vector3.up);
        if (Physics.Raycast(downRay, out _hit, 0.0001f, layerMask))
        {
            distance = _hit.distance;
            return true;
        }

        return false;
    }

    public void SetSceneManager(CP_SceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    void Walk()
    {
        if(_animator != null)
        {
            _animator.Play("walk");
        }

        _playerState = PlayerState.E_WALKING;
    }

    void Jump()
    {
        if(_rigidBody != null)
        {
            _rigidBody.AddForce(Vector3.up * kJumpForce, ForceMode.Impulse);

            if(_animator != null)
            {
                _animator.Play("jump");
                _animator["jump"].time = 0.06f;
            }

            _playerState = PlayerState.E_JUMPING;
        }
    }

    void Stop()
    {
        if(_animator != null)
        {
            _animator.Play("idle");
        }

        _playerState = PlayerState.E_STOPPED;
    }

    void MoveForward()
    {
        if (_playerState == PlayerState.E_WALKING)
        {
            transform.position += (Vector3.right * kMoveForce);
        }
        else
        {
            transform.position += (Vector3.right * kMoveJumpingForce);  
        }
    }

    void LateUpdate()
    {
        if (_sceneManager != null)
        {
            _sceneManager.CheckMapGeneration();
        }

        if(!_holding)
        {
            if(Input.GetMouseButtonDown(0))
            {
                _holding = true;
                _holdingStartTime = TimeUtils.TimestampMilliseconds;
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            _holding = false;

            if (TimeUtils.TimestampMilliseconds <= _holdingStartTime + kHoldingJumpMaxMillis)
            {
                if (_playerState == PlayerState.E_WALKING)
                {
                    Jump();
                } 
            }
            else
            {
                if (_playerState == PlayerState.E_STOPPED)
                {
                    Walk();
                }
            }
        }
        else
        {
            if(_holding)
            {
                if(TimeUtils.TimestampMilliseconds > _holdingStartTime + kHoldingJumpMaxMillis)
                {
                    if (_playerState == PlayerState.E_WALKING)
                    {
                        Stop();
                    }
                }
            }
        }

        if(_playerState == PlayerState.E_JUMPING)
        {
            if(_rigidBody != null)
            {
                if(_rigidBody.velocity.y < 0.0f)
                {
                    _playerState = PlayerState.E_JUMPING_FALL;
                }
            }
        }

        if (_playerState == PlayerState.E_WALKING ||
            _playerState == PlayerState.E_JUMPING ||
            _playerState == PlayerState.E_JUMPING_FALL)
        {
            MoveForward();
        }

        var dist = 0f;
        if (GetHitDistance(out dist))
        {
            Debug.Log(_hit.collider.name);

            if(_playerState == PlayerState.E_JUMPING_FALL)
            {
                Walk();
            }
        }

        if(_gameCamera != null)
        {
            _vectTemp.x = transform.position.x;
            _vectTemp.y = _gameCamera.transform.position.y;
            _vectTemp.z = _gameCamera.transform.position.z;

            _gameCamera.transform.position = _vectTemp;
        }
    }
}
