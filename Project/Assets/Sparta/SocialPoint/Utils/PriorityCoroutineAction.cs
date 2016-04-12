using System;
using System.Collections;
using UnityEngine;

namespace SocialPoint.Utils
{
    public class PriorityCoroutineAction<T> : PriorityQueue<T, Func<IEnumerator>>
    {
        //Func<IEnumerator> _defaultCoroutine;
        ICoroutineRunner _runner;

        public PriorityCoroutineAction(ICoroutineRunner runner):base()
        {
            _runner = runner;
        }

        public PriorityCoroutineAction(PriorityCoroutineAction<T> other):base(other)
        {
        }

        public override object Clone()
        {
            return new PriorityCoroutineAction<T>(this);
        }

        /*public void Add(Func<IEnumerator> coroutine)
        {
            _defaultCoroutine += coroutine;
        }

        public void Remove(Func<IEnumerator> coroutine)
        {
            _defaultCoroutine -= coroutine;
        }
        */
        public void Run()
        {
            _runner.StartCoroutine(RunEvents());
        }

        IEnumerator RunEvents() // Coroutine o update
        {
            var queues = CopyQueues();
            foreach(var kvp in queues)
            {
                Debug.Log("running prio: " + kvp.Key);
                /*if(_defaultCoroutine != null)
                {
                    _defaultCoroutine(kvp.Key);
                }*/
                foreach(var action in kvp.Value)
                {
                    if(action != null)
                    {
                        yield return action();
                    }
                }
            }
        }
    }

    public class PriorityCoroutineAction : PriorityCoroutineAction<int>
    {
        public PriorityCoroutineAction(ICoroutineRunner runner):base(runner)
        {
        }

        public PriorityCoroutineAction(PriorityCoroutineAction other):base(other)
        {
        }
    }
}

