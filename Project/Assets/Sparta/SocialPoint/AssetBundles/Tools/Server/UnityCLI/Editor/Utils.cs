using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using LitJson;


namespace SocialPoint.Tool.Server
{
    public sealed class Utils
    {
        /**
         * Get the closed generic method 'T ToObj<T> (string content)' for the 
         * type T of toObjType.
         * Allows direct json deserialisation into class toObjType
         */
        public static MethodInfo GetJsonMapperToObjGeneric(Type toObjType)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            MethodInfo[] matchingMethods = typeof(JsonMapper).GetMethods(flags);
            for(int i = 0; i < matchingMethods.Length; ++i)
            {
                MethodInfo method = matchingMethods[i];
                ParameterInfo[] parameters = method.GetParameters();
                if(method.Name == "ToObject" &&
                    parameters.Length == 1 &&
                    parameters[0].ParameterType == typeof(string) &&
                    method.IsGenericMethod)
                {
                    Type[] genericMethodTypes = method.GetGenericArguments();
                    if(genericMethodTypes.Length == 1 &&
                        genericMethodTypes[0] == method.ReturnType)
                    {
                        return method.MakeGenericMethod(toObjType);
                    }
                }
            }
            
            throw new Exception("Could not find a 'public static T JsonMapper.ToObj<T> (string)' definition");
        }

        /**
         * Get delegate for JsonMapper method 'string ToJson(object obj)'
         */
        public delegate string JsonMapperToJsonDelegate(object obj);
        public static JsonMapperToJsonDelegate JsonMapperToJson()
        {
            JsonMapperToJsonDelegate handler = JsonMapper.ToJson;
            return handler;
        }

        /**
         * Compares two string content by hash(for large strings)
         */
        public static bool CompareContent(string contentA, string contentB)
        {
            string hashA = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(contentA)));
            string hashB = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(contentB)));
            
            return hashA.Equals(hashB);
        }

        /**
         * Get the content of a system file as a string
         */
        public static string GetFileContents(string filePath)
        {
            if(!File.Exists(filePath))
            {
                throw new Exception(String.Format("file does not exist. (path: '{0}')", filePath));
            }
            
            string content;
            using(StreamReader reader = File.OpenText(filePath))
            {
                content = reader.ReadToEnd();
            }
            return content;
        }

        /**
         * Set the content of a system file
         */
        public static void SetFileContents(string filePath, string text, bool create=false)
        {
            if(!create && !File.Exists(filePath))
            {
                throw new Exception(String.Format("file does not exist. (path: '{0}')", filePath));
            }
            
            using(StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(text);
            }
        }

        /**
         * Search for a full filepath under the /Assets folder or more than one if allowMultiple is true
         */
        public static string[] GetFilePathsInAssets(string fileName, bool allowMultiple=false)
        {
            string[] possibleAssetPaths = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
            
            if(!allowMultiple && possibleAssetPaths.Length > 1)
            {
                string errMsg = String.Format("Multiple asset files found for {0}:\n", fileName);
                foreach(string assetPath in possibleAssetPaths)
                    errMsg += assetPath + "\n";
                
                throw new Exception(errMsg);
            }
            
            return possibleAssetPaths;
        }
    }
}
