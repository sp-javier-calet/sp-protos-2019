using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Animations
{
    public class TimeProfiler
    {
        class AverageData
        {
            public float TotalReal;
            public float TotalDelta;
            public int Amount;
        }

        float _startTimeStamp;
        float _deltaTime;
        float _lastRealTime;
        string _from;
        Dictionary<string, AverageData> _average = new Dictionary<string, AverageData>();

        public void Start(string fromMsg)
        {
            _deltaTime = 0f;
            _startTimeStamp = Time.realtimeSinceStartup;
            _lastRealTime = _startTimeStamp;
            _from = fromMsg;
        }

        public void End(string toMsg)
        {
            float currentRealTime = Time.realtimeSinceStartup;
            float elapsedReal = currentRealTime - _lastRealTime;

            string key = _from + " - " + toMsg;
            AverageData average = null;
            if(!_average.TryGetValue(key, out average))
            {
                average = new AverageData {
                    TotalReal = 0f,
                    TotalDelta = 0f,
                    Amount = 0,
                };
                _average.Add(key, average);
            }
            average.TotalReal += elapsedReal;
            average.TotalDelta += _deltaTime;
            average.Amount += 1;
            float averageReal = average.TotalReal / average.Amount;
            float averageDelta = average.TotalDelta / average.Amount;

            Debug.Log(key + ". Start: " + _startTimeStamp + ". Real: " + elapsedReal + " - Delta: " + _deltaTime + ". AvReal: " + averageReal + " - AvDelta: " + averageDelta);
            Start(toMsg);
        }

        public void Update(float dt)
        {
            _deltaTime += dt;
        }
    }
}