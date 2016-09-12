using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.Multiplayer
{
    public class PhysicsUtilities
    {
        public static void Dispose<T>(ref T disposable) where T: class, IDisposable
        {
            if(disposable != null)
            {

                disposable.Dispose();
                disposable = null;
            }
        }
    }
}
