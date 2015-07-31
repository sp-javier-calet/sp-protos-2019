using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace SocialPoint.Utils
{
    public class ImageUtils
    {
        public static void CreateDirectory(string dirName)
        {
            if(!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        public static bool SaveTextureToFile(Texture2D texture, string fileName, ref string errorMsg)
        {
            bool success = true;
            try
            {
                byte[] bytes = texture.EncodeToPNG();
                string dirName = Path.GetDirectoryName(fileName);
                CreateDirectory(dirName);
                FileStream file = File.Open(fileName, FileMode.Create);
                BinaryWriter binary = new BinaryWriter(file);
                binary.Write(bytes);
                file.Close();
            }
            catch(Exception e)
            {
                success = false;
                if(errorMsg != null)
                {
                    errorMsg = "Could not write the file. " + e.ToString();
                }
            }
            return success;
        }
    }
}

