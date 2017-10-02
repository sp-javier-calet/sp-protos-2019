using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace SocialPoint.Base
{
    [CustomEditor(typeof(BackendEnvironmentsInstaller))]
    public sealed class BackendEnvironmentsInstallerEditor : Editor
    {
        readonly static string[] NoEnvironmentOptions = { "-" };
        readonly Func<Environment, bool> ProductionEnvFilter = env => env.Type == EnvironmentType.Production;

        bool _defaultVisible = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();

            DrawDefaultInspector();

            var installer = (BackendEnvironmentsInstaller)target;
            var envs = installer.Settings.Environments;

            _defaultVisible = EditorGUILayout.Foldout(_defaultVisible, "Default Environments", EditorStyles.foldout);

            if(_defaultVisible)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Default");
                var currentDev = installer.Defaults.DefaultEnvironment;
                var defaultDev = ShowEnvironmentsPopup(envs, currentDev);
                installer.Defaults.DefaultEnvironment = defaultDev;

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Production - Android");
                var currentAndroidProd = installer.Defaults.AndroidProductionEnvironment;
                var defaultAndroidProd = ShowEnvironmentsPopup(envs, currentAndroidProd, ProductionEnvFilter);
                installer.Defaults.AndroidProductionEnvironment = defaultAndroidProd;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Production - iOS");
                var currentIosProd = installer.Defaults.IosProductionEnvironment;
                var defaultIosProd = ShowEnvironmentsPopup(envs, currentIosProd, ProductionEnvFilter);
                installer.Defaults.IosProductionEnvironment = defaultIosProd;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Production - Others");
                var currentProd = installer.Defaults.CommonProductionEnvironment;
                var defaultProd = ShowEnvironmentsPopup(envs, currentProd, ProductionEnvFilter);
                installer.Defaults.CommonProductionEnvironment = defaultProd;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        public string ShowEnvironmentsPopup(Environment[] environments, string current, Func<Environment, bool> filter = null)
        {
            var index = 0;
            var names = new List<string>();
            foreach(var env in environments)
            {
                if(filter == null || filter(env))
                {
                    names.Add(env.Name);
                    if(env.Name == current)
                    {
                        index = names.Count - 1;
                    }
                }
            }

            string[] options = names.Count > 0 ? names.ToArray() : NoEnvironmentOptions;
            index = EditorGUILayout.Popup(index, options, EditorStyles.popup);
            return options[index];
        }
    }
}