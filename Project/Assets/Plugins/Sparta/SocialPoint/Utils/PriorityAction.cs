using System;
using System.Collections.Generic;

namespace SocialPoint.Utils
{    

    public class PriorityAction<T> : PriorityQueue<T, Action>
    {
        Action<T> _defaultAction;

        public PriorityAction()
        {
        }

        public PriorityAction(PriorityAction<T> other):base(other)
        {
        }

        public override object Clone()
        {
            return new PriorityAction<T>(this);
        }

        public void Add(Action<T> action)
        {
            _defaultAction += action;
        }
        
        public void Remove(Action<T> action)
        {
            _defaultAction -= action;
        }

        public void Run()
        {
            var queues = CopyQueues();
            var itr = queues.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                if(_defaultAction != null)
                {
                    _defaultAction(kvp.Key);
                }
                var itr2 = kvp.Value.GetEnumerator();
                while(itr2.MoveNext())
                {
                    var action = itr2.Current;
                    if(action != null)
                    {
                        action();
                    }
                }
                itr2.Dispose();
            }
            itr.Dispose();
        }
    }

    public sealed class PriorityAction : PriorityAction<int>
    {
        public PriorityAction()
        {
        }
        
        public PriorityAction(PriorityAction other):base(other)
        {
        }
    }

}

