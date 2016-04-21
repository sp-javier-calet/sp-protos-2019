using UnityEditor;

namespace BM.Extensions
{
    public interface BundleTreeWinExtension 
    {
        void DrawBuildButtonOptions(GenericMenu menu);
        void DrawAdditionalTabButtons();
    }
}
