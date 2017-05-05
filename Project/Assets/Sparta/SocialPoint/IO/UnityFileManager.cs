
using System;
using System.IO;
using UnityEngine;
using SocialPoint.Utils;

namespace SocialPoint.IO
{
    public class UnityFileManager : IFileManager
    {
        public ReadHandler Read(string asset)
        {
            var resource = Resources.Load<UnityEngine.TextAsset>(asset);
            if(resource == null)
            {
                throw new Exception("file doesn't exist: " + asset);
            }
            return new ReadHandler(new MemoryStream(resource.bytes));
        }

        #if UNITY_EDITOR
        const string BytesExtension = ".bytes";
        StandaloneFileManager _standalone = new StandaloneFileManager();
        #endif

        public virtual WriteHandler Write(string asset)
        {
            #if UNITY_EDITOR
            asset = Path.Combine(UnityEngine.Application.dataPath, asset + BytesExtension);
            return _standalone.Write(asset);
            #else
            throw new Exception("You can't write on unity");
            #endif
        }
    }
}
