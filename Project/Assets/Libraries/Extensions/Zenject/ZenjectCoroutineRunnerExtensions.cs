using SocialPoint.Utils;
using Zenject;
using UnityEngine;
using System;

public static class ZenjectCoroutineRunnerExtensions
{
    public static void LoadSceneAsyncWithZenject(this ICoroutineRunner runner, string sceneName, Action<DiContainer> action)
    {
        runner.LoadSceneAsync(sceneName, ((AsyncOperation op) => {
            if(op.isDone)
            {
                var sceneCompositionRoot = GameObject.FindObjectOfType<SceneCompositionRoot>();
                action(sceneCompositionRoot.Container);
            }
        }));
    }
}