using UnityEngine;
using System.Collections.Generic;
using System;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryConfig
    {
        public static string DbConfig = "Server=GrayboxTool.c4rdfbnb9wen.eu-west-1.rds.amazonaws.com;Database=GrayboxTool;User ID=GrayboxTool;Password=SM9tyR8h21PRoVz;Pooling=true";

        public static string MacVolumePath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("/", 8)) + "/mount/3dshare/";

        public static string WinVolumePath = "//spserver/3dshare/";

        #if UNITY_EDITOR_OSX
        public static string VolumePath = MacVolumePath;
        #else
        public static string VolumePath = WinVolumePath;
#endif
        public static string IconsPath = VolumePath + "TA/UnityAssetLibrary/tool_icons/";

        #if UNITY_EDITOR_OSX
        public static string PkgDefaultFolder = VolumePath + "TA/UnityAssetLibrary/Packages";
        #else
        public static string PkgDefaultFolder = (VolumePath + "TA/UnityAssetLibrary/Packages").Replace("/", "\\");
#endif
        public static string SmbConnectionUrl = "//guest@" + WinVolumePath.Substring(2);

        public static string ContactMail = "technical-art@socialpoint.es";

        public static string ContactUrl = "https://mail.google.com/mail/?view=cm&fs=1&to=" + ContactMail + "&su=Graybox+Tool+Contact";

        public static string HelpUrl = "https://sites.google.com/a/socialpoint.es/technical-art/07---tools-documentation/greybox-library";
        
        public static Dictionary <GrayboxAssetCategory, Type> ScriptOnInstance = new Dictionary<GrayboxAssetCategory, Type> {
            { GrayboxAssetCategory.Buildings, typeof(ModelClickPoping) },
            { GrayboxAssetCategory.Characters, null },
            { GrayboxAssetCategory.Decos, null },
            { GrayboxAssetCategory.Fx, null },
            { GrayboxAssetCategory.Props, null },
            { GrayboxAssetCategory.UI, null },
            { GrayboxAssetCategory.Vehicles, null }
        };

        public static Dictionary<GrayboxAssetCategory, string> CategoryPrefix = new Dictionary<GrayboxAssetCategory, string> {
            { GrayboxAssetCategory.Buildings, "BLD_" },
            { GrayboxAssetCategory.Characters, "CHR_" },
            { GrayboxAssetCategory.Decos, "DCO_" },
            { GrayboxAssetCategory.Fx, "FX_" },
            { GrayboxAssetCategory.Props, "PRP_" },
            { GrayboxAssetCategory.UI, "UI_" },
            { GrayboxAssetCategory.Vehicles, "VHC_" }
        };

    }

    public enum GrayboxAssetCategory
    {
        Buildings,
        Props,
        Decos,
        Fx,
        Characters,
        Vehicles,
        UI

    }

}