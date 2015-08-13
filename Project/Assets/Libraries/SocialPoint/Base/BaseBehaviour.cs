using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Base
{
    public class BaseBehaviour : MonoBehaviour
    {

        public I GetInterfaceComponent<I>() where I : class
        {
            return gameObject.GetInterfaceComponent<I>();
        }

        public List<I> GetInterfaceComponentsInChildren<I>() where I : class
        {
            return gameObject.GetInterfaceComponentsInChildren<I>();
        }
        
        public List<I> FindObjectsOfInterface<I>() where I : class
        {
            return GameObjectExtension.FindObjectsOfInterface<I>();
        }
        
        public T GetSafeComponent<T>() where T : MonoBehaviour
        {
            return gameObject.GetSafeComponent<T>();
        }
    }


}
