using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Utils
{    
    class ReverseComparer<TPriority> : IComparer<TPriority> 
    {
        readonly IComparer<TPriority> _comparer;

        public ReverseComparer(IComparer<TPriority> comparer=null)
        {
            if(comparer == null)
            {
                comparer = Comparer<TPriority>.Default;
            }
            _comparer = comparer;
        }

        public int Compare(TPriority x, TPriority y)
        {
            return -1 * _comparer.Compare(x, y);
        }
    }

    public class PriorityQueue<TPriority, TValue> : IDisposable, ICloneable, IEnumerable<TValue>
    {
        protected SortedList<TPriority, Queue<TValue>> _queues;

        public PriorityQueue(IComparer<TPriority> comparer=null)
        {
            _queues = new SortedList<TPriority, Queue<TValue>>(new ReverseComparer<TPriority>(comparer));
        }
        
        public PriorityQueue(PriorityQueue<TPriority, TValue> other)
        {
            _queues = other.CopyQueues();
        }

        protected SortedList<TPriority, Queue<TValue>> CopyQueues()
        {
            var queues = new SortedList<TPriority, Queue<TValue>>(_queues.Comparer);
            var itr = _queues.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                queues[pair.Key] = new Queue<TValue>(pair.Value);
            }
            itr.Dispose();

            return queues;
        }

        public void Dispose()
        {
            Clear();
        }

        public virtual object Clone()
        {
            return new PriorityQueue<TPriority, TValue>(this);
        }
        
        public void Add(TPriority priority, TValue obj)
        {
            Queue<TValue> queue;
            if(!_queues.TryGetValue(priority, out queue))
            {
                queue = new Queue<TValue>();
                _queues.Add(priority, queue);
            }
            queue.Enqueue(obj);
        }

        [Obsolete("Use Add")]
        public void Enqueue(TPriority priority, TValue obj)
        {
            Add(priority, obj);
        }
        
        public TValue Remove()
        {
            var itr = _queues.GetEnumerator();
            while(itr.MoveNext())
            {
                var currQueue = itr.Current;
                if(currQueue.Value.Count > 0)
                {
                    itr.Dispose();
                    return currQueue.Value.Dequeue();
                }
            }
            itr.Dispose();

            return default(TValue);
        }

        [Obsolete("Use Remove")]
        public TValue Dequeue()
        {
            return Remove();
        }

        public bool Remove(TValue value)
        {
            bool found = false;
            for(int i = 0; i < _queues.Count; ++i)
            {
                var key = _queues.Keys[i];
                if(_queues[key].Contains(value))
                {
                    // If queue contains the value, regenerate the entire queue without it
                    found = true;
                    var newQueue = new Queue<TValue>();
                    var itr = _queues[key].GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var elm = itr.Current;
                        if(!EqualityComparer<TValue>.Default.Equals(elm, value))
                        {
                            newQueue.Enqueue(elm);
                        }
                    }
                    itr.Dispose();
                    _queues[key] = newQueue;
                }
            }
            return found;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            var itr = _queues.GetEnumerator();
            while(itr.MoveNext())
            {
                var currQueue = itr.Current;
                var itr2 = currQueue.Value.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var obj = itr2.Current;
                    yield return obj;
                }
                itr2.Dispose();
            }
            itr.Dispose();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [Obsolete("iterate over the queue")]
        public IEnumerable<TValue> All
        {
            get
            {
                return (IEnumerable<TValue>)GetEnumerator();
            }
        }

        public void Clear()
        {
            _queues.Clear();
        }

        public int Count
        {
            get
            {
                int count = 0;
                var itr = _queues.GetEnumerator();
                while(itr.MoveNext())
                {
                    var currQueue = itr.Current;
                    count += currQueue.Value.Count;
                }
                itr.Dispose();

                return count;
            }
        }
    };

}

