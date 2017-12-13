using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using System.Globalization;
using System.Linq;

namespace AssetBundleGraph
{
    public class ValidatorLog
    {

        private const string DATE = "SavedOn";
        private const string ENTRIES = "logEntries";
        private const string ENTRIES_KEY = "EntryKey";
        private const string ENTRIES_VALUE = "EntryValue";
        private const string EXECUTED_PLATFORMS = "Platforms";
        public const string DATE_FORMAT = "MM/dd/yyyy HH:mm:ss";
        public const string VIEW_DATE_FORMAT = "dd/MM/yyyy HH:mm:ss";

        public bool IsLocal;
        public Dictionary<string, ValidatorEntryData> entries = new Dictionary<string, ValidatorEntryData>();
        public DateTime lastExecuted;
        public Dictionary<BuildTargetGroup, DateTime> executedPlatforms = new Dictionary<BuildTargetGroup, DateTime>();

        public string FormatedDate
        {
            get
            {
                return lastExecuted.ToLocalTime().ToString(VIEW_DATE_FORMAT, CultureInfo.InvariantCulture);
            }
        }

        public ValidatorLog()
        {
            IsLocal = true;
        }

        public ValidatorLog(Dictionary<string, object> jsonData)
        {
            try
            {
                var entriesDict = jsonData[ENTRIES] as Dictionary<string, object>;

                lastExecuted = DateTime.ParseExact(jsonData[DATE] as string, DATE_FORMAT, CultureInfo.InvariantCulture);
                DateTime.SpecifyKind(lastExecuted, DateTimeKind.Utc);
                IsLocal = true;

                foreach(KeyValuePair<string, object> entry in entriesDict)
                {
                    entries.Add(entry.Key, new ValidatorEntryData(entry.Value as Dictionary<string, object>));
                }

                if(jsonData.ContainsKey(EXECUTED_PLATFORMS))
                {
                    var platformsDict = jsonData[EXECUTED_PLATFORMS] as Dictionary<string, object>;

                    foreach(var keyValue in platformsDict)
                    {
                        var date = DateTime.ParseExact(keyValue.Value as string, DATE_FORMAT, CultureInfo.InvariantCulture);
                        DateTime.SpecifyKind(date, DateTimeKind.Utc);
                        executedPlatforms.Add((BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), keyValue.Key), date);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Validation log corrupted, Exception found: " + e);
                entries = new Dictionary<string, ValidatorEntryData>();
                executedPlatforms = new Dictionary<BuildTargetGroup, DateTime>();
                lastExecuted = DateTime.MinValue;
                IsLocal = true;
            }
        }

        public Dictionary<string, object> ToJsonDictionary()
        {
            var dict = new Dictionary<string, object>();
            var auxDict = new Dictionary<string, object>();
            var platformDict = new Dictionary<string, string>();

            foreach(KeyValuePair<string, ValidatorEntryData> pair in entries)
            {
                auxDict.Add(pair.Key, pair.Value.ToJsonDictionary());
            }

            foreach(var keyValue in executedPlatforms)
            {
                platformDict.Add(keyValue.Key.ToString(), keyValue.Value.ToString(DATE_FORMAT, CultureInfo.InvariantCulture));
            }

            dict[ENTRIES] = auxDict;
            dict[DATE] = lastExecuted.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
            dict[EXECUTED_PLATFORMS] = platformDict;

            return dict;
        }

        private static string ValidatorLogPath
        {
            get
            {
                return FileUtility.PathCombine(SaveData.SaveDataDirectoryPath, AssetBundleGraphSettings.ASSETGRAPH_VALIDATOR_DATA_NAME);
            }
        }

        public void Save(string path = null)
        {
            if(path == null)
            {
                path = ValidatorLogPath;
            }

            var dir = Directory.GetParent(path).ToString();
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var prettifiedDataString = Json.Prettify(Json.Serialize(ToJsonDictionary()));

            using(var writter = new StreamWriter(path))
            {
                writter.Write(prettifiedDataString);
            }
        }

        public static ValidatorLog LoadFromText(string text, string name = "")
        {
            var deserialized = Json.Deserialize(text) as Dictionary<string, object>;
            var validatorLog = new ValidatorLog(deserialized);
            if(name != string.Empty)
            {
                validatorLog.IsLocal = false;
            }

            return validatorLog;
        }

        private static ValidatorLog Load(string path)
        {
            var dataStr = string.Empty;
            using(var sr = new StreamReader(path))
            {
                dataStr = sr.ReadToEnd();
            }

            return LoadFromText(dataStr);
        }


        public static bool IsValidatorLogAvailableAtDisk(string path = null)
        {
            if(path == null)
            {
                path = ValidatorLogPath;
            }

            return File.Exists(path);
        }


        public static ValidatorLog LoadFromDisk(string path = null)
        {
            if(path == null)
            {
                path = ValidatorLogPath;
            }

            if(!File.Exists(path))
            {
                var validatorLog = new ValidatorLog();
                validatorLog.Save();
                AssetDatabase.Refresh();
                return validatorLog;
            }

            try
            {
                ValidatorLog vLog = Load(path);
                if(!vLog.Validate())
                {
                    vLog.Save();

                    // reload and construct again from disk
                    return Load(path);
                }
                else
                {
                    return vLog;
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to deserialize ValidatorLog settings. Error:" + e + " File:" + path);
            }

            return new ValidatorLog();
        }

        public static void RemoveFromDisk(string path = null)
        {
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }


        public bool Validate()
        {
            //changes and autocleanups go here
            return true;
        }

        public void ClearOldValidators()
        {
            var data = SaveData.LoadFromDisk();
            var dirty = false;


            foreach(var key in entries.Keys.ToArray().Where(x => !data.Graph.Nodes.Exists(y => y.Id == x)))
            {
                entries.Remove(key);
                dirty = true;
            }

            foreach(var pair in entries)
            {
                dirty = dirty || pair.Value.CleanEmptyObjects() != 0;
            }

            if(dirty)
            {
                Save();
            }
        }

        public void LogToFile(NodeData validator, InvalidObjectInfo asset, BuildTargetGroup target)
        {
            if(entries.ContainsKey(validator.Id))
            {
                var existingEntry = entries[validator.Id];
                existingEntry.AddObjectAndUpdateInfo(validator, asset, target);
            }
            else
            {
                entries.Add(validator.Id, new ValidatorEntryData(validator, asset));
            }
        }

        public void RemoveSingleEntry(string validatorId, string assetId, BuildTargetGroup target)
        {
            if(entries.ContainsKey(validatorId))
            {
                var existingEntry = entries[validatorId];
                existingEntry.RemovePlatformForObject(assetId, target);
                CleanEmptyValidators();
            }
        }

        public void ClearObjectsForTarget(string validatorId, BuildTargetGroup target)
        {
            if(entries.ContainsKey(validatorId))
            {
                var validatorEntry = entries[validatorId];
                validatorEntry.RemovePlatformForAllObjects(target);
                CleanEmptyValidators();
            }
        }

        public void CleanEmptyValidators()
        {
            List<string> keysToRemove = new List<string>();
            foreach(var entry in entries)
            {
                entry.Value.CleanEmptyObjects();
                if(entry.Value.ObjectsWithError.Count == 0)
                {
                    keysToRemove.Add(entry.Key);
                }
            }
            for(int i = 0; i < keysToRemove.Count; i++)
            {
                entries.Remove(keysToRemove[i]);
            }
        }
    }
}
