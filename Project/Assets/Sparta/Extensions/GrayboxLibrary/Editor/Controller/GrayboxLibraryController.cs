using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
            bool mounted = false;
            if(!Directory.Exists(GrayboxLibraryConfig.IconsPath))
            {
                var process = new ProcessStartInfo();
                process.UseShellExecute = false;
                process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.FileName = "mount";
                process.RedirectStandardError = true;
                process.RedirectStandardOutput = true;
                var run = Process.Start(process);
                while(!run.StandardError.EndOfStream)
                {
                    UnityEngine.Debug.LogError(run.StandardError.ReadLine());
                }
                while(!run.StandardOutput.EndOfStream)
                {
                    string outputText = run.StandardOutput.ReadLine();
                    if(outputText.Contains(GrayboxLibraryConfig.SmbFolder))
                    {
                        string newPath = outputText.Substring(outputText.IndexOf(GrayboxLibraryConfig.SmbFolder + " on ") + GrayboxLibraryConfig.SmbFolder.Length + 4);
                        newPath = newPath.Split(' ')[0];
                        GrayboxLibraryConfig.SetVolumePath(newPath);
                        mounted = true;
                    }
                    else if(outputText.Contains(GrayboxLibraryConfig.AltSmbFolder))
                    {
                        string newPath = outputText.Substring(outputText.IndexOf(GrayboxLibraryConfig.AltSmbFolder + " on ") + GrayboxLibraryConfig.AltSmbFolder.Length + 4);
                        newPath = newPath.Split(' ')[0];
                        GrayboxLibraryConfig.SetVolumePath(newPath);
                        mounted = true;
                    }
                }
                run.Close();
            }
            else
            {
                mounted = true;
            }

            if(!mounted)
            {
                var process = new ProcessStartInfo();
                process.UseShellExecute = false;
                process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.FileName = "mkdir";
                process.Arguments = "-p " + GrayboxLibraryConfig.VolumePath;
                process.RedirectStandardError = true;
                var run = Process.Start(process);
                while(!run.StandardError.EndOfStream)
                {
                    UnityEngine.Debug.LogError(run.StandardError.ReadLine());
                }

                process = new ProcessStartInfo();
                process.UseShellExecute = false;
                process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.FileName = "mount_smbfs";
                process.Arguments = GrayboxLibraryConfig.SmbConnectionUrl + " " + GrayboxLibraryConfig.VolumePath;
                process.RedirectStandardError = true;
                run = Process.Start(process);
                while(!run.StandardError.EndOfStream)
                {
                    UnityEngine.Debug.LogError(run.StandardError.ReadLine());
                }

                run.Close();

                for(int i = 0; !Directory.Exists(GrayboxLibraryConfig.PkgDefaultFolder) && i < 100; i++)
                {
                    Thread.Sleep(100);
                    if(i == 99)
                    {
                        EditorUtility.DisplayDialog("Graybox tool", "Connection timeout. Please, make sure that you are connected to the SocialPoint network: \n wifi: 'SP_EMPLOYEE' \n\n Check also that you have specified your Mac's password correctly.", "Close");
                        if(GrayboxLibraryWindow.Window != null)
                        {
                            GrayboxLibraryWindow.Window.Close();
                        }
                        Selection.activeObject = null;
                        return;
                    }
                }
            }
            #endif

            _dbController = GrayboxLibraryDB.GetInstance();
            _downloadController = GrayboxLibraryDownloader.GetInstance();
            Connect();
        }

        public GrayboxAsset GetAsset(string name)
        {
            GrayboxAsset asset = null;

            MySqlCommand command = new MySqlCommand("SELECT a.id_asset, a.name, a.category, a.main_asset_path, a.pkg_path, a.thumb_path, a.animated_thumb_path, DATE_FORMAT(a.creation_date, '%m/%d/%Y %H:%i:%s') as 'creation_date' FROM asset a WHERE a.name LIKE @NAME");
            command.Parameters.AddWithValue("@NAME", name);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

            if(queryResult.Count > 0)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[0];

                Texture2D thumb = null;
                if(row["thumb_path"].Length > 0)
                    thumb = _downloadController.DownloadImage(row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath));

                string[] split1 = row["creation_date"].Split(' ');
                string[] date = split1[0].Split('/');
                string[] time = split1[1].Split(':');
                DateTime finalDate = new DateTime(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));

                asset = new GrayboxAsset(int.Parse(row["id_asset"]), row["name"], (GrayboxAssetCategory)Enum.Parse(typeof(GrayboxAssetCategory), row["category"]),
                    row["main_asset_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                    row["pkg_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                    row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                    row["animated_thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath), thumb, finalDate);
            }

            return asset;
        }

        public ArrayList GetAssets(string[] tags, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000, bool downloadThumbnail = true)
        {
            ArrayList assets = new ArrayList();

            MySqlCommand commandTag = new MySqlCommand("");
            for(int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];
                commandTag.CommandText += " NATURAL JOIN (SELECT id_asset FROM asset_tag NATURAL JOIN tag WHERE name LIKE CONCAT('%', @TAG" + i + ", '%')) as tag" + i;
                commandTag.Parameters.AddWithValue("@TAG" + i, tag);
            }

            string sql = "SELECT DISTINCT a.id_asset, a.name, a.category, a.main_asset_path, a.pkg_path, a.thumb_path, a.animated_thumb_path, DATE_FORMAT(a.creation_date, '%m/%d/%Y %H:%i:%s') as 'creation_date' "
                         + "FROM asset a " + commandTag.CommandText + " WHERE a.category LIKE '" + category.ToString() + "' ORDER BY a.name ASC, a.creation_date DESC LIMIT " + startLimit + "," + endLimit;
            
            MySqlCommand command = new MySqlCommand(sql);

            for(int i = 0; i < commandTag.Parameters.Count; i++)
                command.Parameters.AddWithValue(commandTag.Parameters[i].ParameterName, commandTag.Parameters[i].Value);
            
            ArrayList queryResult = _dbController.ExecuteQuery(command);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                Texture2D thumb = null;
                if(downloadThumbnail)
                    thumb = _downloadController.DownloadImage(row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                        .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath));

                string[] split1 = row["creation_date"].Split(' ');
                string[] date = split1[0].Split('/');
                string[] time = split1[1].Split(':');
                DateTime finalDate = new DateTime(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));

                GrayboxAsset asset = new GrayboxAsset(int.Parse(row["id_asset"]), row["name"], (GrayboxAssetCategory)Enum.Parse(typeof(GrayboxAssetCategory), row["category"]),
                                         row["main_asset_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                                         row["pkg_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                                         row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                                         row["animated_thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath), thumb, finalDate);

                assets.Add(asset);
            }

            return assets;
        }

        public ArrayList GetAssetsByName(string[] filters, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000, bool downloadThumbnail = true)
        {
            ArrayList assets = new ArrayList();

            string sql = "SELECT DISTINCT a.id_asset, a.name, a.category, a.main_asset_path, a.pkg_path, a.thumb_path, a.animated_thumb_path,  DATE_FORMAT(a.creation_date, '%m/%d/%Y %H:%i:%s') as 'creation_date' "
                         + "FROM asset a WHERE a.category LIKE '" + category.ToString() + "'";

            MySqlCommand commandFilteredSQL = new MySqlCommand("");
            
            for(int i = 0; i < filters.Length; i++)
            {
                string filter = filters[i];
                commandFilteredSQL.CommandText = commandFilteredSQL.CommandText + " a.name LIKE CONCAT('%', @FILTER" + i + ", '%') AND";
                commandFilteredSQL.Parameters.AddWithValue("@FILTER" + i, filter);
            }

            if(commandFilteredSQL.CommandText.Length > 0)
                commandFilteredSQL.CommandText = " AND (" + commandFilteredSQL.CommandText.Substring(0, commandFilteredSQL.CommandText.Length - 3) + ")";

            sql = sql + commandFilteredSQL.CommandText + " ORDER BY a.name ASC, a.creation_date DESC LIMIT " + startLimit + "," + endLimit;

            MySqlCommand command = new MySqlCommand(sql);

            for(int i = 0; i < commandFilteredSQL.Parameters.Count; i++)
                command.Parameters.AddWithValue(commandFilteredSQL.Parameters[i].ParameterName, commandFilteredSQL.Parameters[i].Value);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                Texture2D thumb = null;
                if(downloadThumbnail)
                    thumb = _downloadController.DownloadImage(row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                        .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath));

                string[] split1 = row["creation_date"].Split(' ');
                string[] date = split1[0].Split('/');
                string[] time = split1[1].Split(':');
                DateTime finalDate = new DateTime(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));

                GrayboxAsset asset = new GrayboxAsset(int.Parse(row["id_asset"]), row["name"], (GrayboxAssetCategory)Enum.Parse(typeof(GrayboxAssetCategory), row["category"]),
                                         row["main_asset_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                                         row["pkg_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                                         row["thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath),
                                         row["animated_thumb_path"].Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.VolumePath)
                    .Replace(GrayboxLibraryConfig.WinVolumePath, GrayboxLibraryConfig.VolumePath).Replace(GrayboxLibraryConfig.WinVolumePathAlt, GrayboxLibraryConfig.VolumePath), thumb, finalDate);

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
            MySqlCommand commandTagSearchSQL = new MySqlCommand("");
            for(int i = 0; i < tags.Length; i++)
            {
                string tag = tags[i];
                commandTagSearchSQL.CommandText += " NATURAL JOIN (SELECT id_asset FROM asset_tag NATURAL JOIN tag WHERE name LIKE CONCAT('%', @TAG" + i + ", '%')) as tag" + i;
                commandTagSearchSQL.Parameters.AddWithValue("@TAG" + i, tag);
            }

            string sql = "SELECT DISTINCT a.id_asset "
                         + "FROM asset a " + commandTagSearchSQL.CommandText + " WHERE a.category LIKE '" + category.ToString() + "'";

            MySqlCommand command = new MySqlCommand(sql);

            for(int i = 0; i < commandTagSearchSQL.Parameters.Count; i++)
                command.Parameters.AddWithValue(commandTagSearchSQL.Parameters[i].ParameterName, commandTagSearchSQL.Parameters[i].Value);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

            return queryResult.Count;
        }



        public GrayboxTag GetTag(string name)
        {
            ArrayList tags = GetTags(name, 0, 1);
            GrayboxTag tag = null;
            if(tags.Count > 0)
                tag = (GrayboxTag)tags[0];

            return tag;
        }


        public ArrayList GetTags(string name, int startLimit = 0, int endLimit = 1000)
        {
            ArrayList tags = new ArrayList();

            MySqlCommand command = new MySqlCommand("SELECT DISTINCT t.id_tag, t.name FROM tag t WHERE t.name LIKE CONCAT('%', @NAME, '%') LIMIT " + startLimit + ", " + endLimit);
            command.Parameters.AddWithValue("@NAME", name);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

            for(int i = 0; i < queryResult.Count; i++)
            {
                Dictionary<string, string> row = (Dictionary<string, string>)queryResult[i];
                GrayboxTag tag = new GrayboxTag(int.Parse(row["id_tag"]), row["name"]);
                tags.Add(tag);
            }

            return tags;
        }

        public ArrayList GetTagsInCategory(string name, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000)
        {
            ArrayList tags = new ArrayList();

            MySqlCommand command = new MySqlCommand("SELECT DISTINCT t.id_tag, t.name FROM tag t INNER JOIN asset_tag atag on t.id_tag = atag.id_tag INNER JOIN asset a on atag.id_asset = a.id_asset WHERE t.name LIKE CONCAT('%', @NAME, '%') AND a.category LIKE '" + category.ToString() + "' LIMIT " + startLimit + ", " + endLimit);
            command.Parameters.AddWithValue("@NAME", name);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

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

            ArrayList gbTags = GetTags(name, startLimit, endLimit);

            for(int i = 0; i < gbTags.Count; i++)
            {
                GrayboxTag tag = (GrayboxTag)gbTags[i];
                tags.Add(tag.Name);
            }

            return tags.ToArray();
        }

        public string[] GetTagsInCategoryAsText(string name, GrayboxAssetCategory category, int startLimit = 0, int endLimit = 1000)
        {
            List<string> tags = new List<string>();

            ArrayList gbTags = GetTagsInCategory(name, category, startLimit, endLimit);

            for(int i = 0; i < gbTags.Count; i++)
            {
                GrayboxTag tag = (GrayboxTag)gbTags[i];
                tags.Add(tag.Name);
            }

            return tags.ToArray();
        }


        public ArrayList GetAssetTags(GrayboxAsset asset, int startLimit = 0, int endLimit = 1000)
        {
            ArrayList tags = new ArrayList();

            MySqlCommand command = new MySqlCommand("SELECT DISTINCT t.id_tag, t.name FROM tag t, asset_tag atag WHERE t.id_tag = atag.id_tag AND atag.id_asset = " + asset.Id + " LIMIT " + startLimit + "," + endLimit);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

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

            MySqlCommand command = new MySqlCommand("SELECT DISTINCT t.id_tag, t.name FROM tag t LIMIT " + startLimit + "," + endLimit);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

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

            MySqlCommand command = new MySqlCommand(sql);

            ArrayList queryResult = _dbController.ExecuteQuery(command);
            if(queryResult.Count == 0)
            {
                sql = "SELECT a.id_asset FROM asset a WHERE a.name LIKE @NAME";

                command = new MySqlCommand(sql);
                command.Parameters.AddWithValue("@NAME", asset.Name);

                queryResult = _dbController.ExecuteQuery(command);
                if(queryResult.Count == 0)
                {
                    sql = "INSERT INTO asset (name, category, main_asset_path, pkg_path, thumb_path, animated_thumb_path) VALUES (@NAME, @CATEGORY, @MAINASSET, @PKG, @THUMB, @ANIMTHUMB)";

                    command = new MySqlCommand(sql);
                    command.Parameters.AddWithValue("@NAME", asset.Name);
                    command.Parameters.AddWithValue("@CATEGORY", asset.Category.ToString());
                    command.Parameters.AddWithValue("@MAINASSET", asset.MainAssetPath.Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.WinVolumePath));
                    command.Parameters.AddWithValue("@PKG", asset.PackagePath);
                    command.Parameters.AddWithValue("@THUMB", asset.ThumbnailPath);
                    command.Parameters.AddWithValue("@ANIMTHUMB", asset.AnimatedThumbnailPath);

                    _dbController.ExecuteSQL(command);
                }
            }
            else
            {
                sql = "UPDATE asset SET name =@NAME, category =@CATEGORY, main_asset_path =@MAINASSET, pkg_path =@PKG, thumb_path =@THUMB, animated_thumb_path =@ANIMTHUMB WHERE id_asset = " + asset.Id;

                command = new MySqlCommand(sql);
                command.Parameters.AddWithValue("@NAME", asset.Name);
                command.Parameters.AddWithValue("@CATEGORY", asset.Category.ToString());
                command.Parameters.AddWithValue("@MAINASSET", asset.MainAssetPath.Replace(GrayboxLibraryConfig.MacVolumePath, GrayboxLibraryConfig.WinVolumePath));
                command.Parameters.AddWithValue("@PKG", asset.PackagePath);
                command.Parameters.AddWithValue("@THUMB", asset.ThumbnailPath);
                command.Parameters.AddWithValue("@ANIMTHUMB", asset.AnimatedThumbnailPath);
                
                _dbController.ExecuteSQL(command);
            }
        }

        public void RemoveAsset(GrayboxAsset asset)
        {
            string sql = "DELETE FROM asset_tag WHERE id_asset = " + asset.Id;

            MySqlCommand command = new MySqlCommand(sql);

            _dbController.ExecuteSQL(command);

            sql = "DELETE FROM asset WHERE id_asset = " + asset.Id;

            command = new MySqlCommand(sql);

            _dbController.ExecuteSQL(command);

        }


        public void CreateTag(GrayboxTag tag)
        {
            string sql = "INSERT INTO tag (name) VALUES (@NAME)";

            MySqlCommand command = new MySqlCommand(sql);
            command.Parameters.AddWithValue("@NAME", tag.Name);

            _dbController.ExecuteSQL(command);
        }


        public void AssignTag(GrayboxAsset asset, GrayboxTag tag)
        {
            string sql = "SELECT id_asset FROM asset_tag WHERE id_asset = " + asset.Id + " AND id_tag = " + tag.Id;

            MySqlCommand command = new MySqlCommand(sql);

            ArrayList queryResult = _dbController.ExecuteQuery(command);

            if(queryResult.Count == 0)
            {
                sql = "INSERT INTO asset_tag VALUES (" + asset.Id + "," + tag.Id + ")";

                command = new MySqlCommand(sql);

                _dbController.ExecuteSQL(command);
            }
        }

        public void UnassignTag(GrayboxAsset asset, GrayboxTag tag)
        {
            string sql = "DELETE FROM asset_tag WHERE id_asset = " + asset.Id + " AND id_tag = " + tag.Id;

            MySqlCommand command = new MySqlCommand(sql);

            _dbController.ExecuteSQL(command);
        }


        public int GetAssetCategoryByPrefix(string fullname)
        {
            int category = -1;
            var enumerator = GrayboxLibraryConfig.CategoryPrefix.GetEnumerator();
            while(enumerator.MoveNext())
            {
                string prefix = enumerator.Current.Value;
                if(fullname.Contains(prefix))
                    category = (int)enumerator.Current.Key;
            }
            enumerator.Dispose();
            
            return category;
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

        public void FlushImageCache()
        {
            _downloadController.FlushImageCache();
        }

        public GameObject InstantiateAsset(GrayboxAsset asset, Transform parent = null)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GameObject assetGO = (GameObject)AssetDatabase.LoadMainAssetAtPath(asset.MainAssetPath);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(assetGO);
            instance.name = instance.name.Replace("(Clone)", "");
            if(asset.Category == GrayboxAssetCategory.UI)
            {
                if(parent == null)
                    parent = ((Canvas)GameObject.FindObjectOfType(typeof(Canvas))).transform;
                instance.transform.SetParent(parent, false);
            }

            if(GrayboxLibraryConfig.ScriptOnInstance[asset.Category] != null)
                instance.AddComponent(GrayboxLibraryConfig.ScriptOnInstance[asset.Category]);

            return instance;
        }

        public void Disconnect()
        {
            _dbController.Disconnect();
        }

        public void Connect()
        {
            _dbController.Connect();
        }
    }
}