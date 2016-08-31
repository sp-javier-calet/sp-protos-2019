using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LitJson;

namespace SocialPoint.Editor.SPAMGui
{
	public sealed class BundlePackContainer
	{
		private const string JSON_DATA_FILE = "Assets/ToolsConfig/bundle_pack_descritption.json";

		// Data
		public string project_name { get; set; }
		public BundlePackMetaData[] packs { get; set; }

		public static BundlePackContainer Load()
		{
			TextAsset textAsset = AssetDatabase.LoadAssetAtPath( JSON_DATA_FILE, typeof(TextAsset) ) as TextAsset;

			// This will try to find and fill a mainAssetPath for the bundles that are scenes
			BundlePackContainer bundlePackContainer = JsonMapper.ToObject<BundlePackContainer>( textAsset.text );

			bundlePackContainer.FillMainAssetPathForScenes ();
			bundlePackContainer.Link ();

			return bundlePackContainer;
		}

		public void Save()
		{
			string txt = JsonMapper.ToJson( this );
			string jsonDataFileSystemPath = GetSystemPath( JSON_DATA_FILE );
			File.WriteAllText( jsonDataFileSystemPath, txt );
			AssetDatabase.ImportAsset( JSON_DATA_FILE );
		}

		public BundleMetaData FindBundleByName( string name )
		{
			for ( int i = 0; i < packs.Length; i++ ) {
				BundleMetaData b = packs[i].FindBundleByName( name );
				if ( b != null ) {
					return b;
				}
			}

			return null;
		}

		public BundleMetaData[] GetAllBundles()
		{
			List<BundleMetaData> allBundles = new List<BundleMetaData>();

			for ( int i = 0; i < packs.Length; i++ ) {
				BundlePackMetaData p = packs[i];
				allBundles.AddRange( p.bundles );
			}

			return allBundles.ToArray();
		}

        public BundleMetaData[] GetAllCheckedBundles()
        {
            List<BundleMetaData> allCheckedBundles = new List<BundleMetaData>();

            for ( int i = 0; i < packs.Length; i++ ) {
                BundlePackMetaData p = packs[i];
                allCheckedBundles.AddRange( p.GetAllCheckedBundles() );
            }
            
            return allCheckedBundles.ToArray();
        }

        public BundleMetaData[] GetAllPackBundles(string pack)
        {
            for (int i = 0; i < packs.Length; ++i) {
                if (pack == packs[i].name)
                    return packs[i].bundles;
            }
            return null;
        }

        public BundleMetaData[] GetAllPackCheckedBundles(string pack)
        {
            for (int i = 0; i < packs.Length; ++i) {
                if (pack == packs[i].name)
                    return packs[i].GetAllCheckedBundles();
            }
            return null;
        }

        public BundlePackMetaData GetPack(string pack)
        {
            for (int i = 0; i < packs.Length; ++i) {
                if (pack == packs[i].name)
                    return packs[i];
            }
            return null;
        }

        void FillMainAssetPathForScenes()
        {
            var foundBundles = new HashSet<BundleMetaData> ();
            var allScenes = Directory.GetFiles (Application.dataPath, "*.unity", SearchOption.AllDirectories);
            
            for (int i = 0; i < allScenes.Length; ++i) {
                string scenePath = allScenes[i];
                string sceneShortName = Path.GetFileNameWithoutExtension (scenePath);
                
                var bmdata = FindBundleByName(sceneShortName);
                if (bmdata != null) {

                    if (foundBundles.Contains (bmdata)) {
                        //Duplicated scene by name, cannot know the bmdata mainAssetPath for sure so set to Empty  
                        bmdata.mainAssetPath = String.Empty;
                    }
                    else {
                        bmdata.mainAssetPath = Path.Combine ("Assets", scenePath.Substring(Application.dataPath.Length + 1)).Replace ("\\","/");
                        foundBundles.Add (bmdata);
                    }
                }
            }
        }


		public void Link()
		{
			for ( int i = 0; i < packs.Length; i++ ) {
				packs[i].Link( this );
			}
		}

		private string GetSystemPath( string path )
		{
			return Path.Combine( Application.dataPath, path.Replace( "Assets/", "" ) );
		}

		/*
		// Tester
		[MenuItem ("SPAM/Test BundlePackContainer")]
		public static void Test()
		{
			BundlePackContainer bundlePackContainer = BundlePackContainer.Load();
			bundlePackContainer.Save();
		}
		*/
	}

	public sealed class BundlePackMetaData
	{
		// Data
		public string name { get; set; }
		public int version { get; set; }
		public BundleMetaData[] bundles { get; set; }

		private BundlePackContainer _bundlePackContainer;

		public string projectName { get { return _bundlePackContainer.project_name; } }

		public void Link( BundlePackContainer c )
		{
			_bundlePackContainer = c;

			for ( int i = 0; i < bundles.Length; i++ ) {
				bundles[i].Link( this );
			}
		}

		public BundleMetaData FindBundleByName( string name )
		{
			for ( int i = 0; i < bundles.Length; i++ ) {
				BundleMetaData b = bundles[i];
				if ( b.name == name ) {
					return b;
				}
			}

			return null;
		}

        public BundleMetaData[] GetAllCheckedBundles()
        {
            List<BundleMetaData> allCheckedBundles = new List<BundleMetaData> ();

            for ( int i = 0; i < bundles.Length; i++ ) {
                BundleMetaData b = bundles[i];
                if ( b.isChecked )
                    allCheckedBundles.Add( b );
            }
            
            return allCheckedBundles.ToArray();
        }
	}

	public sealed class BundleMetaData
	{
		// Data
		public string name { get; set; }
		public string mainAssetPath { get; set; }
		public bool isChecked { get; set; }
        public bool isBundled { get; set; }
        public int version     { get; set; }

		private BundlePackMetaData _myPack;

		public string packName    { get { return _myPack.name; } }
		public string projectName { get { return _myPack.projectName; } }

		public void Link( BundlePackMetaData pack )
		{
			_myPack = pack;
		}

        public BundleMetaData() {}

        public BundleMetaData(BundleMetaData other) {
            name = other.name;
            mainAssetPath = other.mainAssetPath;
            isChecked = other.isChecked;
            isBundled = other.isBundled;
            version = other.version;
            _myPack = other._myPack;
        }
	}
}
