using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Utils
{    

    public class PriorityAction<T> : PriorityQueue<T, Action>
    {
        Action<T> _defaultAction;

        public PriorityAction():base()
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
            foreach(var kvp in queues)
            {
                if(_defaultAction != null)
                {
                    _defaultAction(kvp.Key);
                }
                foreach(var action in kvp.Value)
                {
                    if(action != null)
                    {
                        action();
                    }
                }
            }
        }
    }

    public class PriorityAction : PriorityAction<int>
    {
        public PriorityAction():base()
        {
        }
        
        public PriorityAction(PriorityAction other):base(other)
        {
        }
    }

}

