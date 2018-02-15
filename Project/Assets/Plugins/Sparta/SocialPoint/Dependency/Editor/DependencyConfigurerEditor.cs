using UnityEditor;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    [CustomEditor(typeof(DependencyConfigurer))]
    public sealed class DependencyConfigurerEditor : Editor
    {
        ReorderableArrayProperty _installers;

        void OnEnable()
        {
            _installers = new ReorderableArrayProperty(serializedObject, "Installers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _installers.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}