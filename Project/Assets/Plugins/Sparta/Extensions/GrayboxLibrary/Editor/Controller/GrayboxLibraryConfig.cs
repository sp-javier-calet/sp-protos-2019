using UnityEngine;
using System.Collections.Generic;
using System;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryConfig
    {
        public static string DbConfig = "Server=GrayboxTool.c4rdfbnb9wen.eu-west-1.rds.amazonaws.com;Database=GrayboxTool;User ID=GrayboxTool;Password=SM9tyR8h21PRoVz;Pooling=true";

        public static string MacVolumePath = "/Users/" + Environment.UserName + "/mount/3dshare";

        public static string SmbFolder = "spserver.spoint.es/3dshare";

        public static string AltSmbFolder = "spserver/3dshare";

        public static string WinVolumePath = "//" + SmbFolder;

        public static string WinVolumePathAlt = "//" + AltSmbFolder;

        #if UNITY_EDITOR_OSX
        public static string VolumePath = MacVolumePath;
        #else
        public static string VolumePath = WinVolumePath;
#endif
        public static string IconsFolder = "/TA/UnityAssetLibrary/tool_icons/";

        public static string IconsPath = VolumePath + IconsFolder;

        public static string PkgFolder = "/TA/UnityAssetLibrary/Packages/";

        #if UNITY_EDITOR_OSX
        public static string PkgDefaultFolder = VolumePath + PkgFolder;
        #else
        public static string PkgDefaultFolder = (VolumePath + PkgFolder).Replace("/", "\\");
#endif
        public static string SmbConnectionUrl = "//guest@" + SmbFolder;

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

        public static void SetVolumePath(string newVolumePath)
        {
            VolumePath = newVolumePath;
            IconsPath = VolumePath + IconsFolder;
            #if UNITY_EDITOR_OSX
            PkgDefaultFolder = VolumePath + PkgFolder;
            #else
            PkgDefaultFolder = (VolumePath + PkgFolder).Replace("/", "\\");
            #endif
        }

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