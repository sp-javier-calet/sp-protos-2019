using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public enum UpdateableTimeMode
    {
        GameTimeScaled,
        GameTimeUnscaled,
        RealTime
    }

    public enum UpdateableTimeIntMode
    {
        Ticks,
        GameTimeScaled,
        GameTimeUnscaled,
        RealTime
    }

    public interface IUpdateable
    {
        void Update();
    }

    public interface IDeltaUpdateable<T>
    {
        void Update(T elapsed);
    }

    public interface IDeltaUpdateable : IDeltaUpdateable<float>
    {
    }

    public interface ICoroutineRunner
    {
        IEnumerator StartCoroutine(IEnumerator enumerator);

        void StopCoroutine(IEnumerator enumerator);
    }

    public interface IUpdateScheduler
    {
        void Add(IUpdateable elm, UpdateableTimeMode mode = UpdateableTimeMode.GameTimeUnscaled, float interval = 0.0f);

        void Remove(IUpdateable elm);

        bool Contains(IUpdateable elm);

        void Add(IDeltaUpdateable elm, UpdateableTimeMode mode = UpdateableTimeMode.GameTimeUnscaled, float interval = 0.0f);

        void Remove(IDeltaUpdateable elm);

        bool Contains(IDeltaUpdateable elm);

        void Add(IDeltaUpdateable<int> elm, UpdateableTimeIntMode mode = UpdateableTimeIntMode.GameTimeUnscaled, int interval = 0);

        void Remove(IDeltaUpdateable<int> elm);

        bool Contains(IDeltaUpdateable<int> elm);
    }

    public static class UpdateSchedulerExtension
    {
        public static void Add(this IUpdateScheduler scheduler, IEnumerable<IUpdateable> elements = null)
        {
            if(elements != null)
            {
                var itr = elements.GetEnumerator();
                while(itr.MoveNext())
                {
                    var elm = itr.Current;
                    scheduler.Add(elm);
                }
                itr.Dispose();
            }
        }

        [Obsolete("Use Add instead")]
        public static void AddFixed(this IUpdateScheduler scheduler, IUpdateable elm, double interval, UpdateableTimeMode updateTimeMode = UpdateableTimeMode.GameTimeUnscaled)
        {
            scheduler.Add(elm, updateTimeMode, (float)interval);
        }

        public static void Add(this IUpdateScheduler scheduler, IUpdateable elm, float interval)
        {
            scheduler.Add(elm, UpdateableTimeMode.GameTimeUnscaled, interval);
        }

        public static void Add(this IUpdateScheduler scheduler, IDeltaUpdateable elm, float interval)
        {
            scheduler.Add(elm, UpdateableTimeMode.GameTimeUnscaled, interval);
        }

        public static void Add(this IUpdateScheduler scheduler, IDeltaUpdateable<int> elm, int interval)
        {
            scheduler.Add(elm, UpdateableTimeIntMode.GameTimeUnscaled, interval);
        }
    }

    public struct UpdateableTimeModeComparer : IEqualityComparer<UpdateableTimeMode>
    {
        public bool Equals(UpdateableTimeMode x, UpdateableTimeMode y)
        {
            return x == y;
        }

        public int GetHashCode(UpdateableTimeMode obj)
        {
            return (int)obj;
        }
    }

    public struct UpdateableTimeIntModeComparer : IEqualityComparer<UpdateableTimeIntMode>
    {
        public bool Equals(UpdateableTimeIntMode x, UpdateableTimeIntMode y)
        {
            return x == y;
        }

        public int GetHashCode(UpdateableTimeIntMode obj)
        {
            return (int)obj;
        }
    }

    public sealed class UpdateScheduler : IUpdateScheduler
    {
        readonly Dictionary<Object, IUpdateableHandler> _elements;
        readonly Dictionary<Object, IUpdateableHandler> _elementsToAdd;
        readonly List<Object> _elementsToRemove;
        readonly Dictionary<UpdateableTimeMode, IDeltaTimeSource<float>> _floatSources;
        readonly Dictionary<UpdateableTimeIntMode, IDeltaTimeSource<int>> _intSources;
        bool _dirty;

        readonly List<Exception> _exceptions = new List<Exception>();

        double _lastUpdateTimestamp;
        long _lastUpdateTimestampMillis;

        public UpdateScheduler()
        {
            _elements = new Dictionary<Object, IUpdateableHandler>();
            _elementsToAdd = new Dictionary<Object, IUpdateableHandler>();
            _elementsToRemove = new List<object>();
            _floatSources = new Dictionary<UpdateableTimeMode, IDeltaTimeSource<float>>(new UpdateableTimeModeComparer());
            _intSources = new Dictionary<UpdateableTimeIntMode, IDeltaTimeSource<int>>(new UpdateableTimeIntModeComparer());
        }

        IUpdateableTimer<int> CreateTimer(int interval)
        {
            if(interval <= 0)
            {
                return new ContinuousTimer<int>();
            }
            else
            {
                return new FixedIntTimer(interval);
            }
        }

        IUpdateableTimer<float> CreateTimer(float interval)
        {
            if(interval <= 0.0f)
            {
                return new ContinuousTimer<float>();
            }
            else
            {
                return new FixedFloatTimer(interval);
            }
        }

        IDeltaTimeSource<float> GetSource(UpdateableTimeMode mode)
        {
            IDeltaTimeSource<float> source;
            if(!_floatSources.TryGetValue(mode, out source))
            {
                source = CreateSource(mode);
                _floatSources.Add(mode, source);
            }
            return source;
        }


        IDeltaTimeSource<int> GetSource(UpdateableTimeIntMode mode)
        {
            IDeltaTimeSource<int> source;
            if(!_intSources.TryGetValue(mode, out source))
            {
                source = CreateSource(mode);
                _intSources.Add(mode, source);
            }
            return source;
        }

        IDeltaTimeSource<float> CreateSource(UpdateableTimeMode mode)
        {
            switch(mode)
            {
            case UpdateableTimeMode.GameTimeScaled:
                return new ScaledDeltaTimeSource();
            case UpdateableTimeMode.RealTime:
                return new RealDeltaTimeSource();
            case UpdateableTimeMode.GameTimeUnscaled:
                return new UnscaledDeltaTimeSource();
            }
            throw new InvalidOperationException("Unsupported time mode.");
        }

        IDeltaTimeSource<int> CreateSource(UpdateableTimeIntMode mode)
        {
            switch(mode)
            {
            case UpdateableTimeIntMode.Ticks:
                return new TicksDeltaTimeSource();
            case UpdateableTimeIntMode.GameTimeScaled:
                return new ScaledMillisDeltaTimeSource();
            case UpdateableTimeIntMode.RealTime:
                return new RealMillisDeltaTimeSource();
            case UpdateableTimeIntMode.GameTimeUnscaled:
                return new UnscaledMillisDeltaTimeSource();
            }
            throw new InvalidOperationException("Unsupported time mode.");
        }

        public void Add(IUpdateable elm, UpdateableTimeMode mode = UpdateableTimeMode.GameTimeUnscaled, float interval = 0.0f)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                var source = GetSource(mode);
                var timer = CreateTimer(interval);
                var handler = new UpdateableHandler<float>(elm, timer, source);
                DoAdd(elm, handler);
            }
        }

        public void Add(IDeltaUpdateable elm, UpdateableTimeMode mode = UpdateableTimeMode.GameTimeUnscaled, float interval = 0.0f)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                var source = GetSource(mode);
                var timer = CreateTimer(interval);
                var handler = new DeltaUpdateableHandler<float>(elm, timer, source);
                DoAdd(elm, handler);
            }
        }

        public void Add(IDeltaUpdateable<int> elm, UpdateableTimeIntMode mode = UpdateableTimeIntMode.GameTimeUnscaled, int interval = 0)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                var source = GetSource(mode);
                var timer = CreateTimer(interval);
                var handler = new DeltaUpdateableHandler<int>(elm, timer, source);
                DoAdd(elm, handler);
            }
        }

        void DoAdd(Object elm, IUpdateableHandler handler)
        {
            _elementsToRemove.Remove(elm);
            _elementsToAdd[elm] = handler;
            _dirty = true;
        }

        public void Remove(IUpdateable elm)
        {
            DoRemove(elm);
        }

        public void Remove(IDeltaUpdateable elm)
        {
            DoRemove(elm);
        }

        public void Remove(IDeltaUpdateable<int> elm)
        {
            DoRemove(elm);
        }

        void DoRemove(object elm)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                _elementsToRemove.Add(elm);
                _elementsToAdd.Remove(elm);
                _dirty = true;
            }
        }

        public bool Contains(IUpdateable elm)
        {
            return DoContains(elm);
        }

        public bool Contains(IDeltaUpdateable elm)
        {
            return DoContains(elm);
        }

        public bool Contains(IDeltaUpdateable<int> elm)
        {
            return DoContains(elm);
        }

        bool DoContains(object elm)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                if(_dirty)
                {
                    if(_elementsToRemove.Contains(elm))
                    {
                        return false;
                    }

                    if(_elementsToAdd.ContainsKey(elm))
                    {
                        return true;
                    }
                }

                return _elements.ContainsKey(elm);
            }

            return false;
        }

        void Synchronize()
        {
            if(_dirty)
            {
                // Remove pending elements
                {
                    var itr = _elementsToRemove.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        _elements.Remove(itr.Current);
                    }
                    itr.Dispose();
                    _elementsToRemove.Clear();
                }

                // Add pending elements
                {
                    var itr = _elementsToAdd.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var current = itr.Current;
                        _elements[current.Key] = current.Value;
                    }
                    itr.Dispose();
                    _elementsToAdd.Clear();
                }

                _dirty = false;
            }
        }

        public void Update(float scaledDt, float unscaledDt)
        {
            // Sync for external changes
            Synchronize();

            _exceptions.Clear();

            UpdateSources(scaledDt, unscaledDt);
            UpdateHandlers();

            // Check new exceptions
            CompoundException.Trigger(_exceptions);

            // Sync for internal changes during update
            Synchronize();
        }

        void UpdateSources(float scaledDelta, float unscaledDelta)
        {
            {
                var itr = _floatSources.GetEnumerator();
                while(itr.MoveNext())
                {
                    itr.Current.Value.Update(scaledDelta, unscaledDelta);
                }
                itr.Dispose();
            }
            {
                var itr = _intSources.GetEnumerator();
                while(itr.MoveNext())
                {
                    itr.Current.Value.Update(scaledDelta, unscaledDelta);
                }
                itr.Dispose();
            }
        }

        void UpdateHandlers()
        {
            var itr = _elements.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                try
                {
                    elm.Value.Update();
                }
                catch(Exception e)
                {
                    _exceptions.Add(e);
                }
            }
            itr.Dispose();
        }

        #region TimeSources

        interface IDeltaTimeSource<T>
        {
            void Update(float scaledDelta, float unscaledDelta);

            T Value { get; }
        }

        class ScaledDeltaTimeSource : IDeltaTimeSource<float>
        {
            public void Update(float scaledDelta, float unscaledDelta)
            {
                Value = scaledDelta;
            }

            public float Value { get; private set; }
        }

        class UnscaledDeltaTimeSource : IDeltaTimeSource<float>
        {
            public void Update(float scaledDelta, float unscaledDelta)
            {
                Value = unscaledDelta;
            }

            public float Value { get; private set; }
        }

        class RealDeltaTimeSource : IDeltaTimeSource<float>
        {
            double _timestamp;

            public void Update(float scaledDelta, float unscaledDelta)
            {
                var ts = TimeUtils.GetTimestampDouble(DateTime.Now);
                if(_timestamp < double.Epsilon)
                {
                    _timestamp = ts;
                }
                Value = (float)(ts - _timestamp);
                _timestamp = ts;
            }

            public float Value { get; private set; }
        }

        class TicksDeltaTimeSource : IDeltaTimeSource<int>
        {
            long _ticks;

            public void Update(float scaledDelta, float unscaledDelta)
            {
                var ticks = DateTime.Now.Ticks;
                if(_ticks < double.Epsilon)
                {
                    _ticks = ticks;
                }
                Value = (int)(ticks - _ticks);
                _ticks = ticks;
            }

            public int Value { get; private set; }
        }

        class RealMillisDeltaTimeSource : IDeltaTimeSource<int>
        {
            long _timestamp;

            public void Update(float scaledDelta, float unscaledDelta)
            {
                var ts = TimeUtils.GetTimestampMilliseconds(DateTime.Now);
                if(_timestamp <= 0)
                {
                    _timestamp = ts;
                }
                Value = (int)(ts - _timestamp);
                _timestamp = ts;
            }

            public int Value { get; private set; }
        }

        class ScaledMillisDeltaTimeSource : IDeltaTimeSource<int>
        {
            public void Update(float scaledDelta, float unscaledDelta)
            {
                Value = (int)((scaledDelta + 0.0005f) * 1000.0f);
            }

            public int Value { get; private set; }
        }

        class UnscaledMillisDeltaTimeSource : IDeltaTimeSource<int>
        {
            public void Update(float scaledDelta, float unscaledDelta)
            {
                Value = (int)((unscaledDelta + 0.0005f) * 1000.0f);
            }

            public int Value { get; private set; }
        }

        #endregion

        #region TimeHandlers

        public interface IUpdateableTimer<T>
        {
            bool Step(T delta, out T interval);
        }

        public class ContinuousTimer<T> : IUpdateableTimer<T>
        {
            public bool Step(T delta, out T interval)
            {
                interval = delta;
                return true;
            }
        }

        public class FixedFloatTimer : IUpdateableTimer<float>
        {
            readonly float _interval;
            float _current;

            public FixedFloatTimer(float interval)
            {
                _current = 0.0f;
                _interval = interval;
            }

            public bool Step(float delta, out float interval)
            {
                _current += delta;
                interval = 0.0f;
                if(_current >= _interval)
                {
                    _current = _current - _interval;
                    interval = _interval;
                    return true;
                }
                return false;
            }
        }

        public struct FixedIntTimer : IUpdateableTimer<int>
        {
            readonly int _interval;
            int _current;

            public FixedIntTimer(int interval)
            {
                _current = 0;
                _interval = interval;
            }

            public bool Step(int delta, out int interval)
            {
                _current += delta;
                interval = 0;
                if(_current >= _interval)
                {
                    _current = _current - _interval;
                    interval = _interval;
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region UpdateableHandlers

        interface IUpdateableHandler
        {
            void Update();
        }

        class UpdateableHandler<T> : IUpdateableHandler
        {
            readonly IUpdateableTimer<T> _timer;
            readonly IUpdateable _updateable;
            readonly IDeltaTimeSource<T> _source;

            public UpdateableHandler(IUpdateable updateable, IUpdateableTimer<T> timer, IDeltaTimeSource<T> source)
            {
                _updateable = updateable;
                _timer = timer;
                _source = source;
            }

            public void Update()
            {
                T interval;
                if(_timer.Step(_source.Value, out interval))
                {
                    _updateable.Update();
                }
            }
        }

        class DeltaUpdateableHandler<T> : IUpdateableHandler
        {
            readonly IUpdateableTimer<T> _timer;
            readonly IDeltaUpdateable<T> _updateable;
            readonly IDeltaTimeSource<T> _source;

            public DeltaUpdateableHandler(IDeltaUpdateable<T> updateable, IUpdateableTimer<T> timer, IDeltaTimeSource<T> source)
            {
                _updateable = updateable;
                _timer = timer;
                _source = source;
            }

            public void Update()
            {
                T interval;
                if(_timer.Step(_source.Value, out interval))
                {
                    _updateable.Update(interval);
                }
            }
        }

        #endregion
    }
}
