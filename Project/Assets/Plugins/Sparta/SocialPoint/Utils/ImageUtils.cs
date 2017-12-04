using System;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Utils
{
    public sealed class ImageUtils
    {
        public static Error SaveTextureToFile(Texture2D texture, string fileName)
        {
            try
            {
                byte[] bytes = texture.EncodeToPNG();
                FileUtils.WriteAllBytes(fileName, bytes);
            }
            catch(Exception e)
            {
                return new Error("Could not write the file. " + e.ToString());
            }
            return null;
        }
    }
}

