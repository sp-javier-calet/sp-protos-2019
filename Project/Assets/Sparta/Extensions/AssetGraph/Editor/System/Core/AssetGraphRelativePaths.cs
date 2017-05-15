using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

namespace AssetBundleGraph
{
    //[InitializeOnLoad]
    public static class AssetGraphRelativePaths
    {
        private const string ARROW_NAME = "AssetGraph_Arrow";
        public static readonly string RELATIVE_FOLDER;
        public static readonly string SCRIPT_TEMPLATE_PATH;
        public static readonly string ASSET_PLACEHOLDER_FOLDER;
        public static readonly string RESOURCE_BASEPATH;
        public static readonly string RESOURCE_NODEPATH;
        public static readonly string RESOURCE_ICONS;

        public const float NODE_BASE_WIDTH = 120f;
        public const float NODE_BASE_HEIGHT = 40f;

        public const float NODE_WARP_WIDTH = 40f;

        public const float CONNECTION_ARROW_WIDTH = 12f;
        public const float CONNECTION_ARROW_HEIGHT = 15f;

        public const float INPUT_POINT_WIDTH = 21f;
        public const float INPUT_POINT_HEIGHT = 29f;

        public const float OUTPUT_POINT_WIDTH = 10f;
        public const float OUTPUT_POINT_HEIGHT = 23f;

        public const float FILTER_OUTPUT_SPAN = 32f;

        public const float CONNECTION_POINT_MARK_SIZE = 19f;

        public const float CONNECTION_CURVE_LENGTH = 10f;

        public const float TOOLBAR_HEIGHT = 20f;

        public static readonly string RESOURCE_ARROW;

        public static readonly string RESOURCE_CONNECTIONPOINT_ENABLE;
        public static readonly string RESOURCE_CONNECTIONPOINT_INPUT;
        public static readonly string RESOURCE_CONNECTIONPOINT_OUTPUT;
        public static readonly string RESOURCE_CONNECTIONPOINT_OUTPUT_CONNECTED;

        public static readonly string RESOURCE_INPUT_BG;
        public static readonly string RESOURCE_OUTPUT_BG;

        public static readonly string RESOURCE_SELECTION;

        public static readonly string RESOURCE_NODE_GREY;
        public static readonly string RESOURCE_NODE_GREY_ON;
        public static readonly string RESOURCE_NODE_GREY_HIGHLIGHT;
        public static readonly string RESOURCE_NODE_BLUE;
        public static readonly string RESOURCE_NODE_BLUE_ON;
        public static readonly string RESOURCE_NODE_BLUE_HIGHLIGHT;
        public static readonly string RESOURCE_NODE_AQUA;
        public static readonly string RESOURCE_NODE_AQUA_ON;
        public static readonly string RESOURCE_NODE_AQUA_HIGHLIGHT;
        public static readonly string RESOURCE_NODE_ORANGE;
        public static readonly string RESOURCE_NODE_ORANGE_ON;
        public static readonly string RESOURCE_NODE_ORANGE_HIGHLIGHT;
        public static readonly string RESOURCE_NODE_RED;
        public static readonly string RESOURCE_NODE_RED_ON;
        public static readonly string RESOURCE_NODE_RED_HIGHLIGHT;
        public static readonly string RESOURCE_NODE_YELLOW;
        public static readonly string RESOURCE_NODE_YELLOW_ON;
        public static readonly string RESOURCE_NODE_YELLOW_HIGHLIGHT;
        public static readonly string RESOURCE_NODE_GREEN;
        public static readonly string RESOURCE_NODE_GREEN_ON;
        public static readonly string RESOURCE_NODE_GREEN_HIGHLIGHT;

        public static readonly string RESOURCE_FOLDER_TEX;
        public static readonly string RESOURCE_INHERITED_FOLDER_TEX;
        public static readonly string RESOURCE_FOLDER_TEX_MINI;
        public static readonly string RESOURCE_INHERITED_FOLDER_TEX_MINI;

