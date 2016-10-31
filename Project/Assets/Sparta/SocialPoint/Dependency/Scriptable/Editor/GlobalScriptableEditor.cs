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
            var configurer = (GlobalScriptableConfigurer)target;
            //var installers = configurer.installers;

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
                var installer = i as IScriptableInstaller;
                if(installer.Type == ModuleType.Service)
                {
                    var obj = (UnityEngine.Object)installer;
                    EditorGUILayout.LabelField(obj.name, EditorStyles.boldLabel);
                    installer.Enabled = EditorGUILayout.Toggle("Enabled", installer.Enabled, EditorStyles.toggle);
                    var editor = CreateEditor(obj);
                    editor.OnInspectorGUI();
                    EditorUtility.SetDirty(obj);
                    EditorGUILayout.Space();
                }
            }


            //_installers.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
