using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Utils
{
    
    class ReverseComparer<TPriority> : IComparer<TPriority> 
    {
        IComparer<TPriority> _comparer;

        public ReverseComparer():
        this(Comparer<TPriority>.Default)
        {
        }

        public ReverseComparer(IComparer<TPriority> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(TPriority x, TPriority y)
        {
            return -1 * _comparer.Compare(x, y);
        }
    }

    public class PriorityQueue<TPriority, TValue>  : ICloneable, IEnumerable<TValue>
    {
        private SortedList<TPriority, Queue<TValue>> _queues;
        
        public PriorityQueue():
        this(new ReverseComparer<TPriority>())
        {
        }
        
        public PriorityQueue(IComparer<TPriority> comparer)
        {
            _queues = new SortedList<TPriority, Queue<TValue>>(comparer);
        }
        
        public PriorityQueue(PriorityQueue<TPriority, TValue> other)
        {
            _queues = new SortedList<TPriority, Queue<TValue>>(other._queues.Comparer);
            foreach(var pair in other._queues)
            {
                _queues[pair.Key] = new Queue<TValue>(pair.Value);
            }
        }

        public object Clone()
        {
            return new PriorityQueue<TPriority, TValue>(this);
        }
        
        public void Enqueue(TPriority priority, TValue obj)
        {
            Queue<TValue> queue;
            if(!_queues.TryGetValue(priority, out queue))
            {
                queue = new Queue<TValue>();
                _queues.Add(priority, queue);
            }
            queue.Enqueue(obj);
        }
        
        public TValue Dequeue()
        {
            foreach(var currQueue in _queues)
            {
                if(currQueue.Value.Count > 0)
                {
                    return currQueue.Value.Dequeue();
                }
            }
            return default(TValue);
        }

        public bool Dequeue(TValue value)
        {
            bool found = false;
            foreach(var key in _queues.Keys)
            {
                var currQueue = _queues[key];
                if(currQueue.Contains(value))
                {
                    found = true;
                    var newQueue = new Queue<TValue>();
                    foreach(var elm in currQueue)
                    {
                        if(!EqualityComparer<TValue>.Default.Equals(elm, value))
                        {
                            newQueue.Enqueue(value);
                        }
                    }
                    _queues[key] = newQueue;
                }
            }
            return found;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach(var currQueue in _queues)
            {
                foreach(var obj in currQueue.Value)
                {
                    yield return obj;
                }
            }
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
            foreach(var currQueue in _queues)
            {
                currQueue.Value.Clear();
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach(var currQueue in _queues)
                {
                    count += currQueue.Value.Count;
                }

                return count;
            }
        }
    };

    public class PriorityAction : PriorityQueue<int, Action>
    {
        public PriorityAction():base()
        {
        }

        public PriorityAction(PriorityAction other):base(other)
        {
        }

        public void Run()
        {
            var queue = new PriorityAction(this);
            foreach(var action in queue)
            {
                if(action != null)
                {
                    action();
                }
            }
        }
    }
}

