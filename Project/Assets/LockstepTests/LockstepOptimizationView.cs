using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Lockstep;
using UnityEngine.UI;

public class IntCircularBuffer
{
    public static readonly IntCircularBuffer Instance = new IntCircularBuffer();
    
    List<int> _values = new List<int>();
    int _bufferSize = 50;
    int _count = 0;

    bool _isLoaded = false;
    void Load()
    {
        if(!_isLoaded)
        {
            _isLoaded = true;
            for(int i = 0; i < 50; ++i)
            {
                _values.Add(0);
            }
        }
    }

    int CircularCount
    {
        get
        { 
            return GetCircularIdx(_count);
        }
    }

    public int Count
    {
        get
        {
            return _count;
        }
    }

    public int GetValue(int flatIdx)
    {
        return _values[GetCircularIdx(flatIdx)];
    }

    int GetCircularIdx(int flatIdx)
    {
        return _count % _bufferSize;
    }

    public void Add(int value)
    {
        _values[CircularCount] = value;
    }

    public float GetAverage(int startFlatIdx, int endFlatIdx)
    {
        int accValue = 0;
        int count = endFlatIdx - startFlatIdx;
        for(int flatIdx = 0; flatIdx < count; ++flatIdx)
        {
            accValue += GetValue(flatIdx);
        }

        return count > 0 ? ((float)accValue) / ((float)count) : 0;
    }
}

public class LockstepOptimizationView : MonoBehaviour 
{
    float _averageBytesSent = 0;
    
    [SerializeField]
    Text _averageBytesSentText;

    void Awake()
    {
        RefreshUI();
        StartCoroutine(ShowSendBytesCo());
    }

    public void OnEnableClientSendTurn()
    {
        RefreshUI();
    }

    public void OnEnableServerSendTurn()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        _averageBytesSentText.text = (8*_averageBytesSent).ToString();
    }

    IEnumerator ShowSendBytesCo()
    {
        int lastIdx = 0;
        while(true)
        {
            yield return new WaitForSeconds(1f);

            int currentIdx = IntCircularBuffer.Instance.Count;
            _averageBytesSent = IntCircularBuffer.Instance.GetAverage(lastIdx, currentIdx);
            lastIdx = currentIdx;

            RefreshUI();
        }
    }
}
