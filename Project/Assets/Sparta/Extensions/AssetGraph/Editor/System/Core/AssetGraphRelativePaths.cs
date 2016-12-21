using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

namespace AssetBundleGraph {
    [InitializeOnLoad]
    public static class AssetGraphRelativePaths {

        private const string ARROW_NAME = "AssetGraph_Arrow";
        //This asset will be at path *****/Editor/GUI/GraphicResources/AssetGraph_Arrow" so we go three times up to get the Editor/ root folder
        // This initialization needs to be done here because static constructors are executed after static field initialization.
        public static readonly string RELATIVE_FOLDER = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(ARROW_NAME)[0])))) + "/";
        public static readonly string SCRIPT_TEMPLATE_PATH = RELATIVE_FOLDER + "ScriptTemplate/";
        public static readonly string ASSET_PLACEHOLDER_FOLDER = RELATIVE_FOLDER + "AssetPlaceholders/";
        public static readonly string RESOURCE_BASEPATH = RELATIVE_FOLDER + "GUI/GraphicResources/";
        public static readonly string RESOURCE_NODEPATH = RESOURCE_BASEPATH + "Nodes/";

        public static readonly float NODE_BASE_WIDTH = 120f;
        public static readonly float NODE_BASE_HEIGHT = 40f;

        public static readonly float NODE_WARP_WIDTH = 40f;

        public static readonly float CONNECTION_ARROW_WIDTH = 12f;
        public static readonly float CONNECTION_ARROW_HEIGHT = 15f;

        public static readonly float INPUT_POINT_WIDTH = 21f;
        public static readonly float INPUT_POINT_HEIGHT = 29f;

        public static readonly float OUTPUT_POINT_WIDTH = 10f;
        public static readonly float OUTPUT_POINT_HEIGHT = 23f;

        public static readonly float FILTER_OUTPUT_SPAN = 32f;

        public static readonly float CONNECTION_POINT_MARK_SIZE = 19f;

        public static readonly float CONNECTION_CURVE_LENGTH = 10f;

        public static readonly float TOOLBAR_HEIGHT = 20f;

        public static readonly string RESOURCE_ARROW = RESOURCE_BASEPATH + ARROW_NAME + ".png";

        public static readonly string RESOURCE_CONNECTIONPOINT_ENABLE = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_EnableMark.png";
        public static readonly string RESOURCE_CONNECTIONPOINT_INPUT = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_InputMark.png";
        public static readonly string RESOURCE_CONNECTIONPOINT_OUTPUT = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_OutputMark.png";
        public static readonly string RESOURCE_CONNECTIONPOINT_OUTPUT_CONNECTED = RESOURCE_BASEPATH + "AssetGraph_ConnectionPoint_OutputMark_Connected.png";

        public static readonly string RESOURCE_INPUT_BG = RESOURCE_BASEPATH + "AssetGraph_InputBG.png";
        public static readonly string RESOURCE_OUTPUT_BG = RESOURCE_BASEPATH + "AssetGraph_OutputBG.png";

        public static readonly string RESOURCE_SELECTION = RESOURCE_BASEPATH + "AssetGraph_Selection.png";


        public static readonly string RESOURCE_NODE_GREY = RESOURCE_NODEPATH + "grey.png";
        public static readonly string RESOURCE_NODE_GREY_ON = RESOURCE_NODEPATH + "grey_on.png";
        public static readonly string RESOURCE_NODE_GREY_HIGHLIGHT = RESOURCE_NODEPATH + "grey_highlight.png";
        public static readonly string RESOURCE_NODE_BLUE = RESOURCE_NODEPATH + "blue.png";
        public static readonly string RESOURCE_NODE_BLUE_ON = RESOURCE_NODEPATH + "blue_on.png";
        public static readonly string RESOURCE_NODE_BLUE_HIGHLIGHT = RESOURCE_NODEPATH + "blue_highlight.png";
        public static readonly string RESOURCE_NODE_AQUA = RESOURCE_NODEPATH + "aqua.png";
        public static readonly string RESOURCE_NODE_AQUA_ON = RESOURCE_NODEPATH + "aqua_on.png";
        public static readonly string RESOURCE_NODE_AQUA_HIGHLIGHT = RESOURCE_NODEPATH + "aqua_highlight.png";
        public static readonly string RESOURCE_NODE_ORANGE = RESOURCE_NODEPATH + "orange.png";
        public static readonly string RESOURCE_NODE_ORANGE_ON = RESOURCE_NODEPATH + "orange_on.png";
        public static readonly string RESOURCE_NODE_ORANGE_HIGHLIGHT = RESOURCE_NODEPATH + "orange_highlight.png";
        public static readonly string RESOURCE_NODE_RED = RESOURCE_NODEPATH + "red.png";
        public static readonly string RESOURCE_NODE_RED_ON = RESOURCE_NODEPATH + "red_on.png";
        public static readonly string RESOURCE_NODE_RED_HIGHLIGHT = RESOURCE_NODEPATH + "red_highlight.png";
        public static readonly string RESOURCE_NODE_YELLOW = RESOURCE_NODEPATH + "yellow.png";
        public static readonly string RESOURCE_NODE_YELLOW_ON = RESOURCE_NODEPATH + "yellow_on.png";
        public static readonly string RESOURCE_NODE_YELLOW_HIGHLIGHT = RESOURCE_NODEPATH + "yellow_highlight.png";
        public static readonly string RESOURCE_NODE_GREEN = RESOURCE_NODEPATH + "green.png";
        public static readonly string RESOURCE_NODE_GREEN_ON = RESOURCE_NODEPATH + "green_on.png";
        public static readonly string RESOURCE_NODE_GREEN_HIGHLIGHT = RESOURCE_NODEPATH + "green_highlight.png";

    }
}
