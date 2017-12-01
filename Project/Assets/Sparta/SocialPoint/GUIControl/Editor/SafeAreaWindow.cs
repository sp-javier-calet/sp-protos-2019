using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEditor;
using UnityEngine;

public class SafeAreaWindow : EditorWindow 
{
    bool _showSafeArea;
    Vector4 _iPhoneXSafeArea = new Vector4(132f, 63f, 2172f, 1062f);
    Color _safeAreaGizmoColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
    Texture _texture;

    [MenuItem("Sparta/GUI/Safe Area")]
    public static void ShowWindow()
    {
        GetWindow<SafeAreaWindow>(false, "Safe Area Editor", true);
    }

//    public static EditorWindow GetMainGameView(){
//        //Creates a game window. Only works if there isn't one already.
//        EditorApplication.ExecuteMenuItem("Window/Game");
//
//        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
//        System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
//        System.Object Res = GetMainGameView.Invoke(null,null);
//        return (EditorWindow)Res;
//    }
//
    public static Vector2 GetAspectRatio(Vector2 resolution)
    {
        return new Vector2(Screen.width, Screen.height);
    }

//    public static string GetMainGameViewAspectRatio()
//    {
//        var xy = GetMainGameViewSize();//EditorGUI.Vector2Field(new Rect(3, 3, Screen.width - 6, 10), "Resolution", xy);
////        xy = new Vector2(xy.x < 1 ? 1 : (int)xy.x, xy.y < 1 ? 1 : (int)xy.y);
////        if(GUI.Button(new Rect(3, 50, Screen.width - 6, 40), "Calculate Aspect Ratio\n" + result)){
//            Vector2 aspectRatio = AspectRatio.GetAspectRatio((int)xy.x, (int)xy.y);
//            result = "Aspect Ratio = " + aspectRatio.x + ":" + aspectRatio.y + " (" + xy.x + "x" + xy.y + ")";
//    }
       
    void OnGUI()
    {
        EditorGUILayout.Space();

        var resolution = UnityGameWindowUtils.GetMainGameViewSize();
        var aspectRatio = GetAspectRatio(resolution);

        EditorGUILayout.BeginVertical();


//        EditorGUILayout.LabelField("Time since start: ", EditorApplication.timeSinceStartup.ToString());

        EditorGUILayout.LabelField("Current resolution:", resolution.x + "x" + resolution.y + " pixels");
        EditorGUILayout.LabelField("Current aspect ratio:",  aspectRatio.x + ":" + aspectRatio.y);

        EditorGUILayout.Space();

        _showSafeArea = EditorGUILayout.Toggle("Show Safe Area:", _showSafeArea);
        if(_showSafeArea)
        {
            EditorGUILayout.Space();

//            EditorGUILayout.Vector4Field("iPhone X: ", _iPhoneXSafeArea);

            if(GUILayout.Button("Apply Safe Area", GUILayout.Width(100), GUILayout.Height(30)))
            {
                ApplySafeArea();
            }
        }

        EditorGUILayout.EndVertical();

//        Repaint();

//        if (_texture == null) 
//        {
//            _texture = CreateOnDrawGizmosTexture();
//
//        }
//        GUI.DrawTexture(new Rect(10, 10, 60, 60), _texture, ScaleMode.ScaleToFit, true, 10.0F);
    }

    void OnDrawGizmos()
    {
        if (_texture == null) 
        {
            _texture = CreateOnDrawGizmosTexture();
        }

        Gizmos.DrawGUITexture(new Rect(100, 100, 600, 600), _texture);
    }

    Texture CreateOnDrawGizmosTexture()
    {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

        // set the pixel values
        texture.SetPixel(0, 0, _safeAreaGizmoColor);
        texture.SetPixel(1, 0, _safeAreaGizmoColor);
        texture.SetPixel(0, 1, _safeAreaGizmoColor);
        texture.SetPixel(1, 1, _safeAreaGizmoColor);

        // Apply all SetPixel calls
        texture.Apply();

        return texture;
    }

    void OnSceneGUI()
    {


        Debug.Log("OnSceneGUI");
    }

    void ApplySafeArea()
    {
        Debug.Log(_iPhoneXSafeArea);
    }
}