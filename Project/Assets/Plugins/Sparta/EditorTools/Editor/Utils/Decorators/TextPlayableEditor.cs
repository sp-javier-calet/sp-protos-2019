using SocialPoint.GUIControl;
using SocialPoint.TimeLinePlayables;
using SocialPoint.Utils;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomEditor(typeof(TextPlayableAsset))]
    public class TextPlayableEditor : UnityEditor.Editor
    {
        ReorderableArrayProperty _params;

        void OnEnable()
        {
            var temp = serializedObject.FindProperty("Template");
            var prop = temp.FindPropertyRelative("Params");

            _params = new ReorderableArrayProperty(serializedObject, prop, "Parameters:");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var playableTarget = (TextPlayableAsset)target;
            var template = playableTarget.Template;

            var scriptReference = serializedObject.FindProperty("m_Script");
            var oldEffect = template.Effect;

            GUI.enabled = false;
            EditorGUILayout.PropertyField(scriptReference, true);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            template.ChangeColor = EditorGUILayout.Toggle("Change Color:", template.ChangeColor);
            if(template.ChangeColor)
            {
                EditorGUI.indentLevel++;
                template.Color = EditorGUILayout.ColorField("Color:", template.Color);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            template.ChangeFontSize = EditorGUILayout.Toggle("Change Font Size:", template.ChangeFontSize);
            if(template.ChangeFontSize)
            {
                EditorGUI.indentLevel++;
                template.FontSize = EditorGUILayout.IntField("Text Size:", template.FontSize);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            template.ChangeText = EditorGUILayout.Toggle("Change Text:", template.ChangeText);
            if(template.ChangeText)
            {
                EditorGUI.indentLevel++;
                template.UseLocalizedData = EditorGUILayout.Toggle("Use Localized Data:", template.UseLocalizedData);
                if(template.UseLocalizedData)
                {
                    template.Text = EditorGUILayout.TextField("Localized Key:", template.Text);
                    template.Effect = (SPText.TextEffect)EditorGUILayout.EnumPopup("Text effect:", template.Effect);
                    _params.OnInspectorGUI();
                }
                else
                {
                    template.Text = EditorGUILayout.TextField("Text:", template.Text);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
           
            EditorGUILayout.HelpBox("Help:\n\nChanging Color and Font Size is only for the clip duration because this way we can use In/out blending.\n\nChanging Text is permanent.", MessageType.None);

            // We need to focre gui changing if some enum popup has changed
            if(oldEffect != template.Effect)
            {
                GUI.changed = true;
            }

            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
