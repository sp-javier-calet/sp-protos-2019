//-----------------------------------------------------------------------
// AutoInstantiateUIScreen.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using System;

public class AutoInstantiateUIScreen : MonoBehaviour
{
    [SerializeField]
    GameObject prefab;

    void Start()
    {
        var stackController = Services.Instance.Resolve<UIStackController>();
        if(stackController == null)
        {
            throw new InvalidOperationException("Could not find stack controller for initial screen");
        }

        if(prefab == null)
        {
            throw new UnityException("Prefab to instantiate is not setted");
        }

        var go = Instantiate(prefab);
        if(go != null)
        {
            stackController.PushImmediate(go);
        }
    }
}
