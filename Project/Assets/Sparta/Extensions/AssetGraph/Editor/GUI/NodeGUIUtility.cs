using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;


namespace AssetBundleGraph
{
    public enum StyleType
    {
        Selected,
        UnSelected,
        Highlighted
    }

    public class NodeGUIUtility
    {

        public struct PlatformButton
        {
            public readonly GUIContent ui;
            public readonly BuildTargetGroup targetGroup;

            public PlatformButton(GUIContent ui, BuildTargetGroup g)
            {
                this.ui = ui;
                this.targetGroup = g;
            }
        }

        public static Action<NodeEvent> NodeEventHandler
        {
            get
            {
                return NodeSingleton.s.emitAction;
            }
            set
            {
                NodeSingleton.s.emitAction = value;
            }
        }

        public static Texture2D inputPointTex
        {
            get
            {
                if(NodeSingleton.s.inputPointTex == null)
                {
                    NodeSingleton.s.inputPointTex = AssetBundleGraphEditorWindow.LoadTextureFromFile(AssetGraphRelativePaths.RESOURCE_INPUT_BG);
                }
                return NodeSingleton.s.inputPointTex;
            }
        }

        public static Texture2D outputPointTex
        {
            get
            {
                if(NodeSingleton.s.outputPointTex == null)
                {
                    NodeSingleton.s.outputPointTex = AssetBundleGraphEditorWindow.LoadTextureFromFile(AssetGraphRelativePaths.RESOURCE_OUTPUT_BG);
                }
                return NodeSingleton.s.outputPointTex;
            }
        }

        public static Texture2D enablePointMarkTex
        {
            get
            {
                if(NodeSingleton.s.enablePointMarkTex == null)
                {
                    NodeSingleton.s.enablePointMarkTex = AssetBundleGraphEditorWindow.LoadTextureFromFile(AssetGraphRelativePaths.RESOURCE_CONNECTIONPOINT_ENABLE);
                }
                return NodeSingleton.s.enablePointMarkTex;
            }
        }

        public static Texture2D inputPointMarkTex
        {
            get
            {
                if(NodeSingleton.s.inputPointMarkTex == null)
                {
                    NodeSingleton.s.inputPointMarkTex = AssetBundleGraphEditorWindow.LoadTextureFromFile(AssetGraphRelativePaths.RESOURCE_CONNECTIONPOINT_INPUT);
                }
                return NodeSingleton.s.inputPointMarkTex;
            }
        }

        public static Texture2D outputPointMarkTex
        {
            get
            {
                if(NodeSingleton.s.outputPointMarkTex == null)
                {
                    NodeSingleton.s.outputPointMarkTex = AssetBundleGraphEditorWindow.LoadTextureFromFile(AssetGraphRelativePaths.RESOURCE_CONNECTIONPOINT_OUTPUT);
                }
                return NodeSingleton.s.outputPointMarkTex;
            }
        }

        public static Texture2D outputPointMarkConnectedTex
        {
            get
            {
                if(NodeSingleton.s.outputPointMarkConnectedTex == null)
                {
                    NodeSingleton.s.outputPointMarkConnectedTex = AssetBundleGraphEditorWindow.LoadTextureFromFile(AssetGraphRelativePaths.RESOURCE_CONNECTIONPOINT_OUTPUT_CONNECTED);
                }
                return NodeSingleton.s.outputPointMarkConnectedTex;
            }
        }

        public static PlatformButton[] platformButtons
        {
            get
            {
                if(NodeSingleton.s.platformButtons == null)
                {
                    NodeSingleton.s.SetupPlatformButtons();
                }
                return NodeSingleton.s.platformButtons;
            }
        }

        public static PlatformButton GetPlatformButtonFor(BuildTargetGroup g)
        {
            foreach(var button in platformButtons)
            {
                if(button.targetGroup == g)
                {
                    return button;
                }
            }

            throw new AssetBundleGraphException("Fatal: unknown target group requsted(can't happen)" + g);
        }

        public static List<string> allNodeNames
        {
            get
            {
                return NodeSingleton.s.allNodeNames;
            }
            set
            {
                NodeSingleton.s.allNodeNames = value;
            }
        }

        public static List<BuildTarget> SupportedBuildTargets
        {
            get
            {
                if(NodeSingleton.s.supportedBuildTargets == null)
                {
                    NodeSingleton.s.SetupSupportedBuildTargets();
                }
                return NodeSingleton.s.supportedBuildTargets;
            }
        }
        public static string[] supportedBuildTargetNames
        {
            get
            {
                if(NodeSingleton.s.supportedBuildTargetNames == null)
                {
                    NodeSingleton.s.SetupSupportedBuildTargets();
                }
                return NodeSingleton.s.supportedBuildTargetNames;
            }
        }


