﻿using UnityEditor;
using UnityEngine;

namespace SocialPoint.Exporter
{
    public static class ExporterSettings
    {
        const string _projectExporterAssetPath = "Assets/Sparta/Config/Exporters.asset";
        static ExporterContainer _exporterContainer;

        public static ExporterContainer ExporterContainer
        {
            get
            {
                if(_exporterContainer == null)
                {
                    _exporterContainer = AssetDatabase.LoadAssetAtPath<ExporterContainer>(_projectExporterAssetPath);
                    if(_exporterContainer == null)
                    {
                        _exporterContainer = ScriptableObject.CreateInstance<ExporterContainer>();
                        AssetDatabase.CreateAsset(_exporterContainer, _projectExporterAssetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
                return _exporterContainer;
            }
        }

        [MenuItem("Sparta/Project/Exporters")]
        public static void Edit()
        {
            Selection.activeObject = ExporterContainer;
        }
    }
}