using UnityEngine;
using UnityEditor;

namespace SocialPoint.GUIAnimation
{
    public sealed class GUIAnimatorRenderer : GUIDefaultActionRenderer
    {
        public override bool CanRender(Effect action)
        {
            return action is AnimatorEffect;
        }

        public override void Render(Effect effect, StepsSelection stepsSelection, OnActionChanged onChanged)
        {
            AnimatorEffect animEffect = (AnimatorEffect)effect;

            base.Render(effect, stepsSelection, onChanged);

            if(GUILayout.Button("Open Animator", GUILayout.ExpandWidth(false)))
            {
                OpenAnimator(animEffect.Target != null ? animEffect.Target.gameObject : null);
            }
        }

        void OpenAnimator(GameObject target)
        {
            Selection.activeGameObject = target;
            EditorApplication.ExecuteMenuItem("Window/Animation");
        }
    }
}
