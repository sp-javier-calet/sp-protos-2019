using UnityEditor;
using UnityEngine;

namespace SocialPoint.Helpshift
{
    [CustomEditor(typeof(HelpshiftInstaller))]
    public sealed class HelpshiftInstallerEditor : UnityEditor.Editor
    {
        const string SerializeTooltip = "Helpshift install config is serialized to json in order to be read from native code before Unity initialization";
        static bool ActionsVisible = true;

        public override void OnInspectorGUI()
        {
            var installer = (HelpshiftInstaller)target;

            DrawDefaultInspector();

            ActionsVisible = EditorGUILayout.Foldout(ActionsVisible, "Actions");
            if(ActionsVisible)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(18);
                if(GUILayout.Button(new GUIContent("Serialize to JSON", SerializeTooltip)))
                {
                    HelpshiftConfigSerializer.Serialize(installer);    
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
    }
}
