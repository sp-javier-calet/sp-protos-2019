using UnityEditor;
using UnityEditor.UI;
using SocialPoint.GUIControl;

namespace UnityEngine.UI.Extensions
{
    [CanEditMultipleObjects, CustomEditor(typeof(NonDrawingGraphic), false)]
    public class SPNonDrawingGraphicEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Script, new GUILayoutOption[0]);

            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}