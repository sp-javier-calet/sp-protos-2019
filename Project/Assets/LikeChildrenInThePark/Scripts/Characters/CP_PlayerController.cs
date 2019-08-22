
using DG.Tweening;
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
        E_STOPPED,
        E_DAMAGED,
        E_DAMAGED_FALL
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
    RaycastHit _hitDown;
    RaycastHit _hitForward;
    float _initialDistance = 0f;
    Vector3 _direction = Vector3.right;
    Vector3 _suicideLastPosition = Vector3.zero;

    bool _pressedDown = false;
    bool _pressedUp = false;

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
        _suicideLastPosition = transform.position;

        var dist = 0f;
        GetHitDistance(out dist, out _hitDown, -Vector3.up);
        _initialDistance = dist;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("CP_PlayerController OnTriggerEnter: " + other.name);

        if (string.CompareOrdinal(other.name, "Checkpoint") == 0)
        {
            CP_Checkpoint checkpoint = other.GetComponent<CP_Checkpoint>();
            if(checkpoint != null)
            {
                checkpoint.PlayAnimation();
            }

            _suicideLastPosition = transform.position;
        }
    }

    bool GetHitDistance(out float distance, out RaycastHit hit, Vector3 direction, float maxDistance = 0.0001f)
    {
        distance = 0f;

        int layerMask = 1 << 9;
        layerMask = ~layerMask;
        layerMask -= (1 << 12);

        Ray downRay = new Ray(transform.position, direction);
        if (Physics.Raycast(downRay, out hit, maxDistance, layerMask))
        {
            distance = hit.distance;
            return true;
        }

        return false;
    }

    public void SetSceneManager(CP_SceneManager sceneManager)
    {
        _sceneManager = sceneManager;

        Walk();
    }

    public void Turn(bool withAnim = true)
    {
        if (_direction == Vector3.right)
        {
            _direction = Vector3.left;
            if (withAnim) transform.DOLocalRotate(new Vector3(0f, 220f, 0f), 0.1f);
        }
        else
        {
            _direction = Vector3.right;
            if (withAnim) transform.DOLocalRotate(new Vector3(0f, 130f, 0f), 0.1f);
        }
    }

    void Walk()
    {
        if (_playerState != PlayerState.E_WALKING)
        {
            if(_animator != null)
            {
                _animator.Play("walk");
            }

            if (_sceneManager.GirlHeadUI != null)
            {
                _sceneManager.GirlHeadUI["walk"].speed = 2.0f;
                _sceneManager.GirlHeadUI.Play("walk");
            }

            _playerState = PlayerState.E_WALKING;
        }
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

        if (_sceneManager.GirlHeadUI != null)
        {
            _sceneManager.GirlHeadUI.Play("idle");
        }

        _playerState = PlayerState.E_STOPPED;
    }

    void Hurt()
    {
        if(_rigidBody != null)
        {
            Turn(false);

            _rigidBody.AddForce(Vector3.up * kJumpForce * 0.75f, ForceMode.Impulse);

            if(_animator != null)
            {
                _animator.Play("damage");
            }

            if (_sceneManager.GirlHeadUI != null)
            {
                _sceneManager.GirlHeadUI.Play("damage");
            }

            _sceneManager.SetTurnEnabled(false);
            _sceneManager.SetSuicideEnabled(false);
            _sceneManager.PlayerStats.TakeDamage(0.5f);

            _playerState = PlayerState.E_DAMAGED;
        }
    }

    void MoveForward()
    {
        if (_playerState == PlayerState.E_WALKING)
        {
            transform.position += (_direction * kMoveForce);
        }
        else
        {
            if (_playerState == PlayerState.E_DAMAGED || _playerState == PlayerState.E_DAMAGED_FALL)
            {
                transform.position += (_direction * kMoveJumpingForce * 2.0f);
            }
            else
            {
                transform.position += (_direction * kMoveJumpingForce);
            }
        }
    }

    public void OnPressedDown()
    {
        _pressedDown = true;
    }
    public void OnPressedUp()
    {
        _pressedUp = true;
    }
    public void OnPressedSuicide()
    {
        //Hurt();

        transform.position = _suicideLastPosition;
    }

    void LateUpdate()
    {
        if (_sceneManager != null)
        {
            _sceneManager.CheckMapGeneration();
        }

        if(!_holding)
        {
            if (_pressedDown)
            {
                _holding = true;
                _holdingStartTime = TimeUtils.TimestampMilliseconds;
            }
        }

        if (_pressedUp)
        {
            _holding = false;

            if (TimeUtils.TimestampMilliseconds <= _holdingStartTime + kHoldingJumpMaxMillis)
            {
                if (_playerState == PlayerState.E_WALKING || _playerState == PlayerState.E_STOPPED)
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
        if(_playerState == PlayerState.E_DAMAGED)
        {
            if(_rigidBody != null)
            {
                if(_rigidBody.velocity.y < 0.0f)
                {
                    _playerState = PlayerState.E_DAMAGED_FALL;
                }
            }
        }

        var dist = 0f;
        if (GetHitDistance(out dist, out _hitDown, -Vector3.up))
        {
            Debug.Log(_hitDown.collider.name);

            if(_playerState == PlayerState.E_JUMPING_FALL ||
               _playerState == PlayerState.E_DAMAGED_FALL)
            {
                if (_playerState == PlayerState.E_DAMAGED_FALL)
                {
                    _sceneManager.SetTurnEnabled(true);
                    _sceneManager.SetSuicideEnabled(true);

                    Turn(false);
                }

                Walk();
            }
        }

        if ((_playerState == PlayerState.E_JUMPING_FALL || _playerState == PlayerState.E_DAMAGED_FALL) && _rigidBody.velocity.y == 0.0f)
        {
            Walk();
        }

        if (!GetHitDistance(out dist, out _hitForward, _direction, 1.25f))
        {
            if (_playerState == PlayerState.E_WALKING ||
                _playerState == PlayerState.E_JUMPING ||
                _playerState == PlayerState.E_JUMPING_FALL ||
                _playerState == PlayerState.E_DAMAGED ||
                _playerState == PlayerState.E_DAMAGED_FALL)
            {
                MoveForward();
            }
        }

        if(_gameCamera != null)
        {
            _vectTemp.x = transform.position.x;
            _vectTemp.y = _gameCamera.transform.position.y;
            _vectTemp.z = _gameCamera.transform.position.z;

            _gameCamera.transform.position = _vectTemp;
        }

        _pressedDown = false;
        _pressedUp = false;
    }
}
