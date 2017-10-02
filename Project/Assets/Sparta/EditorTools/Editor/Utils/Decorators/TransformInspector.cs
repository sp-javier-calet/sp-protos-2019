using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CustomEditor(typeof(Text)), CanEditMultipleObjects]
    public class TransformInspector : TransformResetter
    {
        SerializedProperty m_position;
        SerializedProperty m_rotation;
        SerializedProperty m_scale;

        public TransformInspector() : base("TransformInspector")
        {
        }

        void OnEnable()
        {
            m_position = serializedObject.FindProperty("m_LocalPosition");
            m_rotation = serializedObject.FindProperty("m_LocalRotation");
            m_scale = serializedObject.FindProperty("m_LocalScale");
        }

        protected override void ResetPosition()
        {
            m_position.vector3Value = m_resetPosition;
            serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        protected override void ResetRotation()
        {
            m_rotation.quaternionValue = m_resetRotation;
            serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        protected override void ResetScale()
        {
            m_scale.vector3Value = m_resetScale;
            serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }
    }
}