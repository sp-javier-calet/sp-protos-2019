using UnityEditor;

[CustomEditor(typeof(SceneBundleInpectorObj))]
public sealed class SceneBundleEditor : Editor
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
