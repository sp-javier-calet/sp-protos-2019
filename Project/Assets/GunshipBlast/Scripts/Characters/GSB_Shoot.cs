
using SocialPoint.Utils;
using UnityEngine;

public class GSB_Shoot : MonoBehaviour
{
    public Vector3 OriginPosition;
    public Vector3 DestPosition;
    public int TimeTravel;

    long _startTime;
    Vector3 _diffSpace;

    void Start()
    {
        _startTime = TimeUtils.TimestampMilliseconds;
        _diffSpace = DestPosition - OriginPosition;
    }

    void Update()
    {
        float delta = (TimeUtils.TimestampMilliseconds - _startTime) / (float)TimeTravel;

        transform.position = OriginPosition + (_diffSpace * delta);

        if(TimeUtils.TimestampMilliseconds > _startTime + TimeTravel)
        {
            Destroy(gameObject);
        }
    }
}
