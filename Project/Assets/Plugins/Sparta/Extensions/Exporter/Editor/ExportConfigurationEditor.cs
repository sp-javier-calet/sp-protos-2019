using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Exporter
{
    /*
     [CustomEditor(typeof(ExportConfiguration))]
    public class ExportConfigurationEditor : UnityEditor.Editor
    {
        ExporterContainer _container;
        List<string> _notUsedTags;
        List<string> _tags;

        public override void OnInspectorGUI()
        {
            var configuration = (ExportConfiguration)target;
            if(configuration == null)
            {
                return;
            }
            if(_container == null)
            {
                _container = ExporterSettings.ExporterContainer;
                _notUsedTags = new List<string>(_container.Tags);
                _notUsedTags.Insert(0, "");
            }
            if(_tags == null)
            {
                if(configuration.Tags == null)
                {
                    _tags = new List<string>();
                }
                else
                {
                    _tags = new List<string>(configuration.Tags);
                }
            }

            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            if(configuration.Tags != null)
            {
                for(var i = 0; i < configuration.Tags.Length; i++)
                {
                    var tag = configuration.Tags[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(tag);
                    if(GUILayout.Button("-", GUILayout.MaxWidth(30)))
                    {
                        _tags.Remove(tag);
                        _notUsedTags.Add(tag);
                    }
                    else
                    {
                        _notUsedTags.Remove(tag);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            var idx = EditorGUILayout.Popup("Add Tag", 0, _notUsedTags.ToArray());
            if(idx != 0)
            {
                _tags.Add(_notUsedTags[idx]);
                _notUsedTags.RemoveAt(idx);
            }
            configuration.Tags = _tags.ToArray();
        }
    }*/
}
