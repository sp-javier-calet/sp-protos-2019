using SocialPoint.GUIControl;
using SocialPoint.Utils;
using UnityEditor;
using UnityEngine;

public class SafeAreaWindow : EditorWindow 
{
    Vector2 _iPhoneXScreen = new Vector2(2436f, 1125f);
    Rect _iPhoneXLandscape = new Rect(132f, 63f, 2172f, 1062f);
    Rect _iPhoneXPortrait = new Rect(0f, 102f, 1125f, 2202f);
    Vector2 _screenSize = Vector2.zero;

    bool _showSafeArea;
    bool _firstTimeShowSafeArea = true;
    Rect _safeArea;
    string _safeAreaX, _safeAreaY, _safeAreaWidth, _safeAreaHeight;
    Texture _texture;

    [MenuItem("Sparta/GUI/Safe Area Editor")]
    static void Init()
    {
        var window = (SafeAreaWindow)EditorWindow.GetWindow(typeof(SafeAreaWindow), true, "Safe Area Window");
        window.Show();
    }
        
    void Awake()
    {
        SetupSafeArea();
        ApplySafeArea();
    }

    void SetupSafeArea()
    {
        if(_screenSize == Vector2.zero)
        {
            var currentScreeSize = UnityGameWindowUtils.GetMainGameViewSize();
            _screenSize = currentScreeSize;
        }

        if(_screenSize.x == _iPhoneXScreen.x && _screenSize.y == _iPhoneXScreen.y)
        {
            // IphoneX resolution in landscape mode
            _safeAreaX = _iPhoneXLandscape.x.ToString();
            _safeAreaY = _iPhoneXLandscape.y.ToString();
            _safeAreaWidth = _iPhoneXLandscape.width.ToString();
            _safeAreaHeight = _iPhoneXLandscape.height.ToString();
        }
        else if(_screenSize.x == _iPhoneXScreen.y && _screenSize.y == _iPhoneXScreen.x)
        {
            // IphoneX resolution in portrait mode
            _safeAreaX = _iPhoneXPortrait.x.ToString();
            _safeAreaY = _iPhoneXPortrait.y.ToString();
            _safeAreaWidth = _iPhoneXPortrait.width.ToString();
            _safeAreaHeight = _iPhoneXPortrait.height.ToString();
        }
        else
        {
            _safeAreaX = "0";
            _safeAreaY = "0";
            _safeAreaWidth = _screenSize.x.ToString();
            _safeAreaHeight = _screenSize.y.ToString();
        }
    }

    void OnGUI()
    {  
        if(!Application.isPlaying)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            _showSafeArea = EditorGUILayout.Toggle("Show Safe Area:", _showSafeArea);
            if(_showSafeArea)
            {
                var currentScreeSize = UnityGameWindowUtils.GetMainGameViewSize();
                if(_screenSize != currentScreeSize)
                {
                    // If we have changed the game resolution we need to setup and apply the correct safe area
                    _screenSize = currentScreeSize;
                    SetupSafeArea();
                    ApplySafeArea();
                }
                else
                {
                    if(_firstTimeShowSafeArea)
                    {
                        _firstTimeShowSafeArea = false;
                        SetupSafeArea();
                        ApplySafeArea();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Current resolution:", _screenSize.x + "x" + _screenSize.y + " pixels");
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Safe Area Rect", EditorStyles.boldLabel);
                _safeAreaX = EditorGUILayout.TextField("x: ", _safeAreaX, GUILayout.Width(250));
                _safeAreaY = EditorGUILayout.TextField("y: ", _safeAreaY, GUILayout.Width(250));
                _safeAreaWidth = EditorGUILayout.TextField("width: ", _safeAreaWidth, GUILayout.Width(250));
                _safeAreaHeight = EditorGUILayout.TextField("height: ", _safeAreaHeight, GUILayout.Width(250));

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Reset Safe Area", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    _screenSize = currentScreeSize;
                    SetupSafeArea();
                    ApplySafeArea();
                }

                if(GUILayout.Button("Apply Safe Area", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    ApplySafeArea();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                var views = FindObjectsOfType<UISafeAreaViewController>();
                if(views.Length == 0)
                {
                    EditorGUILayout.LabelField("Help: No UIVIewcontroller with UISafeAreaViewController found in the hierarchy", EditorStyles.helpBox);
                }
                else
                {
                    EditorGUILayout.LabelField("Help: " + views.Length + " UISafeAreaViewController found in the hierarchy", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                    for(int i = 0; i < views.Length; ++i)
                    {
                        var view = views[i];
                        if(view != null)
                        {
                            var root = PrefabUtility.FindPrefabRoot(view.gameObject);
                            if(root != null)
                            {
                                EditorGUILayout.LabelField(root.name);
                            }

                            EditorGUILayout.ObjectField(view, typeof(Object), true);
                        }
                    }
                }
            }
            else
            {
                ApplySafeArea(new Rect(0f, 0f, _screenSize.x, _screenSize.y));
                _firstTimeShowSafeArea = true;
            }

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.LabelField("Help: Safe Area window helper is only available to use when Unity Editor is not playing. Use Admin Panel options instead.", EditorStyles.helpBox);
        }

        Repaint();
    }
        
    void ApplySafeArea()
    {
        float valueX;
        float.TryParse(_safeAreaX, out valueX);

        float valueY;
        float.TryParse(_safeAreaY, out valueY);

        float valueWidth;
        float.TryParse(_safeAreaWidth, out valueWidth);

        float valueHeight;
        float.TryParse(_safeAreaHeight, out valueHeight);

        _safeArea = new Rect(valueX, valueY, valueWidth, valueHeight);

        ApplySafeArea(_safeArea);
    }

    void ApplySafeArea(Rect rect)
    {
        var ratioX = Screen.width / _screenSize.x;
        var ratioY = Screen.height / _screenSize.y;

        var finalRect = new Rect(rect.x * ratioX, rect.y * ratioY, rect.width * ratioX, rect.height * ratioY);

        var views = FindObjectsOfType<UISafeAreaViewController>();
        for(int i = 0; i < views.Length; ++i)
        {
            var view = views[i];
            if(view != null)
            {
                view.ApplySafeArea(finalRect);
                view.ApplyGizmoSafeArea(rect);
                view.ShowGizmos = rect.width != _screenSize.x && rect.height != _screenSize.y;
            }
        }
    }

    static float ConvertRectToScreenResolution(float value, float screenValue, float gameScreenValue)
    {
        return value * screenValue / gameScreenValue;
    }
}