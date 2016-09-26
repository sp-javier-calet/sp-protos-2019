using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SocialPoint.GrayboxLibrary
{
    public class GrayboxLibraryController
    {

        private GrayboxLibraryDB _dbController;
        private GrayboxLibraryDownloader _downloadController;

        public GrayboxLibraryController()
        {
            //Mounts the smb folder
            #if  UNITY_EDITOR_OSX
            if(!Directory.Exists(GrayboxLibraryConfig.PkgDefaultFolder))
            {
                ProcessStartInfo process = new ProcessStartInfo();
                process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.FileName = "mkdir";
                process.Arguments = GrayboxLibraryConfig.VolumePath;
                Process.Start(process);

                process = new ProcessStartInfo();
                process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.FileName = "mount_smbfs";
                process.Arguments = GrayboxLibraryConfig.SmbConnectionUrl + " " + GrayboxLibraryConfig.VolumePath;
                Process.Start(process);

                while(!Directory.Exists(GrayboxLibraryConfig.PkgDefaultFolder))
                {
                }
            }
            #endif

            _dbController = GrayboxLibraryDB.GetInstance();
            _downloadController = GrayboxLibraryDownloader.GetInstance();
            _dbController.Connect();
        }


        public GrayboxAsset GetAsset(string name)
        {
            GrayboxAsset asset = null;

            ArrayList queryResult = _dbController.ExecuteQuery("SELECT a.id_asset, a.name, a.category, a.main_asset_path, a.pkg_path, a.thumb_path, a.animated_thumb_path, a.creation_date FROM asset a WHERE a.name LIKE '" + name + "'");

            if(queryResult.Count > 0)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[0];

                Texture2D thumb = null;
                if(row["thumb_path"].Length > 0)
                    thumb = _downloadController.DownloadImage(row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath));

                string[] split1 = row["creation_date"].Split(' ');
                string[] date = split1[0].Split('/');
                string[] time = split1[1].Split(':');
                DateTime finalDate = new DateTime(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));

                asset = new GrayboxAsset(int.Parse(row["id_asset"]), row["name"], (GrayboxAssetCategory)Enum.Parse(typeof(GrayboxAssetCategory), row["category"]),
                    row["main_asset_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                    row["pkg_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                    row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                    row["animated_thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath), thumb, finalDate);
            }

            return asset;
        }

        public ArrayList GetAssets(string[] tags, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000, bool downloadThumbnail = true)
        {
            ArrayList assets = new ArrayList();

            string tagSearchSQL = "";
            for(int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];
                tagSearchSQL += " NATURAL JOIN (SELECT id_asset FROM asset_tag NATURAL JOIN tag WHERE name LIKE '%" + tag + "%') as tag" + i;
            }

            string sql = "SELECT DISTINCT a.id_asset, a.name, a.category, a.main_asset_path, a.pkg_path, a.thumb_path, a.animated_thumb_path, a.creation_date "
                         + "FROM asset a " + tagSearchSQL + " WHERE a.category LIKE '" + category.ToString() + "' ORDER BY a.name ASC, a.creation_date DESC LIMIT " + startLimit + "," + endLimit;
            //Debug.Log(sql);
            ArrayList queryResult = _dbController.ExecuteQuery(sql);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                Texture2D thumb = null;
                if(downloadThumbnail)
                    thumb = _downloadController.DownloadImage(row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                        .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath));

                string[] split1 = row["creation_date"].Split(' ');
                string[] date = split1[0].Split('/');
                string[] time = split1[1].Split(':');
                DateTime finalDate = new DateTime(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));

                GrayboxAsset asset = new GrayboxAsset(int.Parse(row["id_asset"]), row["name"], (GrayboxAssetCategory)Enum.Parse(typeof(GrayboxAssetCategory), row["category"]),
                                         row["main_asset_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                                         row["pkg_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                                         row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                                         row["animated_thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath), thumb, finalDate);

                assets.Add(asset);
            }

            return assets;
        }

        public ArrayList GetAssetsByName(string[] filters, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000, bool downloadThumbnail = true)
        {
            ArrayList assets = new ArrayList();

            string tagSearchSQL = "";

            string sql = "SELECT DISTINCT a.id_asset, a.name, a.category, a.main_asset_path, a.pkg_path, a.thumb_path, a.animated_thumb_path,  a.creation_date "
                         + "FROM asset a " + tagSearchSQL + " WHERE a.category LIKE '" + category.ToString() + "'";

            string filteredSQL = "";
            foreach(string filter in filters)
            {
                filteredSQL = filteredSQL + " a.name LIKE '%" + filter + "%' AND";
            }
            if(filteredSQL.Length > 0)
                filteredSQL = " AND (" + filteredSQL.Substring(0, filteredSQL.Length - 3) + ")";

            sql = sql + filteredSQL + " ORDER BY a.name ASC, a.creation_date DESC LIMIT " + startLimit + "," + endLimit;
            //Debug.Log(sql);
            ArrayList queryResult = _dbController.ExecuteQuery(sql);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                Texture2D thumb = null;
                if(downloadThumbnail)
                    thumb = _downloadController.DownloadImage(row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                        .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath));

                string[] split1 = row["creation_date"].Split(' ');
                string[] date = split1[0].Split('/');
                string[] time = split1[1].Split(':');
                DateTime finalDate = new DateTime(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));

                GrayboxAsset asset = new GrayboxAsset(int.Parse(row["id_asset"]), row["name"], (GrayboxAssetCategory)Enum.Parse(typeof(GrayboxAssetCategory), row["category"]),
                                         row["main_asset_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                                         row["pkg_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                                         row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath),
                                         row["animated_thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath), thumb, finalDate);

                assets.Add(asset);
            }

            return assets;
        }

        public string[] GetAssetsAsText(string[] tags, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000)
        {
            List<string> result = new List<string>();

            ArrayList assetList = GetAssets(tags, category, startLimit, endLimit, false);
            for(int i = 0; i < assetList.Count; i++)
            {
                GrayboxAsset asset = (GrayboxAsset)assetList[i];
                result.Add(asset.Name);
            }

            return result.ToArray();
        }

        public string[] GetAssetsByNameAsText(string[] filters, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000)
        {
            List<string> result = new List<string>();

            ArrayList assetList = GetAssetsByName(filters, category, startLimit, endLimit, false);
            for(int i = 0; i < assetList.Count; i++)
            {
                GrayboxAsset asset = (GrayboxAsset)assetList[i];
                result.Add(asset.Name);
            }

            return result.ToArray();
        }


        public int GetAssetCount(string[] tags, GrayboxAssetCategory category)
        {
            string tagSearchSQL = "";
            for(int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];
                tagSearchSQL += " NATURAL JOIN (SELECT id_asset FROM asset_tag NATURAL JOIN tag WHERE name LIKE '%" + tag + "%') as tag" + i;
            }

            string sql = "SELECT DISTINCT a.id_asset "
                         + "FROM asset a " + tagSearchSQL + " WHERE a.category LIKE '" + category.ToString() + "'";
            //Debug.Log(sql);
            ArrayList queryResult = _dbController.ExecuteQuery(sql);

            return queryResult.Count;
        }



        public GrayboxTag GetTag(string tagName)
        {
            GrayboxTag tag = null;

            ArrayList queryResult = _dbController.ExecuteQuery("SELECT t.id_tag, t.name FROM tag t WHERE t.name LIKE '" + tagName + "'");

            if(queryResult.Count > 0)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[0];
                tag = new GrayboxTag(int.Parse(row["id_tag"]), row["name"]);
            }

            return tag;
        }


        public ArrayList GetTags(string name, int startLimit = 0, int endLimit = 1000)
        {
            ArrayList tags = new ArrayList();

            ArrayList queryResult = _dbController.ExecuteQuery("SELECT t.id_tag, t.name FROM tag t WHERE t.name LIKE '%" + name + "%' LIMIT " + startLimit + ", " + endLimit);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                GrayboxTag tag = new GrayboxTag(int.Parse(row["id_tag"]), row["name"]);
                tags.Add(tag);
            }

            return tags;
        }

        public string[] GetTagsAsText(string name, int startLimit = 0, int endLimit = 1000)
        {
            List<string> tags = new List<string>();

            ArrayList queryResult = _dbController.ExecuteQuery("SELECT t.id_tag, t.name FROM tag t WHERE t.name LIKE '%" + name + "%' LIMIT " + startLimit + ", " + endLimit);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                tags.Add(row["name"]);
            }

            return tags.ToArray();
        }


        public ArrayList GetAssetTags(GrayboxAsset asset, int startLimit = 0, int endLimit = 1000)
        {
            ArrayList tags = new ArrayList();

            ArrayList queryResult = _dbController.ExecuteQuery("SELECT DISTINCT t.id_tag, t.name FROM tag t, asset_tag atag WHERE t.id_tag = atag.id_tag AND atag.id_asset = " + asset.Id + " LIMIT " + startLimit + "," + endLimit);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                GrayboxTag tag = new GrayboxTag(int.Parse(row["id_tag"]), row["name"]);
                tags.Add(tag);
            }

            return tags;
        }

        public string[] GetAssetTagsAsText(GrayboxAsset asset, int startLimit = 0, int endLimit = 1000)
        {
            List<string> result = new List<string>();

            ArrayList tagList = GetAssetTags(asset, startLimit, endLimit);
            for(int i = 0; i < tagList.Count; i++)
            {
                GrayboxTag tag = (GrayboxTag)tagList[i];
                result.Add(tag.Name);
            }

            return result.ToArray();
        }



        public ArrayList GetAllAssetTags(int startLimit = 0, int endLimit = 1000)
        {
            ArrayList tags = new ArrayList();

            ArrayList queryResult = _dbController.ExecuteQuery("SELECT DISTINCT t.id_tag, t.name FROM tag t LIMIT " + startLimit + "," + endLimit);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                GrayboxTag tag = new GrayboxTag(int.Parse(row["id_tag"]), row["name"]);
                tags.Add(tag);
            }

            return tags;
        }

        public string[] GetAllAssetTagsAsText(int startLimit = 0, int endLimit = 1000)
        {
            List<string> result = new List<string>();

            ArrayList tagList = GetAllAssetTags(startLimit, endLimit);
            for(int i = 0; i < tagList.Count; i++)
            {
                GrayboxTag tag = (GrayboxTag)tagList[i];
                result.Add(tag.Name);
            }

            return result.ToArray();
        }



        public void RegisterAsset(GrayboxAsset asset)
        {
            string sql = "SELECT a.id_asset FROM asset a WHERE a.id_asset = " + asset.Id;
            //Debug.Log(sql);
            ArrayList queryResult = _dbController.ExecuteQuery(sql);
            if(queryResult.Count == 0)
            {
                sql = "SELECT a.id_asset FROM asset a WHERE a.name LIKE '" + asset.Name+"'";
                //Debug.Log(sql);
                queryResult = _dbController.ExecuteQuery(sql);
                if (queryResult.Count == 0)
                {

                    sql = "INSERT INTO asset (name, category, main_asset_path, pkg_path, thumb_path, animated_thumb_path) VALUES ('" +
                    asset.Name + "','" +
                    asset.Category + "','" +
                    asset.MainAssetPath.Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.WinVolumePath) + "','" +
                    asset.PackagePath + "','" +
                    asset.ThumbnailPath + "','" +
                    asset.AnimatedThumbnailPath +
                    "')";
                    //Debug.Log(sql);
                    _dbController.ExecuteSQL(sql);
                }
            }
            else
            {
                sql = "UPDATE asset SET " +
                "name ='" + asset.Name + "', " +
                "category ='" + asset.Category.ToString() + "', " +
                "main_asset_path ='" + asset.MainAssetPath + "', " +
                "pkg_path ='" + asset.PackagePath + "', " +
                "thumb_path ='" + asset.ThumbnailPath + "', " +
                "animated_thumb_path ='" + asset.AnimatedThumbnailPath + "' " +
                "WHERE id_asset = " + asset.Id;
                //Debug.Log(sql);
                _dbController.ExecuteSQL(sql);
            }
        }

        public void RemoveAsset(GrayboxAsset asset)
        {
            string sql = "DELETE FROM asset_tag WHERE id_asset = " + asset.Id;
            //Debug.Log(sql);
            _dbController.ExecuteSQL(sql);

            sql = "DELETE FROM asset WHERE id_asset = " + asset.Id;
            //Debug.Log(sql);
            _dbController.ExecuteSQL(sql);

        }


        public void CreateTag(GrayboxTag tag)
        {
            string sql = "INSERT INTO tag (name) VALUES ('" + tag.Name + "')";
            //Debug.Log(sql);
            _dbController.ExecuteSQL(sql);
        }


        public void AssignTag(GrayboxAsset asset, GrayboxTag tag)
        {
            string sql = "INSERT INTO asset_tag VALUES (" + asset.Id + "," + tag.Id + ")";
            //Debug.Log(sql);
            _dbController.ExecuteSQL(sql);
        }

        public void UnassignTag(GrayboxAsset asset, GrayboxTag tag)
        {
            string sql = "DELETE FROM asset_tag WHERE id_asset = " + asset.Id + " AND id_tag = " + tag.Id;
            //Debug.Log(sql);
            _dbController.ExecuteSQL(sql);
        }



        public void DownloadAsset(GrayboxAsset asset)
        {
            if(EditorUtility.DisplayCancelableProgressBar("Loading Asset", "Loading Asset...", 0.5f))
                return;

            _downloadController.ImportPackage(asset.PackagePath);
            EditorUtility.ClearProgressBar();
        }

        public Texture2D DownloadImage(string path)
        {
            return _downloadController.DownloadImage(path);
        }

        public GameObject InstanciateAsset(GrayboxAsset asset)
        {
            GameObject assetGO = (GameObject)AssetDatabase.LoadMainAssetAtPath(asset.MainAssetPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(assetGO);
            instance.name = instance.name.Replace("(Clone)", "");

            return instance;
        }

        public void Disconnect()
        {
            _dbController.Disconnect();
        }
    }
}