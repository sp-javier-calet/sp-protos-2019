using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Base;

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

        public static Error SaveTextureToFile(Texture2D texture, string fileName)
        {
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
                return new Error("Could not write the file. " + e.ToString());
            }
            return null;
        }
    }
}

