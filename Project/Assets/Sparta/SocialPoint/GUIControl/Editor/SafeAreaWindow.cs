using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEditor;
using UnityEngine;
using SocialPoint.GUIControl;

public class SafeAreaWindow : EditorWindow 
{
    bool _showSafeArea;
    Rect _safeArea = new Rect(132f, 63f, 2172f, 1062f);
    Vector2 _screenSize = Vector2.zero;
    Texture _texture;

    [MenuItem("Sparta/GUI/Safe Area Editor")]
    static void Init()
    {
        var window = (SafeAreaWindow)EditorWindow.GetWindow(typeof(SafeAreaWindow));
        window.Show();
    }
        
    public static Vector2 GetAspectRatio(Vector2 resolution)
    {
        return new Vector2(Screen.width, Screen.height);
    }

    void OnGUI()
    {
        var _currentScreeSize = UnityGameWindowUtils.GetMainGameViewSize();
        if(_screenSize == Vector2.zero)
        {
            _screenSize = _currentScreeSize;
        }
        else if(_showSafeArea && _screenSize != _currentScreeSize)
        {
            _screenSize = _currentScreeSize;
            ApplySafeArea();
        }

        EditorGUILayout.Space();

        var aspectRatio = GetAspectRatio(_screenSize);

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Current resolution:", _screenSize.x + "x" + _screenSize.y + " pixels");
        EditorGUILayout.LabelField("Current aspect ratio:",  aspectRatio.x + ":" + aspectRatio.y);

        EditorGUILayout.Space();

        _showSafeArea = EditorGUILayout.Toggle("Show Safe Area:", _showSafeArea);
        if(_showSafeArea)
        {
            EditorGUILayout.Space();

            if(GUILayout.Button("Apply Safe Area", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ApplySafeArea();
            }
        }

        EditorGUILayout.EndVertical();
    }
        
    void ApplySafeArea()
    {
        var views = FindObjectsOfType<UISafeAreaViewController>();
        for(int i = 0; i < views.Length; ++i)
        {
            views[i].ApplySafeArea(_safeArea);
        }
    }
}