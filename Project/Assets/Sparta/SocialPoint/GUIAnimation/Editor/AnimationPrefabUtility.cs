using UnityEditor;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Helper Class to save a prefab from an instance
    public static class AnimationPrefabUtility
    {
        public static GameObject SaveScreenPrefab(GameObject instance)
        {
            var prefabParent = (GameObject)PrefabUtility.GetPrefabParent(instance);

            if(prefabParent != null)
            {
                PrefabUtility.ReplacePrefab(instance, prefabParent, ReplacePrefabOptions.ConnectToPrefab);

                return prefabParent;
            }

            return null;
        }
    }
}
