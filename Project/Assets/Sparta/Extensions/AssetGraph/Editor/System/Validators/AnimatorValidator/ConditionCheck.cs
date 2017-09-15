using UnityEditor;
using UnityEngine;
using System;
using UnityEditor.Animations;

namespace AssetBundleGraph
{
    [Serializable]
    public class ConditionCheck
    {
        [SerializeField]
        public string parameter;
        [SerializeField]
        public AnimatorConditionMode mode;
        [SerializeField]
        public float threshold;

        public void OnInspectorGUI(Action onValueChanged, Action OnRemoved)
        {

            using(new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    var newName = EditorGUILayout.TextField("Condition Name", parameter);

                    if(newName != parameter)
                    {
                        parameter = newName;
                        onValueChanged();
                    }
                    if(GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        OnRemoved();
                        onValueChanged();
                    }
                }
                var newMode = (AnimatorConditionMode)EditorGUILayout.EnumPopup("Condition Mode", mode);

                if(newMode != mode)
                {
                    mode = newMode;
                    onValueChanged();
                }
                var newValue = EditorGUILayout.FloatField("Condition Value", threshold);

                if(newValue != threshold)
                {
                    threshold = newValue;
                    onValueChanged();
                }
            }
        }
    }
}
