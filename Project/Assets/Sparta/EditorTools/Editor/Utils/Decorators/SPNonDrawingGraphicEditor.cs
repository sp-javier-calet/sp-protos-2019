using UnityEditor;
using UnityEditor.UI;
using SocialPoint.GUIControl;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CanEditMultipleObjects, CustomEditor(typeof(NonDrawingGraphic), false)]
    public class SPNonDrawingGraphicEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Script);

            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}