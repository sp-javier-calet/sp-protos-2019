using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SocialPoint.Base;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.Exporter
{
    [CustomEditor(typeof(ExporterContainer))]
    public class ExporterContainerEditor : UnityEditor.Editor
    {
        ExporterContainer TargetExporter
        {
            get
            {
                return (ExporterContainer)target;
            }
        }

        List<Type> _exporterTypes;

        List<Type> ExporterTypes
        {
            get
            {
                if(_exporterTypes == null)
                {
                    _exporterTypes = new List<Type>();
                    foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            foreach(Type t in a.GetTypes())
                            {
                                if(!t.IsAbstract && typeof(BaseExporter).IsAssignableFrom(t))
                                {
                                    _exporterTypes.Add(t);
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Log.w("Exception while exporting Types: " + e);
                        }
                    }
                    _exporterTypes.Sort((x, y) => x.Name.CompareTo(y.Name));
                }
                return _exporterTypes;
            }
        }

        string[] _exporterTypeNames;

        string[] ExporterTypeNames
        {
            get
            {
                if(_exporterTypeNames == null)
                {
                    _exporterTypeNames = new string[ExporterTypes.Count];
                    for(int i = 0; i < ExporterTypes.Count; ++i)
                    {
                        _exporterTypeNames[i] = ExporterTypes[i].FullName;
                    }
                }
                return _exporterTypeNames;
            }
        }

        string GetRelativePath(string path)
        {
            return path.Substring(Application.dataPath.Length + 1);
        }

        void GUIExportPathInput()
        {
            EditorGUILayout.BeginHorizontal();
            var path = TargetExporter.ExportPath;
            path = EditorGUILayout.TextField("Export path", TargetExporter.ExportPath);
            if(GUILayout.Button("Browse", GUILayout.MaxWidth(60)))
            {
                path = EditorUtility.OpenFolderPanel("Select Export Folder", path, path);

                // Check for cancelled popup
                if(string.IsNullOrEmpty(path) || !path.StartsWith(Application.dataPath))
                {
                    path = TargetExporter.ExportPath;
                    EditorUtility.DisplayDialog("Invalid folder", "You must select a folder located inside of\n<Project folder>/Assets", "OK");
                }
                else
                {
                    path = GetRelativePath(path);
                }
            }

            TargetExporter.ExportPath = path;

            EditorGUILayout.EndHorizontal();
        }

        bool GUIExporter(BaseExporter exporter, bool foldout)
        {
            if(exporter != null)
            {
                EditorGUILayout.BeginHorizontal();
                foldout = EditorGUILayout.Foldout(foldout, exporter.ToString());
                if(GUILayout.Button("Apply", GUILayout.MaxWidth(50)))
                {
                    _currentOutputView = ProjectExportersOutputView.Show("");
                    TargetExporter.Export(exporter, _currentOutputView);
                }
                if(GUILayout.Button("-", GUILayout.MaxWidth(30)))
                {
                    TargetExporter.Exporters.Remove(exporter);
                    UnityEngine.Object.DestroyImmediate(exporter, true);
                }
                EditorGUILayout.EndHorizontal();
                if(foldout)
                {
                    UnityEditor.Editor.CreateEditor(exporter).DrawDefaultInspector();
                }
            }
            return foldout;
        }

        int _selectedAddExporterIndex = 0;

        void GUIAddExporter()
        {
            EditorGUILayout.BeginHorizontal();
            _selectedAddExporterIndex = EditorGUILayout.Popup("Add exporter", _selectedAddExporterIndex, ExporterTypeNames);
            if(GUILayout.Button("+", GUILayout.MaxWidth(60)))
            {
                if(_selectedAddExporterIndex >= 0)
                {
                    if(ExporterTypes.Count > 0)
                    {
                        var exporter = (BaseExporter)ScriptableObject.CreateInstance(ExporterTypes[_selectedAddExporterIndex]);
                        TargetExporter.Exporters.Add(exporter);
                        AssetDatabase.AddObjectToAsset(exporter, TargetExporter);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("No Exporters Available", "Before adding an exporter, you must create a class that inherits from SocialPoint.Exporter.BaseExporter", "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        Dictionary<BaseExporter, bool> _folds = new Dictionary<BaseExporter, bool>();

        void GUIExporters()
        {
            GUILayout.Label("Project Exporters", EditorStyles.boldLabel);
            TargetExporter.CleanEmptyExporters();
            var exporters = TargetExporter.Exporters;
            for(int i = 0; i < exporters.Count; ++i)
            {
                var exporter = exporters[i];
                if(!_folds.ContainsKey(exporter))
                {
                    _folds.Add(exporter, false);
                }
                _folds[exporter] = GUIExporter(exporters[i], _folds[exporter]);
            }
        }

        ProjectExportersOutputView _currentOutputView = null;

        IEnumerator _exportEnumerator;

        void GUIApplyExporters()
        {
            if(GUILayout.Button("Apply All Exporters", GUILayout.ExpandWidth(true)))
            {
                _currentOutputView = ProjectExportersOutputView.Show("");
                TargetExporter.Export(_currentOutputView);
            }
        }

        void GUITags()
        {
            GUILayout.Label("Project Export Tags", EditorStyles.boldLabel);;

            var prop = serializedObject.FindProperty("Tags");
            var drawZone = GUILayoutUtility.GetRect(0f, 16f);
            var showChildren = EditorGUI.PropertyField(drawZone, prop);
            var toBeContinued = prop.NextVisible(showChildren);

            var listElement = 0;
            while (toBeContinued)
            {
                drawZone = GUILayoutUtility.GetRect(0f, 16f);
                showChildren = EditorGUI.PropertyField(drawZone, prop);
                toBeContinued = prop.NextVisible(showChildren);
                listElement++;
            }
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            GUIExporters();

            EditorGUILayout.Space();

            GUIAddExporter();

            EditorGUILayout.Space();

            GUIExportPathInput();

            EditorGUILayout.Space();

            GUIApplyExporters();

            EditorGUILayout.Space();

            GUITags();
        }
    }
}