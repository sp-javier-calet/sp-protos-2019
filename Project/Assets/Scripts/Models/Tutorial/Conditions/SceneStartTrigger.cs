using System;
using UnityEngine.SceneManagement;

namespace SocialPoint.Tutorial
{
    [Serializable]
    public class SceneStartTrigger : EventTriggerConditionBase
    {
        public string SceneName;

        public SceneStartTrigger()
        {
            SceneName = "scene_name";
        }
        
        public override void RegisterEvents()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void UnregisterEvents()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if(SceneName.Equals(scene.name))
            {
                OnTriggerEvent();
            }
        }

        public override void Update(float elapsed)
        {
        }
    }
}
