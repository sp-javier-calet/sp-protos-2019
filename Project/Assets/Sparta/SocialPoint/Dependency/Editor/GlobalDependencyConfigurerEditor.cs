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
            Reload();
            try
            {
                EnabledInstaller = new GUIStyle(EditorStyles.foldout);
                DisabledInstaller = new GUIStyle(EditorStyles.foldout);
                SetStyleColor(DisabledInstaller, Color.gray);
            }
            catch(NullReferenceException)
            {
            }
        }

        void Reload()
        {
            var configurer = (GlobalDependencyConfigurer)target;
            var installers = Load(configurer);
            EditorUtility.SetDirty(configurer);

            _installers = new InstallerData[installers.Length];

            for(var i = 0; i < _installers.Length; ++i)
            {
                var data = new InstallerData(installers[i]);
                _installers[i] = data;
            }
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
            var installerType = typeof(Installer);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                // Ignore Unity-Editor assemblies
                if(assembly.GetName().Name.Contains("CSharp-Editor"))
                {
                    continue;
                }
                Debug.LogError("Dependency Configurer Name: " + assembly.GetName().Name);
                if(assembly.GetName().Name.Contains("BehaviorDesignerEditor"))
                {
                    continue;
                }

                foreach(var t in assembly.GetTypes())
                {
                    if(t.IsSubclassOf(installerType) && !t.IsAbstract)
                    {
                        InstallerAssetsManager.CreateDefault(t);
                    }
                }
            }

            configurer.Installers = InstallerAssetsManager.Installers;
            return configurer.Installers;
        }

        void Duplicate(Installer installer)
        {
            if(!InstallerAssetsManager.Duplicate(installer))
            {
                Debug.LogWarning(string.Format("Could not duplicate '{0}' installer asset", installer.name));
            }
        }

        void Delete(Installer installer)
        {
            if(InstallerAssetsManager.Delete(installer))
            {
                Reload();
                Repaint();
            }
            else
            {
                Debug.LogWarning(string.Format("Could not delete '{0}' installer asset", installer.name));
            }
        }

        void GUIToolbar()
        {
            _filter = EditorGUILayout.TextField("Search", _filter);
            _filterString = _filter.ToLower();
            EditorGUILayout.Space();
        }

        void GUIContextMenu(Installer installer)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Duplicate"), false, a => Duplicate(installer), "Duplicate");

            if(!installer.IsDefault)
            {
                menu.AddItem(new GUIContent("Delete"), false, a => Delete(installer), "Delete");
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Delete"));
            }

            menu.ShowAsContext();
        }

        Texture _actionsIcon;

        Texture ActionsIcon
        {
            get
            {
                if(_actionsIcon == null)
                {
                    _actionsIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sparta/EditorTools/Editor/EditorResources/more.icon.png");
                }
                return _actionsIcon;
            }
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
                
                    var style = installer.IsGlobal ? EnabledInstaller : DisabledInstaller;
                    var label = string.Format("{0} - {1} installer", installer.name, installer.Type);

                    GUILayout.BeginHorizontal();
                    data.Visible = EditorGUILayout.Foldout(data.Visible, label, style);
                    GUILayout.FlexibleSpace();
                    if(GUILayout.Button(ActionsIcon, EditorStyles.label, GUILayout.MaxWidth(15.0f)))
                    {
                        GUIContextMenu(data.Installer);
                    }
                    GUILayout.EndHorizontal();

                    if(data.Visible)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        EditorGUILayout.BeginVertical();

                        var editor = CreateEditor(installer);
                        editor.OnInspectorGUI();
                        EditorUtility.SetDirty(installer);

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        bool IsFiltered(InstallerData data)
        {
            if(string.IsNullOrEmpty(_filter))
            {
                return false;
            }

            return !data.LowerCaseName.Contains(_filterString);
        }
    }
}