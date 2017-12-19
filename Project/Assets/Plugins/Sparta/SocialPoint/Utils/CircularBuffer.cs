using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public abstract class CircularBuffer<T>
    {
        protected T[] buffer;
        protected int count = 0;

        public CircularBuffer(int size)
        {
            buffer = new T[size];
            Reset();
        }

        public int Count
        {
            get{ return count > buffer.Length ? buffer.Length : count; }
        }

        public void Reset()
        {
            count = 0;
        }

        public void Add(T item)
        {
            int idx = count % buffer.Length;
            buffer[idx] = item;
            count++;
        }

        public T GetItemAt(int index)
        {
            int idx = index % buffer.Length;
            return buffer[idx];
        }

        public abstract T Zero();

        public abstract float Magnitude(T a);

        public abstract T Add(T a, T b);

        public abstract T Divide(T a, float b);

        public float GetMagnitude()
        {
            return GetMagnitude(Count);
        }
        
        public float GetMagnitude(int nSamples)
        {
            float mag = 0.0f;
            
            int samples = Math.Min(Count, nSamples);
            
            if(samples == 0)
            {
                return mag;
            }
            
            for(int i = 1; i <= samples; i++)
            {
                T item = Zero();
                float itemMag = 0.0f;
                
                int currentIdx = count - i;
                if(currentIdx >= 0)
                {
                    int idx = currentIdx % buffer.Length;
                    
                    item = buffer[idx];
                    itemMag = Magnitude(item);
                }
                
                mag = mag + itemMag;
            }
            
            return mag;
        }

        public float GetAverageMagnitude()
        {
            return GetAverageMagnitude(Count);
        }

        public float GetAverageMagnitude(int nSamples)
        {
            float avg = 0.0f;
            
            int samples = Math.Min(Count, nSamples);
            
            if(samples == 0)
            {
                return avg;
            }
            
            for(int i = 1; i <= samples; i++)
            {
                T item = Zero();
                float itemMag = 0.0f;
                
                int currentIdx = count - i;
                if(currentIdx >= 0)
                {
                    int idx = currentIdx % buffer.Length;

                    item = buffer[idx];
                    itemMag = Magnitude(item);
                }
                
                avg = avg + itemMag;
            }
            
            return avg / (float)samples;
        }

        public T GetAverage()
        {
            return GetAverage(Count);
        }

        public T GetAverage(int nSamples)
        {
            T avg = default(T);

            int samples = Math.Min(Count, nSamples);

            if(samples == 0)
            {
                return avg;
            }

            for(int i = 1; i <= samples; i++)
            {
                T item = Zero();

                int currentIdx = count - i;
                if(currentIdx >= 0)
                {
                    int idx = currentIdx % buffer.Length;
                    item = buffer[idx];
                }

                avg = Add(avg, item);
            }
            
            return Divide(avg, samples);
        }
    }

    public sealed class FloatCircularBuffer : CircularBuffer<float>
    {
        public FloatCircularBuffer(int size) : base(size)
        {
        }

        public override float Zero()
        {
            return 0.0f;
        }

        public override float Magnitude(float a)
        {
            return Mathf.Abs(a);
        }

        public override float Add(float a, float b)
        {
            return a + b;
        }

        public override float Divide(float a, float b)
        {
            return a / b;
        }
    }

    public sealed class IntCircularBuffer : CircularBuffer<int>
    {
        public IntCircularBuffer(int size) : base(size)
        {
        }

        public override int Zero()
        {
            return 0;
        }

        public override float Magnitude(int a)
        {
            return ((float)Mathf.Abs(a));
        }

        public override int Add(int a, int b)
        {
            return a + b;
        }

        public override int Divide(int a, float b)
        {
            return a / ((int)b);
        }
    }

    public sealed class Vector2CircularBuffer : CircularBuffer<Vector2>
    {
        public Vector2CircularBuffer(int size) : base(size)
        {
        }
            
        public override Vector2 Zero()
        {
            return Vector2.zero;
        }

        public override float Magnitude(Vector2 a)
        {
            return a.magnitude;
        }

        public override Vector2 Add(Vector2 a, Vector2 b)
        {
            return a + b;
        }

        public override Vector2 Divide(Vector2 a, float b)
        {
            return a / b;
        }
    }

    public sealed class Vector3CircularBuffer : CircularBuffer<Vector3>
    {
        public Vector3CircularBuffer(int size) : base(size)
        {
        }

        public override Vector3 Zero()
        {
            return Vector3.zero;
        }

        public override float Magnitude(Vector3 a)
        {
            return a.magnitude;
        }

        public override Vector3 Add(Vector3 a, Vector3 b)
        {
            return a + b;
        }

        public override Vector3 Divide(Vector3 a, float b)
        {
            return a / b;
        }
    }
}