        public static List<BuildTargetGroup> SupportedBuildTargetGroups
        {
            get
            {
                if(NodeSingleton.s.supportedBuildTargetGroups == null)
                {
                    NodeSingleton.s.SetupSupportedBuildTargets();
                }
                return NodeSingleton.s.supportedBuildTargetGroups;
            }
        }


        public static GUIStyle GetStyle(NodeKind kind, StyleType style)
        {
            GUIStyle res = new GUIStyle("flow node 0 on");
            res.border = new RectOffset(13, 13, 13, 13);
            switch(kind)
            {
            case NodeKind.LOADER_GUI:
            case NodeKind.EXPORTER_GUI:
                {
                    if(style == StyleType.Highlighted)
                    {
                        if(NodeSingleton.s.nodeGreyHighlight == null)
                        {
                            Debug.LogWarning("Highlight texture not found, showing selected style");
                            style = StyleType.Selected;
                        }
                        else
                        {
                            res.name = "Grey node Highlight";
                            res.normal.background = NodeSingleton.s.nodeGreyHighlight;
                        }
                    }

                    if(style == StyleType.UnSelected)
                    {
                        res.name = "Grey node";
                        res.normal.background = NodeSingleton.s.nodeGrey;
                    }
                    else if(style == StyleType.Selected)
                    {
                        res.name = "Grey node On";
                        res.normal.background = NodeSingleton.s.nodeGreyOn;
                    }
                    break;
                }
            case NodeKind.FILTER_GUI:
                {
                    if(style == StyleType.Highlighted)
                    {
                        if(NodeSingleton.s.nodeBlueHighlight == null)
                        {
                            Debug.LogWarning("Highlight texture not found, showing selected style");
                            style = StyleType.Selected;
                        }
                        else
                        {
                            res.name = "Blue node Highlight";
                            res.normal.background = NodeSingleton.s.nodeBlueHighlight;
                        }
                    }
                    if(style == StyleType.UnSelected)
                    {
                        res.name = "Blue node";
                        res.normal.background = NodeSingleton.s.nodeBlue;
                    }
                    else if(style == StyleType.Selected)
                    {
                        res.name = "Blue node On";
                        res.normal.background = NodeSingleton.s.nodeBlueOn;
                    }

                    break;
                }
            case NodeKind.IMPORTSETTING_GUI:
            case NodeKind.GROUPING_GUI:
                {
                    if(style == StyleType.Highlighted)
                    {
                        if(NodeSingleton.s.nodeAquaHighlight == null)
                        {
                            Debug.LogWarning("Highlight texture not found, showing selected style");
                            style = StyleType.Selected;
                        }
                        else
                        {
                            res.name = "Aqua node Highlight";
                            res.normal.background = NodeSingleton.s.nodeAquaHighlight;
                        }
                    }
                    if(style == StyleType.UnSelected)
                    {
                        res.name = "Aqua node";
                        res.normal.background = NodeSingleton.s.nodeAqua;
                    }
                    else if(style == StyleType.Selected)
                    {
                        res.name = "Aqua node On";
                        res.normal.background = NodeSingleton.s.nodeAquaOn;
                    }
                    break;
                }
            case NodeKind.WARP_IN:
            case NodeKind.WARP_OUT:
                {
                    if(style == StyleType.Highlighted)
                    {
                        if(NodeSingleton.s.nodeOrangeHighlight == null)
                        {
                            Debug.LogWarning("Highlight texture not found, showing selected style");
                            style = StyleType.Selected;
                        }
                        else
                        {
                            res.name = "Orange node Highlight";
                            res.normal.background = NodeSingleton.s.nodeOrangeHighlight;
                        }
                    }

                    if(style == StyleType.UnSelected)
                    {
                        res.name = "Orange node";
                        res.normal.background = NodeSingleton.s.nodeOrange;
                    }
                    else if(style == StyleType.Selected)
                    {
                        res.name = "Orange node On";
                        res.normal.background = NodeSingleton.s.nodeOrangeOn;
                    }
                    break;
                }
            case NodeKind.PREFABBUILDER_GUI:
            case NodeKind.BUNDLEBUILDER_GUI:
                {
                    if(style == StyleType.Highlighted)
                    {
                        if(NodeSingleton.s.nodeRedHighlight == null)
                        {
                            Debug.LogWarning("Highlight texture not found, showing selected style");
                            style = StyleType.Selected;
                        }
                        else
                        {
                            res.name = "Red node Highlight";
                            res.normal.background = NodeSingleton.s.nodeRedHighlight;
                        }
                    }

                    if(style == StyleType.UnSelected)
                    {
                        res.name = "Red node";
                        res.normal.background = NodeSingleton.s.nodeRed;
                    }
                    else if(style == StyleType.Selected)
                    {
                        res.name = "Red node On";
                        res.normal.background = NodeSingleton.s.nodeRedOn;
                    }
                    break;
                }
            case NodeKind.BUNDLECONFIG_GUI:
            case NodeKind.MODIFIER_GUI:
                {
                    if(style == StyleType.Highlighted)
                    {
                        if(NodeSingleton.s.nodeYellowHighlight == null)
                        {
                            Debug.LogWarning("Highlight texture not found, showing selected style");
                            style = StyleType.Selected;
                        }
                        else
                        {
                            res.name = "Yellow node Highlight";
                            res.normal.background = NodeSingleton.s.nodeYellowHighlight;
                        }
                    }

                    if(style == StyleType.UnSelected)
                    {
                        res.name = "Yellow node";
                        res.normal.background = NodeSingleton.s.nodeYellow;
                    }
                    else if(style == StyleType.Selected)
                    {
                        res.name = "Yellow node On";
                        res.normal.background = NodeSingleton.s.nodeYellowOn;
                    }
                    break;
                }

            case NodeKind.VALIDATOR_GUI:
                {
                    if(style == StyleType.Highlighted)
                    {
                        if(NodeSingleton.s.nodeGreenHighlight == null)
                        {
                            Debug.LogWarning("Highlight texture not found, showing selected style");
                            style = StyleType.Selected;
                        }
                        else
                        {
                            res.name = "Green node Highlight";
                            res.normal.background = NodeSingleton.s.nodeGreenHighlight;
                        }
                    }

                    if(style == StyleType.UnSelected)
                    {
                        res.name = "Green node";
                        res.normal.background = NodeSingleton.s.nodeGreen;
                    }
                    else if(style == StyleType.Selected)
                    {
                        res.name = "Green node On";
                        res.normal.background = NodeSingleton.s.nodeGreenOn;
                    }

                    break;
                }
            }

            return res;
        }

