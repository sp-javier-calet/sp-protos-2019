using ModestTree;
using UnityEditor;

namespace Zenject
{
    [CustomEditor(typeof(AdvancedCompositionRoot))]
    public class AdvancedCompositionRootEditor : UnityInspectorListEditor
    {
        protected override string[] PropertyNames
        {
            get
            {
                return new string[]
                {
					"GlobalRootInstallers"
                };
            }
        }

        protected override string[] PropertyDescriptions
        {
            get
            {
                return new string[]
                {
					"For this to work, go to Edit -> Projects Settings -> Script Execution Order and add Zenject.AdvancedCompositionRoot before Zenject.CompositionRoot"
                };
            }
        }
    }
}
