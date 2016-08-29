using UnityEditor;

[CustomEditor(typeof(AssetBundleInspectorObj))]
public sealed class AssetBundleEditor : Editor
{
    public override bool UseDefaultMargins()
    {
        return false;
    }

    public override void OnInspectorGUI()
    {
        BundleEditorDrawer.DrawInspector();
    }

    void OnEnable()
    {
        BundleEditorDrawer.CurrentBundleEditor = this;
    }
}
