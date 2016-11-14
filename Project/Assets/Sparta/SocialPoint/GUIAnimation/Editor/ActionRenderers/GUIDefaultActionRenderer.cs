using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
            GUI.changed = false;

            if(action != null)
            {
                var serializedStep = new SerializedObject(action);
                SerializedProperty prop = serializedStep.GetIterator();
                bool moreAvailable = prop.NextVisible(true);
                while(moreAvailable)
                {
                    if(CanBeShownOnInspector(action, prop.name))
                    {
                        somethingShown = true;
                        EditorGUILayout.PropertyField(prop, true, GUILayout.ExpandWidth(false), GUILayout.MinWidth(250f));
                    }
                    moreAvailable = prop.NextVisible(false);
                }

                serializedStep.ApplyModifiedProperties();

                if(GUI.changed)
                {
                    serializedStep.ApplyModifiedProperties();
                }

                if(!somethingShown)
                {
                    GUILayout.Label("No common properties,\nThe property must be changed in the object itself");
                }

                if(GUI.changed)
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

        static bool CanBeShownOnInspector(object obj, string name)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public |
                                 BindingFlags.Instance | BindingFlags.Static;
            FieldInfo field = obj.GetType().GetField(name, flags);

            if(field != null)
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(ShowInEditorAttribute)) as ShowInEditorAttribute;
                if(attribute != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
