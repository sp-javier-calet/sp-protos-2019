using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Base
{
    public static class MonoBehaviourExtensions
    {
        public static I GetInterfaceComponent<I>(this MonoBehaviour behaviour) where I : class
        {
            return behaviour.gameObject.GetInterfaceComponent<I>();
        }

        public static List<I> GetInterfaceComponentsInChildren<I>(this MonoBehaviour behaviour) where I : class
        {
            return behaviour.gameObject.GetInterfaceComponentsInChildren<I>();
        }

        public static T GetSafeComponent<T>(this MonoBehaviour behaviour) where T : MonoBehaviour
        {
            return behaviour.gameObject.GetSafeComponent<T>();
        }
    }
}

