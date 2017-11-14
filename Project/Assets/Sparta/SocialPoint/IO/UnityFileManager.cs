using System;
using System.IO;
using UnityEngine;

namespace SocialPoint.IO
{
    public class UnityFileManager : IFileManager
    {
        public ReadHandler Read(string asset)
        {
            var resource = Resources.Load<TextAsset>(asset);
            if(resource == null)
            {
                throw new Exception("file doesn't exist: " + asset);
            }
            return new ReadHandler(new MemoryStream(resource.bytes));
        }

        #if UNITY_EDITOR
        const string BytesExtension = ".bytes";
        readonly StandaloneFileManager _standalone = new StandaloneFileManager();
        #endif

        public virtual WriteHandler Write(string asset)
        {
            #if UNITY_EDITOR
            asset = Path.Combine(Application.dataPath, asset + BytesExtension);
            return _standalone.Write(asset);
            #else
            throw new Exception("You can't write on unity");
            #endif
        }
    }
}
