using System;
using System.Collections;

namespace SocialPoint.GUIControl
{
    public interface UIViewAnimation : ICloneable
    {
        void Load(UIViewController ctrl);

        IEnumerator Appear();

        IEnumerator Disappear();

        void Reset();
    }
}