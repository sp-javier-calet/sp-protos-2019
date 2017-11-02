using UnityEditor;
using SocialPoint.GUIControl;

namespace SpartaTools.Editor.Utils.Decorators
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIScrollRectExtensionInspector), true)]
        public class SPScrollRectExtensionEditor : UnityEditor.Editor
    {
                public override void OnInspectorGUI()
        {
            var script = (UIScrollRectExtensionInspector)target;

            SerializedProperty scrollRectValue = serializedObject.FindProperty("_scrollRect");
            SerializedProperty verticalLayoutGroupValue = serializedObject.FindProperty("_verticalLayoutGroup");
            SerializedProperty horizontalLayoutGroupValue = serializedObject.FindProperty("_horizontalLayoutGroup");
            SerializedProperty gridLayoutGroupValue = serializedObject.FindProperty("_gridLayoutGroup");
            SerializedProperty boundsDeltaValue = serializedObject.FindProperty("_boundsDelta");
            SerializedProperty initialIndexValue = serializedObject.FindProperty("_initialIndex");
            SerializedProperty deltaDragCellValue = serializedObject.FindProperty("_deltaDragCell");
            //            SerializedProperty magnifyMinScaleValue = serializedObject.FindProperty("_maginifyMinScale");
            //            SerializedProperty magnifyMaxScaleValue = serializedObject.FindProperty("_maginifyMaxScale");
            SerializedProperty paginationValue = serializedObject.FindProperty("_pagination");
            SerializedProperty loadingGroupValue = serializedObject.FindProperty("_loadingGroup");
            SerializedProperty mainCanvasValue = serializedObject.FindProperty("_mainCanvas");
            SerializedProperty scrollAnimationTimeValue = serializedObject.FindProperty("_scrollAnimationTime");
            SerializedProperty scrollAnimationEaseTypeValue = serializedObject.FindProperty("_scrollAnimationEaseType");
            SerializedProperty scrollAnimationCurveValue = serializedObject.FindProperty("_scrollAnimationCurve");
            SerializedProperty prefabsValue = serializedObject.FindProperty("_prefabs");

            EditorGUILayout.LabelField("UI Base Components", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(scrollRectValue, true);
            EditorGUILayout.PropertyField(loadingGroupValue, true);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("UI Layout Components", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(verticalLayoutGroupValue, true);
            EditorGUILayout.PropertyField(horizontalLayoutGroupValue, true);
            EditorGUILayout.PropertyField(gridLayoutGroupValue, true);
            EditorGUILayout.LabelField("Help: Layout component references(Vertical, Horizontal, Grid). Only one layout type is allowed at same time", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Cell prefabs List", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(prefabsValue, true);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("ScrollRect Behaviour", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(initialIndexValue, true);
            EditorGUILayout.LabelField("Help: Initial index we want to access when creating ScrollRect Cells. Default value is 0", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(boundsDeltaValue, true);
            EditorGUILayout.LabelField("Help: Number of pixels that we will add to bounds to check if we need to show/hide new cells. Default value is 50px", EditorStyles.helpBox);
            EditorGUILayout.Space();

            script._usePooling = EditorGUILayout.Toggle("Use pooling", script._usePooling);
            EditorGUILayout.LabelField("Help: Use pooling to show/hide cells instead of creating/destroying cells every time", EditorStyles.helpBox);
            EditorGUILayout.Space();

            script._centerOnCell = EditorGUILayout.Toggle("Use snap", script._centerOnCell);
            EditorGUILayout.LabelField("Help: Force center cells on the center", EditorStyles.helpBox);
            EditorGUILayout.Space();

            if(script._centerOnCell)
            {
                EditorGUILayout.PropertyField(deltaDragCellValue, true);
                EditorGUILayout.LabelField("Help: Number of pixels that we will allow when dragging cells to move to previous/next. Default value is 50px", EditorStyles.helpBox);
                EditorGUILayout.Space();

//                EditorGUILayout.LabelField("Magnify effects", EditorStyles.boldLabel);
//                EditorGUILayout.PropertyField(magnifyMinScaleValue, true);
//                EditorGUILayout.PropertyField(magnifyMaxScaleValue, true);
//                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Pagination", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(paginationValue, true);
                EditorGUILayout.LabelField("Help: Pagination component reference if we want to enable pagination to Scroll View. Only works if 'Use snap' is checked", EditorStyles.helpBox);
                EditorGUILayout.Space();
                script._useNavigationButtons = EditorGUILayout.Toggle("Use navigation buttons", script._useNavigationButtons);
                script._usePaginationButtons = EditorGUILayout.Toggle("Use pagination buttons", script._usePaginationButtons);
            }
            else
            {
                script._showLastCellPosition = (UIScrollRectExtensionInspector.ShowLastCellPosition)EditorGUILayout.EnumPopup("Last row behaviour", script._showLastCellPosition);
                EditorGUILayout.LabelField("Help: If we want to show the last cell in the beginning scrolling position or at last scrolling position", EditorStyles.helpBox);
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Scroll Animation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(scrollAnimationTimeValue, true);
            EditorGUILayout.PropertyField(scrollAnimationEaseTypeValue, true);
            EditorGUILayout.PropertyField(scrollAnimationCurveValue, true);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mainCanvasValue, true);
        }
    }
}