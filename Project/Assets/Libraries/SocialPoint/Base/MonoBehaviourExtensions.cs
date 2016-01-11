using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Base
{
    public static class MonoBehaviourExtensions
    {

        public static AsyncOperation LoadSceneAsync(this MonoBehaviour behaviour, string name, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, name, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsync(this MonoBehaviour behaviour, int index, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, index, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsync(this MonoBehaviour behaviour, string name, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, name, false, mode, finished);
        }

        public static AsyncOperation LoadSceneAsync(this MonoBehaviour behaviour, int index, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, index, false, mode, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this MonoBehaviour behaviour, int index, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, index, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this MonoBehaviour behaviour, string name, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, name, LoadSceneMode.Single, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this MonoBehaviour behaviour, int index, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, index, true, mode, finished);
        }

        public static AsyncOperation LoadSceneAsyncProgress(this MonoBehaviour behaviour, string name, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            return LoadSceneAsync(behaviour, name, true, mode, finished);
        }

        static AsyncOperation LoadSceneAsync(this MonoBehaviour behaviour, string name, bool progress, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            var op = SceneManager.LoadSceneAsync(name, mode);
            behaviour.StartCoroutine(CheckAsyncOp(op, progress, finished));
            return op;
        }

        static AsyncOperation LoadSceneAsync(this MonoBehaviour behaviour, int index, bool progress, LoadSceneMode mode, Action<AsyncOperation> finished)
        {
            var op = SceneManager.LoadSceneAsync(index, mode);
            behaviour.StartCoroutine(CheckAsyncOp(op, progress, finished));
            return op;
        }

        static IEnumerator CheckAsyncOp(AsyncOperation op, bool progress, Action<AsyncOperation> finished)
        {
            while(!op.isDone)
            {
                if(progress && finished != null)
                {
                    finished(op);
                }
                yield return null;
            }
            if(finished != null)
            {
                finished(op);
            }
        }

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

