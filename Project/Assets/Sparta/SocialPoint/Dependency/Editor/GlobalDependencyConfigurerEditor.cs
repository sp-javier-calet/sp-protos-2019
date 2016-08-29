using UnityEditor;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
	[CustomEditor(typeof(GlobalDependencyConfigurer))]
	public sealed class GlobalDependencyConfigurerEditor : UnityEditor.Editor
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