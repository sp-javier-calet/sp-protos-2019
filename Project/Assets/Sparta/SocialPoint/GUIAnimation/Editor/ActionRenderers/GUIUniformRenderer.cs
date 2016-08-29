using UnityEngine;
using UnityEditor;

namespace SocialPoint.GUIAnimation
{
	public sealed class GUIUniformRenderer : BaseGUIActionRenderer
	{
		public override bool CanRender(Effect action)
		{
			return action is UniformEffect;
		}
		
		public override void Render(Effect effect, StepsSelection stepsSelection, OnActionChanged onChanged)
		{
			UniformEffect uniformEffect = (UniformEffect) effect;
			
			UnityEngine.GUI.changed = false;
			bool guiChanged = false;

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Uniforms:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(54f));
			int numUniforms = EditorGUILayout.IntField(uniformEffect.Values.Count, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(65f));
			GUILayout.EndHorizontal();
			UpdateNumUniforms(uniformEffect, numUniforms);

			GUILayout.Space(16f);
			for (int i = 0; i < uniformEffect.Values.Count; ++i) 
			{
				guiChanged = guiChanged || DoRenderUniform(uniformEffect.Values[i]);
				GUILayout.Space(16f);
			}
			
			if(guiChanged)
			{
				CopyEffectToSelection(effect, stepsSelection);

				if(onChanged != null)
				{
					onChanged(effect);
				}
			}
		}

		void UpdateNumUniforms(UniformEffect effect, int newUniforms)
		{
			int diff = newUniforms - effect.Values.Count;
			if(diff < 0)
			{
				effect.RemoveUniforms(-diff);
			}
			if(diff > 0)
			{
				effect.CreateUniforms(diff);
			}
		}

		bool DoRenderUniform(UniformEffect.UniformValues uniformValues)
		{
			bool guiChanged = false;

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Uniform:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(54f));
			uniformValues.UniformName = EditorGUILayout.TextField(uniformValues.UniformName, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle3, UnityEngine.GUI.skin.textField, TextAnchor.MiddleLeft), GUILayout.Width(250f));
			GUILayout.EndHorizontal();
			
			uniformValues.ValueType = (UniformEffect.UniformValueType) EditorGUILayout.EnumPopup(uniformValues.ValueType, GUILayout.ExpandWidth(false));
			
			switch (uniformValues.ValueType) 
			{
			case UniformEffect.UniformValueType.Float:
				
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Start:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
				uniformValues.FloatStartValue = EditorGUILayout.FloatField(uniformValues.FloatStartValue, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(65f));
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("End:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
				uniformValues.FloatEndValue = EditorGUILayout.FloatField(uniformValues.FloatEndValue, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(65f));
				GUILayout.EndHorizontal();
				
				guiChanged |= UnityEngine.GUI.changed;
				break;
				
			case UniformEffect.UniformValueType.Integer:
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Start:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
				uniformValues.IntStartValue = EditorGUILayout.IntField(uniformValues.IntStartValue, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(65f));
				GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("End:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
				uniformValues.IntEndValue = EditorGUILayout.IntField(uniformValues.IntEndValue, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(65f));
				GUILayout.EndHorizontal();
				
				guiChanged |= UnityEngine.GUI.changed;
				break;
				
			case UniformEffect.UniformValueType.Vector:
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Start:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
				uniformValues.VectorStartValue = EditorGUILayout.Vector4Field("",uniformValues.VectorStartValue, GUILayout.ExpandWidth(false));
				
				// Color
				guiChanged |= UnityEngine.GUI.changed; UnityEngine.GUI.changed = false;
				Color tempColor = new Color(uniformValues.VectorStartValue.x, uniformValues.VectorStartValue.y, uniformValues.VectorStartValue.z, uniformValues.VectorStartValue.w);
				tempColor = EditorGUILayout.ColorField(tempColor, GUILayout.MaxWidth(65f));
				GUILayout.EndHorizontal();
				if(UnityEngine.GUI.changed)
				{
					uniformValues.VectorStartValue  = new Vector4(tempColor.r, tempColor.g, tempColor.b, tempColor.a);
				}
				guiChanged |= UnityEngine.GUI.changed;
				
				
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("End:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
				uniformValues.VectorEndValue = EditorGUILayout.Vector4Field("",uniformValues.VectorEndValue, GUILayout.ExpandWidth(false));
				
				
				// Color
				guiChanged |= UnityEngine.GUI.changed; UnityEngine.GUI.changed = false;
				tempColor = new Color(uniformValues.VectorEndValue.x, uniformValues.VectorEndValue.y, uniformValues.VectorEndValue.z, uniformValues.VectorEndValue.w);
				tempColor = EditorGUILayout.ColorField(tempColor, GUILayout.MaxWidth(65f));
				GUILayout.EndHorizontal();
				if(UnityEngine.GUI.changed)
				{
					uniformValues.VectorEndValue  = new Vector4(tempColor.r, tempColor.g, tempColor.b, tempColor.a);
				}
				guiChanged |= UnityEngine.GUI.changed;
				break;
			}

			return guiChanged;
		}
	}
}
