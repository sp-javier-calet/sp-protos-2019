using System;
using UnityEngine.SceneManagement;

namespace SocialPoint.Tutorial
{
    [Serializable]
    public class SceneChangedTrigger : EventTriggerConditionBase
    {
        public string NewSceneName;
        public string OldSceneName;

        public SceneChangedTrigger()
        {
            NewSceneName = "new_scene_name";
            OldSceneName = string.Empty;
        }
        
        public override void RegisterEvents()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        public override void UnregisterEvents()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if(string.IsNullOrEmpty(OldSceneName) && string.IsNullOrEmpty(NewSceneName))
            {
                throw new InvalidOperationException("You need at least one scene name for this to work");
            }

            if((string.IsNullOrEmpty(OldSceneName) || OldSceneName.Equals(oldScene.name)) &&
               (string.IsNullOrEmpty(NewSceneName) || NewSceneName.Equals(newScene.name)))
            {
                OnTriggerEvent();
            }
        }

        public override void Update(float elapsed)
        {
        }
    }
}
