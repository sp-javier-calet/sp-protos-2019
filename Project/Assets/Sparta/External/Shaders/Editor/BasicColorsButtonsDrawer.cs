using UnityEngine;
using UnityEditor;
using System;
using System.IO;

// To use this in a shader, add "[BasicColorsButtons]" before a color property.
// Example: 
// [BasicColorsButtons] _MaskColor("Masked color", Color) = (1,1,1,1)

public class BasicColorsButtonsDrawer : MaterialPropertyDrawer
{
    private readonly Color[][] basicColorPalettes = {
        new Color[] {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.white,
            Color.gray,
            Color.black
        },
        new Color [] {
            new Color(0.0039f, 0.3867f, 0f),
            new Color(0.3554f, 0.4570f, 0.1679f),
            new Color(0.6914f, 0.5351f, 0.1953f),
            new Color(0.9101f, 0.8554f, 0.3671f),
            new Color(0.7031f, 0.7343f, 0.6484f),
            new Color(0.2968f, 0.4375f, 0.6406f)
        }
    };

    private readonly string paletteIndexShaderPropName = "__BASICCOLORSBUTTONDRAWER_PALETTEINDEX__";


    private int m_paletteIndex = 0;
    private GUIStyle m_colorButtonsStyle;
    private GUIStyle m_paletteButtonStyle;


    public BasicColorsButtonsDrawer()
    {
        SetupUI();
    }
    public BasicColorsButtonsDrawer(int i)
    {
        m_paletteIndex = i;
        SetupUI();
    }


    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {   
        Color previousColor = prop.colorValue;

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;

        Rect colorFieldRect = new Rect(position);
        Color newColor = editor.ColorProperty(colorFieldRect, prop, label);

        MaterialProperty paletteIndexProp = MaterialEditor.GetMaterialProperty(prop.targets, paletteIndexShaderPropName);
        if (paletteIndexProp != null && !paletteIndexProp.hasMixedValue && paletteIndexProp.name != null)
        {
            m_paletteIndex = (int)paletteIndexProp.floatValue;
        }
        int oldPaletteIndex = m_paletteIndex;
        newColor = DrawBasicColorsButtons(newColor);
        if (m_paletteIndex != oldPaletteIndex)
        {
            paletteIndexProp.floatValue = m_paletteIndex;
            editor.PropertiesChanged();
        }
        
        EditorGUI.showMixedValue = false;

        // Set the new value if it has changed
        if (EditorGUI.EndChangeCheck() || previousColor != newColor)
        {   
            prop.colorValue = newColor;
        }
    }


    private void SetupUI()
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
        m_colorButtonsStyle.fixedHeight = 16f;
        m_colorButtonsStyle.fixedWidth = 16f;
        m_colorButtonsStyle.margin = new RectOffset(2, 2, 2, 8);

        m_paletteButtonStyle = new GUIStyle();
        string paletteIconPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("editorResource_icon_palettePainBrush")[0]);
        Texture2D paletteTexture = AssetDatabase.LoadAssetAtPath(paletteIconPath, typeof(Texture2D)) as Texture2D;
        m_paletteButtonStyle.normal.background = paletteTexture;
        m_paletteButtonStyle.hover.background = paletteTexture;
        m_paletteButtonStyle.active.background = paletteTexture;
        m_paletteButtonStyle.focused.background = paletteTexture;
        m_paletteButtonStyle.fixedHeight = 16f;
        m_paletteButtonStyle.fixedWidth = 16f;
        m_paletteButtonStyle.margin = new RectOffset(12, 2, 2, 8);
    }


    private Color DrawBasicColorsButtons(Color originalColor)
    {
        Color returnedColor = originalColor;

        Color oldGUIColor = GUI.backgroundColor;

        Color[] palette = basicColorPalettes[m_paletteIndex];

        GUILayout.BeginHorizontal();

        // Add this to justify the buttons to the right.
        GUILayout.FlexibleSpace(); 

        foreach (Color c in palette)
        {
            GUI.backgroundColor = c;
            if (GUILayout.Button("", m_colorButtonsStyle))
                returnedColor = c;
        }

        GUI.backgroundColor = oldGUIColor;

        if (GUILayout.Button("", m_paletteButtonStyle))
        {
            m_paletteIndex = (m_paletteIndex + 1) % basicColorPalettes.Length;

        }
        GUILayout.EndHorizontal();

        return returnedColor;
    }
}
