using System;
using UnityEditor;

namespace AssetBundleGraph {
	public class BuildTargetUtility {

		public const BuildTargetGroup DefaultTarget = BuildTargetGroup.Unknown;

		/**
		 *  from build target to human friendly string for display purpose.
		 */ 
		public static string TargetToHumaneString(UnityEditor.BuildTarget t) {

			switch(t) {
			case BuildTarget.Android:
				return "Android";
			case BuildTarget.iOS:
				return "iOS";
            #if UNITY_5_5_OR_NEWER
            case BuildTarget.N3DS:
            #else
			case BuildTarget.Nintendo3DS:
            #endif
				return "Nintendo 3DS";
			case BuildTarget.PS3:
				return "PlayStation 3";
			case BuildTarget.PS4:
				return "PlayStation 4";
			case BuildTarget.PSM:
				return "PlayStation Mobile";
			case BuildTarget.PSP2:
				return "PlayStation Vita";
			case BuildTarget.SamsungTV:
				return "Samsung TV";
			case BuildTarget.StandaloneLinux:
				return "Linux Standalone";
			case BuildTarget.StandaloneLinux64:
				return "Linux Standalone(64-bit)";
			case BuildTarget.StandaloneLinuxUniversal:
				return "Linux Standalone(Universal)";
			case BuildTarget.StandaloneOSXIntel:
				return "OSX Standalone";
			case BuildTarget.StandaloneOSXIntel64:
				return "OSX Standalone(64-bit)";
			case BuildTarget.StandaloneOSXUniversal:
				return "OSX Standalone(Universal)";
			case BuildTarget.StandaloneWindows:
				return "Windows Standalone";
			case BuildTarget.StandaloneWindows64:
				return "Windows Standalone(64-bit)";
			case BuildTarget.Tizen:
				return "Tizen";
			case BuildTarget.tvOS:
				return "tvOS";
			case BuildTarget.WebGL:
				return "WebGL";
			case BuildTarget.WiiU:
				return "Wii U";
			case BuildTarget.WSAPlayer:
				return "Windows Store Apps";
			case BuildTarget.XBOX360:
				return "Xbox 360";
			case BuildTarget.XboxOne:
				return "Xbox One";
			default:
				return t.ToString() + "(deprecated)";
			}
		}

		//returns the same value defined in AssetBundleManager
		public static string TargetToAssetBundlePlatformName(BuildTarget t)
		{
			switch(t) {
			case BuildTarget.Android:
			return "Android";
			case BuildTarget.iOS:
			return "iOS";
            #if UNITY_5_5_OR_NEWER
            case BuildTarget.N3DS:
            #else
			case BuildTarget.Nintendo3DS:
            #endif
			return "N3DS";
			case BuildTarget.PS3:
			return "PS3";
			case BuildTarget.PS4:
			return "PS4";
			case BuildTarget.PSM:
			return "PSM";
			case BuildTarget.PSP2:
			return "PSVita";
			case BuildTarget.SamsungTV:
			return "SamsungTV";
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			return "Linux";
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.StandaloneOSXUniversal:
			return "OSX";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
			return "Windows";
			case BuildTarget.Tizen:
			return "Tizen";
			case BuildTarget.tvOS:
			return "tvOS";
			case BuildTarget.WebGL:
			return "WebGL";
			case BuildTarget.WiiU:
			return "WiiU";
			case BuildTarget.WSAPlayer:
			return "WindowsStoreApps";
			case BuildTarget.XBOX360:
			return "Xbox360";
			case BuildTarget.XboxOne:
			return "XboxOne";
			default:
			return t.ToString() + "(deprecated)";
			}
		}

		/**
		 *  from build target group to human friendly string for display purpose.
		 */ 
		public static string GroupToHumaneString(UnityEditor.BuildTargetGroup g) {

			switch(g) {
			case BuildTargetGroup.Android:
				return "Android";
			case BuildTargetGroup.iOS:
				return "iOS";
            #if UNITY_5_5_OR_NEWER
            case BuildTargetGroup.N3DS:
            #else
			case BuildTargetGroup.Nintendo3DS:
            #endif
				return "Nintendo 3DS";
			case BuildTargetGroup.PS3:
				return "PlayStation 3";
			case BuildTargetGroup.PS4:
				return "PlayStation 4";
			case BuildTargetGroup.PSM:
				return "PlayStation Mobile";
			case BuildTargetGroup.PSP2:
				return "PlayStation Vita";
			case BuildTargetGroup.SamsungTV:
				return "Samsung TV";
			case BuildTargetGroup.Standalone:
				return "PC/Mac/Linux Standalone";
			case BuildTargetGroup.Tizen:
				return "Tizen";
			case BuildTargetGroup.tvOS:
				return "tvOS";
			case BuildTargetGroup.WebGL:
				return "WebGL";
			case BuildTargetGroup.WiiU:
				return "Wii U";
			case BuildTargetGroup.WSA:
				return "Windows Store Apps";
			case BuildTargetGroup.XBOX360:
				return "Xbox 360";
			case BuildTargetGroup.XboxOne:
				return "Xbox One";
			case BuildTargetGroup.Unknown:
				return "Unknown";
			default:
				return g.ToString() + "(deprecated)";
			}
		}