        private class NodeSingleton
        {
            public Action<NodeEvent> emitAction;


            public Texture2D nodeAqua;
            public Texture2D nodeAquaOn;
            public Texture2D nodeAquaHighlight;
            public Texture2D nodeGrey;
            public Texture2D nodeGreyOn;
            public Texture2D nodeGreyHighlight;
            public Texture2D nodeOrange;
            public Texture2D nodeOrangeOn;
            public Texture2D nodeOrangeHighlight;
            public Texture2D nodeBlue;
            public Texture2D nodeBlueOn;
            public Texture2D nodeBlueHighlight;
            public Texture2D nodeRed;
            public Texture2D nodeRedOn;
            public Texture2D nodeRedHighlight;
            public Texture2D nodeYellow;
            public Texture2D nodeYellowOn;
            public Texture2D nodeYellowHighlight;
            public Texture2D nodeGreen;
            public Texture2D nodeGreenOn;
            public Texture2D nodeGreenHighlight;

            public Texture2D inputPointTex;
            public Texture2D outputPointTex;

            public Texture2D enablePointMarkTex;

            public Texture2D inputPointMarkTex;
            public Texture2D outputPointMarkTex;
            public Texture2D outputPointMarkConnectedTex;
            public PlatformButton[] platformButtons;

            public List<BuildTarget> supportedBuildTargets;
            public string[] supportedBuildTargetNames;
            public List<BuildTargetGroup> supportedBuildTargetGroups;

            public List<string> allNodeNames;

            private static NodeSingleton s_singleton;

            public static NodeSingleton s
            {
                get
                {
                    if(s_singleton == null)
                    {
                        s_singleton = new NodeSingleton();
                    }

                    return s_singleton;
                }
            }


