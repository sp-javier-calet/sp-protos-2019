using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;

namespace BundleManagerJSON
{
    [InitializeOnLoad]
    internal class JSONDataAccessor
    {   
        static public JSONDataConfig JSONDataConfig
        {
            get
            {
                if(m_JSONDataConfig == null)
                {
                    m_JSONDataConfig = BMDataAccessor.loadObjectFromJsonFile<JSONDataConfig>(JSONConfigBundleData);
                }
                
                if(m_JSONDataConfig == null)
                {
                    m_JSONDataConfig = new JSONDataConfig();
                }
                
                return m_JSONDataConfig;
            }
        }
        
        static JSONDataAccessor()
        {
            JSONConfigBundleData = BMDataAccessor.BasePath + "/JSONConfigBundleData.txt";
            if (AssetDatabase.LoadAssetAtPath(JSONConfigBundleData, typeof(TextAsset)) == null)
            {
                SaveJSONDataConfig();
            }
        }

        static public void SaveJSONDataConfig()
        {
            saveObjectToJsonFile(JSONDataConfig, JSONConfigBundleData);
        }

        static public void SaveJSONPrefabData(string prefabString, string path, string fileName)
        {
            string saveDir = path + "/" + JSONConfigFolder + "/";
            if(!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            saveObjectToJsonFile<string>(prefabString, saveDir + fileName, true);
        }
        
        static private void saveObjectToJsonFile<T>(T data, string path, bool isJson = false)
        {
            TextWriter tw = new StreamWriter(path);
            if(tw == null)
            {
                Debug.LogError("Cannot write to " + path);
                return;
            }

            string jsonStr;

            if(isJson && data.GetType() == typeof(string))
            {
                jsonStr = data as string;
            }
            else
            {
                jsonStr = JsonFormatter.PrettyPrint(JsonMapper.ToJson(data));
            }

            tw.Write(jsonStr);
            tw.Flush();
            tw.Close();
            
            AssetDatabase.ImportAsset(path);
        }

        static private JSONDataConfig m_JSONDataConfig = null;

        public const string JSONConfigFolder = "json-components";
        public static string JSONConfigBundleData;
    }
}
