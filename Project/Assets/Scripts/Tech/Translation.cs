
using System;
using SocialPoint.Utils;
using UnityEngine;

public class Translation : MonoBehaviour
{
    public Vector3 OriginPosition;
    public Vector3 DestPosition;
    public int TimeTravel;
    public Action TimeFinishedCallBack;

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
        if(delta > 1f)
            delta = 1f;

        transform.position = OriginPosition + (_diffSpace * delta);

        if(TimeUtils.TimestampMilliseconds > _startTime + TimeTravel)
        {
            if(TimeFinishedCallBack != null)
            {
                TimeFinishedCallBack();
            }
        }
    }
}
