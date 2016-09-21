using UnityEngine;
using UnityEditor;
using System;

// The property drawer class should be placed in an editor script, inside a folder called Editor.
// Use with "[BasicColorsButtons]" before a float shader property

public class BasicColorsButtonsDrawer : MaterialPropertyDrawer
{
    private readonly Color[] basicColors = {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.white,
        Color.gray,
        Color.black
    };


    private GUIStyle m_colorButtonsStyle;


    public BasicColorsButtonsDrawer()
    {
        // Load the UI images that Unity uses internally.
        Texture2D focusedButtonImg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd").texture;
        Texture2D bgrButtonImg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd").texture;

        // Set up the GUI style for the color buttons.
        m_colorButtonsStyle = new GUIStyle();
        m_colorButtonsStyle.normal.background = bgrButtonImg;
        m_colorButtonsStyle.hover.background = focusedButtonImg;
        m_colorButtonsStyle.active.background = focusedButtonImg;
        m_colorButtonsStyle.focused.background = focusedButtonImg;
        /*colorButtonsStyle.onActive.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd").texture;
        colorButtonsStyle.onFocused.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd").texture;
        colorButtonsStyle.onHover.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd").texture;
        colorButtonsStyle.onNormal.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd").texture;*/
        m_colorButtonsStyle.fixedHeight = 12f;
        m_colorButtonsStyle.alignment = TextAnchor.MiddleCenter;
        m_colorButtonsStyle.border = new RectOffset(4, 4, 2, 2);
        m_colorButtonsStyle.margin = new RectOffset(2, 2, 2, 8);
        m_colorButtonsStyle.stretchWidth = true;
        m_colorButtonsStyle.stretchHeight = true;
    }


    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        Color previousColor = prop.colorValue;

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        Rect colorFieldRect = new Rect(position);
        Color newColor = EditorGUI.ColorField(colorFieldRect, label, previousColor);

        newColor = _DrawBasicColorsButtons(newColor);
        
        EditorGUI.showMixedValue = false;

        // Set the new value if it has changed
        if (EditorGUI.EndChangeCheck() || previousColor != newColor)
        {   
            prop.colorValue = newColor;
        }
    }


    private Color _DrawBasicColorsButtons(Color originalColor)
    {
        Color returnedColor = originalColor;

        Color oldGUIColor = GUI.backgroundColor;

        GUILayout.BeginHorizontal();
        foreach (Color c in basicColors)
        {
            GUI.backgroundColor = c;
            if (GUILayout.Button("", m_colorButtonsStyle))
                returnedColor = c;
        }
        GUILayout.EndHorizontal();

        GUI.backgroundColor = oldGUIColor;

        return returnedColor;
    }
}
