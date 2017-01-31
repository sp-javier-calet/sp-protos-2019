﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace SocialPoint.Base
{
    [CustomEditor(typeof(BackendEnvironmentsInstaller))]
    public sealed class BackendEnvironmentsInstallerEditor : UnityEditor.Editor
    {
        readonly static string[] NoEnvironmentOptions = new string[] { "-" };

        bool _defaultVisible = true;

        public override void OnInspectorGUI()
        {
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
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Production");
                var currentProd = installer.Defaults.ProductionEnvironment;
                var defaultProd = ShowEnvironmentsPopup(envs, currentProd, env => env.Type == EnvironmentType.Production);
                installer.Defaults.ProductionEnvironment = defaultProd;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorUtility.SetDirty(installer);
            }

            serializedObject.ApplyModifiedProperties();
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