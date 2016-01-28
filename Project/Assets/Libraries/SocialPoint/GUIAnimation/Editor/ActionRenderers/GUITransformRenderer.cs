using UnityEngine;
using UnityEditor;

namespace SocialPoint.GUIAnimation
{
	public class GUITransformRenderer : BaseGUIActionRenderer
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
			IPositionable transEffect = (IPositionable) action;
			
			UnityEngine.GUI.changed = false;

			transEffect.AnchorsMode = (AnchorMode) EditorGUILayout.EnumPopup(transEffect.AnchorsMode, GUILayout.ExpandWidth(false));

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

			if(UnityEngine.GUI.changed)
			{
				transEffect.SetAnchors();
				
				CopyAnchorsToSelection(transEffect, stepsSelection);
				
				if(onChanged != null)
				{
					onChanged(action);
				}
			}

			UnityEngine.GUI.changed = false;

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Is Local:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(150f));
			transEffect.IsLocal = EditorGUILayout.Toggle(transEffect.IsLocal, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
			GUILayout.EndHorizontal();

			if(transEffect is TransformEffect)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Freeze Position:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(150f));
				((TransformEffect)transEffect).FreezePosition = EditorGUILayout.Toggle(((TransformEffect)transEffect).FreezePosition, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Freeze Rotation:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(150f));
				((TransformEffect)transEffect).FreezeRotation = EditorGUILayout.Toggle(((TransformEffect)transEffect).FreezeRotation, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Freeze Scale:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(150f));
				((TransformEffect)transEffect).FreezeScale = EditorGUILayout.Toggle(((TransformEffect)transEffect).FreezeScale, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
				GUILayout.EndHorizontal();
			}
			if(UnityEngine.GUI.changed)
			{
				if(onChanged != null)
				{
					onChanged(action);
				}
			}
		}

		void CopyAnchorsToSelection(IPositionable effect, StepsSelection stepsSelection)
		{
			for (int i = 0; i < stepsSelection.Count; ++i) 
			{
				Step step = stepsSelection.Steps[i];
				if(step == (Step) effect)
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
			bool preEnabled = UnityEngine.GUI.enabled;
			UnityEngine.GUI.enabled = preEnabled && isEditable;

			float labelMaxWidth = 100f;

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Start Anchor:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(labelMaxWidth));
			transAction.StartAnchor.AnchorMin = EditorGUILayout.Vector2Field("", transAction.StartAnchor.AnchorMin, GUILayout.ExpandWidth(false));
			transAction.StartAnchor.AnchorMax = transAction.StartAnchor.AnchorMin;
			GUILayout.EndHorizontal();
			
//			GUILayout.BeginHorizontal();
//			EditorGUILayout.LabelField("Start Anchor Max:", AnimationUtility.GetStyle(AnimationUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(labelMaxWidth));
//			transAction.StartAnchor.AnchorMax = EditorGUILayout.Vector2Field("", transAction.StartAnchor.AnchorMax, GUILayout.ExpandWidth(false));
//			GUILayout.EndHorizontal();

			Rect lastRect = GUILayoutUtility.GetLastRect();
			Vector2 startPos = lastRect.position + new Vector2(100f, lastRect.size.y);
			Vector2 endPos = startPos + new Vector2(100f, 0f);
			Color prevHandlesColor = Handles.color;
			Handles.color = new Color(1f, 1f, 1f, 0.25f);
			Handles.DrawLine(startPos, endPos);
			Handles.color = prevHandlesColor;

			GUILayout.Space(10f);
			
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("End Anchor:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(labelMaxWidth));
			transAction.EndAnchor.AnchorMin = EditorGUILayout.Vector2Field("", transAction.EndAnchor.AnchorMin, GUILayout.ExpandWidth(false));
			transAction.EndAnchor.AnchorMax = transAction.EndAnchor.AnchorMin;
			GUILayout.EndHorizontal();
			UnityEngine.GUI.enabled = preEnabled;
			
//			GUILayout.BeginHorizontal();
//			EditorGUILayout.LabelField("End Anchor Max:", AnimationUtility.GetStyle(AnimationUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.MaxWidth(labelMaxWidth));
//			transAction.EndAnchor.AnchorMax = EditorGUILayout.Vector2Field("", transAction.EndAnchor.AnchorMax, GUILayout.ExpandWidth(false));
//			GUILayout.EndHorizontal();

		}
	}
}
