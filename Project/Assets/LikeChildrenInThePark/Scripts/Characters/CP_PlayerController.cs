
using System.Collections.Generic;
using DG.Tweening;
using SocialPoint.Rendering.Components;
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

    public enum PowerUpType
    {
        E_NONE,
        E_SPEED_UP,
        E_ANGRY,
        E_LIFE,
        E_DOUBLE_JUMP,
        E_INVINCIBLE
    }

    int[] PowerUpTimes =
    {
        0, 12000, 16000, 0, 16000, 8000
    };

    const int kHoldingJumpMaxMillis = 150;
    const int kDamageInvulnerableMaxMillis = 2750;
    const int kAccumulatedStartTotalTimeMillis = 1000;
    const float kMoveForce = 0.05f;
    const float kMoveForceStart = 0.5f;
    const float kMoveJumpingForce = 0.05f;
    const float kJumpForce = 11.0f;
    const float kFallingThreshold = 1f;
    const float kMaxFallingThreshold = 20f;
    const float kAccumulatedStartThreshold = 0.75f;

    public BCSHModifier HeadBCSH = null;

    Vector3 _vectTemp = new Vector3();
    Color _colorTemp = Color.white;

    bool _holding = false;
    bool _holdingStart = false;
    bool _failStart = false;
    float _accumulatedStart = 0f;
    long _accumulatedStartTime = 0;
    float _accumulatedSpeedAdded = 0f;
    long _holdingStartTime = 0;
    bool _memoryJump = false;
    bool _damageInvulnerable = false;
    long _damageInvulnerableStartTime = 0;
    PlayerState _playerState = PlayerState.E_NONE;
    long _powerUpStartTime = 0;
    float _moveForcePowerUp = 0.0f;
    PowerUpType _currentPowerUp = PowerUpType.E_NONE;
    bool _hasDoubleHump = false;
    long _lastAngryTurnTime = 0;
    int _currentAngryRandomTime = 0;

    Rigidbody _rigidBody = null;
    Animation _animator = null;
    Camera _gameCamera = null;
    CP_SceneManager _sceneManager = null;
    RaycastHit _hitDown;
    RaycastHit _hitForward;
    float _initialDistance = 0f;
    Vector3 _direction = Vector3.right;
    Vector3 _suicideLastPosition = Vector3.zero;
    List<Material> _playerMaterials = new List<Material>();

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

        transform.position = new Vector3(CP_SceneManager.kScenePieceSize * 0.5f, 1.0f, -1.2f);

        if(CP_GameManager.Instance.CurrentGameState == CP_GameManager.GameState.E_PLAYING_1_PLAYER)
        {
            Init();
        }
    }

    public void Init()
    {
        if(_rigidBody != null)
        {
            _rigidBody.useGravity = true;
        }

        _gameCamera = GameObject.Find("GameCamera").GetComponent<Camera>();

        var dist = 0f;
        GetHitDistance(out dist, out _hitDown, transform.position, -Vector3.up);
        _initialDistance = dist;
    }

    void AddPowerUp(PowerUpType powerUp)
    {
        _powerUpStartTime = TimeUtils.TimestampMilliseconds;

        switch(powerUp)
        {
            case PowerUpType.E_SPEED_UP:
            {
                _moveForcePowerUp = 0.075f;
                _animator["walk"].speed = 4f;

                break;
            }
            case PowerUpType.E_ANGRY:
            {
                _moveForcePowerUp = 0.0375f;
                _animator["walk"].speed = 3f;

                _lastAngryTurnTime = TimeUtils.TimestampMilliseconds;
                _currentAngryRandomTime = RandomUtils.Range(1000, 3500);

                break;
            }
            case PowerUpType.E_INVINCIBLE:
            {
                _moveForcePowerUp = 0.0375f;
                _animator["walk"].speed = 3f;

                break;
            }
            case PowerUpType.E_LIFE:
            {
                _sceneManager.PlayerStats.Heal(2f);
                break;
            }
        }

        if(powerUp != PowerUpType.E_LIFE)
        {
            if(_sceneManager.PowerUpTime != null)
            {
                _sceneManager.PowerUpTime.ShowPowerUpTime(powerUp);
            }

            _currentPowerUp = powerUp;
        }
    }

    void ResetPowerUp()
    {
        switch(_currentPowerUp)
        {
            case PowerUpType.E_SPEED_UP:
            {
                _moveForcePowerUp = 0f;
                _animator["walk"].speed = 2.0f;

                break;
            }
            case PowerUpType.E_ANGRY:
            {
                _moveForcePowerUp = 0f;
                _animator["walk"].speed = 2.0f;

                if(HeadBCSH != null)
                {
                    HeadBCSH.ApplyBCSHState("default");
                }

                break;
            }
            case PowerUpType.E_INVINCIBLE:
            {
                _moveForcePowerUp = 0f;
                _animator["walk"].speed = 2.0f;

                for(var i = 0; i < _playerMaterials.Count; ++i)
                {
                    _colorTemp = Color.white;
                    _colorTemp.a = 1f;

                    _playerMaterials[i].SetColor("_Color", _colorTemp);
                }

                break;
            }
        }

        if(_sceneManager.PowerUpTime != null)
        {
            _sceneManager.PowerUpTime.SetEnabled(false);
        }

        _currentPowerUp = PowerUpType.E_NONE;
    }

    void OnTriggerEnter(Collider other)
    {
        if (string.CompareOrdinal(other.name, "Checkpoint") == 0)
        {
            CP_Checkpoint checkpoint = other.GetComponent<CP_Checkpoint>();
            if(checkpoint != null)
            {
                checkpoint.PlayAnimation();
            }

            if(_suicideLastPosition == Vector3.zero)
            {
                _sceneManager.SetSuicideEnabled(true);
            }

            _suicideLastPosition = transform.position;
        }

        if(!_damageInvulnerable)
        {
            if(string.CompareOrdinal(other.name, "Water") == 0)
            {
                Hurt();
            }
            else if(string.CompareOrdinal(other.name, "Fish") == 0)
            {
                Hurt();
            }
            else if(string.CompareOrdinal(other.name, "Ball") == 0)
            {
                Hurt();
            }
            else if(string.CompareOrdinal(other.name, "Apple") == 0)
            {
                CP_Chestnut chestNut = other.GetComponent<CP_Chestnut>();
                if(chestNut != null && chestNut._fishState == CP_Chestnut.BallState.E_FALLING)
                {
                    Hurt();
                }
            }
        }

        if(other.name.Contains("PowerUp"))
        {
            CP_PowerUp powerUp = other.GetComponent<CP_PowerUp>();
            if(powerUp != null)
            {
                if(_currentPowerUp != PowerUpType.E_NONE)
                {
                    ResetPowerUp();
                }

                AddPowerUp((PowerUpType) (powerUp.PowerUpType + 1));

                powerUp.Taken();
            }
        }
    }

    public void StartRun()
    {
        if(_animator != null)
        {
            _animator["walk"].speed = 2.0f;
        }

        if(_accumulatedStart > kAccumulatedStartThreshold)
        {
            _failStart = true;
            _accumulatedStart = 0f;

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOLocalRotate(new Vector3(0f, 220f, 0f), 0.1f));
            seq.Append(transform.DOLocalRotate(new Vector3(0f, 20f, 0f), 0.1f));
            seq.SetLoops(8);
            seq.onComplete += StartRunAfterFail;
        }
        else
        {
            _accumulatedStartTime = TimeUtils.TimestampMilliseconds;

            Walk();
        }
    }

    public void StartRunAfterFail()
    {
        transform.localRotation = Quaternion.Euler(0f, 130f, 0f);
        _failStart = false;

        Walk();
    }

    bool GetHitDistance(out float distance, out RaycastHit hit, Vector3 initPosition, Vector3 direction, float maxDistance = 0.0001f)
    {
        distance = 0f;

        int layerMask = 1 << 9;
        layerMask = ~layerMask;
        layerMask -= (1 << 12);
        layerMask -= (1 << 13);
        layerMask -= (1 << 14);
        layerMask -= (1 << 15);
        layerMask -= (1 << 16);
        layerMask -= (1 << 17);
        layerMask -= (1 << 18);
        layerMask -= (1 << 31);

        Ray downRay = new Ray(initPosition, direction);
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

        Stop();
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

            _hasDoubleHump = false;
            _playerState = PlayerState.E_WALKING;
        }
    }

    void Jump()
    {
        if(_rigidBody != null)
        {
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.AddForce(Vector3.up * kJumpForce, ForceMode.Impulse);

            if(_animator != null)
            {
                _animator.Play("jump");
                _animator["jump"].time = 0.06f;
            }

            _memoryJump = false;

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

    void Die()
    {
        for(var i = 0; i < _playerMaterials.Count; ++i)
        {
            _colorTemp = Color.white;
            _colorTemp.a = 1.0f;

            _playerMaterials[i].SetColor("_Color", _colorTemp);
        }

        transform.DOScale(Vector3.zero, 3.0f);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalRotate(new Vector3(0f, 310f, 0f), 0.1f));
        seq.Append(transform.DOLocalRotate(new Vector3(0f, 130f, 0f), 0.1f));
        seq.SetLoops(15);
        seq.onComplete += AfterDying;
        seq.Play();
    }

    void AfterDying()
    {
        _sceneManager.SetCurrentGameState(CP_SceneManager.BattleState.E_GAMEOVER_AFTER);
    }

    void Hurt()
    {
        if(_rigidBody != null && _currentPowerUp != PowerUpType.E_INVINCIBLE)
        {
            Turn(false);

            _rigidBody.velocity = Vector3.zero;
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

            _damageInvulnerable = true;
            _damageInvulnerableStartTime = TimeUtils.TimestampMilliseconds;

            _playerState = PlayerState.E_DAMAGED;
        }
    }

    void MoveForward()
    {
        if (_playerState == PlayerState.E_WALKING)
        {
            transform.position += (_direction * (kMoveForce + _accumulatedSpeedAdded + _moveForcePowerUp));
        }
        else
        {
            if (_playerState == PlayerState.E_DAMAGED || _playerState == PlayerState.E_DAMAGED_FALL)
            {
                transform.position += (_direction * kMoveJumpingForce * 2.0f);
            }
            else
            {
                transform.position += (_direction * (kMoveJumpingForce + _accumulatedSpeedAdded));
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
        _sceneManager.PlayerStats.TakeDamage(1.0f);
        if(_sceneManager.PlayerStats.Health <= 0.0f)
        {
            Die();
            _sceneManager.SetCurrentGameState(CP_SceneManager.BattleState.E_GAMEOVER);
        }
        else
        {
            transform.position = _suicideLastPosition;
            if (_sceneManager != null)
            {
                _sceneManager.CheckMapGeneration(true);
            }
        }
    }

    void AfterDamage()
    {
        _sceneManager.SetTurnEnabled(true);
        _sceneManager.SetSuicideEnabled(_sceneManager.SuicideEnabled);

        Turn(false);

        if(_sceneManager.PlayerStats.Health <= 0.0f)
        {
            Die();

            _sceneManager.SetCurrentGameState(CP_SceneManager.BattleState.E_GAMEOVER);
        }
    }

    void LateUpdate()
    {
        if(_sceneManager.CurrentBattleState == CP_SceneManager.BattleState.E_SEMAPHORE)
        {
            if(_sceneManager.Semaphore.CurrentsemaphoreState == CP_Semaphore.SemaphoreState.E_P3)
            {
                if(!_holdingStart)
                {
                    if(_pressedDown)
                    {
                        _holdingStart = true;
                        _accumulatedStart = 1.0f - _sceneManager.Semaphore.DeltaInterStates();

                        if(_animator != null)
                        {
                            _animator.Play("walk");
                        }
                    }
                }

                if(!_pressedDown && _pressedUp)
                {
                    _holdingStart = false;
                    _accumulatedStart = 0.0f;

                    if(_animator != null)
                    {
                        _animator.Play("idle");
                    }
                }

                if(_holdingStart)
                {
                    if(_animator != null)
                    {
                        float deltaSpeed = _sceneManager.Semaphore.DeltaInterStates() * 5.0f;

                        _animator["walk"].speed = 2.0f + (deltaSpeed * _accumulatedStart);
                    }
                }
            }
        }

        if(_sceneManager.CurrentBattleState == CP_SceneManager.BattleState.E_PLAYING)
        {
            if(_failStart)
            {
                return;
            }

            if(_accumulatedStart > 0f)
            {
                var delta = 1f - ((TimeUtils.TimestampMilliseconds - _accumulatedStartTime) / (float)kAccumulatedStartTotalTimeMillis);

                _accumulatedSpeedAdded = kMoveForceStart * delta;

                if(delta >= 1f || delta < 0f)
                {
                    _accumulatedSpeedAdded = 0f;
                }
            }
            else
            {
                _accumulatedSpeedAdded = 0f;
            }

            if(_currentPowerUp != PowerUpType.E_NONE)
            {
                var powerUpDelta = (TimeUtils.TimestampMilliseconds - _powerUpStartTime) / (float)PowerUpTimes[(int)_currentPowerUp];

                if(_sceneManager.PowerUpTime != null)
                {
                    _sceneManager.PowerUpTime.SetDelta(powerUpDelta);
                }

                if(TimeUtils.TimestampMilliseconds > _powerUpStartTime + PowerUpTimes[(int)_currentPowerUp])
                {
                    ResetPowerUp();
                }
            }

            if(_currentPowerUp == PowerUpType.E_INVINCIBLE)
            {
                if(_playerMaterials.Count == 0)
                {
                    Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
                    if(renderers != null)
                    {
                        for(var j = 0; j < renderers.Length; ++j)
                        {
                            _playerMaterials.Add(renderers[j].material);
                        }
                    }
                }

                for(var i = 0; i < _playerMaterials.Count; ++i)
                {
                    _colorTemp = Color.white;
                    _colorTemp.a = 0.85f + (0.25f * Mathf.Sin(Time.time * 16.0f));

                    _playerMaterials[i].SetColor("_Color", _colorTemp);
                }
            }

            if(_currentPowerUp == PowerUpType.E_ANGRY)
            {
                if(HeadBCSH != null)
                {
                    float delta = (TimeUtils.TimestampMilliseconds - _lastAngryTurnTime) / (float)_currentAngryRandomTime;
                    if(delta > 0.75f && HeadBCSH.CurrentAppliedBCSHState == 0)
                    {
                        HeadBCSH.ApplyBCSHState("angry");
                    }
                }

                if(TimeUtils.TimestampMilliseconds > _lastAngryTurnTime + _currentAngryRandomTime)
                {
                    if(_playerState != PlayerState.E_DAMAGED && _playerState != PlayerState.E_DAMAGED_FALL)
                    {
                        Turn();
                    }

                    if(HeadBCSH != null)
                    {
                        HeadBCSH.ApplyBCSHState("default");
                    }

                    _lastAngryTurnTime = TimeUtils.TimestampMilliseconds;
                    _currentAngryRandomTime = RandomUtils.Range(1000, 3500);
                }
            }

            if(_damageInvulnerable)
            {
                if(TimeUtils.TimestampMilliseconds > _damageInvulnerableStartTime + kDamageInvulnerableMaxMillis)
                {
                    _damageInvulnerable = false;
                }

                if(_playerMaterials.Count == 0)
                {
                    Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
                    if(renderers != null)
                    {
                        for(var j = 0; j < renderers.Length; ++j)
                        {
                            _playerMaterials.Add(renderers[j].material);
                        }
                    }
                }

                for(var i = 0; i < _playerMaterials.Count; ++i)
                {
                    if(!_damageInvulnerable)
                    {
                        _colorTemp = Color.white;
                        _colorTemp.a = 1.0f;
                    }
                    else
                    {
                        _colorTemp = Color.red;
                        _colorTemp.g = 0.5f;
                        _colorTemp.b = 0.5f;
                        _colorTemp.a = 0.85f + (0.25f * Mathf.Sin(Time.time * 16.0f));
                    }

                    _playerMaterials[i].SetColor("_Color", _colorTemp);
                }
            }

            if(_sceneManager != null)
            {
                _sceneManager.CheckMapGeneration();
            }

            if(!_holding)
            {
                if(_pressedDown
                    && (_playerState == PlayerState.E_WALKING
                        || _playerState == PlayerState.E_JUMPING
                        || _playerState == PlayerState.E_JUMPING_FALL))
                {
                    _holding = true;
                    _holdingStartTime = TimeUtils.TimestampMilliseconds;
                    _memoryJump = false;
                }
            }

            if(!_pressedDown && _pressedUp)
            {
                _holding = false;

                if(TimeUtils.TimestampMilliseconds <= _holdingStartTime + kHoldingJumpMaxMillis)
                {
                    var canJumpAgain = false;
                    if(_currentPowerUp == PowerUpType.E_DOUBLE_JUMP && !_hasDoubleHump)
                    {
                        if(_playerState == PlayerState.E_JUMPING || _playerState == PlayerState.E_JUMPING_FALL)
                        {
                            _hasDoubleHump = true;
                            canJumpAgain = true;
                        }
                    }

                    if(_playerState == PlayerState.E_WALKING || _playerState == PlayerState.E_STOPPED || canJumpAgain)
                    {
                        Jump();
                    }

                    if(_playerState == PlayerState.E_JUMPING_FALL)
                    {
                        _memoryJump = true;
                    }
                }
                else
                {
                    if(_playerState == PlayerState.E_STOPPED)
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
                        if(_playerState == PlayerState.E_WALKING)
                        {
                            Stop();
                        }
                    }
                }
            }

            var dist = 0f;
            if(GetHitDistance(out dist, out _hitDown, transform.position, -Vector3.up, 0.1f))
            {
                if(_playerState == PlayerState.E_JUMPING_FALL || _playerState == PlayerState.E_DAMAGED_FALL)
                {
                    if(_rigidBody != null)
                    {
                        _rigidBody.velocity = Vector3.zero;
                    }

                    if(_playerState == PlayerState.E_DAMAGED_FALL)
                    {
                        AfterDamage();
                    }

                    if(_sceneManager.CurrentBattleState == CP_SceneManager.BattleState.E_PLAYING)
                    {
                        if(_playerState == PlayerState.E_JUMPING_FALL && _memoryJump)
                        {
                            _hasDoubleHump = false;
                            _memoryJump = false;

                            Jump();
                        }
                        else
                        {
                            Walk();
                        }
                    }
                }
            }
            else
            {
                if((_playerState == PlayerState.E_JUMPING_FALL || _playerState == PlayerState.E_DAMAGED_FALL)
                    && _rigidBody.velocity.y == 0.0f)
                {
                    if(_rigidBody != null)
                    {
                        _rigidBody.velocity = Vector3.zero;
                    }

                    if(_playerState == PlayerState.E_DAMAGED_FALL)
                    {
                        AfterDamage();
                    }

                    if(_sceneManager.CurrentBattleState == CP_SceneManager.BattleState.E_PLAYING)
                    {
                        if(_playerState == PlayerState.E_JUMPING_FALL && _memoryJump)
                        {
                            _hasDoubleHump = false;
                            _memoryJump = false;

                            Jump();
                        }
                        else
                        {
                            Walk();
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
        }

        if(_sceneManager.CurrentBattleState == CP_SceneManager.BattleState.E_PLAYING || _sceneManager.CurrentBattleState == CP_SceneManager.BattleState.E_WIN)
        {
            var dist = 0f;

            _vectTemp.x = 0.0f;
            _vectTemp.y = 0.4f;
            _vectTemp.z = 0.0f;
            if (!GetHitDistance(out dist, out _hitForward, transform.position + _vectTemp, _direction, 1.25f))
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
        }

        if(_gameCamera != null)
        {
            _vectTemp.x = transform.position.x + (2.2f);
            _vectTemp.y = _gameCamera.transform.position.y;
            _vectTemp.z = _gameCamera.transform.position.z;

            _gameCamera.transform.position = _vectTemp;
        }

        _pressedDown = false;
        _pressedUp = false;
    }
}
