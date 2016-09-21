using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryWindowAdmin : EditorWindow
    {
        private static GrayboxLibraryController tool;
        private static GrayboxAsset asset;
        private static int assetToLoad = 0;
        private static string newTag = "";
        private static List<string> assetTags;
        private static List<string> unassignedAssetTags;
        private string[] tagList = new string[0];
        private const float TIME_TO_SEARCH_TAGS = 0.1f;
        private float timeKeyPressed = Time.realtimeSinceStartup;
        private const float KEY_DELAY = 0.2f;
        private static int focusChangeDelay = 0;
        private float timeFilterUpdated = Time.realtimeSinceStartup;
        private bool filterUpdated = true;
        private static bool displayFilterOptions = false;
        private static string currentSelectedOption = "";

        private UnityEngine.Object mainAsset;
        private Rect assetRect = new Rect();
        private Rect packageRect = new Rect();
        private Rect thumbRect = new Rect();
        private Rect animThumbRect = new Rect();
        private static Texture2D assetPreview;
        private static Texture2D packagePreview;
        private static Texture2D thumbPreview;
        private static Texture2D animThumbPreview;

        private string previousAsset = "";
        private bool assetDrop = false;
        private string previousPkg = "";
        private bool pkgDrop = false;
        private string previousThumb = "";
        private bool thumbDrop = false;
        private string previousAnimThumb = "";
        private bool animThumbDrop = false;

        private static List<string> existingAssets;
        private string assetFilter = "";
        private float timeAssetFilterUpdated = Time.realtimeSinceStartup;
        private static bool assetFilterUpdated = true;
        private static bool displayAssetFilterOptions = false;
        private static string currentSelectedAssetOption = "";

        private GUIStyle dropAreaStyle, dropAreaTextStyle, searchOptionStyle, searchSelectedOptionStyle, separatorStyle;


        [MenuItem("Social Point/Graybox Library Admin")]
        public static void Launch()
        {
            GrayboxLibraryWindowAdmin window = (GrayboxLibraryWindowAdmin)EditorWindow.GetWindow(typeof(GrayboxLibraryWindowAdmin));
            window.titleContent.text = "Graybox Admin";
            window.minSize = new Vector2(350, 750);
            tool = new GrayboxLibraryController();
            asset = new GrayboxAsset();
            assetTags = new List<string>();
            unassignedAssetTags = new List<string>();
            existingAssets = new List<string>();

            thumbPreview = new Texture2D(1, 1);
            thumbPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
            thumbPreview.Apply();

            animThumbPreview = tool.DownloadImage(GrayboxLibraryConfig.ICONS_PATH + "animatedThumb.png");

            assetPreview = new Texture2D(1, 1);
            assetPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
            assetPreview.Apply();

            packagePreview = tool.DownloadImage(GrayboxLibraryConfig.ICONS_PATH + "package.png");
        }

        void OnGUI()
        {
            if (tool == null)
                Launch();

            if (dropAreaStyle == null)
            {
                dropAreaStyle = new GUIStyle(GUI.skin.button);
                dropAreaStyle.alignment = TextAnchor.MiddleCenter;
                dropAreaStyle.wordWrap = true;
                dropAreaStyle.hover.background = dropAreaStyle.active.background;
            }
            if (dropAreaTextStyle == null)
            {
                dropAreaTextStyle = new GUIStyle(GUI.skin.label);
                dropAreaTextStyle.alignment = TextAnchor.LowerCenter;
                dropAreaTextStyle.wordWrap = true;
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

            GUILayout.BeginVertical();
            GUILayout.Label("Add new asset to Graybox Library", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2f - 25f));
            GUILayout.Label("Category", GUILayout.Width(60));
            Type grayboxAssetCategoryType = typeof(GrayboxAssetCategory);
            asset.category = (GrayboxAssetCategory)EditorGUILayout.Popup((int)asset.category, Enum.GetNames(grayboxAssetCategoryType), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Width(20));

            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width / 2f - 15f));
            GUILayout.Label("Name", GUILayout.Width(40));
            asset.name = EditorGUILayout.TextField(asset.name, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(20));
            GUILayout.Label("", separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Main Asset", GUILayout.Width(position.width - 120));
            assetRect = GUILayoutUtility.GetRect(100, 100);
            if (GUI.Button(assetRect, asset.mainAssetPath.Length > 0 ? assetPreview : Texture2D.blackTexture, dropAreaStyle))
            {
                EditorGUIUtility.ShowObjectPicker<UnityEngine.Object>(null, false, "", 1);
            }
            if (EditorGUIUtility.GetObjectPickerControlID() == 1)
            {
                mainAsset = EditorGUIUtility.GetObjectPickerObject();
                if (mainAsset != null)
                {
                    string path = AssetDatabase.GetAssetPath(mainAsset);
                    asset.mainAssetPath = path.Length == 0 ? asset.mainAssetPath : path;
                    LoadPreview(DragAndDropItemType.asset);
                }
            }
            Rect assetLabelRect = new Rect(0, assetRect.y + 50, position.width - 120, 50);
            GUI.Label(assetLabelRect, asset.mainAssetPath.Length == 0 ? "" : asset.mainAssetPath.Substring(asset.mainAssetPath.LastIndexOf("/") + 1), dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Package", GUILayout.Width(position.width - 120));
            packageRect = GUILayoutUtility.GetRect(100, 100);
            if (GUI.Button(packageRect, asset.packagePath.Length > 0 ? packagePreview : Texture2D.blackTexture, dropAreaStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select Package", asset.packagePath.Length == 0 ? GrayboxLibraryConfig.PKG_DEFFAULT_FOLDER : asset.packagePath, "unitypackage");
                asset.packagePath = path.Length == 0 ? asset.packagePath : path;
                if (asset.name.Length == 0)
                {
                    asset.name = Path.GetFileNameWithoutExtension(asset.packagePath);
                    asset.name = asset.name.Replace("[GRAYBOX]_", "");
                }
            }
            Rect labelRect = new Rect(0, packageRect.y + 50, position.width - 120, 50);
            GUI.Label(labelRect, asset.packagePath.Length == 0 ? "" : asset.packagePath.Substring(asset.packagePath.LastIndexOf("/") + 1), dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Thumbnail", GUILayout.Width(position.width - 120));
            thumbRect = GUILayoutUtility.GetRect(100, 100);
            if (GUI.Button(thumbRect, asset.thumbnailPath.Length > 0 ? thumbPreview : Texture2D.blackTexture, dropAreaStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select Thumbnail", asset.thumbnailPath.Length == 0 ? GrayboxLibraryConfig.PKG_DEFFAULT_FOLDER : asset.thumbnailPath, "");
                asset.thumbnailPath = path.Length == 0 ? asset.thumbnailPath : path;
                LoadPreview(DragAndDropItemType.thumb);
            }
            Rect thumbLabelRect = new Rect(0, thumbRect.y + 50, position.width - 120, 50);
            GUI.Label(thumbLabelRect, asset.thumbnailPath.Length == 0 ? "" : asset.thumbnailPath.Substring(asset.thumbnailPath.LastIndexOf("/") + 1), dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Animated Thumbnail", GUILayout.Width(position.width - 120));
            animThumbRect = GUILayoutUtility.GetRect(100, 100);
            if (GUI.Button(animThumbRect, asset.animatedThumbnailPath.Length > 0 ? animThumbPreview : Texture2D.blackTexture, dropAreaStyle))
            {
                string path = EditorUtility.OpenFilePanel("Select Animated Thumbnail", asset.animatedThumbnailPath.Length == 0 ? GrayboxLibraryConfig.PKG_DEFFAULT_FOLDER : asset.animatedThumbnailPath, "gif");
                asset.animatedThumbnailPath = path.Length == 0 ? asset.animatedThumbnailPath : path;
            }
            Rect animThumbLabelRect = new Rect(0, animThumbRect.y + 50, position.width - 120, 50);
            GUI.Label(animThumbLabelRect, asset.animatedThumbnailPath.Length == 0 ? "" : asset.animatedThumbnailPath.Substring(asset.animatedThumbnailPath.LastIndexOf("/") + 1), dropAreaTextStyle);
            GUILayout.Label("", GUILayout.Width(20));
            GUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Height(10));
            GUILayout.Label("", separatorStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("", GUILayout.Height(5));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Tags", GUILayout.Width(70));
            DisplaySearchBar();
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove", GUILayout.Width(100)))
            {
                asset = tool.GetAsset(asset.name);
                if (asset != null && EditorUtility.DisplayDialog("Remove asset: " + asset.name, "WARNING! You are about to remove the asset " + asset.name + " from the database. Keep in mind that you will not be able to rollback this operation. Do you want to delete it?", "REMOVE", "Cancel"))
                {
                    tool.RemoveAsset(asset);
                    asset = new GrayboxAsset();
                }
            }

            GUILayout.Label("", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Clear", GUILayout.Width(100)))
            {
                asset = new GrayboxAsset();
            }
            if (GUILayout.Button("Save Asset", GUILayout.Width(100)))
            {
                tool.RegisterAsset(asset);
                asset = tool.GetAsset(asset.name);

                foreach (string tag in unassignedAssetTags)
                {
                    tool.UnassignTag(asset, tool.GetTag(tag));
                }

                foreach (string tag in assetTags)
                {
                    tool.AssignTag(asset, tool.GetTag(tag));
                }

                asset = new GrayboxAsset();

                EditorUtility.DisplayDialog("Graybox Library", "The asset has been succesfully registered to the Graybox Library", "Close");
            }
            GUILayout.EndVertical();

            GUILayout.Label("", GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
            GUILayout.Label("Load asset", GUILayout.Width(70));
            DisplayAssetSearchBar();
            if (GUILayout.Button("Load", GUILayout.Width(70)))
                LoadAsset();
            GUILayout.Label("", GUILayout.Width(10));
            GUILayout.EndHorizontal();
            GUILayout.Label("", GUILayout.Height(10));
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
            {
                if (displayFilterOptions)
                    currentSelectedOption = GUI.tooltip;
                else if (displayAssetFilterOptions)
                    currentSelectedAssetOption = GUI.tooltip;
            }

            ManageKeyInputs();
            ManageDragAndDrop();
        }

        private void ManageDragAndDrop()
        {
            Event evt = Event.current;

            if (DragAndDrop.paths.Length > 0)
            {
                if (animThumbRect.width > 1 && thumbRect.width > 1 && packageRect.width > 1 && assetRect.width > 1 && position.Contains(evt.mousePosition + position.position))
                {
                    string[] paths = DragAndDrop.paths;
                    if (assetRect.Contains(evt.mousePosition) && DragAndDrop.objectReferences.Length > 0)
                    {
                        mainAsset = DragAndDrop.objectReferences[0];
                        if (!assetDrop && (mainAsset == null || AssetDatabase.Contains(mainAsset)))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                            assetDrop = true;
                            previousAsset = asset.mainAssetPath;
                            asset.mainAssetPath = paths[0];
                            LoadPreview(DragAndDropItemType.asset);
                        }
                    }
                    else if (packageRect.Contains(evt.mousePosition))
                    {
                        if (!pkgDrop && DragAndDrop.paths[0].Contains(".unitypackage"))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                            pkgDrop = true;
                            previousPkg = asset.packagePath;
                            asset.packagePath = paths[0];
                            if (asset.name.Length == 0)
                            {
                                asset.name = Path.GetFileNameWithoutExtension(asset.packagePath);
                                asset.name = asset.name.Replace("[GRAYBOX]_", "");
                            }
                        }
                    }
                    else if (thumbRect.Contains(evt.mousePosition))
                    {
                        if (!thumbDrop && (DragAndDrop.paths[0].Contains(".png") || DragAndDrop.paths[0].Contains(".jpg") || DragAndDrop.paths[0].Contains(".tga")))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                            thumbDrop = true;
                            previousThumb = asset.thumbnailPath;
                            asset.thumbnailPath = paths[0];
                            LoadPreview(DragAndDropItemType.thumb);
                        }
                    }
                    else if (animThumbRect.Contains(evt.mousePosition))
                    {
                        if (!animThumbDrop && DragAndDrop.paths[0].Contains(".gif"))
                        {
                            RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                            RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                            animThumbDrop = true;
                            previousAnimThumb = asset.animatedThumbnailPath;
                            asset.animatedThumbnailPath = paths[0];
                        }
                    }
                    else
                    {
                        RecoverLastItemDragAndDrop(DragAndDropItemType.asset);
                        RecoverLastItemDragAndDrop(DragAndDropItemType.package);
                        RecoverLastItemDragAndDrop(DragAndDropItemType.thumb);
                        RecoverLastItemDragAndDrop(DragAndDropItemType.animatedThumb);
                    }
                }
            }
            else
            {
                previousPkg = asset.packagePath;
                previousThumb = asset.thumbnailPath;
                previousAnimThumb = asset.animatedThumbnailPath;

                if (previousAsset != asset.mainAssetPath && mainAsset != null && asset.mainAssetPath.Length > 0)
                {
                    if (asset.packagePath.Length == 0)
                    {
                        string[] search = Directory.GetFiles(GrayboxLibraryConfig.PKG_DEFFAULT_FOLDER, mainAsset.name + ".unitypackage", SearchOption.AllDirectories);
                        if (search.Length > 0)
                            asset.packagePath = search[0].Replace("\\", "/");
                    }
                    if (asset.thumbnailPath.Length == 0)
                    {
                        string[] search = Directory.GetFiles(GrayboxLibraryConfig.PKG_DEFFAULT_FOLDER, mainAsset.name + ".*", SearchOption.AllDirectories);
                        foreach (string found in search)
                        {
                            if (found.Contains(".png") || found.Contains(".jpg") || found.Contains(".tga"))
                            {
                                asset.thumbnailPath = found.Replace("\\", "/");
                                LoadPreview(DragAndDropItemType.thumb);
                            }
                        }
                    }
                    if (asset.animatedThumbnailPath.Length == 0)
                    {
                        string[] search = Directory.GetFiles(GrayboxLibraryConfig.PKG_DEFFAULT_FOLDER, mainAsset.name + ".*", SearchOption.AllDirectories);
                        foreach (string found in search)
                        {
                            if (found.Contains(".gif"))
                            {
                                asset.animatedThumbnailPath = found.Replace("\\", "/");
                            }
                        }
                    }
                    if (asset.name.Length == 0 && asset.packagePath.Length > 0)
                    {
                        asset.name = Path.GetFileNameWithoutExtension(asset.packagePath);
                        asset.name = asset.name.Replace("[GRAYBOX]_", "");
                    }
                }

                previousAsset = asset.mainAssetPath;
            }
        }

        public enum DragAndDropItemType { asset, package, thumb, animatedThumb };

        private void LoadPreview(DragAndDropItemType type)
        {
            switch (type)
            {
                case DragAndDropItemType.asset:

                    if (asset.mainAssetPath.Length > 0)
                    {
                        assetPreview = AssetPreview.GetMiniThumbnail(mainAsset);
                        /*assetPreview = AssetPreview.GetAssetPreview(mainAsset);
                        for (int i = 0; i < 75 && assetPreview == null; i++)
                        {
                            assetPreview = AssetPreview.GetAssetPreview(mainAsset);
                            System.Threading.Thread.Sleep(15);
                        }*/
                    }
                    else
                    {
                        assetPreview = new Texture2D(1, 1);
                        assetPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
                        assetPreview.Apply();
                    }
                    break;

                case DragAndDropItemType.thumb:
                    if (asset.thumbnailPath.Length > 0 && (asset.thumbnailPath.Contains(".png") || asset.thumbnailPath.Contains(".jpg") || asset.thumbnailPath.Contains(".tga")))
                        thumbPreview.LoadImage(File.ReadAllBytes(asset.thumbnailPath));
                    else
                    {
                        thumbPreview = new Texture2D(1, 1);
                        thumbPreview.SetPixel(0, 0, new Color(0, 0, 0, 0));
                        thumbPreview.Apply();
                    }
                    break;
            }
        }

        private void RecoverLastItemDragAndDrop(DragAndDropItemType type)
        {
            switch (type)
            {
                case DragAndDropItemType.asset:
                    if (assetDrop)
                    {
                        assetDrop = false;
                        asset.mainAssetPath = previousAsset;
                        LoadPreview(DragAndDropItemType.asset);
                    }
                    break;
                case DragAndDropItemType.package:
                    if (pkgDrop)
                    {
                        pkgDrop = false;
                        asset.packagePath = previousPkg;
                        if (asset.name.Length == 0)
                        {
                            asset.name = Path.GetFileNameWithoutExtension(asset.packagePath);
                            asset.name = asset.name.Replace("[GRAYBOX]_", "");
                        }
                    }
                    break;
                case DragAndDropItemType.thumb:
                    if (thumbDrop)
                    {
                        thumbDrop = false;
                        asset.thumbnailPath = previousThumb;
                        LoadPreview(DragAndDropItemType.thumb);
                    }
                    break;
                case DragAndDropItemType.animatedThumb:
                    if (animThumbDrop)
                    {
                        animThumbDrop = false;
                        asset.animatedThumbnailPath = previousAnimThumb;
                    }
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
                            if (displayFilterOptions && currentSelectedOption.Length != 0)
                                AddTag(currentSelectedOption);

                            else if (displayAssetFilterOptions)
                            {
                                assetFilter = currentSelectedAssetOption;
                                displayAssetFilterOptions = false;
                                LoadAsset();
                            }

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
                        else if (displayAssetFilterOptions && timeKeyPressed + KEY_DELAY < Time.realtimeSinceStartup)
                        {
                            int currentIndex = existingAssets.IndexOf(currentSelectedAssetOption);
                            if (currentIndex < existingAssets.Count - 1)
                                currentSelectedAssetOption = existingAssets[currentIndex + 1];
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
                        else if (displayAssetFilterOptions && timeKeyPressed + KEY_DELAY < Time.realtimeSinceStartup)
                        {
                            int currentIndex = existingAssets.IndexOf(currentSelectedAssetOption);
                            if (currentIndex > 0)
                                currentSelectedAssetOption = existingAssets[currentIndex - 1];
                            timeKeyPressed = Time.realtimeSinceStartup;
                        }
                        break;
                }
            }
        }

        private void DisplayTags()
        {
            for (int i = 0; i < assetTags.Count; i++)
            {
                string tag = assetTags[i];

                if (GUILayout.Button("x  " + tag, GUILayout.ExpandWidth(false)))
                {
                    unassignedAssetTags.Add(tag);
                    assetTags.Remove(tag);
                    break;
                }
            }
        }

        private void DisplaySearchBar()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            int previousFilterCount = assetTags.Count;
            DisplayTags();
            if (previousFilterCount != assetTags.Count)
                DisplayTags();

            string previousFilter = newTag;

            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(3));
            GUI.SetNextControlName("SearchBar");
            newTag = GUILayout.TextField(newTag, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUILayout.EndVertical();

            if (previousFilter != newTag && newTag.Length > 0)
            {
                filterUpdated = true;
                timeFilterUpdated = Time.realtimeSinceStartup;
            }

            if (filterUpdated && timeFilterUpdated + TIME_TO_SEARCH_TAGS < Time.realtimeSinceStartup)
            {
                filterUpdated = false;
                displayFilterOptions = false;
                tagList = tool.GetTagsAsText(newTag, 0, 10);
                if (tagList.Length > 0 && newTag.Length > 0)
                    displayFilterOptions = true;
            }

            GUILayout.BeginVertical(GUILayout.Width(22), GUILayout.Height(25));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(1));
            if (GUILayout.Button("+", GUILayout.Width(22), GUILayout.Height(22)))
            {
                if (newTag.Length > 0 && EditorUtility.DisplayDialog("Create New Tag", "You are about to create a new tag. Are you sure?", "Yes, create it", "Cancel"))
                {
                    GrayboxTag tag = new GrayboxTag();
                    tag.name = newTag;
                    tool.CreateTag(tag);
                    assetTags.Add(tag.name);
                    newTag = "";
                }
            }
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (displayFilterOptions && newTag.Length > 0)
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

        private void DisplayAssetSearchBar()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            string previousAssetFilter = assetFilter;

            GUILayout.BeginVertical();
            GUILayout.Label("", GUILayout.Height(3));
            assetFilter = GUILayout.TextField(assetFilter, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            GUILayout.EndVertical();

            if (previousAssetFilter != assetFilter && assetFilter.Length > 0)
            {
                assetFilterUpdated = true;
                timeAssetFilterUpdated = Time.realtimeSinceStartup;
            }

            if (assetFilterUpdated && timeAssetFilterUpdated + TIME_TO_SEARCH_TAGS < Time.realtimeSinceStartup)
            {
                assetFilterUpdated = false;
                displayAssetFilterOptions = false;
                string[] filterSplited = assetFilter.Split(' ');
                existingAssets = new List<string>(tool.GetAssetsByNameAsText(filterSplited, asset.category, 0, 10));

                if (existingAssets.Count > 0 && assetFilter.Length > 0)
                    displayAssetFilterOptions = true;
            }

            GUILayout.BeginVertical(GUILayout.Width(22), GUILayout.Height(25));
            GUILayout.Label("", GUILayout.Height(3), GUILayout.Width(1));
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (displayAssetFilterOptions && assetFilter.Length > 0)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                for (int i = 0; i < existingAssets.Count; i++)
                {
                    string option = existingAssets[i];

                    if (currentSelectedAssetOption == option)
                    {
                        if (GUILayout.Button(new GUIContent(option, option), searchSelectedOptionStyle))
                        {
                            assetFilter = option;
                            displayAssetFilterOptions = false;
                            LoadAsset();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(new GUIContent(option, option), searchOptionStyle))
                        {
                            assetFilter = option;
                            displayAssetFilterOptions = false;
                            LoadAsset();
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();

        }

        public void LoadAsset()
        {
            asset = tool.GetAsset(existingAssets[assetToLoad]);
            assetTags = new List<string>(tool.GetAssetTagsAsText(asset));
            mainAsset = AssetDatabase.LoadMainAssetAtPath(asset.mainAssetPath);
            LoadPreview(DragAndDropItemType.asset);
            LoadPreview(DragAndDropItemType.thumb);
        }

        public static void AddTag(string tag)
        {
            assetTags.Add(tag);
            newTag = "";
            currentSelectedOption = "";
            displayFilterOptions = false;
            focusChangeDelay = 1;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnDestroy()
        {
            tool.Disconnect();
        }
    }
}

