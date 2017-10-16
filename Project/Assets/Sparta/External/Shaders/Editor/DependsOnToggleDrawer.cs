using UnityEngine;
using UnityEditor;
using System;

// This property drawer script should be placed in an editor script, inside a folder called Editor.
// Use with "[DependsOnToggle(MY_TOGGLE_NAME)]" before a float shader property.
// This will make the property visible only if the said toggle is checked (!= 0.0)
public class DependsOnToggleDrawer : MaterialPropertyDrawer
{
    protected string m_toggleName = "";


    public DependsOnToggleDrawer()
    {
    }

    public DependsOnToggleDrawer(string toggleName)
    {
        m_toggleName = toggleName;
    }


    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        if (m_toggleName == "")
            Debug.LogError("The AutoToggle shader attribute requires a keyword name!", editor.target);
        else if ((editor.target as Material).IsKeywordEnabled(m_toggleName))
        {
            editor.DefaultShaderProperty(prop, label);
        }
    }


    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 0f;
    }
}
