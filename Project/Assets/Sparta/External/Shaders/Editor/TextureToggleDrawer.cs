using UnityEngine;
using UnityEditor;
using System;

// This property drawer script should be placed in an editor script, inside a folder called Editor.
// Use by writing "[AutoToggle(MY_TOGGLE_NAME)]" before a texture shader property.
// Leaving the texture undefined will disable the toggle (the keyword in the shader).
public class TextureToggleDrawer : MaterialPropertyDrawer
{
    protected string m_toggleName = "";


    public TextureToggleDrawer()
    {
    }

    public TextureToggleDrawer(string toggleName)
    {
        m_toggleName = toggleName;
    }


    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        if (m_toggleName == "")
            Debug.LogError("The AutoToggle shader attribute requires a keyword name!", editor.target);
        else
        {
            if (prop.type == MaterialProperty.PropType.Texture)
            { 
                if (prop.textureValue == null)
                    (editor.target as Material).DisableKeyword(m_toggleName);
                else
                    (editor.target as Material).EnableKeyword(m_toggleName);            
            }
            else
            {
                Debug.LogError("The TextureToggle shader attribute can only be used for Texture properties!", editor.target);
            }
        }

        editor.DefaultShaderProperty(prop, label);
    }


    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 0f;
    }
}
