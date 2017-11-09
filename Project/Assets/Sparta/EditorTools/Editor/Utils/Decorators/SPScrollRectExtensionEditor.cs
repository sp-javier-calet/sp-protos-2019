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

            SerializedProperty scrollRectValue = serializedObject.FindProperty("ScrollRect");
            SerializedProperty verticalLayoutGroupValue = serializedObject.FindProperty("VerticalLayoutGroup");
            SerializedProperty horizontalLayoutGroupValue = serializedObject.FindProperty("HorizontalLayoutGroup");
            SerializedProperty gridLayoutGroupValue = serializedObject.FindProperty("GridLayoutGroup");
            SerializedProperty boundsDeltaValue = serializedObject.FindProperty("BoundsDelta");
            SerializedProperty initialIndexValue = serializedObject.FindProperty("InitialIndex");
            SerializedProperty deltaDragCellValue = serializedObject.FindProperty("DeltaDragCell");
            SerializedProperty paginationValue = serializedObject.FindProperty("Pagination");
            SerializedProperty loadingGroupValue = serializedObject.FindProperty("LoadingGroup");
            SerializedProperty mainCanvasValue = serializedObject.FindProperty("MainCanvas");
            SerializedProperty scrollAnimationTimeValue = serializedObject.FindProperty("ScrollAnimationTime");
            SerializedProperty scrollAnimationEaseTypeValue = serializedObject.FindProperty("ScrollAnimationEaseType");
            SerializedProperty scrollAnimationCurveValue = serializedObject.FindProperty("ScrollAnimationCurve");
            SerializedProperty disableDragWhileScrollingAnimationValue = serializedObject.FindProperty("DisableDragWhileScrollingAnimation");
            SerializedProperty prefabsValue = serializedObject.FindProperty("BasePrefabs");
            SerializedProperty useNavigationButttonsValue = serializedObject.FindProperty("UseNavigationButtons");
            SerializedProperty usePaginationButttonsValue = serializedObject.FindProperty("UsePaginationButtons");
            SerializedProperty usePoolingValue = serializedObject.FindProperty("UsePooling");
            SerializedProperty centerOnCellValue = serializedObject.FindProperty("CenterOnCell");

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

            EditorGUILayout.PropertyField(usePoolingValue, true);
            EditorGUILayout.LabelField("Help: Use pooling to show/hide cells instead of creating/destroying cells every time", EditorStyles.helpBox);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(centerOnCellValue, true);
            EditorGUILayout.LabelField("Help: Force snapping cells on the center of scroll", EditorStyles.helpBox);
            EditorGUILayout.Space();

            if(centerOnCellValue.boolValue)
            {
                EditorGUILayout.PropertyField(deltaDragCellValue, true);
                EditorGUILayout.LabelField("Help: Number of pixels that we will allow when dragging cells to move to previous/next. Default value is 50px", EditorStyles.helpBox);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(paginationValue, true);
                EditorGUILayout.LabelField("Help: Pagination component reference if we want to enable pagination to Scroll View. Only works if 'Use snap' is checked", EditorStyles.helpBox);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(useNavigationButttonsValue, true);
                EditorGUILayout.PropertyField(usePaginationButttonsValue, true);
                EditorGUILayout.Space();
            }
            else
            {
                script.LastCellPosition = (UIScrollRectExtensionInspector.ShowLastCellPosition)EditorGUILayout.EnumPopup("Last row behaviour", script.LastCellPosition);
                EditorGUILayout.LabelField("Help: If we want to show the last cell in the beginning scrolling position or at last scrolling position", EditorStyles.helpBox);
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Scroll Animation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(scrollAnimationTimeValue, true);
            EditorGUILayout.PropertyField(scrollAnimationEaseTypeValue, true);
            EditorGUILayout.PropertyField(scrollAnimationCurveValue, true);
            EditorGUILayout.PropertyField(disableDragWhileScrollingAnimationValue, true);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mainCanvasValue, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}