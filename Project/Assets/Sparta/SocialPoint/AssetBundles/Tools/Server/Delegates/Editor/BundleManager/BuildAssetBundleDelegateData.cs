using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using BundleManagerJSON;


namespace SocialPoint.Tool.Server
{
    public class BundleManagerDataFile
    {
        public TextAsset asset { get; set; }
        public string assetPath { get; set; }
        public string filePath { get; set; }
        public string text { get; set; }
		
        public BundleManagerDataFile()
        {
            asset = null;
        }
		
        public BundleManagerDataFile(BundleManagerDataFile other)
        {
            this.asset = other.asset;
            this.assetPath = String.Copy(other.assetPath);
            this.filePath = String.Copy(other.filePath);
            this.text = String.Copy(other.text);
        }
		
        public void LoadFromAsset(TextAsset textAsset)
        {
            asset = textAsset;
            assetPath = AssetDatabase.GetAssetPath(asset);
            filePath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
            text = Utils.GetFileContents(filePath);
        }

        public void LoadFromFile(string filePath)
        {
            try
            {
                LoadFromAsset((TextAsset)AssetDatabase.LoadAssetAtPath (filePath, typeof (TextAsset)));
            }
            catch
            {
                asset = null;
                assetPath = null;
                this.filePath = filePath;
                text = Utils.GetFileContents(filePath);
            }
        }
		
        public void UpdateAsset(string newContent=null)
        {
            if(asset != null || assetPath != null)
            {
                // Just update and reimport current text content into file
                if(String.IsNullOrEmpty(newContent))
                {
                    Utils.SetFileContents(filePath, text);
                    AssetDatabase.ImportAsset(assetPath);
                    asset = (TextAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset));
                }
    			// If contents are equal, do nothing at all
    			else if(!Utils.CompareContent(text, newContent))
                {
                    text = newContent;
                    Utils.SetFileContents(filePath, text);
                    AssetDatabase.ImportAsset(assetPath);
                    asset = (TextAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset));
                }
            }
        }

        public void UpdateFile(string newContent=null)
        {
            try
            {
                UpdateAsset(newContent: newContent);
            }
            catch
            {
                if(String.IsNullOrEmpty(newContent))
                {
                    Utils.SetFileContents(filePath, text);
                }
                else if(!Utils.CompareContent(text, newContent))
                {
                    text = newContent;
                    Utils.SetFileContents(filePath, text);
                }
            }
        }
    }
	
    public class BundleManagerData
    {
        public BundleManagerDataFile BMConfiger { get; set; }
        public BundleManagerDataFile BundleData { get; set; }
        public BundleManagerDataFile BuildStates { get; set; }
        public BundleManagerDataFile JSONConfigBundleData { get; set; }
        public BundleManagerDataFile Urls { get; set; }
		
        public BundleManagerData()
        {
            BMConfiger = new BundleManagerDataFile();
            BundleData = new BundleManagerDataFile();
            BuildStates = new BundleManagerDataFile();
            Urls = new BundleManagerDataFile();

            JSONConfigBundleData = new BundleManagerDataFile();
        }
		
        public BundleManagerData(BundleManagerData other)
        {
            BMConfiger = new BundleManagerDataFile(other.BMConfiger);
            BundleData = new BundleManagerDataFile(other.BundleData);
            BuildStates = new BundleManagerDataFile(other.BuildStates);
            Urls = new BundleManagerDataFile(other.Urls);

            JSONConfigBundleData = new BundleManagerDataFile(other.JSONConfigBundleData);
        }
		
        public void Init()
        {
            BMConfiger.LoadFromFile(BMDataAccessor.Paths.BMConfigerPath);
            BundleData.LoadFromFile(BMDataAccessor.Paths.BundleDataPath);
            BuildStates.LoadFromFile(BMDataAccessor.Paths.BundleBuildStatePath);
            Urls.LoadFromFile(BMDataAccessor.Paths.UrlDataPath);

            JSONConfigBundleData.LoadFromFile(JSONDataAccessor.JSONConfigBundleData);
        }
    }
}
