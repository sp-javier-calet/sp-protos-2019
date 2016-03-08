using UnityEngine;
using System.IO;
using UnityEditor;

namespace SpartaTools.Editor.Build
{
    public class BuildSet : ScriptableObject
    {
        public string CommonFlags;
        public string AndroidFlags;
        public string IosFlags;

        public bool UseKeytore;
        public string KeystorePath;
        public string KeystoreFilePassword;
        public string KeystoreAlias;
        public string KeystorePassword;


        [MenuItem( "Sparta/Create/Instance" )]
        public static void CreateInstance() {
            CreateAsset(typeof(BuildSet));
        }

        private static void CreateAsset(System.Type type) {
            var asset = ScriptableObject.CreateInstance( type );
            string path = AssetDatabase.GetAssetPath( Selection.activeObject );
            if( path == "" )  {
                path = "Assets";
            } else if( Path.GetExtension( path ) != "" ) {
                path = path.Replace( Path.GetFileName( AssetDatabase.GetAssetPath( Selection.activeObject ) ), "" );
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( "Assets/BuildSet/" + type.ToString() + ".asset" );
            AssetDatabase.CreateAsset( asset, assetPathAndName );
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}