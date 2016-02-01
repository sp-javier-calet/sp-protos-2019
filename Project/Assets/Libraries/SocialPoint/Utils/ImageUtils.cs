using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Utils
{
    public class ImageUtils
    {
        //TODO: Delete this function? Only used from SaveTextureToFile in BaseGame
        public static void CreateDirectory(string dirName)
        {
            FileUtils.CreateDirectory(dirName);//FileUtils verify if directory already exists and creates it if not
        }

        public static Error SaveTextureToFile(Texture2D texture, string fileName)
        {
            try
            {
                //TODO: Use FileUtils.WryteAllBytes instead? (delete "using System.IO;" if not needed anymore)
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

