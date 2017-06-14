#if !BEHAVIOR_DESIGNER_STANDALONE
using UnityEngine;
using UnityEditor;
using BehaviorSourceSerializer = BehaviorDesigner.Runtime.Standalone.BehaviorSourceSerializer;

namespace BehaviorDesigner.Runtime
{
    public class BehaviorDesignerExporter : EditorWindow
    {
        public static class GUIStyles
        {
            private static GUIStyle line = null;
    
            static GUIStyles()
            {
                line = new GUIStyle("box");
                line.border.top = line.border.bottom = 1;
                line.margin.top = line.margin.bottom = 1;
                line.padding.top = line.padding.bottom = 1;
            }
    
            public static GUIStyle EditorLine
            {
                get { return line; }
            }
        }
    
        string fileName = "Behavior";
        string behaviorName = "Behavior";
        BehaviorSource _lastBehaviorSource = null;

        [MenuItem("Tools/Behavior Designer/Standalone Exporter")]
        static void Init()
        {
            BehaviorDesignerExporter window = (BehaviorDesignerExporter)EditorWindow.GetWindow(typeof(BehaviorDesignerExporter));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Standalone Exporter", EditorStyles.boldLabel);

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Selected:", EditorStyles.boldLabel, GUILayout.MaxWidth(100f));
            GUILayout.Label(behaviorName, EditorStyles.label, GUILayout.MaxWidth(100f));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            BehaviorSource behaviorSource = FindBehaviorSource();
            if(behaviorSource != null)
            {
                behaviorName = string.IsNullOrEmpty(behaviorSource.behaviorName) ? "Behavior" : behaviorSource.behaviorName;
                if(_lastBehaviorSource != behaviorSource)
                {
                    fileName = string.Format("Assets/{0}", behaviorName);
                }
            }

            _lastBehaviorSource = behaviorSource;

            GUI.enabled = behaviorSource != null;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Filename", EditorStyles.boldLabel, GUILayout.MaxWidth(100f));
            fileName = EditorGUILayout.TextField(fileName);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Space(4f);
            GUILayout.Box(GUIContent.none, GUIStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
            GUILayout.Box(GUIContent.none, GUIStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
            GUILayout.Space(4f);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Space(110f);
            if(GUILayout.Button("Export", GUILayout.MaxWidth(100f)))
            {
                ExportBehaviorTree(behaviorSource);
            }
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        BehaviorSource FindBehaviorSource()
        {
            // Load Asset
            if(Selection.activeObject != null)
            {
                Object obj = Selection.activeObject;
                if(obj is ExternalBehaviorTree)
                {
                    ExternalBehaviorTree external = obj as ExternalBehaviorTree;
                    if(external.BehaviorSource != null)
                    {
                        return external.BehaviorSource;
                    }
                }
            }

            // Load GameObject
            GameObject behaviorTreeObject = Selection.activeGameObject;
            if(behaviorTreeObject != null)
            {
                BehaviorTree behaviorTree = behaviorTreeObject.GetComponentInChildren<BehaviorTree>();
                if(behaviorTree != null)
                {
                    if(behaviorTree.ExternalBehavior != null && behaviorTree.ExternalBehavior.BehaviorSource != null)
                    {
                        return behaviorTree.ExternalBehavior.BehaviorSource;
                    }
                    return behaviorTree.mBehaviorSource;
                }
            }

            return null;
        }

        void ExportBehaviorTree(BehaviorSource behaviorSource)
        {
            BehaviorSourceSerializer.Instance.SaveFile(behaviorSource, fileName);
        }
    }
}
#endif