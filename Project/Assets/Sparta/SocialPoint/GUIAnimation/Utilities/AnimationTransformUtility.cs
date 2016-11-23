using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public static class AnimationTransformUtility
    {
        public static RectTransform CreateRectTransform(string name)
        {
            var go = new GameObject(name);
            return GetAddRectTransform(go);
        }

        public static RectTransform GetAddRectTransform(Transform trans)
        {
            return GetAddRectTransform(trans.gameObject);
        }

        public static RectTransform GetAddRectTransform(GameObject go)
        {
            RectTransform transform = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            return transform;
        }
    }
}
