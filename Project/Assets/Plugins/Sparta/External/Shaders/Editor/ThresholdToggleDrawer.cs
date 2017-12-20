using UnityEngine;
using UnityEditor;
using System;

// This property drawer script should be placed in an editor script, inside a folder called Editor.
// Use by writing "[AutoToggle(MY_TOGGLE_NAME, <threshold>)]" before a float shader property.
// Setting a value lower than the provided threshold disables the toggle (the keyword in the shader).
public class ThresholdToggleDrawer : MaterialPropertyDrawer
{
    protected string m_toggleName = "";
    protected float m_threshold = 0.05f;


    public ThresholdToggleDrawer()
    {
    }

    public ThresholdToggleDrawer(string toggleName)
    {
        m_toggleName = toggleName;
    }

    public ThresholdToggleDrawer(string toggleName, float threshold)
    {
        m_toggleName = toggleName;
        m_threshold = threshold;
    }


    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        if (m_toggleName == "")
            Debug.LogError("The AutoToggle shader attribute requires a keyword name!", editor.target);
        else
        {
            if (prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range)
            {
                if (prop.floatValue < m_threshold)
                    (editor.target as Material).DisableKeyword(m_toggleName);
                else
                    (editor.target as Material).EnableKeyword(m_toggleName);
            }
            else
            {
                Debug.LogError("The AutoToggle shader attribute can only be used for Float or Range properties!", editor.target);
            }
        }

        editor.DefaultShaderProperty(prop, label);
    }


    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 0f;
    }
}
