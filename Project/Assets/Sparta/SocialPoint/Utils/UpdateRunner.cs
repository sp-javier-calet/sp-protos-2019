using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
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
        void Add(IUpdateable elm, bool timeScaled, float interval);

        void Remove(IUpdateable elm);

        bool Contains(IUpdateable elm);

        void Add(IDeltaUpdateable elm, bool timeScaled, float interval);

        void Remove(IDeltaUpdateable elm);

        bool Contains(IDeltaUpdateable elm);
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
        public static void AddFixed(this IUpdateScheduler scheduler, IUpdateable elm, double interval, bool usesTimeScale = false)
        {
            scheduler.Add(elm, usesTimeScale, (float)interval);
        }

        public static void Add(this IUpdateScheduler scheduler, IUpdateable elm)
        {
            scheduler.Add(elm, false, -1);
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

        public void Start(double interval = 0)
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
                _scheduler.AddFixed(this, interval);
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
            Stop();
            _scheduler = null;
            _action = null;
        }
    }

    public sealed class UpdateScheduler : IUpdateScheduler
    {
        readonly Dictionary<Object, IUpdateableHandler> _elements;
        readonly List<Object> _elementsToRemove;

        readonly List<Exception> _exceptions = new List<Exception>();

        double _lastUpdateTimestamp;

        public UpdateScheduler()
        {
            _elements = new Dictionary<Object, IUpdateableHandler>();
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

        public void Add(IUpdateable elm, bool timeScaled = false, float interval = -1)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                IUpdateableHandler handler;
                if(timeScaled)
                {
                    handler = new ScaledUpdateableHandler(elm, CreateTimer(interval));
                }
                else
                {
                    handler = new UpdateableHandler(elm, CreateTimer(interval));   
                }
                DoAdd(elm, handler);
            }
        }

        public void Add(IDeltaUpdateable elm, bool timeScaled = false, float interval = -1)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                IUpdateableHandler handler;
                if(timeScaled)
                {
                    handler = new DeltaScaledUpdateableHandler(elm, CreateTimer(interval));
                }
                else
                {
                    handler = new DeltaUpdateableHandler(elm, CreateTimer(interval));   
                }
                DoAdd(elm, handler);
            }
        }

        void DoAdd(Object elm, IUpdateableHandler handler)
        {
            _elementsToRemove.Remove(elm);
            _elements[elm] = handler;
        }

        public void Remove(IUpdateable elm)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                _elementsToRemove.Add(elm);
            }
        }

        public void Remove(IDeltaUpdateable elm)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                _elementsToRemove.Add(elm);
            }
        }

        public bool Contains(IUpdateable elm)
        { 
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                return _elements.ContainsKey(elm);
            }
            return false;
        }

        public bool Contains(IDeltaUpdateable elm)
        { 
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                return _elements.ContainsKey(elm);
            }
            return false;
        }

        void DoRemove()
        {
            var itr = _elementsToRemove.GetEnumerator();
            while(itr.MoveNext())
            {
                _elements.Remove(itr.Current);
            }
            itr.Dispose();
            _elementsToRemove.Clear();
        }

        public void Update(float deltaTime)
        {
            DoRemove();
            _exceptions.Clear();

            var currentTimeStamp = TimeUtils.GetTimestampDouble(DateTime.Now);
            if(_lastUpdateTimestamp == 0)
            {
                _lastUpdateTimestamp = currentTimeStamp;
            }
            var nonScaledDeltaTime = currentTimeStamp - _lastUpdateTimestamp;
            _lastUpdateTimestamp = currentTimeStamp;

            var time = new UpdateableTime(){ DeltaTime = deltaTime, NonScaledDeltaTime = (float)nonScaledDeltaTime };

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
                    _exceptions.Add(e);
                }
            }
            itr.Dispose();

            var exceptionsCount = _exceptions.Count;
            if(exceptionsCount > 0)
            {
                throw new AggregateException(_exceptions);
            }
        }

        sealed class AggregateException : Exception
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
        public float NonScaledDeltaTime;
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
        float _current;
        float _interval;

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

    struct ScaledUpdateableHandler : IUpdateableHandler
    {
        IUpdateableTimer _timer;
        IUpdateable _updateable;

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

    struct UpdateableHandler : IUpdateableHandler
    {
        IUpdateableTimer _timer;
        IUpdateable _updateable;

        public UpdateableHandler(IUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.NonScaledDeltaTime, out interval))
            {
                _updateable.Update();
            }
        }
    }

    struct DeltaUpdateableHandler : IUpdateableHandler
    {
        IUpdateableTimer _timer;
        IDeltaUpdateable _updateable;

        public DeltaUpdateableHandler(IDeltaUpdateable updateable, IUpdateableTimer timer)
        {
            _updateable = updateable;
            _timer = timer;
        }

        public void Update(UpdateableTime time)
        {
            float interval;
            if(_timer.Step(time.NonScaledDeltaTime, out interval))
            {
                _updateable.Update(interval);
            }
        }
    }

    struct DeltaScaledUpdateableHandler : IUpdateableHandler
    {
        IUpdateableTimer _timer;
        IDeltaUpdateable _updateable;

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
