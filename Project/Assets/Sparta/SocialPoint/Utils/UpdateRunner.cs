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

    public interface IUpdateable
    {
        void Update();
    }

    public interface IDeltaUpdateable
    {
        void Update(float elapsed);
    }

    public interface ICoroutineRunner
    {
        IEnumerator StartCoroutine(IEnumerator enumerator);

        void StopCoroutine(IEnumerator enumerator);
    }

    public interface IUpdateScheduler
    {
        void Add(IUpdateable elm, UpdateableTimeMode updateTimeMode, float interval);

        void Remove(IUpdateable elm);

        bool Contains(IUpdateable elm);

        void Add(IDeltaUpdateable elm, UpdateableTimeMode updateTimeMode, float interval);

        void Remove(IDeltaUpdateable elm);

        bool Contains(IDeltaUpdateable elm);

        event Action<Exception> UpdateExceptionThrown;
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

        public static void Add(this IUpdateScheduler scheduler, IUpdateable elm)
        {
            scheduler.Add(elm, UpdateableTimeMode.GameTimeUnscaled, -1.0f);
        }

        public static void Add(this IUpdateScheduler scheduler, IUpdateable elm, float interval)
        {
            scheduler.Add(elm, UpdateableTimeMode.GameTimeUnscaled, interval);
        }

        public static void Add(this IUpdateScheduler scheduler, IDeltaUpdateable elm)
        {
            scheduler.Add(elm, UpdateableTimeMode.GameTimeUnscaled, -1.0f);
        }

        public static void Add(this IUpdateScheduler scheduler, IDeltaUpdateable elm, float interval)
        {
            scheduler.Add(elm, UpdateableTimeMode.GameTimeUnscaled, interval);
        }
    }

    public sealed class ScheduledAction : IUpdateable, IDisposable
    {
        Action _action;
        IUpdateScheduler _scheduler;
        bool _started;

        public ScheduledAction(IUpdateScheduler scheduler, Action action)
        {
            _scheduler = scheduler;
            _action = action;
        }

        public void Start(float interval = 0)
        {
            if(_started)
            {
                Stop();
            }

            _started = true;
            if(interval <= 0)
            {
                _scheduler.Add(this);
            }
            else
            {
                _scheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, interval);
            }
        }

        public void Update()
        {
            _action();
        }

        public void Stop()
        {
            _started = false;
            _scheduler.Remove(this);
        }

        public void Dispose()
        {
            if(_started)
            {
                Stop();
            }
            _scheduler = null;
            _action = null;
        }
    }

    public sealed class UpdateScheduler : IUpdateScheduler
    {
        readonly Dictionary<Object, IUpdateableHandler> _elements;
        readonly Dictionary<Object, IUpdateableHandler> _elementsToAdd;
        readonly List<Object> _elementsToRemove;
        bool _dirty;

        readonly List<Exception> _exceptions = new List<Exception>();

        double _lastUpdateTimestamp;

        public event Action<Exception> UpdateExceptionThrown;

        public UpdateScheduler()
        {
            _elements = new Dictionary<Object, IUpdateableHandler>();
            _elementsToAdd = new Dictionary<Object, IUpdateableHandler>();
            _elementsToRemove = new List<object>();
        }

        IUpdateableTimer CreateTimer(float interval = -1)
        {
            if(interval == -1)
            {
                return new ContinuousTimer();
            }
            else
            {
                return new FixedTimer(interval);
            }
        }

        public void Add(IUpdateable elm, UpdateableTimeMode updateTimeMode = UpdateableTimeMode.GameTimeUnscaled, float interval = -1)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                IUpdateableHandler handler = null;
                switch(updateTimeMode)
                {
                case UpdateableTimeMode.GameTimeScaled:
                    handler = new ScaledUpdateableHandler(elm, CreateTimer(interval));
                    break;
                case UpdateableTimeMode.GameTimeUnscaled:
                    handler = new UpdateableHandler(elm, CreateTimer(interval));  
                    break;
                case UpdateableTimeMode.RealTime:
                    handler = new RealTimeUpdateableHandler(elm, CreateTimer(interval));
                    break;
                }
                DoAdd(elm, handler);
            }
        }

        public void Add(IDeltaUpdateable elm, UpdateableTimeMode updateTimeMode = UpdateableTimeMode.GameTimeUnscaled, float interval = -1)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                IUpdateableHandler handler = null;
                switch(updateTimeMode)
                {
                case UpdateableTimeMode.GameTimeScaled:
                    handler = new DeltaScaledUpdateableHandler(elm, CreateTimer(interval));
                    break;
                case UpdateableTimeMode.GameTimeUnscaled:
                    handler = new DeltaUpdateableHandler(elm, CreateTimer(interval));   
                    break;
                case UpdateableTimeMode.RealTime:
                    handler = new RealTimeDeltaUpdateableHandler(elm, CreateTimer(interval));   
                    break;
                }
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
            return Contains((object)elm);
        }

        public bool Contains(IDeltaUpdateable elm)
        { 
            return Contains((object)elm);
        }

        bool Contains(object elm)
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

        public void Update(float deltaTime, float unscaledDeltaTime)
        {
            // Sync for external changes
            Synchronize();

            _exceptions.Clear();

            // Update registered elements
            var time = GetUpdateTime(deltaTime, unscaledDeltaTime);
            DoUpdate(time);

            // Check new exceptions
            var exceptionsCount = _exceptions.Count;
            if(exceptionsCount > 0)
            {
                throw new AggregateException(_exceptions);
            }

            // Sync for internal changes during update
            Synchronize();
        }

        void DoUpdate(UpdateableTime time)
        {
            var itr = _elements.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                try
                {
                    elm.Value.Update(time);
                }
                catch(Exception e)
                {
                    if(UpdateExceptionThrown != null)
                    {
                        UpdateExceptionThrown(e);
                    }
                    _exceptions.Add(e);
                }
            }
            itr.Dispose();
        }

        UpdateableTime GetUpdateTime(float deltaTime, float unscaledDeltaTime)
        {
            var currentTimeStamp = TimeUtils.GetTimestampDouble(DateTime.Now);
            if(_lastUpdateTimestamp < float.Epsilon)
            {
                _lastUpdateTimestamp = currentTimeStamp;
            }
            var nonScaledDeltaTime = currentTimeStamp - _lastUpdateTimestamp;
            _lastUpdateTimestamp = currentTimeStamp;

            var time = new UpdateableTime {
                DeltaTime = deltaTime,
                UnscaledDeltaTime = unscaledDeltaTime,
                RealDeltaTime = (float)nonScaledDeltaTime
            };
            return time;
        }

        public sealed class AggregateException : Exception
        {
            static string CreateMessage(IEnumerable<Exception> exceptions)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Multiple Exceptions thrown:");
                var count = 1;
                var itr = exceptions.GetEnumerator();
                while(itr.MoveNext())
                {
                    var ex = itr.Current;
                    sb.Append(count++)
                        .Append(". ")
                        .Append(ex.GetType().Name)
                        .Append(": ")
                        .Append(ex.Message)
                        .AppendLine(ex.StackTrace);
                    sb.AppendLine();
                }
                itr.Dispose();
                return sb.ToString();
            }

            public List<Exception> Exceptions { get; private set; }

            public AggregateException(IEnumerable<Exception> exceptions) : base(CreateMessage(exceptions))
            {
                Exceptions = new List<Exception>(exceptions);
            }
        }

        sealed class TimeScaleDependantInterval
        {
            public readonly double Interval;
            public double AccumTime;

            public TimeScaleDependantInterval(double interval)
            {
                Interval = interval;
                AccumTime = 0.0;
            }
        }

        sealed class TimeScaleNonDependantInterval
        {
            public readonly double Interval;
            public double CurrentTimeStamp;

            public TimeScaleNonDependantInterval(double interval)
            {
                Interval = interval;
                CurrentTimeStamp = TimeUtils.GetTimestampDouble(DateTime.Now);
            }
        }
    }

    struct UpdateableTime
    {
        public int Ticks;
        public float DeltaTime;
        public float UnscaledDeltaTime;
        public float RealDeltaTime;
    }

    #region TimeHandlers

    public interface IUpdateableTimer
    {
        bool Step(float delta, out float interval);
    }

    public struct ContinuousTimer : IUpdateableTimer
    {
        public bool Step(float delta, out float interval)
        {
            interval = delta;
            return true;
        }
    }

    public struct FixedTimer : IUpdateableTimer
    {
        readonly float _interval;
        float _current;

        public FixedTimer(float interval)
        {
            _current = 0;
            _interval = interval;
        }

        public bool Step(float delta, out float interval)
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
        void Update(UpdateableTime time);
    }

    struct UpdateableHandler : IUpdateableHandler
    {
        readonly IUpdateableTimer _timer;
        readonly IUpdateable _updateable;

        public UpdateableHandler(IUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.UnscaledDeltaTime, out interval))
            {
                _updateable.Update();
            }
        }
    }

    struct ScaledUpdateableHandler : IUpdateableHandler
    {
        readonly IUpdateableTimer _timer;
        readonly IUpdateable _updateable;

        public ScaledUpdateableHandler(IUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.DeltaTime, out interval))
            {
                _updateable.Update();
            }
        }
    }

    struct RealTimeUpdateableHandler : IUpdateableHandler
    {
        readonly IUpdateableTimer _timer;
        readonly IUpdateable _updateable;

        public RealTimeUpdateableHandler(IUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.RealDeltaTime, out interval))
            {
                _updateable.Update();
            }
        }
    }

    struct DeltaUpdateableHandler : IUpdateableHandler
    {
        readonly IUpdateableTimer _timer;
        readonly IDeltaUpdateable _updateable;

        public DeltaUpdateableHandler(IDeltaUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.UnscaledDeltaTime, out interval))
            {
                _updateable.Update(interval);
            }
        }
    }

    struct DeltaScaledUpdateableHandler : IUpdateableHandler
    {
        readonly IUpdateableTimer _timer;
        readonly IDeltaUpdateable _updateable;

        public DeltaScaledUpdateableHandler(IDeltaUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.DeltaTime, out interval))
            {
                _updateable.Update(interval);
            }
        }
    }

    struct RealTimeDeltaUpdateableHandler : IUpdateableHandler
    {
        readonly IUpdateableTimer _timer;
        readonly IDeltaUpdateable _updateable;

        public RealTimeDeltaUpdateableHandler(IDeltaUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.RealDeltaTime, out interval))
            {
                _updateable.Update(interval);
            }
        }
    }

    #endregion

    public class ImmediateCoroutineRunner : ICoroutineRunner
    {
        public IEnumerator StartCoroutine(IEnumerator enumerator)
        {
            while(enumerator.MoveNext())
            {
            }
            return enumerator;
        }

        public void StopCoroutine(IEnumerator enumerator)
        {
        }
    }
}
