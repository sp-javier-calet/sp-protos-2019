
using SocialPoint.Utils;
using UnityEngine;

public class CP_PlayerController : MonoBehaviour
{
    const int kHoldingJumpMaxMillis = 150;
    const float kJumpForce = 10.0f;

    Vector3 _vectTemp = new Vector3();

    bool _holding = false;
    long _holdingStartTime = 0;
    bool _jumping = false;
    bool _jumpingGoingDown = false;

    Rigidbody _rigidBody = null;
    Animation _animator = null;
    Camera _gameCamera = null;

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

        Walk();
    }

    void Walk()
    {
        if(_animator != null)
        {
            _animator.Play("walk");
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

            _jumping = true;
        }
    }

    void Stop()
    {
        if(_animator != null)
        {
            _animator.Play("idle");
        }
    }

    void Update()
    {
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
                Jump();
            }
            else
            {
                Walk();
            }
        }
        else
        {
            if(_holding)
            {
                if(TimeUtils.TimestampMilliseconds > _holdingStartTime + kHoldingJumpMaxMillis)
                {
                    Stop();
                }
            }
        }

        if(_jumping)
        {
            if(!_jumpingGoingDown)
            {
                if(_rigidBody != null)
                {
                    if(_rigidBody.velocity.y < 0.0f)
                    {
                        _jumpingGoingDown = true;
                    }
                }
            }

            if(_jumpingGoingDown)
            {
                if(_rigidBody.velocity.y == 0.0f)
                {
                    _jumping = false;
                    _jumpingGoingDown = false;

                    Walk();
                }
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
