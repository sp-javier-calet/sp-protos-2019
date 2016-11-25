using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace SocialPoint.Dependency
{
    [CustomEditor(typeof(GlobalDependencyConfigurer))]
    public sealed class GlobalDependencyConfigurerEditor : UnityEditor.Editor
    {
        GUIStyle EnabledInstaller { get; set; }
        GUIStyle DisabledInstaller { get; set; }

        sealed class InstallerData
        {
            public Installer Installer;
            public bool Visible;

            public string LowerCaseName;

            public InstallerData(Installer installer)
            {
                Installer = installer;
                LowerCaseName = installer.name.ToLower();
            }
        }

        InstallerData[] _installers;

        string _filter = string.Empty;
        string _filterString = string.Empty;

        void OnEnable()
        {
            var configurer = (GlobalDependencyConfigurer)target;
            var installers = Load(configurer);

            _installers = new InstallerData[installers.Length];

            for(var i = 0; i < _installers.Length; ++i)
            {
                var data = new InstallerData(installers[i]);
                _installers[i] = data;
            }

            EnabledInstaller = new GUIStyle(EditorStyles.foldout);
            DisabledInstaller = new GUIStyle(EditorStyles.foldout);
            SetStyleColor(DisabledInstaller, Color.gray);
        }

        static void SetStyleColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.onNormal.textColor = color;
            style.active.textColor = color;
            style.onActive.textColor = color;
            style.focused.textColor = color;
            style.onFocused.textColor = color;
            style.hover.textColor = color;
            style.onHover.textColor = color;
        }

        static Installer[] Load(GlobalDependencyConfigurer configurer)
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

        public void GUIToolbar()
        {
            _filter = EditorGUILayout.TextField("Search", _filter);
            _filterString = _filter.ToLower();
            EditorGUILayout.Space();
        }

        public override void OnInspectorGUI()
        {
            GUIToolbar();

            serializedObject.Update();

            foreach(var data in _installers)
            {
                if(!IsFiltered(data))
                {
                    var installer = data.Installer;
                
                    var style = installer.Enabled ? EnabledInstaller : DisabledInstaller;
                    data.Visible = EditorGUILayout.Foldout(data.Visible, installer.name, style);
                    if(data.Visible)
                    {
                        var editor = CreateEditor(installer);
                        editor.OnInspectorGUI();
                        EditorUtility.SetDirty(installer);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        bool IsFiltered(InstallerData data)
        {
            if(data.Installer.Type != ModuleType.Service)
            {
                return true;
            }

            if(string.IsNullOrEmpty(_filter))
            {
                return false;
            }

            return !data.LowerCaseName.Contains(_filterString);
        }
    }
}