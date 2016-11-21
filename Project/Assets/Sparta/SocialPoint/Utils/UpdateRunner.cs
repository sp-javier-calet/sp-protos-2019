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

    public interface ICoroutineRunner
    {
        IEnumerator StartCoroutine(IEnumerator enumerator);

        void StopCoroutine(IEnumerator enumerator);
    }

    public interface IUpdateScheduler
    {
        void Add(IUpdateable elm);

        void AddFixed(IUpdateable elm, double interval, bool usesTimeScale = false);

        void Remove(IUpdateable elm);
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
        readonly HashSet<IUpdateable> _elementsToRemove;
        readonly HashSet<IUpdateable> _elements;
        readonly Dictionary<IUpdateable, TimeScaleDependantInterval> _intervalTimeScaleDependantElements;
        readonly Dictionary<IUpdateable, TimeScaleNonDependantInterval> _intervalTimeScaleNonDependantElements;
        readonly List<Exception> _exceptions = new List<Exception>();

        public UpdateScheduler()
        {
            var comparer = new ReferenceComparer<IUpdateable>();
            _elements = new HashSet<IUpdateable>(comparer);
            _elementsToRemove = new HashSet<IUpdateable>(comparer);
            _intervalTimeScaleDependantElements = new Dictionary<IUpdateable, TimeScaleDependantInterval>(comparer);
            _intervalTimeScaleNonDependantElements = new Dictionary<IUpdateable, TimeScaleNonDependantInterval>(comparer);
        }

        public void Add(IUpdateable elm)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                _elementsToRemove.Remove(elm);
                _elements.Add(elm);
            }
        }

        public void AddFixed(IUpdateable elm, double interval, bool usesTimeScale = false)
        {
            DebugUtils.Assert(elm != null);
            if(elm != null)
            {
                if(usesTimeScale)
                {
                    var intervalData = new TimeScaleDependantInterval(interval);
                    _intervalTimeScaleDependantElements.Add(elm, intervalData);
                }
                else
                {
                    var intervalData = new TimeScaleNonDependantInterval(interval);
                    _intervalTimeScaleNonDependantElements.Add(elm, intervalData);
                }
            }
        }

        public void Remove(IUpdateable elm)
        {
            if(elm != null && !_elementsToRemove.Contains(elm))
            {
                _elementsToRemove.Add(elm);
            }
        }

        void DoRemove()
        {
            var itr = _elementsToRemove.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                if(_elements.Contains(elm))
                {
                    _elements.Remove(elm);
                }
                if(_intervalTimeScaleDependantElements.ContainsKey(elm))
                {
                    _intervalTimeScaleDependantElements.Remove(elm);
                }
                if(_intervalTimeScaleNonDependantElements.ContainsKey(elm))
                {
                    _intervalTimeScaleNonDependantElements.Remove(elm);
                }
            }
            itr.Dispose();
            _elementsToRemove.Clear();
        }

        public void Update(float deltaTime)
        {
            DoRemove();
            _exceptions.Clear();

            var itr = _elements.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                try
                {
                    elm.Update();
                }
                catch(Exception e)
                {
                    _exceptions.Add(e);
                }
            }
            itr.Dispose();

            var itr2 = _intervalTimeScaleDependantElements.GetEnumerator();
            while(itr2.MoveNext())
            {
                var data = itr2.Current.Value;

                data.AccumTime += deltaTime;

                var interval = data.Interval;
                var accumTime = data.AccumTime;
                var timeDiff = accumTime - interval;
                if(timeDiff >= 0)
                {
                    var elm = itr2.Current.Key;
                    try
                    {
                        elm.Update();
                    }
                    catch(Exception e)
                    {
                        _exceptions.Add(e);
                    }
                    data.AccumTime = timeDiff;
                }
            }
            itr2.Dispose();

            var currentTimeStamp = TimeUtils.GetTimestampDouble(DateTime.Now);
            var itr3 = _intervalTimeScaleNonDependantElements.GetEnumerator();
            while(itr3.MoveNext())
            {
                var data = itr3.Current.Value;
                var interval = data.Interval;
                var timeStampDelta = currentTimeStamp - data.CurrentTimeStamp;
                var timeDiff = timeStampDelta - interval;
                if(timeDiff >= 0)
                {
                    var elm = itr3.Current.Key;
                    try
                    {
                        elm.Update();
                    }
                    catch(Exception e)
                    {
                        _exceptions.Add(e);
                    }
                    data.CurrentTimeStamp = currentTimeStamp + timeDiff;
                }
            }
            itr3.Dispose();

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
