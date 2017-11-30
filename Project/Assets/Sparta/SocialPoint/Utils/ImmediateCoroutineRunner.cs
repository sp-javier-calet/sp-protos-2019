using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
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
