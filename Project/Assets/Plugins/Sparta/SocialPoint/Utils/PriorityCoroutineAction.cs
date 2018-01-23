using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public class PriorityCoroutineAction<T> : PriorityQueue<T, Func<IEnumerator>>
    {
        ICoroutineRunner _runner;
        List<Func<T, IEnumerator>> _defaultActions = new List<Func<T, IEnumerator>>();
        List<IEnumerator> _itrs = new List<IEnumerator>();

        public PriorityCoroutineAction(ICoroutineRunner runner = null) : base()
        {
            _runner = runner;
        }

        public PriorityCoroutineAction(PriorityCoroutineAction<T> other) : base(other)
        {
            _runner = other._runner;
        }

        public override object Clone()
        {
            return new PriorityCoroutineAction<T>(this);
        }

        public void Add(Func<T, IEnumerator> action)
        {
            if(!_defaultActions.Contains(action))
            {
                _defaultActions.Add(action);
            }
        }

        public void Remove(Func<T, IEnumerator> action)
        {
            _defaultActions.Remove(action);
        }

        public void Run()
        {
            if(_runner == null)
            {
                throw new InvalidOperationException("Please specify a coroutine runner in the constructor");
            }
            _runner.StartCoroutine(RunCoroutine());
        }

        public IEnumerator RunCoroutine()
        {
            var queues = CopyQueues();
            var itr = queues.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                _itrs.Clear();
                var itr2 = kvp.Value.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var action = itr2.Current;
                    if (action != null)
                    {
                        _itrs.Add(action());
                    }
                }
                for(var i = 0; i < _defaultActions.Count; i++)
                {
                    var action = _defaultActions[i];
                    if(action != null)
                    {
                        _itrs.Add(action(kvp.Key));
                    }
                }
                while(_itrs.Count > 0)
                {
                    for(var i = _itrs.Count - 1; i >= 0; i--)
                    {
                        var itr3 = _itrs[i];
                        if(!itr3.MoveNext())
                        {
                            _itrs.RemoveAt(i);
                        }
                    }
                    yield return null;
                }
            }
            itr.Dispose();
        }
    }

    public sealed class PriorityCoroutineAction : PriorityCoroutineAction<int>
    {
        public PriorityCoroutineAction(ICoroutineRunner runner = null) : base(runner)
        {
        }

        public PriorityCoroutineAction(PriorityCoroutineAction other) : base(other)
        {
        }
    }
}