        static AssetGraphRelativePaths()
        {
            RELATIVE_FOLDER = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(ARROW_NAME)[0])))) + "/";
            SCRIPT_TEMPLATE_PATH = RELATIVE_FOLDER + "ScriptTemplate/";
            ASSET_PLACEHOLDER_FOLDER = RELATIVE_FOLDER + "AssetPlaceholders/";
            RESOURCE_BASEPATH = RELATIVE_FOLDER + "GUI/GraphicResources/";
            RESOURCE_NODEPATH = RESOURCE_BASEPATH + "Nodes/";
            RESOURCE_ICONS = RESOURCE_BASEPATH + "ProjectIcons/";

            RESOURCE_ARROW = RESOURCE_BASEPATH + ARROW_NAME + ".png";

            RESOURCE_CONNECTIONPOINT_ENABLE = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_EnableMark.png";
            RESOURCE_CONNECTIONPOINT_INPUT = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_InputMark.png";
            RESOURCE_CONNECTIONPOINT_OUTPUT = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_OutputMark.png";
            RESOURCE_CONNECTIONPOINT_OUTPUT_CONNECTED = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_OutputMark_Connected.png";

            RESOURCE_INPUT_BG = RESOURCE_BASEPATH + "AssetGraph_InputBG.png";
            RESOURCE_OUTPUT_BG = RESOURCE_BASEPATH + "AssetGraph_OutputBG.png";
            RESOURCE_SELECTION = RESOURCE_BASEPATH + "AssetGraph_Selection.png";

            RESOURCE_NODE_GREY = RESOURCE_NODEPATH + "grey.png";
            RESOURCE_NODE_GREY_ON = RESOURCE_NODEPATH + "grey_on.png";
            RESOURCE_NODE_GREY_HIGHLIGHT = RESOURCE_NODEPATH + "grey_highlight.png";
            RESOURCE_NODE_BLUE = RESOURCE_NODEPATH + "blue.png";
            RESOURCE_NODE_BLUE_ON = RESOURCE_NODEPATH + "blue_on.png";
            RESOURCE_NODE_BLUE_HIGHLIGHT = RESOURCE_NODEPATH + "blue_highlight.png";
            RESOURCE_NODE_AQUA = RESOURCE_NODEPATH + "aqua.png";
            RESOURCE_NODE_AQUA_ON = RESOURCE_NODEPATH + "aqua_on.png";
            RESOURCE_NODE_AQUA_HIGHLIGHT = RESOURCE_NODEPATH + "aqua_highlight.png";
            RESOURCE_NODE_ORANGE = RESOURCE_NODEPATH + "orange.png";
            RESOURCE_NODE_ORANGE_ON = RESOURCE_NODEPATH + "orange_on.png";
            RESOURCE_NODE_ORANGE_HIGHLIGHT = RESOURCE_NODEPATH + "orange_highlight.png";
            RESOURCE_NODE_RED = RESOURCE_NODEPATH + "red.png";
            RESOURCE_NODE_RED_ON = RESOURCE_NODEPATH + "red_on.png";
            RESOURCE_NODE_RED_HIGHLIGHT = RESOURCE_NODEPATH + "red_highlight.png";
            RESOURCE_NODE_YELLOW = RESOURCE_NODEPATH + "yellow.png";
            RESOURCE_NODE_YELLOW_ON = RESOURCE_NODEPATH + "yellow_on.png";
            RESOURCE_NODE_YELLOW_HIGHLIGHT = RESOURCE_NODEPATH + "yellow_highlight.png";
            RESOURCE_NODE_GREEN = RESOURCE_NODEPATH + "green.png";
            RESOURCE_NODE_GREEN_ON = RESOURCE_NODEPATH + "green_on.png";
            RESOURCE_NODE_GREEN_HIGHLIGHT = RESOURCE_NODEPATH + "green_highlight.png";

            RESOURCE_FOLDER_TEX = RESOURCE_ICONS + "folder.png";
            RESOURCE_INHERITED_FOLDER_TEX = RESOURCE_ICONS + "folder_inherit.png";
            RESOURCE_FOLDER_TEX_MINI = RESOURCE_ICONS + "folder_mini.png";
            RESOURCE_INHERITED_FOLDER_TEX_MINI = RESOURCE_ICONS + "folder_inherit_mini.png";


            var oldSettingsPath = Application.dataPath + "/AssetGraph";
            if(Directory.Exists(oldSettingsPath))
            {
                var destPath = Application.dataPath + "/Sparta/Config/AssetGraph";

                if(!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }

                foreach(string dir in Directory.GetDirectories(oldSettingsPath))
                {
                    var dirFolder = dir + "/";
                    var newDir = dirFolder.Replace(oldSettingsPath, destPath);
                    if(Directory.Exists(newDir))
                    {
                        Debug.Log("Replacing folder " + newDir);
                        Directory.Delete(newDir, true);
                    }
                    Directory.Move(dirFolder, newDir);
                }
                Directory.Delete(oldSettingsPath, true);

                EditorUtility.DisplayDialog("AssetGraph Upgrade", "The AssetGraph Settings folder has been auto relocated due to a version upgrade, before commiting please open the Graph and check if it is correct and has not been corrupted.\n\nIf you have any doubts contact the Tools department.", "Ok");
            }
        }
    }
}