		public static BuildTargetGroup TargetToGroup(UnityEditor.BuildTarget t) {

			if((int)t == int.MaxValue) {
				return BuildTargetGroup.Unknown;
			}

			switch(t) {
			case BuildTarget.Android:
				return BuildTargetGroup.Android;
			case BuildTarget.iOS:
				return BuildTargetGroup.iOS;
            #if UNITY_5_5_OR_NEWER
            case BuildTarget.N3DS:
                return BuildTargetGroup.N3DS;
            #else
			case BuildTarget.Nintendo3DS:
				return BuildTargetGroup.Nintendo3DS;
            #endif
			case BuildTarget.PS3:
				return BuildTargetGroup.PS3;
			case BuildTarget.PS4:
				return BuildTargetGroup.PS4;
			case BuildTarget.PSM:
				return BuildTargetGroup.PSM;
			case BuildTarget.PSP2:
				return BuildTargetGroup.PSP2;
			case BuildTarget.SamsungTV:
				return BuildTargetGroup.SamsungTV;
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return BuildTargetGroup.Standalone;
			case BuildTarget.Tizen:
				return BuildTargetGroup.Tizen;
			case BuildTarget.tvOS:
				return BuildTargetGroup.tvOS;
			case BuildTarget.WebGL:
				return BuildTargetGroup.WebGL;
			case BuildTarget.WiiU:
				return BuildTargetGroup.WiiU;
			case BuildTarget.WSAPlayer:
				return BuildTargetGroup.WSA;
			case BuildTarget.XBOX360:
				return BuildTargetGroup.XBOX360;
			case BuildTarget.XboxOne:
				return BuildTargetGroup.XboxOne;
			default:
				return BuildTargetGroup.Unknown;
			}
		}

		public static BuildTarget GroupToTarget(UnityEditor.BuildTargetGroup g) {

			switch(g) {
			case BuildTargetGroup.Android:
				return BuildTarget.Android;
			case BuildTargetGroup.iOS:
				return BuildTarget.iOS;
            #if UNITY_5_5_OR_NEWER
            case BuildTargetGroup.N3DS:
                return BuildTarget.N3DS;
            #else
			case BuildTargetGroup.Nintendo3DS:
				return BuildTarget.Nintendo3DS;
            #endif
			case BuildTargetGroup.PS3:
				return BuildTarget.PS3;
			case BuildTargetGroup.PS4:
				return BuildTarget.PS4;
			case BuildTargetGroup.PSM:
				return BuildTarget.PSM;
			case BuildTargetGroup.PSP2:
				return BuildTarget.PSP2;
			case BuildTargetGroup.SamsungTV:
				return BuildTarget.SamsungTV;
			case BuildTargetGroup.Standalone:
				return BuildTarget.StandaloneWindows;
			case BuildTargetGroup.Tizen:
				return BuildTarget.Tizen;
			case BuildTargetGroup.tvOS:
				return BuildTarget.tvOS;
			case BuildTargetGroup.WebGL:
				return BuildTarget.WebGL;
			case BuildTargetGroup.WiiU:
				return BuildTarget.WiiU;
			case BuildTargetGroup.WSA:
				return BuildTarget.WSAPlayer;
			case BuildTargetGroup.XBOX360:
				return BuildTarget.XBOX360;
			case BuildTargetGroup.XboxOne:
				return BuildTarget.XboxOne;
			default:
				// temporarily assigned for default value (BuildTargetGroup.Unknown)
				return (BuildTarget)int.MaxValue;
			}
		}

		public static BuildTarget BuildTargetFromString (string val) {
			return (BuildTarget)Enum.Parse(typeof(BuildTarget), val);
		}

        public static bool IsBuildTargetSupported(BuildTarget t) {

            //[WrapperlessIcall]
            //[MethodImpl (MethodImplOptions.InternalCall)]
            //internal static extern bool IsBuildTargetSupported (BuildTarget target);

            // Types.GetType is obsolete
            // var objType = Types.GetType ("UnityEditor.BuildPipeline", "UnityEditor.dll");

            var objType = typeof(BuildPipeline);
            var method =  objType.GetMethod("IsBuildTargetSupported", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var retval = method.Invoke(null, new object[]{System.Enum.ToObject(typeof(BuildTarget), t)});
            return Convert.ToBoolean(retval);
        }
	}
}