using UnityEngine;
using SocialPoint.Pooling;

public class ExplosionBehaviour : MonoBehaviour, IRecyclable
{
    [SerializeField]
    float _lifetime = 0.5f;

    float _timeSinceSpawn = 0.0f;

    public void OnSpawn()
    {
        _timeSinceSpawn = 0.0f;
    }

    public void OnRecycle()
    {
        _timeSinceSpawn = 0.0f;
    }

    void Update()
    {
        _timeSinceSpawn += Time.deltaTime;
        if(_timeSinceSpawn >= _lifetime)
        {
            UnityObjectPool.Recycle(gameObject);
        }
    }
}
