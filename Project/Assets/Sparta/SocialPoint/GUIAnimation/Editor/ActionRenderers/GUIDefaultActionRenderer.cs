using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace SocialPoint.GUIAnimation
{
    public class GUIDefaultActionRenderer : BaseGUIActionRenderer
	{
		public override bool CanRender(Effect selectedActionTemplate)
		{
			return true;
		}

		public override void Render(Effect action, StepsSelection stepsSelection, OnActionChanged onChanged)
		{
			bool somethingShown = false;
			UnityEngine.GUI.changed = false;
			
			if(action != null)
			{
				SerializedObject serializedStep = new SerializedObject(action);
				SerializedProperty prop = serializedStep.GetIterator();
				bool moreAvailable = prop.NextVisible(true);
				while (moreAvailable)
				{
					if(CanBeShownOnInspector(action, prop.name))
					{
						somethingShown = true;
						EditorGUILayout.PropertyField(prop, true, GUILayout.ExpandWidth(false), GUILayout.MinWidth(250f));
					}
					moreAvailable = prop.NextVisible(false);
				}
				
				serializedStep.ApplyModifiedProperties();
				
				if (UnityEngine.GUI.changed)
				{
					serializedStep.ApplyModifiedProperties();
				}

				if(!somethingShown)
				{
					GUILayout.Label("No common properties,\nThe property must be changed in the object itself");
				}

				if(UnityEngine.GUI.changed)
				{
					if(onChanged != null)
					{
						CopyEffectToSelection(action, stepsSelection);
						onChanged(action);
					}
				}
			}
			else
			{
				if(!somethingShown)
				{
					GUILayout.Label("Property is disabled,\nEnable it to animate it");
				}
			}
		}
		
		bool CanBeShownOnInspector(UnityEngine.Object obj, string name)
		{
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;
			FieldInfo field = obj.GetType().GetField(name, flags);
			
			if(field != null)
			{
				ShowInEditorAttribute attribute = Attribute.GetCustomAttribute(field, typeof(ShowInEditorAttribute)) as ShowInEditorAttribute;
				if(attribute != null)
				{
					return true;
				}
			}
			
			return false;
		}
	}
}
