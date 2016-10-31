using UnityEditor;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    [CustomEditor(typeof(GlobalScriptableConfigurer))]
    public sealed class GlobalScriptableEditor : UnityEditor.Editor
    {
        ReorderableArrayProperty _installers;
        UnityEditor.Editor _editor;

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
            var installers = configurer.installers;

            foreach(var i in installers)
            {
                var installer = i as IScriptableInstaller;
                if(installer.Type == ModuleType.Service)
                {
                    var obj = (UnityEngine.Object)installer;
                    CreateCachedEditor(obj, null, ref _editor);
                    EditorGUILayout.LabelField(obj.name, EditorStyles.boldLabel);
                    _editor.OnInspectorGUI();
                }
            }


            //_installers.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
