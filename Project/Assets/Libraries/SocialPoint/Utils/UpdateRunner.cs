
using System;
using System.Collections;

namespace SocialPoint.Utils
{
    public interface IUpdateable
    {
        void Update();
    }

    public interface ICoroutineRunner
    {
        void StartCoroutine(IEnumerator enumerator);
        void StopCoroutine(IEnumerator enumerator);
    }

    public interface IUpdateScheduler
    {
        void Add(IUpdateable elm);
        void Remove(IUpdateable elm);
    }
}
