using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;
using System;

[Serializable]
public class UnityViewAnimation : MonoBehaviour, UIViewAnimation 
{
    #region UIViewAnimation implementation
    public virtual void Load(UIViewController ctrl) {}

    public virtual IEnumerator Appear()
    {
        yield return null;
    }

    public virtual IEnumerator Disappear() 
    { 
        yield return null; 
    }

    public virtual void Reset() {}

    #endregion

    #region ICloneable implementation

    public virtual object Clone() 
    {
        return null;
    }

    #endregion
}
