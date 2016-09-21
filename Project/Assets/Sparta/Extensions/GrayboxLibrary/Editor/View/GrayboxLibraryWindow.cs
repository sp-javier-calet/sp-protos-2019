using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWindow : EditorWindow
    {
        private static ArrayList currentAssetList;
        private static ArrayList currentGUIContent;
        public static GrayboxLibraryController tool;
        private static List<GrayboxAsset> toInstanciate;
        private static List<GrayboxAsset> toDownload;

        private static string[] categories;
        public static string filter = "";
        public static List<string> filters = new List<string>();
        private float timeFilterUpdated = Time.realtimeSinceStartup;
        private static bool filterUpdated = false;
        private static bool displayFilterOptions = false;
        private static string currentSelectedOption = "";
        private string[] tagList = new string[0];
        private const float TIME_TO_SEARCH_TAGS = 0.1f;
        private static int currentCategory = 0;
        private static int currentPage = 0;
        private static int maxPage = 0;
        private Vector2 scrollPos;
        private GUIStyle buttonStyle, buttonAreaStyle, bottomMenuStyle, bottomMenuTextStyle, bottomMenuTextBoldStyle, searchOptionStyle, searchSelectedOptionStyle, separatorStyle;
        public static float thumbWidth = 640;
        public static float thumbHeight = 480;
        public static float animatedThumbWidth = 640;
        public static float animatedThumbHeight = 480;
        private float thumbSizeMultiplier = 0.3f;
        private const float THUMB_MIN_SIZE = 0.1f;
        private const float THUMB_MAX_SIZE = 0.5f;
        private const int ASSETS_PER_PAGE = 20;
        public static GrayboxLibraryWindow window;
        private float timeKeyPressed = Time.realtimeSinceStartup;
        private const float KEY_DELAY = 0.2f;
        private static int focusChangeDelay = 0;
        public static GrayboxAsset assetChosen = null;
        public static GrayboxAsset assetDragged = null;
        private static bool dragging = false;
        private string currentDraggedAsset = "";
        private bool secondGUIDraw = false;
        private static GrayboxLibraryInspectorDummy inspectorDummyA, inspectorDummyB;
        private static int currentInspectorDummy;

        [MenuItem("Social Point/Graybox Library/Buildings")]
        public static void LaunchBuldingsClient()
        {
            currentCategory = (int)GrayboxAssetCategory.Buildings;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Props")]
        public static void LaunchPropsClient()
        {
            currentCategory = (int)GrayboxAssetCategory.Props;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Fx")]
        public static void LaunchFxClient()
        {
            currentCategory = (int)GrayboxAssetCategory.Fx;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Characters")]
        public static void LaunchCharactersClient()
        {
            currentCategory = (int)GrayboxAssetCategory.Characters;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/Vehicles")]
        public static void LaunchVehiclesClient()
        {
            currentCategory = (int)GrayboxAssetCategory.Vehicles;
            LaunchClient();
        }

        [MenuItem("Social Point/Graybox Library/UI")]
        public static void LaunchUIClient()
        {
            currentCategory = (int) GrayboxAssetCategory.UI;
            LaunchClient();
        }

        public static void LaunchClient()
        {
            window = (GrayboxLibraryWindow)EditorWindow.GetWindow(typeof(GrayboxLibraryWindow));
            window.titleContent.text = "Library";
            tool = new GrayboxLibraryController();
            currentGUIContent = new ArrayList();
            currentAssetList = tool.GetAssets(filters.ToArray(), (GrayboxAssetCategory)currentCategory, currentPage * ASSETS_PER_PAGE, ASSETS_PER_PAGE);
            LoadThumbnails();
            toInstanciate = new List<GrayboxAsset>();
            toDownload = new List<GrayboxAsset>();
            maxPage = (int)Math.Ceiling(tool.GetAssetCount(filters.ToArray(), (GrayboxAssetCategory)currentCategory) / (float)ASSETS_PER_PAGE);
            categories = Enum.GetNames(typeof(GrayboxAssetCategory));
            filterUpdated = true;
        }


        void OnGUI()
        {
            if (tool == null)
                LaunchClient();

            ManageDragAndDrop();

            if (Event.current.clickCount == 2 && assetChosen != null && secondGUIDraw)
                InstantiateAsset();

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.label);
                buttonStyle.border = new RectOffset(0, 0, 0, 0);
                buttonStyle.margin = new RectOffset(10, 10, 10, 0);
            }
            if (buttonAreaStyle == null)
            {
                buttonAreaStyle = new GUIStyle(GUI.skin.label);
                Texture2D texH = new Texture2D(1, 1);
                texH.SetPixel(0, 0, new Color(0f, 0.2f, 0.5f, 0.5f));
                texH.Apply();
                buttonAreaStyle.active.background = Texture2D.blackTexture;
                buttonAreaStyle.hover.background = texH;

                buttonAreaStyle.border = new RectOffset(0, 0, 0, 0);
                buttonAreaStyle.margin = new RectOffset(0, 0, 0, 10);
            }
            if (bottomMenuStyle == null)
            {
                bottomMenuStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.25f, 0.25f, 0.25f, 1f));
                tex.Apply();
                bottomMenuStyle.normal.background = tex;
            }
            if (separatorStyle == null)
            {
                separatorStyle = new GUIStyle(GUI.skin.label);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 1f));
                tex.Apply();
                separatorStyle.normal.background = tex;
                separatorStyle.border = new RectOffset(0, 0, 0, 0);
                separatorStyle.margin = separatorStyle.border;
            }
            if (bottomMenuTextStyle == null)
            {
                bottomMenuTextStyle = new GUIStyle(GUI.skin.label);
                bottomMenuTextStyle.fontSize = 12;
            }
            if (bottomMenuTextBoldStyle == null)
            {
                bottomMenuTextBoldStyle = new GUIStyle(bottomMenuTextStyle);
                bottomMenuTextBoldStyle.fontStyle = FontStyle.Bold;
            }
            if (searchOptionStyle == null)
            {
                searchOptionStyle = new GUIStyle(GUI.skin.label);
                searchOptionStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                searchOptionStyle.alignment = TextAnchor.UpperLeft;
                searchOptionStyle.active.textColor = new Color(0.6f, 0.6f, 0.6f);
                searchOptionStyle.hover.textColor = new Color(1f, 1f, 1f);
                Texture2D texH = new Texture2D(1, 1);
                texH.SetPixel(0, 0, new Color(0f, 0.2f, 0.5f, 0.5f));
                texH.Apply();
                searchOptionStyle.active.background = Texture2D.blackTexture;
                searchOptionStyle.hover.background = texH;
                searchOptionStyle.border = new RectOffset(0, 0, 0, 0);
                searchOptionStyle.margin = searchOptionStyle.border;
            }
            if (searchSelectedOptionStyle == null)
            {
                searchSelectedOptionStyle = new GUIStyle(searchOptionStyle);
                searchSelectedOptionStyle.normal = searchSelectedOptionStyle.hover;
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(bottomMenuStyle);
            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(7));
            int previousCategory = currentCategory;
            currentCategory = EditorGUILayout.Popup(currentCategory, categories, GUILayout.Width(120), GUILayout.Height(20));
            if (previousCategory != currentCategory)
            {
                Search(filters);
            }

            GUILayout.EndVertical();

            GUILayout.Label("", GUILayout.Width(100));

            DisplaySearchBar();

            GUILayout.EndHorizontal();
            GUILayout.Label("", separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(false));
            if (thumbSizeMultiplier == THUMB_MIN_SIZE)
            {
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                for (int i = 0; i < currentAssetList.Count; i++)
                {
                    GrayboxAsset asset = (GrayboxAsset)currentAssetList[i];

                    if (Event.current.clickCount < 2 && GUILayout.Button(asset.name, searchOptionStyle))
                    {
                        assetChosen = asset;
                        DisplayInspector();
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("", GUILayout.Height(15));
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                int column = 0;
                for (int i = 0; i < currentAssetList.Count; i++)
                {
                    float buttonHeight = thumbHeight * thumbSizeMultiplier;
                    float buttonWidth = thumbWidth * thumbSizeMultiplier;

                    if (((column + 1) * (buttonWidth + 30)) > position.width)
                    {
                        column = 0;
                        GUILayout.Label("", GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    }

                    GrayboxAsset asset = (GrayboxAsset)currentAssetList[i];

                    GUILayout.BeginVertical(buttonAreaStyle, GUILayout.MaxWidth(buttonWidth));

                    if (Event.current.clickCount < 2 && GUILayout.Button((GUIContent)currentGUIContent[i], buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight), GUILayout.ExpandWidth(false)))
                    {
                        assetChosen = asset;
                        DisplayInspector();
                    }

                    GUILayout.Label(asset.name, GUILayout.Width(buttonWidth));
                    GUILayout.EndVertical();

                    column++;
                }
                GUILayout.Label("", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Label("", separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));

            GUILayout.BeginHorizontal(bottomMenuStyle, GUILayout.Height(25));
            GUILayout.Label("Page " + (currentPage + 1) + "/" + maxPage, bottomMenuTextStyle, GUILayout.Width(80));

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("", GUILayout.ExpandWidth(true));
            if (currentPage > 0)
            {
                if (GUILayout.Button("<<", bottomMenuTextStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    changePage(0);

                if (GUILayout.Button("<", bottomMenuTextStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    changePage(currentPage - 1);
            }
            else
                GUILayout.Label("", GUILayout.Width(40), GUILayout.Height(20));

            if (maxPage > 1)
            {
                for (int i = 1; i <= maxPage && i < 10; i++)
                {
                    if (currentPage == (i - 1))
                        GUILayout.Button(i.ToString(), bottomMenuTextBoldStyle, GUILayout.Width(15), GUILayout.Height(20));

                    else
                    {
                        if (GUILayout.Button(i.ToString(), bottomMenuTextStyle, GUILayout.Width(15), GUILayout.Height(20)))
                            changePage(i - 1);
                    }
                }
            }

            if (currentPage < (maxPage - 1))
            {
                if (GUILayout.Button(">", bottomMenuTextStyle, GUILayout.Width(15), GUILayout.Height(20)))
                    changePage(currentPage + 1);

                if (GUILayout.Button(">>", bottomMenuTextStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    changePage(maxPage - 1);
            }
            else
                GUILayout.Label("", GUILayout.Width(40), GUILayout.Height(20));

            GUILayout.Label("", GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            thumbSizeMultiplier = GUILayout.HorizontalSlider(thumbSizeMultiplier, THUMB_MIN_SIZE, THUMB_MAX_SIZE, GUILayout.Width(50));

            if (GUILayout.Button("Contact", GUILayout.Width(100)))
                Application.OpenURL(GrayboxLibraryConfig.CONTACT_URL);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (focusChangeDelay > 0)
                focusChangeDelay--;

            else if (focusChangeDelay == 0)
            {
                focusChangeDelay = -1;
                EditorGUI.FocusTextInControl("");
                EditorGUI.FocusTextInControl("SearchBar");
            }

            if (GUI.tooltip.Length > 0)
                currentSelectedOption = GUI.tooltip;

            if (secondGUIDraw)
            {
                secondGUIDraw = false;
                currentDraggedAsset = GUI.tooltip;
            }
            else
                secondGUIDraw = true;

            if (dragging && assetDragged != null)
            {
                GUI.DrawTexture(new Rect(Event.current.mousePosition, new Vector2(thumbWidth / 1.5f, thumbHeight / 1.5f)), assetDragged.thumbnail);
                EditorGUIUtility.AddCursorRect(new Rect(Vector2.zero, position.size), MouseCursor.MoveArrow);
            }

            ManageKeyInputs();
        }



        public void changePage(int pageIndex)
        {
            currentPage = pageIndex;
            currentAssetList = tool.GetAssets(filters.ToArray(), (GrayboxAssetCategory)currentCategory, currentPage * ASSETS_PER_PAGE, ASSETS_PER_PAGE);
            LoadThumbnails();
        }





        public static void DisplayInspector()
        {
            if (inspectorDummyA == null)
            {
                inspectorDummyA = ScriptableObject.CreateInstance<GrayboxLibraryInspectorDummy>();
                inspectorDummyA.hideFlags = HideFlags.DontSave;
            }
            if (inspectorDummyB == null)
            {
                inspectorDummyB = ScriptableObject.CreateInstance<GrayboxLibraryInspectorDummy>();
                inspectorDummyB.hideFlags = HideFlags.DontSave;
            }

            if (currentInspectorDummy == 1)
            {
                Selection.activeObject = inspectorDummyA;
                currentInspectorDummy = 0;
            }
            else
            {
                Selection.activeObject = inspectorDummyB;
                currentInspectorDummy = 1;
            }
        }




        private void DisplaySearchBar()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            int previousFilterCount = filters.Count;
            DisplayTags();
            if (previousFilterCount != filters.Count)
                DisplayTags();

            string previousFilter = filter;

            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(3));
            GUI.SetNextControlName("SearchBar");
            filter = GUILayout.TextField(filter, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUILayout.EndVertical();

            if (previousFilter != filter && filter.Length > 0)
            {
                filterUpdated = true;
                timeFilterUpdated = Time.realtimeSinceStartup;
            }

            if (filterUpdated && timeFilterUpdated + TIME_TO_SEARCH_TAGS < Time.realtimeSinceStartup)
            {
                filterUpdated = false;
                displayFilterOptions = false;
                tagList = tool.GetTagsAsText(filter, 0, 10);
                if (tagList.Length > 0 && filter.Length > 0)
                    displayFilterOptions = true;
            }

            GUILayout.BeginVertical(GUILayout.Width(22), GUILayout.Height(25));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(1));
            if (GUILayout.Button(tool.DownloadImage(GrayboxLibraryConfig.ICONS_PATH + "search.png"), GUILayout.Width(22), GUILayout.Height(22)))
                Search(filters);
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (displayFilterOptions && filter.Length > 0)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                for (int i = 0; i < tagList.Length; i++)
                {
                    string option = tagList[i];

                    if (currentSelectedOption == option)
                    {
                        if (GUILayout.Button(new GUIContent(option, option), searchSelectedOptionStyle))
                            AddTag(option);
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent(option, option), searchOptionStyle))
                            AddTag(option);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();

        }


        private void ManageDragAndDrop()
        {
            Event evt = Event.current;

            if (evt.clickCount > 0 && dragging)
            {
                if (assetDragged != null && !position.Contains(evt.mousePosition + position.position))
                    InstantiateAsset(true);
            }

            switch (evt.type)
            {
                case EventType.mouseDown:
                    if (Event.current.clickCount == 1 && assetDragged == null && currentDraggedAsset.Length > 0)
                        assetDragged = tool.GetAsset(currentDraggedAsset);
                    break;
                case EventType.mouseUp:
                    dragging = false;
                    assetDragged = null;
                    break;
                case EventType.MouseDrag:
                    dragging = true;
                    break;
            }
        }


        private void ManageKeyInputs()
        {
            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:

                        if (timeKeyPressed + KEY_DELAY < Time.realtimeSinceStartup)
                        {
                            if (currentSelectedOption.Length == 0)
                                Search(filters);
                            else
                                AddTag(currentSelectedOption);

                            timeKeyPressed = Time.realtimeSinceStartup;
                        }
                        break;

                    case KeyCode.DownArrow:

                        if (displayFilterOptions && timeKeyPressed + KEY_DELAY < Time.realtimeSinceStartup)
                        {
                            int currentIndex = ArrayUtility.IndexOf(tagList, currentSelectedOption);
                            if (currentIndex < tagList.Length - 1)
                                currentSelectedOption = tagList[currentIndex + 1];
                            timeKeyPressed = Time.realtimeSinceStartup;
                        }
                        break;

                    case KeyCode.UpArrow:
                        if (displayFilterOptions && timeKeyPressed + KEY_DELAY < Time.realtimeSinceStartup)
                        {
                            int currentIndex = ArrayUtility.IndexOf(tagList, currentSelectedOption);
                            if (currentIndex > 0)
                                currentSelectedOption = tagList[currentIndex - 1];
                            timeKeyPressed = Time.realtimeSinceStartup;
                        }
                        break;
                }
            }
        }

        private void DisplayTags()
        {
            for (int i = 0; i < filters.Count; i++)
            {
                string tag = filters[i];

                if (GUILayout.Button("x  " + tag, GUILayout.ExpandWidth(false)))
                {
                    RemoveTag(tag);
                    break;
                }
            }
        }

        private static void Search(List<string> filters)
        {
            currentGUIContent = new ArrayList();
            currentAssetList = tool.GetAssets(filters.ToArray(), (GrayboxAssetCategory)currentCategory, currentPage * ASSETS_PER_PAGE, ASSETS_PER_PAGE);
            LoadThumbnails();
            maxPage = (int)Math.Ceiling(tool.GetAssetCount(filters.ToArray(), (GrayboxAssetCategory)currentCategory) / (float)ASSETS_PER_PAGE);
        }

        private static void LoadThumbnails()
        {
            for (int i = 0; i < currentAssetList.Count; i++)
            {
                GrayboxAsset asset = (GrayboxAsset)currentAssetList[i];
                currentGUIContent.Add(new GUIContent(asset.thumbnail, asset.name));
            }
        }

        public static void AddTag(string tag)
        {
            filters.Add(tag);
            filter = "";
            currentSelectedOption = "";
            displayFilterOptions = false;
            focusChangeDelay = 1;
            Search(filters);
        }

        public static void RemoveTag(string tag)
        {
            filters.Remove(tag);
            Search(filters);
        }

        public static void InstantiateAsset(bool dragAndDrop = false)
        {
            if (dragAndDrop)
            {
                if (assetDragged != null)
                {
                    toDownload.Add(assetDragged);
                }
            }
            else if (assetChosen != null)
            {
                toDownload.Add(assetChosen);
            }

            dragging = false;
            assetDragged = null;
            focusChangeDelay = 0;
        }


        void Update()
        {
            if (toDownload != null)
            {
                for (int i = 0; i < toDownload.Count; i++)
                {
                    tool.DownloadAsset(toDownload[i]);
                    toInstanciate.Add(toDownload[i]);
                    toDownload.RemoveAt(i);
                }
            }

            if (toInstanciate != null)
            {
                for (int i = 0; i < toInstanciate.Count; i++)
                {
                    if (AssetDatabase.LoadMainAssetAtPath(toInstanciate[i].mainAssetPath) != null)
                    {
                        tool.InstanciateAsset(toInstanciate[i]);
                        toInstanciate.RemoveAt(i);
                    }
                }
            }
        }


        void OnDestroy()
        {
            tool.Disconnect();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}