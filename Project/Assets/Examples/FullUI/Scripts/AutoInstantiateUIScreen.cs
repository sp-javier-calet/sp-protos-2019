using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using System;

public class AutoInstantiateUIScreen : MonoBehaviour 
{
    [SerializeField]
    GameObject prefab;

	void Start () 
    {
        var stackController = Services.Instance.Resolve<UIStackController>();
        if(stackController == null)
        {
            throw new InvalidOperationException("Could not find screens controller for initial screen");
        }

        if(prefab != null)
        {
            var go = Instantiate(prefab);
            if(go != null)
            {
                stackController.PushImmediate(go);
            }
        }
	}
}
