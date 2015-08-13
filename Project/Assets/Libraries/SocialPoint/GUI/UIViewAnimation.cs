using UnityEngine;
using System;
using System.Collections;

namespace SocialPoint.GUI
{
    public interface UIViewAnimation : ICloneable
    {
        void Load(UIViewController ctrl);

        IEnumerator Appear();

        IEnumerator Disappear();

        void Reset();

    }

}