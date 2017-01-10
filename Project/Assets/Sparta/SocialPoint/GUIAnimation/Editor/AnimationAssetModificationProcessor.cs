
namespace SocialPoint.GUIAnimation
{
    public class AnimationAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            GUIAnimationTool.ResetTimeLine();
            return paths;
        }
    }
}
