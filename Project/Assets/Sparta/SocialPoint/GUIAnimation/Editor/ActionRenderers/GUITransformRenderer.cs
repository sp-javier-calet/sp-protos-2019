using UnityEditor;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class GUITransformRenderer : BaseGUIActionRenderer
    {
        public bool _showGeneratedAnchors;

        public GUITransformRenderer(bool showGeneratedAnchors)
        {
            _showGeneratedAnchors = showGeneratedAnchors;
        }

        public override bool CanRender(Effect action)
        {
            return action is IPositionable;
        }

        public override void Render(Effect action, StepsSelection stepsSelection, OnActionChanged onChanged)
        {
            var transEffect = (IPositionable)action;

            GUI.changed = false;

            transEffect.AnchorsMode = (AnchorMode)EditorGUILayout.EnumPopup(transEffect.AnchorsMode, GUILayout.ExpandWidth(false));

            if(transEffect.AnchorsMode == AnchorMode.Disabled)
            {
            }
            else if(transEffect.AnchorsMode == AnchorMode.Custom)
            {
                DoShowAnchors(transEffect, true);
            }
            else
            {
                DoShowAnchors(transEffect, false);
            }

            if(GUI.changed)
            {
                transEffect.SetAnchors();
				
                CopyAnchorsToSelection(transEffect, stepsSelection);
				
                if(onChanged != null)
                {
                    onChanged(action);
                }
            }

            GUI.changed = false;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Local:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, GUI.skin.label), GUILayout.Width(100f));
            transEffect.IsLocal = EditorGUILayout.Toggle(transEffect.IsLocal, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
            GUILayout.EndHorizontal();

            if(transEffect is TransformEffect)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Freeze Position:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, GUI.skin.label), GUILayout.MaxWidth(150f));
                ((TransformEffect)transEffect).FreezePosition = EditorGUILayout.Toggle(((TransformEffect)transEffect).FreezePosition, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Freeze Rotation:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, GUI.skin.label), GUILayout.MaxWidth(150f));
                ((TransformEffect)transEffect).FreezeRotation = EditorGUILayout.Toggle(((TransformEffect)transEffect).FreezeRotation, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Freeze Scale:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, GUI.skin.label), GUILayout.MaxWidth(150f));
                ((TransformEffect)transEffect).FreezeScale = EditorGUILayout.Toggle(((TransformEffect)transEffect).FreezeScale, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
                GUILayout.EndHorizontal();
            }
            if(GUI.changed)
            {
                if(onChanged != null)
                {
                    onChanged(action);
                }
            }
        }

        void CopyAnchorsToSelection(IPositionable effect, StepsSelection stepsSelection)
        {
            for(int i = 0; i < stepsSelection.Count; ++i)
            {
                Step step = stepsSelection.Steps[i];
                if(step == (Step)effect)
                {
                    continue;
                }

                if(step.GetType() == effect.GetType())
                {
                    ((IPositionable)step).AnchorsMode = effect.AnchorsMode;
                    ((IPositionable)step).StartAnchor.Copy(effect.StartAnchor);
                    ((IPositionable)step).EndAnchor.Copy(effect.EndAnchor);

                    ((IPositionable)step).SetAnchors();
                }
            }
        }

        void DoShowAnchors(IPositionable transAction, bool isEditable)
        {
            bool preEnabled = GUI.enabled;
            GUI.enabled = preEnabled && isEditable;

            const float labelMaxWidth = 100f;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anchor Start State:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, GUI.skin.label), GUILayout.MaxWidth(labelMaxWidth));
            transAction.StartAnchor.AnchorMin = EditorGUILayout.Vector2Field("", transAction.StartAnchor.AnchorMin, GUILayout.ExpandWidth(false));
            transAction.StartAnchor.AnchorMax = transAction.StartAnchor.AnchorMin;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anchor End State:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, GUI.skin.label), GUILayout.MaxWidth(labelMaxWidth));
            transAction.EndAnchor.AnchorMin = EditorGUILayout.Vector2Field("", transAction.StartAnchor.AnchorMin, GUILayout.ExpandWidth(false));
            transAction.EndAnchor.AnchorMax = transAction.EndAnchor.AnchorMin;
            GUILayout.EndHorizontal();
        }
    }
}
