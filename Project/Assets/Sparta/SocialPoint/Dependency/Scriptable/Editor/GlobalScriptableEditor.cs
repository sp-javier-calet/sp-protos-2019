using UnityEditor;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    [CustomEditor(typeof(GlobalScriptableConfigurer))]
    public sealed class GlobalScriptableEditor : UnityEditor.Editor
    {
        ReorderableArrayProperty _installers;

        void OnEnable()
        {
            _installers = new ReorderableArrayProperty(serializedObject, "Installers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var configurer = (GlobalScriptableConfigurer)target;
            configurer.Load();
            var installers = configurer.installers;

            foreach(var i in installers)
            {
                var installer = i as ScriptableInstaller;
                if(installer.Type == ModuleType.Service)
                {
                    EditorGUILayout.LabelField(installer.name, EditorStyles.boldLabel);
                    installer.Enabled = EditorGUILayout.Toggle("Enabled", installer.Enabled, EditorStyles.toggle);
                    var editor = CreateEditor(installer);
                    editor.OnInspectorGUI();
                    EditorUtility.SetDirty(installer);
                    EditorGUILayout.Space();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
