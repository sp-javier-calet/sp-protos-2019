using System;

namespace BehaviorDesigner.Runtime.Standalone
{   
    public class WaitForSeconds : Coroutine
    {
        public WaitForSeconds(float duration)
        {
            throw new NotImplementedException(string.Format("{0} WaitForSeconds is not implemented in Standalone version", GetType()));
        }
    }
}