            public NodeSingleton()
            {
                nodeAqua = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_AQUA);
                nodeAquaOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_AQUA_ON);
                nodeAquaHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_AQUA_HIGHLIGHT);
                nodeGrey = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_GREY);
                nodeGreyOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_GREY_ON);
                nodeGreyHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_GREY_HIGHLIGHT);
                nodeBlue = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_BLUE);
                nodeBlueOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_BLUE_ON);
                nodeBlueHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_BLUE_HIGHLIGHT);
                nodeOrange = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_ORANGE);
                nodeOrangeOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_ORANGE_ON);
                nodeOrangeHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_ORANGE_HIGHLIGHT);
                nodeRed = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_RED);
                nodeRedOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_RED_ON);
                nodeRedHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_RED_HIGHLIGHT);
                nodeRed = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_RED);
                nodeRedOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_RED_ON);
                nodeRedHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_RED_HIGHLIGHT);
                nodeYellow = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_YELLOW);
                nodeYellowOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_YELLOW_ON);
                nodeYellowHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_YELLOW_HIGHLIGHT);
                nodeGreen = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_GREEN);
                nodeGreenOn = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_GREEN_ON);
                nodeGreenHighlight = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetGraphRelativePaths.RESOURCE_NODE_GREEN_HIGHLIGHT);
            }

            public void SetupPlatformButtons()
            {
                SetupSupportedBuildTargets();
                var buttons = new List<PlatformButton>();

                Dictionary<BuildTargetGroup, string> icons = new Dictionary<BuildTargetGroup, string> {
                    {BuildTargetGroup.Android,      "BuildSettings.Android.Small"},
                    {BuildTargetGroup.iOS,          "BuildSettings.iPhone.Small"},
                    #if UNITY_5_5_OR_NEWER
                    {BuildTargetGroup.N3DS,  "BuildSettings.N3DS.Small"},
                    #else
					{BuildTargetGroup.Nintendo3DS,  "BuildSettings.N3DS.Small"},
					{BuildTargetGroup.PS3,          "BuildSettings.PS3.Small"},
                    #endif
                    {BuildTargetGroup.PS4,          "BuildSettings.PS4.Small"},
                    {BuildTargetGroup.PSM,          "BuildSettings.PSM.Small"},
                    {BuildTargetGroup.PSP2,         "BuildSettings.PSP2.Small"},
                    {BuildTargetGroup.SamsungTV,    "BuildSettings.Android.Small"},
                    {BuildTargetGroup.Standalone,   "BuildSettings.Standalone.Small"},
                    {BuildTargetGroup.Tizen,        "BuildSettings.Tizen.Small"},
                    {BuildTargetGroup.tvOS,         "BuildSettings.tvOS.Small"},
                    {BuildTargetGroup.Unknown,      "BuildSettings.Standalone.Small"},
                    {BuildTargetGroup.WebGL,        "BuildSettings.WebGL.Small"},
                    {BuildTargetGroup.WiiU,         "BuildSettings.WiiU.Small"},
                    {BuildTargetGroup.WSA,          "BuildSettings.WP8.Small"},
                    #if !UNITY_5_5_OR_NEWER
                    {BuildTargetGroup.XBOX360,      "BuildSettings.Xbox360.Small"},
                    #endif
                    {BuildTargetGroup.XboxOne,      "BuildSettings.XboxOne.Small"}
                };

                buttons.Add(new PlatformButton(new GUIContent("Default", "Default settings"), BuildTargetGroup.Unknown));

                foreach(var g in supportedBuildTargetGroups)
                {
                    buttons.Add(new PlatformButton(new GUIContent(GetPlatformIcon(icons[g]), BuildTargetUtility.GroupToHumaneString(g)), g));
                }

                this.platformButtons = buttons.ToArray();
            }

            public void SetupSupportedBuildTargets()
            {

                if(supportedBuildTargets == null)
                {
                    supportedBuildTargets = new List<BuildTarget>();
                    supportedBuildTargetGroups = new List<BuildTargetGroup>();

                    try
                    {
                        foreach(BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
                        {
                            if(BuildTargetUtility.IsBuildTargetSupported(target))
                            {
                                if(!supportedBuildTargets.Contains(target))
                                {
                                    supportedBuildTargets.Add(target);
                                }
                                BuildTargetGroup g = BuildTargetUtility.TargetToGroup(target);
                                if(g == BuildTargetGroup.Unknown)
                                {
                                    // skip unknown platform
                                    continue;
                                }
                                if(!supportedBuildTargetGroups.Contains(g))
                                {
                                    supportedBuildTargetGroups.Add(g);
                                }
                            }
                        }

                        supportedBuildTargetNames = new string[supportedBuildTargets.Count];
                        for(int i = 0; i < supportedBuildTargets.Count; ++i)
                        {
                            supportedBuildTargetNames[i] = BuildTargetUtility.TargetToHumaneString(supportedBuildTargets[i]);
                        }

                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }
            }

            private Texture2D GetPlatformIcon(string name)
            {
                return EditorGUIUtility.IconContent(name).image as Texture2D;
            }
        }
    }
}
