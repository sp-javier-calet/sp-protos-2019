
using System;
using System.Collections;
using System.Collections.Generic;

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
        void Remove(IUpdateable elm);
    }

    public static class UpdateSchedulerExtension
    {
        public static void Add(this IUpdateScheduler scheduler, IEnumerable<IUpdateable> elements = null)
        {
            if(elements != null)
            {
                foreach(var elm in elements)
                {
                    scheduler.Add(elm);
                }
            }
        }
    }
}
