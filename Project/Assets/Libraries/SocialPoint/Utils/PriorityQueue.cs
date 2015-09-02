using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Utils
{
    public class PriorityQueue<TPriority, TValue> 
        where TPriority : struct, IComparable, IConvertible, IFormattable 
        where TValue : class
    {
        private SortedList<TPriority, Queue<TValue>> _queues;
        
        public PriorityQueue()
        {
            _queues = new SortedList<TPriority, Queue<TValue>>();
            InitializeQueues();
        }
        
        public PriorityQueue(Comparer<TPriority> comparer)
        {
            _queues = new SortedList<TPriority, Queue<TValue>>(comparer);
            InitializeQueues();
        }
        
        public PriorityQueue(PriorityQueue<TPriority, TValue> other)
        {
            _queues = new SortedList<TPriority, Queue<TValue>>(other._queues);
        }
        
        private void InitializeQueues()
        {
            if(typeof(TPriority).IsEnum)
            {
                var priorityValues = Enum.GetValues(typeof(TPriority)).Cast<TPriority>();
                foreach(var priority in priorityValues)
                {
                    _queues[priority] = new Queue<TValue>();
                }
            }
            else
            {
                throw new ArgumentException("TPriority must be an enum type");
            }
        }
        
        public void Enqueue(TPriority priority, TValue obj)
        {
            _queues[priority].Enqueue(obj);
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
            return null;
        }
        
        public IEnumerable<TValue> All
        {
            get
            {
                foreach(var currQueue in _queues)
                {
                    foreach(TValue obj in currQueue.Value)
                    {
                        yield return obj;
                    }
                }
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
}

