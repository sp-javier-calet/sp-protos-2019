using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.Utils.Decorators
{
    public abstract class TransformResetter : DecoratorEditor
    {
        protected static Vector3 m_resetPosition = Vector3.zero;
        protected static Quaternion m_resetRotation = Quaternion.identity;
        protected static Vector3 m_resetScale = Vector3.one;

        public TransformResetter(string name) : base(name)
        {
        }

        protected virtual void ResetPosition()
        {
        }

        protected virtual void ResetRotation()
        {
        }

        protected virtual void ResetScale()
        {
        }

        void ResetAll()
        {
            ResetPosition();
            ResetRotation();
            ResetScale();
        }
            
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reset", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.75f, 1f, 0f);

            if(GUILayout.Button("Position", EditorStyles.miniButtonMid))
            {
                ResetPosition();
            }

            if(GUILayout.Button("Rotation", EditorStyles.miniButtonMid))
            {
                ResetRotation();
            }

            if(GUILayout.Button("Scale", EditorStyles.miniButtonRight))
            {
                ResetScale();
            }
                
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(0.25f, 0.75f, 0f);
            if(GUILayout.Button("All", EditorStyles.miniButton))
            {
                ResetAll();
            }
        }
    }
}
