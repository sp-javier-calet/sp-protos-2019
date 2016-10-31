using UnityEditor;
using SocialPoint.Utils;
using System;
using System.Reflection;

namespace SocialPoint.Dependency
{
	[CustomEditor(typeof(GlobalDependencyConfigurer))]
	public sealed class GlobalDependencyConfigurerEditor : UnityEditor.Editor
	{
        Installer[] _installers;

        void OnEnable()
        {
            var configurer = (GlobalDependencyConfigurer)target;
            _installers = Load(configurer);
        }

        Installer[] Load(GlobalDependencyConfigurer configurer)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                foreach(var t in assembly.GetTypes())
                {
                    if(t.BaseType == typeof(Installer))
                    {
                        InstallerAssetsManager.Create(t);
                    }
                }
            }

            configurer.Installers = InstallerAssetsManager.Installers;
            return configurer.Installers;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach(var installer in _installers)
            {
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