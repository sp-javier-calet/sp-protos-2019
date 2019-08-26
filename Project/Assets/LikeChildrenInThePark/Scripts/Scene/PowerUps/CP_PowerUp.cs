
using DG.Tweening;
using SocialPoint.Utils;
using TMPro;
using UnityEngine;

public class CP_PowerUp : MonoBehaviour
{
    public int PowerUpType = -1;
    public GameObject PowerUpObject;
    public Renderer PowerUpRenderer;
    public TextMeshPro PowerUpText;

    const int TakenMillis = 5000;

    long _takenTimeStamp = -1;
    bool _taken = false;
    BoxCollider _collider = null;
    RaycastHit _hitDown;
    bool _detectedCollision = false;

    void Awake()
    {
        if(PowerUpObject != null)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(PowerUpObject.transform.DOLocalRotate(new Vector3(0f, -15f, 0f), 1f, RotateMode.Fast));
            seq.SetLoops(-1, LoopType.Yoyo);
        }

        _collider = GetComponent<BoxCollider>();

        if(PowerUpText != null)
        {
            PowerUpText.enabled = false;
            PowerUpText.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        }
    }

    bool GetHitDistance(out float distance, out RaycastHit hit, Vector3 initPosition, Vector3 direction,
        float maxDistance = 0.0001f)
    {
        distance = 0f;

        int layerMask = 1 << 10;
        layerMask += 1 << 11;

        Ray downRay = new Ray(initPosition, direction);
        if(Physics.Raycast(downRay, out hit, maxDistance, layerMask))
        {
            distance = hit.distance;
            return true;
        }

        return false;
    }

    public void SetPosition(int powerUpPos)
    {
        transform.localPosition = new Vector3(-4f + (powerUpPos * 2f), 15f, -1.6f);

        _detectedCollision = false;
    }

    public void Taken()
    {
        _taken = true;
        _takenTimeStamp = TimeUtils.TimestampMilliseconds;

        if(_collider != null)
        {
            _collider.enabled = false;
        }

        if(PowerUpRenderer != null)
        {
            PowerUpRenderer.material.SetColor("_Color", new Color(1f, 1f, 1f, 0.3f));
        }

        if(PowerUpText != null)
        {
            PowerUpText.enabled = true;

            Sequence seq = DOTween.Sequence();
            seq.Append(PowerUpText.transform.DOLocalMove(new Vector3(0f, 0.26f, 0f), 2f));
            seq.onComplete += OnTextMoveFinished;
        }
    }

    void OnTextMoveFinished()
    {
        if(PowerUpText != null)
        {
            PowerUpText.enabled = false;
            PowerUpText.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        }
    }

    void Update()
    {
        if(!_detectedCollision)
        {
            var dist = 0f;
            if(GetHitDistance(out dist, out _hitDown, (transform.position + new Vector3(0f, 0f, 1.6f)), -Vector3.up, 50.0f))
            {
                transform.position = new Vector3(transform.position.x,_hitDown.transform.position.y + 4f, transform.position.z);

                _detectedCollision = true;
            }
        }

        if(_taken)
        {
            if(TimeUtils.TimestampMilliseconds > _takenTimeStamp + TakenMillis)
            {
                _taken = false;

                if(PowerUpRenderer != null)
                {
                    PowerUpRenderer.material.SetColor("_Color", new Color(1f, 1f, 1f, 1.0f));
                }

                if(_collider != null)
                {
                    _collider.enabled = true;
                }
            }
        }
    }
}
