using UnityEngine;
using System.Collections.Generic;

public abstract class CircularBuffer<T>
{
    List<T> _values = new List<T>();
    int _bufferSize = 50;
    int _count = 0;

    bool _isLoaded = false;
    void Load()
    {
        if(!_isLoaded)
        {
            _isLoaded = true;
            for(int i = 0; i < _bufferSize; ++i)
            {
                _values.Add(default(T));
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

    public T GetValue(int flatIdx)
    {
        return _values[GetCircularIdx(flatIdx)];
    }

    int GetCircularIdx(int flatIdx)
    {
        return flatIdx % _bufferSize;
    }

    public void Add(T value)
    {
        Load();

        _values[CircularCount] = value;
        _count++;
    }

    public T GetSum(int startFlatIdx, int endFlatIdx)
    {
        startFlatIdx = Mathf.Max(startFlatIdx, 0);
        endFlatIdx = Mathf.Min(endFlatIdx, Count);

        T accValue = default(T);
        int count = endFlatIdx - startFlatIdx;
        for(int flatIdx = 0; flatIdx < count; ++flatIdx)
        {
            accValue = Sum(accValue, GetValue(startFlatIdx + flatIdx));
        }

        return accValue;
    }

    public float GetAvg(int lastSamplesCount)
    {
        int startFlatIdx = Mathf.Max(Count - lastSamplesCount, 0);
        int count = Count - startFlatIdx;
        if(count == 0)
        {
            return 0f;
        }

        T accValue = default(T);
        for(int flatIdx = 0; flatIdx < count; ++flatIdx)
        {
            Sum(accValue, GetValue(startFlatIdx + flatIdx));
        }

        //return ((float)accValue) / ((float)count);
        return AvgToFloat(accValue, count);
    }

    protected abstract T Sum(T a, T b);
    protected abstract float AvgToFloat(T a, int count);
}

public class IntCircularBuffer : CircularBuffer<int>
{
    protected override int Sum(int a, int b)
    {
        return a + b;
    }


    protected override float AvgToFloat(int a, int b)
    {
        return ((float)a) / ((float)b);
    }
}

public class FloatCircularBuffer : CircularBuffer<float>
{
    protected override float Sum(float a, float b)
    {
        return a + b;
    }


    protected override float AvgToFloat(float a, int b)
    {
        return a / ((float)b);
    }
}

